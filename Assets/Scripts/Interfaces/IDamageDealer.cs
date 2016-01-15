using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public interface IDamageDealer
    {
        int GetDamage();
        void HadContact(GameObject go);
    }
}