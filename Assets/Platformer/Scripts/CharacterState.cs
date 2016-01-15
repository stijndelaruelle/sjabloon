using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public interface CharacterState
    {
        void OnEnter();
        void OnExit();
        Vector2 Update(Vector2 velocity);
    }
}