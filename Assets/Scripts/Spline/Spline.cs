using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    //Abstract class instead of interface as we have to 100% certain it's a monobehaviour.
    public abstract class Spline : MonoBehaviour
    {
        public abstract Vector3 GetPoint(float f);
        public abstract bool GetLoop();
        public abstract float GetTotalLength();
    }
}