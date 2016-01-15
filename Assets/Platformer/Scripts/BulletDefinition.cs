using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class BulletDefinition : ScriptableObject
    {
        [SerializeField]
        private Sprite m_Sprite;
        public Sprite Sprite
        {
            get { return m_Sprite; }
        }

        [SerializeField]
        private float m_Speed;
        public float Speed
        {
            get { return m_Speed; }
        }

        [SerializeField]
        private int m_Damage;
        public int Damage
        {
            get { return m_Damage; }
        }
    }
}