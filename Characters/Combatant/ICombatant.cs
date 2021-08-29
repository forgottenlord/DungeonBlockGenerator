using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace ThunderPulse.Characters.Combatant
{
	interface ICombatant
	{
		/// <summary>
		/// Здоровье (красная полоса)
		/// </summary>
		int Health { get; set; }
        /// <summary>
        /// Выносливость (желтая полоса)
        /// </summary>
        int Stamina { get; set; }
        /// <summary>
        /// Выносливость (желтая полоса)
        /// </summary>
        int Mana { get; set; }
        /// <summary>
        /// Если закончилась жизнь
        /// </summary>
        void OnHealthIs0();
		/// <summary>
		/// Если закончилась выносливость
		/// </summary>
		void OnStaminaIs0();
	}
}
