using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ThunderPulse.Characters;
using ThunderPulse.Common;
using ThunderPulse.Items;
using ThunderPulse.Skills;
using ThunderPulse.Interfaces;

namespace ThunderPulse.Characters
{
    /// <summary>
    /// Назначаем профессию.
    /// Забираем профессию.
    /// Принадлежность к особой группе.
    /// Род занятий.
    /// </summary>
    public static class ProfessionSystem
    {
        /// <summary>
        /// Назначаем профессию.
        /// </summary>
        public static void AddProfession(Character character, KeyWord profession)
        {
            character.SetKeyWord(profession); 
        }

        /// <summary>
        /// Забираем профессию.
        /// </summary>
        public static void RemoveProfession(Character character, KeyWord profession)
        {
            character.RemoveKeyWord(profession); 
        }
    }
}
