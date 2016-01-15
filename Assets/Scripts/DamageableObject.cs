using UnityEngine;
using System.Collections;
using System;

namespace Sjabloon
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DamageableObject : MonoBehaviour, IDamageable, IScoreable
    {
        //Datamembers
        [SerializeField]
        private int m_MaxHealth;
        public int MaxHealth
        {
            get { return m_MaxHealth; }
        }

        private int m_Health;
        public int Health
        {
            get { return m_Health; }
        }

        [SerializeField]
        private int m_Score;
        public int Score
        {
            get { return m_Score; }
        }

        [SerializeField]
        private string m_ExcludedTag = "";

        //Events
        private event Action m_HealEvent;
        public Action HealEvent
        {
            get { return m_HealEvent; }
            set { m_HealEvent = value; }
        }

        private event Action m_DamageEvent;
        public Action DamageEvent
        {
            get { return m_DamageEvent; }
            set { m_DamageEvent = value; }
        }

        private event Action m_DeathEvent;
        public Action DeathEvent
        {
            get { return m_DeathEvent; }
            set { m_DeathEvent = value; }
        }

        private event Action<int> m_ScoreEvent;
        public Action<int> ScoreEvent
        {
            get { return m_ScoreEvent; }
            set { m_ScoreEvent = value; }
        }

        //Functions
        private void Awake()
        {
            m_Health = m_MaxHealth;
        }

	    private void Start()
        {
            GlobalGameManager.Instance.GameResetEvent += OnGameReset;
	    }

        private void OnDestroy()
        {
            if (GlobalGameManager.Instance != null)
                GlobalGameManager.Instance.GameResetEvent -= OnGameReset;
        }

        private void OnGameReset()
        {
            m_Health = m_MaxHealth;

            if (m_HealEvent != null)
                m_HealEvent();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == m_ExcludedTag)
                return;

            //Check if the object that we hit was a damagedealer
            IDamageDealer damageDealer = other.gameObject.GetComponent<IDamageDealer>();

            if (damageDealer != null)
            {
                int damage = damageDealer.GetDamage();
                Damage(damage);
                damageDealer.HadContact(gameObject);
            }
        }

        #region IDamageable

        public void Heal(int health)
        {
            if (health <= 0 || m_Health >= m_MaxHealth)
                return;

            m_Health += health;

            if (m_Health > m_MaxHealth)
                m_Health = m_MaxHealth;

            if (m_HealEvent != null)
                m_HealEvent();
        }

        public void Damage(int damage)
        {
            if (damage <= 0 || m_Health <= 0 || m_MaxHealth == 0)
                return;

            m_Health -= damage;

            if (m_DamageEvent != null)
                m_DamageEvent();

            if (m_Health <= 0)
            {
                m_Health = 0;
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            if (m_DeathEvent != null)
                m_DeathEvent();

            if (m_ScoreEvent != null && m_Score > 0)
                m_ScoreEvent(m_Score);
        }

        public bool IsAlive()
        {
            return (m_Health > 0);
        }

        #endregion
    }
}