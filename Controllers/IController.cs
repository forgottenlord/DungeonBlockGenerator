using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThunderPulse.Controllers
{
	public interface IController
	{
		/// <summary>
		/// Если персонаж двигается
		/// </summary>
		/// <param name="Speed">вперед(Speed>0)/назад(Speed<0)</param>
		/*void Moving(float Speed);
		/// <summary>
		/// Если персонаж двигается
		/// </summary>
		/// <param name="Speed">вправо(Speed>0)/влево(Speed<0)</param>
		void Strafing(float Speed);
		/// <summary>
		/// Если персонаж наводится по горизонтали
		/// </summary>
		/// <param name="Speed">вправо(Speed>0)/влево(Speed<0)</param>*/
		void Targeting(float HSpeed, float VSpeed);
		/// <summary>
		/// Если персонаж наводится по горизонтали
		/// </summary>
		/// <param name="Speed">вправо(Speed>0)/влево(Speed<0)</param>
		/*void VTargeting(float Speed);
		/// <summary>
		/// Если персонаж перемещается между слоями четырехмерного пространства
		/// </summary>
		/// <param name="Speed">пространственная координата V</param>*/
		/*void Temporating(float Speed);
		/// <summary>
		/// Если персонаж садится или ложится
		/// </summary>
		/// <param name="Speed">вправо(Speed>0)/влево(Speed<0)</param>*/
		/*void Laying(float Speed);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Speed"></param>*/
		void Action();
	}
}
