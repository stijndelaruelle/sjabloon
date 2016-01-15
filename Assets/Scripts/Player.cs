using UnityEngine;
using System.Collections;

namespace Platformer
{
    public class Player : MonoBehaviour
    {
        public enum PlayerState
        {
            Walk = 0,
            Jump = 1,
            Fall = 2
        }

        [SerializeField]
        private CharacterController2D m_CharacterController;
        public CharacterController2D CharacterController
        {
            get { return m_CharacterController; }
        }

        private CharacterState m_CurrentState;
        private CharacterState[] m_CharacterStates = new CharacterState[3]; //Cache of all the states

        [SerializeField]
        private Animator m_Animator;
        private bool m_IsFiring;

        //Movement parameters (maybe put in another struct)
        [SerializeField]
        private float m_Gravity;
        public float Gravity
        {
            get { return m_Gravity; }
        }

        [SerializeField]
        private float m_Acceleration;
        public float Acceleration
        {
            get { return m_Acceleration; }
        }

        [SerializeField]
        private float m_Friction;
        public float Friction
        {
            get { return m_Friction; }
        }

        [SerializeField]
        private float m_MaxRunSpeed;
        public float MaxRunSpeed
        {
            get { return m_MaxRunSpeed; }
        }

        private Vector2 m_Velocity = new Vector2();
        public Vector2 Velocity
        {
            get { return m_Velocity; }
        }

        [SerializeField]
        private float m_JumpAcceleration;
        public float JumpAcceleration
        {
            get { return m_JumpAcceleration; }
        }

        [SerializeField]
        private float m_MaxJumpTime;
        public float MaxJumpTime
        {
            get { return m_MaxJumpTime; }
        }

        [SerializeField]
        private float m_MaxFallSpeed;
        public float MaxFallSpeed
        {
            get { return m_MaxFallSpeed; }
        }

        private void Start()
        {
            //Movement
            InputManager inputManager = InputManager.Instance;
            inputManager.BindAxis("HorizontalAxis", KeyCode.RightArrow, KeyCode.LeftArrow);
            inputManager.BindAxis("HorizontalAxis", 0, ControllerButtonCode.Right, ControllerButtonCode.Left);
            inputManager.BindAxis("HorizontalAxis", 0, ControllerAxisCode.LeftStickX);

            //Jumping
            inputManager.BindButton("Jump", KeyCode.UpArrow, InputManager.ButtonState.OnPress);
            inputManager.BindButton("Jump", 0, ControllerButtonCode.A, InputManager.ButtonState.OnPress);

            inputManager.BindButton("JumpPressed", KeyCode.UpArrow, InputManager.ButtonState.Pressed);
            inputManager.BindButton("JumpPressed", 0, ControllerButtonCode.A, InputManager.ButtonState.Pressed);

            //Shooting
            inputManager.BindButton("Fire", KeyCode.Space, InputManager.ButtonState.Pressed);
            inputManager.BindButton("Fire", 0, ControllerButtonCode.X, InputManager.ButtonState.Pressed);

            //Create all the states
            m_CharacterStates[0] = new WalkState(this);
            m_CharacterStates[1] = new JumpState(this);
            m_CharacterStates[2] = new FallState(this);

            SetState(PlayerState.Fall);
        }

        private void Update()
        {
            UpdateMovement();
            UpdateAnimations();
            UpdateScale();
        }

        private void UpdateMovement()
        {
            Vector2 newDelta = m_CurrentState.Update(m_Velocity);

            //Even megaman has to obey the speed limits!
            if (Mathf.Abs(newDelta.x) > m_MaxRunSpeed)
                newDelta.x = m_MaxRunSpeed * Mathf.Sign(m_Velocity.x);

            if (newDelta.y < -m_MaxFallSpeed)
                newDelta.y = -m_MaxFallSpeed;

            //Move
            m_Velocity = m_CharacterController.Move(newDelta.x, newDelta.y);
        }

        private void UpdateAnimations()
        {
            float runFranction = Mathf.Abs(m_Velocity.x / m_MaxRunSpeed);
            m_Animator.SetFloat("VelocityFraction", runFranction);

            if (m_Animator.GetBool("IsGrounded") != m_CharacterController.IsGrounded)
            {
                m_Animator.SetBool("IsGrounded", m_CharacterController.IsGrounded);
            }

            if (m_Animator.GetBool("IsFiring") != m_IsFiring)
            {
                m_Animator.SetBool("IsFiring", m_IsFiring);
            }
        }

        private void UpdateScale()
        {
            if (Mathf.Abs(m_Velocity.x) > 0.0f)
            {
                transform.localScale = new Vector3(Mathf.Sign(m_Velocity.x), 1.0f, 1.0f);
            }
        }

