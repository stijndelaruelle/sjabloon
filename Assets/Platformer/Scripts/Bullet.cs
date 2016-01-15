using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class Bullet : PoolableObject, IDamageDealer
    {
        [SerializeField]
        private SpriteRenderer m_SpriteRenderer;

        [SerializeField]
        private Pool m_HitEffectPool;

        public Sprite Sprite
        {
            get { return m_SpriteRenderer.sprite; }
            set { m_SpriteRenderer.sprite = value; }
        }

        private float m_Speed;
        public float Speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        private int m_Damage;
        public int Damage
        {
            get { return m_Damage; }
            set { m_Damage = value; }
        }

        private MovePattern m_MovePattern;
        public MovePattern MovePattern
        {
            get { return m_MovePattern; }
            set { m_MovePattern = value; }
        }

        private float m_Amplitude;
        public float Amplitude
        {
            get { return m_Amplitude; }
            set { m_Amplitude = value; }
        }

        private float m_Frequency;
        public float Frequency
        {
            get { return m_Frequency; }
            set { m_Frequency = value; }
        }

        private float m_Timer;
        private Vector3 m_StartPosition;

        private void Start()
        {
            GlobalGameManager.Instance.GameResetEvent += OnGameReset;
        }

        private void OnDestroy()
        {
            if (GlobalGameManager.Instance != null)
                GlobalGameManager.Instance.GameResetEvent -= OnGameReset;
        }

        private void Update()
        {
            HandleMovement();
            DisableIfOffScreen();
        }

        private void HandleMovement()
        {
            int minMult = 1;
            if (m_MovePattern == MovePattern.MinSinus || m_MovePattern == MovePattern.MinCosinus)
            {
                minMult = -1;
            }

            switch (m_MovePattern)
            {
                case MovePattern.Linear:
                {
                    Vector3 velocity = new Vector3(transform.up.x * m_Speed, transform.up.y * m_Speed, 0.0f);
                    Vector3 deltaMovement = velocity * Time.deltaTime;

                    transform.Translate(deltaMovement, Space.World);
                }
                break;

                case MovePattern.Sinus:
                case MovePattern.MinSinus:
                {
                    float sinValue = m_Amplitude * Mathf.Sin((Mathf.PI * 2) * m_Timer * m_Frequency) * minMult;

                    Vector3 newPosition = new Vector3();
                    newPosition.x = m_StartPosition.x + sinValue;
                    newPosition.y = transform.position.y + (transform.up.y * m_Speed) * Time.deltaTime;

                    transform.position = newPosition;
                }
                break;

                case MovePattern.Cosinus:
                case MovePattern.MinCosinus:
                {
                    float sinValue = m_Amplitude * Mathf.Cos((Mathf.PI * 2) * m_Timer * m_Frequency) * minMult;

                    Vector3 newPosition = new Vector3();
                    newPosition.x = m_StartPosition.x + sinValue;
                    newPosition.y = transform.position.y + (transform.up.y * m_Speed) * Time.deltaTime;

                    transform.position = newPosition;
                }
                break;

                default:
                break;
            }

            m_Timer += Time.deltaTime;
        }

        private void DisableIfOffScreen()
        {
            Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
            if (viewPos.x < 0.0f || viewPos.x > 1.0f || viewPos.y < 0.0f || viewPos.y > 1.0f)
                Deactivate();
        }

        #region PoolableObject

        public override void Initialize()
        {
            //STIJN: FIX, DIRTY
            GameObject go = null; //GameObject.Find("HitEffectPool");
            
            if (go != null)
                m_HitEffectPool = go.GetComponent<Pool>();
        }

        public override void Activate(Vector3 pos, Quaternion rot)
        {
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;

            m_StartPosition = pos;
            gameObject.SetActive(true);
        }

        public override void Deactivate()
        {
            gameObject.SetActive(false);
            m_Timer = 0.0f;
        }

        public override bool IsAvailable()
        {
            return (!gameObject.activeSelf);
        }

        #endregion

        #region IDamageDealer

        public int GetDamage()
        {
            return m_Damage;
        }

        public void HadContact(GameObject go)
        {
            //Find the contact point
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up * -1.0f);
            if (m_HitEffectPool != null)
            {
                PoolableObject effect = m_HitEffectPool.ActivateAvailableObject(transform.position, transform.rotation * Quaternion.Euler(0.0f, 0.0f, 180.0f));
                effect.transform.parent = go.transform;
            }

            Deactivate();
        }

        #endregion

        private void OnGameReset()
        {
            Deactivate();
        }
    }
}