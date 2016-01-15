using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    //Data class
    [System.Serializable]
    public class MovementProperties
    {
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
    }

    public class Player : MonoBehaviour
    {
        public enum PlayerState
        {
            Walk = 0,
            Jump = 1,
            Fall = 2
        }

        [SerializeField]
        private int m_PlayerID;
        public int PlayerID
        {
            get { return m_PlayerID; }
        }

        [SerializeField]
        private CharacterController2D m_CharacterController;
        public CharacterController2D CharacterController
        {
            get { return m_CharacterController; }
        }

        [SerializeField]
        private Animator m_Animator;
        private bool m_IsFiring;

        [SerializeField]
        private Gun m_Gun;

        [SerializeField]
        private MovementProperties m_MovementProperties;
        public MovementProperties MovementProperties
        {
            get { return m_MovementProperties; }
        }

        private CharacterState m_CurrentState;
        private CharacterState[] m_CharacterStates = new CharacterState[3]; //Cache of all the states

        private Vector2 m_Velocity = new Vector2();
        public Vector2 Velocity
        {
            get { return m_Velocity; }
        }

        //Functions
        private void Start()
        {
            InitializeControls();
            InitializeStates();
        }

        private void InitializeControls()
        {
            //Movement
            InputManager inputManager = InputManager.Instance;
            inputManager.BindAxis("HorizontalAxis_" + m_PlayerID, KeyCode.RightArrow, KeyCode.LeftArrow);
            inputManager.BindAxis("HorizontalAxis_" + m_PlayerID, m_PlayerID, ControllerButtonCode.Right, ControllerButtonCode.Left);
            inputManager.BindAxis("HorizontalAxis_" + m_PlayerID, m_PlayerID, ControllerAxisCode.LeftStickX);

            //Jumping
            inputManager.BindButton("Jump_" + m_PlayerID, KeyCode.UpArrow, InputManager.ButtonState.OnPress);
            inputManager.BindButton("Jump_" + m_PlayerID, m_PlayerID, ControllerButtonCode.A, InputManager.ButtonState.OnPress);

            inputManager.BindButton("JumpPressed_" + m_PlayerID, KeyCode.UpArrow, InputManager.ButtonState.Pressed);
            inputManager.BindButton("JumpPressed_" + m_PlayerID, m_PlayerID, ControllerButtonCode.A, InputManager.ButtonState.Pressed);

            //Shooting
            inputManager.BindButton("Fire_" + m_PlayerID, KeyCode.Space, InputManager.ButtonState.Pressed);
            inputManager.BindButton("Fire_" + m_PlayerID, m_PlayerID, ControllerButtonCode.X, InputManager.ButtonState.Pressed);
        }

        private void InitializeStates()
        {
            //Create all the states
            m_CharacterStates[0] = new WalkState(this);
            m_CharacterStates[1] = new JumpState(this);
            m_CharacterStates[2] = new FallState(this);

            SetState(PlayerState.Fall);
        }

        private void Update()
        {
            UpdateMovement();
            UpdateShooting();
            UpdateAnimations();
            UpdateRotation();
        }

        private void UpdateMovement()
        {
            Vector2 newDelta = m_CurrentState.Update(m_Velocity);

            //Even megaman has to obey the speed limits!
            if (Mathf.Abs(newDelta.x) > m_MovementProperties.MaxRunSpeed)
                newDelta.x = m_MovementProperties.MaxRunSpeed * Mathf.Sign(m_Velocity.x);

            if (newDelta.y < -m_MovementProperties.MaxFallSpeed)
                newDelta.y = -m_MovementProperties.MaxFallSpeed;

            //Move
            m_Velocity = m_CharacterController.Move(newDelta.x, newDelta.y);
        }

        private void UpdateShooting()
        {
            if (m_Gun == null)
                return;

            if (m_IsFiring)
            {
                m_Gun.Fire();
            }
        }

        private void UpdateAnimations()
        {
            float runFranction = Mathf.Abs(m_Velocity.x / m_MovementProperties.MaxRunSpeed);
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

        private void UpdateRotation()
        {
            if (m_Velocity.x == 0.0f)
                return;

            float rotation = 0.0f;
            if (Mathf.Sign(m_Velocity.x) < 0.0f)
            {
                rotation = 180.0f;
            }

            transform.localRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                
            //Used to update the scale instead of rotation (but then all our children don't come along!)    
            //transform.localScale = new Vector3(Mathf.Sign(m_Velocity.x), 1.0f, 1.0f);
        }

        public void SetState(PlayerState newState)
        {
            if (m_CurrentState != null)
                m_CurrentState.OnExit();

            m_CurrentState = m_CharacterStates[(int)newState];
            m_CurrentState.OnEnter();
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
            MovementProperties movementProperties = m_PlayerRef.MovementProperties;

            float deltaX = velocity.x;
            float deltaY = velocity.y - movementProperties.Gravity;

            //Moving horizontally
            float horizValue = InputManager.Instance.GetAxis("HorizontalAxis_" + m_PlayerRef.PlayerID);
            deltaX += horizValue * movementProperties.Acceleration;

            if (horizValue == 0.0f && deltaX != 0.0f)
            {
                float sign = Mathf.Sign(m_PlayerRef.Velocity.x);
                deltaX -= movementProperties.Friction * sign;

                if (sign != Mathf.Sign(deltaX))
                    deltaX = 0.0f;
            }

            //Change to jump state
            if (m_PlayerRef.CharacterController.IsGrounded)
            {
                if (InputManager.Instance.GetButton("Jump_" + m_PlayerRef.PlayerID))
                {
                    //Change to the jump state
                    m_PlayerRef.SetState(Player.PlayerState.Jump);
                }
                else
                {
                    //Stick better to the ground
                    deltaY -= movementProperties.Gravity * 10.0f;
                }
            }
            else
            {
                //Change to the fall state
                deltaY = 0.0f;
                m_PlayerRef.SetState(Player.PlayerState.Fall);
            }

            //Shoot
            m_PlayerRef.SetFiring((InputManager.Instance.GetButton("Fire_" + m_PlayerRef.PlayerID)));

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
            MovementProperties movementProperties = m_PlayerRef.MovementProperties;

            float deltaX = velocity.x;
            float deltaY = velocity.y - movementProperties.Gravity;

            //Moving horizontally
            float horizValue = InputManager.Instance.GetAxis("HorizontalAxis_" + m_PlayerRef.PlayerID);
            deltaX += horizValue * movementProperties.Acceleration;

            if (horizValue == 0.0f && deltaX != 0.0f)
            {
                float sign = Mathf.Sign(m_PlayerRef.Velocity.x);
                deltaX -= movementProperties.Friction * sign;

                if (sign != Mathf.Sign(deltaX))
                    deltaX = 0.0f;
            }

            if (InputManager.Instance.GetButton("JumpPressed_" + m_PlayerRef.PlayerID))
            {
                float factor = 1.0f - (m_CurrentJumpTime / movementProperties.MaxJumpTime);
                deltaY += (movementProperties.JumpAcceleration * (factor * factor));

                m_CurrentJumpTime += Time.deltaTime;
                if (m_CurrentJumpTime > movementProperties.MaxJumpTime)
                {
                    m_PlayerRef.SetState(Player.PlayerState.Fall);
                }
            }
            else
            {
                m_PlayerRef.SetState(Player.PlayerState.Fall);
            }

            //Shoot
            m_PlayerRef.SetFiring((InputManager.Instance.GetButton("Fire_" + m_PlayerRef.PlayerID)));

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
            MovementProperties movementProperties = m_PlayerRef.MovementProperties;

            float deltaX = velocity.x;
            float deltaY = velocity.y - movementProperties.Gravity;

            //Potentional double jump

            //Moving horizontally
            float horizValue = InputManager.Instance.GetAxis("HorizontalAxis_" + m_PlayerRef.PlayerID);
            deltaX += horizValue * movementProperties.Acceleration;

            if (horizValue == 0.0f && deltaX != 0.0f)
            {
                float sign = Mathf.Sign(m_PlayerRef.Velocity.x);
                deltaX -= movementProperties.Friction * sign;

                if (sign != Mathf.Sign(deltaX))
                    deltaX = 0.0f;
            }

            if (m_PlayerRef.CharacterController.IsGrounded)
            {
                m_PlayerRef.SetState(Player.PlayerState.Walk);
            }

            //Shoot
            m_PlayerRef.SetFiring((InputManager.Instance.GetButton("Fire_" + m_PlayerRef.PlayerID)));

            return new Vector2(deltaX, deltaY);
        }
    }
}