using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_MainMenuPanel;

        [SerializeField]
        private GameObject m_GameCompletePanel;

        [SerializeField]
        private GameObject m_GameOverPanel;

        [SerializeField]
        private GameObject m_HUDPanel;

        private GlobalGameManager m_GlobalGameManager;

        private void Start()
        {
            m_GlobalGameManager = GlobalGameManager.Instance;

            if (m_GlobalGameManager == null)
            {
                Debug.LogError("UIManager doesn't have a gamemanager reference!");
                return;
            }

            m_GlobalGameManager.GameStartEvent += OnGameStart;
            m_GlobalGameManager.GameCompleteEvent += OnGameComplete;
            m_GlobalGameManager.GameOverEvent += OnGameOver;

            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (m_GlobalGameManager == null)
                return;

            m_GlobalGameManager.GameStartEvent -= OnGameStart;
            m_GlobalGameManager.GameCompleteEvent -= OnGameComplete;
            m_GlobalGameManager.GameOverEvent -= OnGameOver;
        }

        public void ShowMainMenu()
        {
            m_MainMenuPanel.SetActive(true);
            m_GameCompletePanel.SetActive(false);
            m_GameOverPanel.SetActive(false);
            m_HUDPanel.SetActive(false);
        }

        private void OnGameStart()
        {
            m_MainMenuPanel.SetActive(false);
            m_GameCompletePanel.SetActive(false);
            m_GameOverPanel.SetActive(false);
            m_HUDPanel.SetActive(true);
        }

        private void OnGameComplete()
        {
            m_MainMenuPanel.SetActive(false);
            m_GameCompletePanel.SetActive(true);
            m_GameOverPanel.SetActive(false);
            m_HUDPanel.SetActive(false);
        }

        private void OnGameOver()
        {
            m_MainMenuPanel.SetActive(false);
            m_GameCompletePanel.SetActive(false);
            m_GameOverPanel.SetActive(true);
            m_HUDPanel.SetActive(false);
        }

        public void RestartGame()
        {
            m_GlobalGameManager.RestartGame();
        }
    }
}