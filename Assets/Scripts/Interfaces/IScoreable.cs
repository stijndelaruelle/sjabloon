using UnityEngine;
using System.Collections;
using System;

namespace Sjabloon
{
    public interface IScoreable
    {
        Action<int> ScoreEvent { get; set; }
    }
}