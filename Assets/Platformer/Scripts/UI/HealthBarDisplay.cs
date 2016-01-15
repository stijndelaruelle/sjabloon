using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Sjabloon
{
    public class HealthBarDisplay : MonoBehaviour
    {
        [SerializeField]
        private DamageableObject m_DamageableObject;

        [SerializeField]
        private RectTransform m_HealthBar;
        private Vector2 m_OriginalSize;

        private void Start()
        {
            m_OriginalSize = m_HealthBar.sizeDelta.Copy();

            m_DamageableObject.HealEvent += OnHeal;
            m_DamageableObject.DamageEvent += OnDamage;
            m_DamageableObject.DeathEvent += OnDeath;
        }

        private void OnDestroy()
        {
            m_DamageableObject.HealEvent -= OnHeal;
            m_DamageableObject.DamageEvent -= OnDamage;
            m_DamageableObject.DeathEvent -= OnDeath;
        }

        private void OnHeal()
        {
            UpdateHealth();
        }

        private void OnDamage()
        {
            UpdateHealth();
        }

        private void UpdateHealth()
        {
            int currentHealth = m_DamageableObject.Health;
            int maxHealth = m_DamageableObject.MaxHealth;

            float percent = (float)currentHealth / (float)maxHealth;

            m_HealthBar.sizeDelta = new Vector2(m_OriginalSize.x * percent, m_OriginalSize.y);
        }

        private void OnDeath()
        {
            //m_HealthBar.gameObject.SetActive(false);
        }
    }
}