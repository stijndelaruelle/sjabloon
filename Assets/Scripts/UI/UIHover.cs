using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHover : MonoBehaviour
    {
        [SerializeField]
        private float m_Amplitude = 1.0f;

        [SerializeField]
        private float m_Frequency = 1.0f;

        private RectTransform m_RectTransform;
        private float m_Timer = 0.0f;
        private float m_DefaultY = 0.0f;

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_DefaultY = m_RectTransform.anchoredPosition.y;
        }

        private void Update()
        {
            float sinValue = Mathf.Sin(m_Timer * m_Frequency) * m_Amplitude;
            Vector3 anchoredPos = m_RectTransform.anchoredPosition;
            anchoredPos.y = m_DefaultY + sinValue;

            m_RectTransform.anchoredPosition = anchoredPos;

            m_Timer += Time.deltaTime;
        }
    }
}
