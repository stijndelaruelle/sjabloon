using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class ResetOnGameStart : MonoBehaviour
    {
        private Vector3 m_LocalPosition;
        private Quaternion m_LocalRotation;
        private Transform m_Parent;
        private bool m_IsActive;

        private void Start()
        {
            //Get the base state
            m_LocalPosition = transform.localPosition.Copy();
            m_LocalRotation = transform.localRotation.Copy();
            m_Parent = transform.parent;
            m_IsActive = gameObject.activeSelf;

            GlobalGameManager.Instance.GameResetEvent += OnGameReset;
        }

        private void OnDestroy()
        {
            if (GlobalGameManager.Instance != null)
                GlobalGameManager.Instance.GameResetEvent -= OnGameReset;
        }

        private void OnGameReset()
        {
            transform.localPosition = m_LocalPosition.Copy();
            transform.localRotation = m_LocalRotation.Copy();
            transform.SetParent(m_Parent);

            gameObject.SetActive(m_IsActive);
        }
    }
}