using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Sjabloon
{
    public class GlobalEffects : Singleton<GlobalEffects>
    {
        [SerializeField]
        private Screenshake m_Screenshake;
        public Screenshake Screenshake
        {
            get { return m_Screenshake; }
        }
    }
}