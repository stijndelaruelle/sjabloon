using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Sjabloon
{
    public class GlobalGameManager : Singleton<GlobalGameManager>
    {
        //[SerializeField]
        //private List<PlayerController> m_Players;
        private int m_DeathPlayers;

        private event Action m_GameStartEvent;
        public Action GameStartEvent
        {
            get { return m_GameStartEvent; }
            set { m_GameStartEvent = value; }
        }

        private event Action m_GameResetEvent;
        public Action GameResetEvent
        {
            get { return m_GameResetEvent; }
            set { m_GameResetEvent = value; }
        }

        private event Action m_GameOverEvent;
        public Action GameOverEvent
        {
            get { return m_GameOverEvent; }
            set { m_GameOverEvent = value; }
        }

        private event Action m_GameCompleteEvent;
        public Action GameCompleteEvent
        {
            get { return m_GameCompleteEvent; }
            set { m_GameCompleteEvent = value; }
        }

        private void Start()
        {
            //foreach(PlayerController player in m_Players)
            //{
            //    player.DeathEvent += OnPlayerDeath;
            //}

            //Time.timeScale = 0.0f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //foreach (PlayerController player in m_Players)
            //{
            //    player.DeathEvent -= OnPlayerDeath;
            //}
            //m_Players.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && Time.timeScale == 1.0f)
            {
                RestartGame();
            }
        }

        private void OnPlayerDeath()
        {
            m_DeathPlayers += 1;

            //if (m_DeathPlayers >= m_Players.Count)
            //{
            //    StartCoroutine(DelayRoutine(1.0f, GameOver));
            //    GlobalEffects.Instance.Screenshake.StartShake(1.0f, 1.0f);
            //}
        }

        private void OnBossDeath()
        {
            StartCoroutine(DelayRoutine(1.5f, GameComplete));
        }

        private void StartGame()
        {
            if (m_GameStartEvent != null)
                m_GameStartEvent();

            StopAllCoroutines();
            Time.timeScale = 1.0f;
        }

        private void GameOver()
        {
            if (m_GameOverEvent != null)
                m_GameOverEvent();

            StartCoroutine(GameDownSlowMotionRoutine(0.5f, 1.0f));
        }

        private void GameComplete()
        {
            if (m_GameCompleteEvent != null)
                m_GameCompleteEvent();

            StartCoroutine(GameDownSlowMotionRoutine(0.5f, 0.5f));
        }

        //FIX ME: Should not be in the game manager
        private IEnumerator DelayRoutine(float delay, Action function)
        {
            yield return new WaitForSeconds(delay);
            function();
        }

        private IEnumerator GameDownSlowMotionRoutine(float startValue, float speed)
        {
            float timeScale = startValue;

            while (timeScale > 0.0f)
            {
                timeScale -= speed * Time.deltaTime;
                if (timeScale < 0.0f)
                    timeScale = 0.0f;

                Time.timeScale = timeScale;
                yield return new WaitForEndOfFrame();
            }

            Time.timeScale = 0.0f;
        }

        public void RestartGame()
        {
            //Reset everything
            if (m_GameResetEvent != null)
                m_GameResetEvent();

            StartGame();
        }
    }
}