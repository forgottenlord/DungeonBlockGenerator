using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThunderPulse.Characters.Combatant
{
	interface ICombatantWizard
	{
		/// <summary>
		/// Мана (синяя полоса)
		/// </summary>
		float Mana { get; set; }
		/// <summary>
		/// Если закончилась мана
		/// </summary>
		void OnManaIs0();
	}
}