        public void SetState(PlayerState newState)
        {
            if (m_CurrentState != null)
                m_CurrentState.OnExit();

            m_CurrentState = m_CharacterStates[(int)newState];
            m_CurrentState.OnEnter();

            //Debug.Log("Entered state: " + newState);
        }

        public void SetFiring(bool fireState)
        {
            m_IsFiring = fireState;
        }
    }

    public class WalkState : CharacterState
    {
        private Player m_PlayerRef = null;

        public WalkState(Player player)
        {
            m_PlayerRef = player;
        }

        public void OnEnter()
        {

        }

        public void OnExit()
        {

        }

        public Vector2 Update(Vector2 velocity)
        {
            float deltaX = velocity.x;
            float deltaY = velocity.y - m_PlayerRef.Gravity;

            //Moving horizontally
            float horizValue = InputManager.Instance.GetAxis("HorizontalAxis");
            deltaX += horizValue * m_PlayerRef.Acceleration;

            if (horizValue == 0.0f && deltaX != 0.0f)
            {
                float sign = Mathf.Sign(m_PlayerRef.Velocity.x);
                deltaX -= m_PlayerRef.Friction * sign;

                if (sign != Mathf.Sign(deltaX))
                    deltaX = 0.0f;
            }

            //Change to jump state
            if (m_PlayerRef.CharacterController.IsGrounded)
            {
                if (InputManager.Instance.GetButton("Jump"))
                {
                    //Change to the jump state
                    m_PlayerRef.SetState(Player.PlayerState.Jump);
                }
                else
                {
                    //Stick better to the ground
                    deltaY -= m_PlayerRef.Gravity * 10.0f;
                }
            }

            //Shoot
            m_PlayerRef.SetFiring((InputManager.Instance.GetButton("Fire")));

            return new Vector2(deltaX, deltaY);
        }
    }

    public class JumpState : CharacterState
    {
        private Player m_PlayerRef = null;
        private float m_CurrentJumpTime = 0.0f;

        public JumpState(Player player)
        {
            m_PlayerRef = player;
        }

        public void OnEnter()
        {
            m_CurrentJumpTime = 0.0f;
        }

        public void OnExit()
        {

        }

        public Vector2 Update(Vector2 velocity)
        {
            float deltaX = velocity.x;
            float deltaY = velocity.y - m_PlayerRef.Gravity;

            //Moving horizontally
            float horizValue = InputManager.Instance.GetAxis("HorizontalAxis");
            deltaX += horizValue * m_PlayerRef.Acceleration;

            if (horizValue == 0.0f && deltaX != 0.0f)
            {
                float sign = Mathf.Sign(m_PlayerRef.Velocity.x);
                deltaX -= m_PlayerRef.Friction * sign;

                if (sign != Mathf.Sign(deltaX))
                    deltaX = 0.0f;
            }

            if (InputManager.Instance.GetButton("JumpPressed"))
            {
                float factor = 1.0f - (m_CurrentJumpTime / m_PlayerRef.MaxJumpTime);
                deltaY += (m_PlayerRef.JumpAcceleration * (factor * factor));

                m_CurrentJumpTime += Time.deltaTime;
                if (m_CurrentJumpTime > m_PlayerRef.MaxJumpTime)
                {
                    m_PlayerRef.SetState(Player.PlayerState.Fall);
                }
            }
            else
            {
                m_PlayerRef.SetState(Player.PlayerState.Fall);
            }

            //Shoot
            m_PlayerRef.SetFiring((InputManager.Instance.GetButton("Fire")));

            return new Vector2(deltaX, deltaY);
        }
    }

    public class FallState : CharacterState
    {
        private Player m_PlayerRef = null;

        public FallState(Player player)
        {
            m_PlayerRef = player;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {

        }

        public Vector2 Update(Vector2 velocity)
        {
            float deltaX = velocity.x;
            float deltaY = velocity.y - m_PlayerRef.Gravity;

            //Potentional double jump

            //Moving horizontally
            float horizValue = InputManager.Instance.GetAxis("HorizontalAxis");
            deltaX += horizValue * m_PlayerRef.Acceleration;

            if (horizValue == 0.0f && deltaX != 0.0f)
            {
                float sign = Mathf.Sign(m_PlayerRef.Velocity.x);
                deltaX -= m_PlayerRef.Friction * sign;

                if (sign != Mathf.Sign(deltaX))
                    deltaX = 0.0f;
            }

            if (m_PlayerRef.CharacterController.IsGrounded)
            {
                m_PlayerRef.SetState(Player.PlayerState.Walk);
            }

            //Shoot
            m_PlayerRef.SetFiring((InputManager.Instance.GetButton("Fire")));

            return new Vector2(deltaX, deltaY);
        }
    }
}