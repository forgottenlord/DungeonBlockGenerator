using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonAssault
{
    [System.Flags]
    public enum DungeonBlockNeighbors : byte
    {
        Left = 1,
        Back = 2,
        Right = 4,
        Front = 8,
    }
}
