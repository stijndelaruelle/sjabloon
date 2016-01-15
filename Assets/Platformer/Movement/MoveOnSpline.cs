using UnityEngine;

namespace Sjabloon
{
    public class MoveOnSpline : MoveableObject
    {
        [SerializeField]
        private Spline m_Spline;

        [SerializeField]
        private float m_Speed;
        public float Speed
        {
            get { return m_Speed; }
            set
            {
                m_Speed = value;
                CalculateDuration();
            }
        }

        [SerializeField]
        private bool m_UseBackAndForth;
        private int m_MoveDirection = 1;

        private float m_Duration;
        private float m_Progress;

        private void Start()
        {
            if (m_Spline == null)
            {
                Debug.LogWarning("SplineWalker doesn't have a spline!");
            }

            CalculateDuration();
            GlobalGameManager.Instance.GameResetEvent += OnGameReset;
        }

        private void OnDestroy()
        {
            if (GlobalGameManager.Instance != null)
                GlobalGameManager.Instance.GameResetEvent -= OnGameReset;
        }

        public override void Move()
        {
            m_Progress += (Time.deltaTime / m_Duration) * m_MoveDirection;

            if (m_Progress < 0.0f)
            {
                if (m_UseBackAndForth)
                {
                    m_MoveDirection *= -1;
                }
            }

            if (m_Progress > 1.0f)
            {
                //TODO: FIX DIRTYNESS
                if (m_Spline.GetLoop() == true)
                {
                    m_Progress = 0.0f;
                }
                else if (m_UseBackAndForth)
                {
                    m_MoveDirection *= -1;
                }
                else
                {
                    m_Progress = 1f;
                }
            }

            transform.position = m_Spline.GetPoint(m_Progress);
        }

        private void CalculateDuration()
        {
            m_Duration = (1.0f / m_Speed) * m_Spline.GetTotalLength();
        }

        private void OnGameReset()
        {
            m_Progress = 0.0f;
        }
    }
}