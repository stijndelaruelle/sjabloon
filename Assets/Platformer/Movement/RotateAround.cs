using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class RotateAround : MoveableObject
    {
        [SerializeField]
        private float m_RotationSpeed;
        public float RotationSpeed
        {
            get { return m_RotationSpeed; }
            set { m_RotationSpeed = value; }
        }

        private float m_DefaultRotationSpeed;

        private void Start()
        {
            m_DefaultRotationSpeed = m_RotationSpeed;
            GlobalGameManager.Instance.GameResetEvent += OnGameReset;
        }

        private void OnDestroy()
        {
            if (GlobalGameManager.Instance != null)
                GlobalGameManager.Instance.GameResetEvent -= OnGameReset;
        }


        public override void Move()
        {
            transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), m_RotationSpeed * Time.deltaTime);
        }

        private void OnGameReset()
        {
            m_RotationSpeed = m_DefaultRotationSpeed;
        }
    }
}