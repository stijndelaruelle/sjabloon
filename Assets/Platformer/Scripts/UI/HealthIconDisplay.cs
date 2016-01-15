using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Sjabloon
{
    public class HealthIconDisplay : MonoBehaviour
    {
        [SerializeField]
        private DamageableObject m_DamageableObject;

        [SerializeField]
        private Image m_HealthIcon;
        private List<Image> m_HealthIcons;

        private void Start()
        {
            if (m_DamageableObject == null)
            {
                Debug.LogError("HealthIconDisplay doesn't have a DamageableObject reference!");
                return;
            }

            m_DamageableObject.HealEvent += OnHeal;
            m_DamageableObject.DamageEvent += OnDamage;

            m_HealthIcons = new List<Image>();
            for (int i = 0; i < m_DamageableObject.MaxHealth; ++i)
            {
                Image go = GameObject.Instantiate(m_HealthIcon);
                go.gameObject.transform.SetParent(this.transform);

                m_HealthIcons.Add(go);
            }
        }

        private void OnDestroy()
        {
            if (m_DamageableObject != null)
            {
                m_DamageableObject.HealEvent -= OnHeal;
                m_DamageableObject.DamageEvent -= OnDamage;
            }

            for (int i = 0; i < m_HealthIcons.Count; ++i)
            {
                GameObject.Destroy(m_HealthIcons[i]);
            }

            m_HealthIcons.Clear();
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

            for (int i = 0; i < currentHealth; ++i)
            {
                m_HealthIcons[i].gameObject.SetActive(true);
            }

            for (int i = currentHealth; i < m_HealthIcons.Count; ++i)
            {
                m_HealthIcons[i].gameObject.SetActive(false);
            }
        }
    }
}