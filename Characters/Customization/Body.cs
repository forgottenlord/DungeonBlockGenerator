using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ThunderPulse.Characters.Customization
{
    /// <summary>
    /// Настраиваемое тело с произвольной конфигурацией, и произвольным количеством конечностей.
    /// </summary>
    public class CustomizableBody
    {
        /// <summary>
        /// Запчасти.
        /// </summary>
        public SkinnedMeshRenderer SMR;
        /// <summary>
        /// Запчасти.
        /// </summary>
        public List<Bodypart> bodyparts = new List<Bodypart>();
        /// <summary>
        /// Присоединить конечность.
        /// </summary>
        public void Attach()
        {

        }
        /// <summary>
        /// Отсоединить конечность.
        /// </summary>
        public void Detach(int num)
        {
            bodyparts.RemoveAt(num);
        }
        /// <summary>
        /// Отсоединить конечность.
        /// </summary>
        public void Detach(Bodypart part)
        {
            bodyparts.Remove(part);
        }
    }
}
