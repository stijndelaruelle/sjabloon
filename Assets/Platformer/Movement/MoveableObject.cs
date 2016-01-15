using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    //Abstract class instead of interface as we have to 100% certain it's a monobehaviour.
    public abstract class MoveableObject : MonoBehaviour
    {
        public abstract void Move();
    }
}