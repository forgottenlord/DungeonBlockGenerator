using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonAssault.Dungeons
{
    public enum DungeonBlockType : byte
    {
        /// <summary>
        /// Тупик - Stop
        /// </summary>
        S = 1,
        /// <summary>
        /// Поворот - L.
        /// </summary>
        L = 2,
        /// <summary>
        /// Коридор - I.
        /// </summary>
        I = 3,
        /// <summary>
        /// Развилка - T.
        /// </summary>
        T = 4,
        /// <summary>
        /// Перекрёсток - X.
        /// </summary>
        X = 5,
    }
}
