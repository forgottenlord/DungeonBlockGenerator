using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderPulse.AI.Orders;

namespace ThunderPulse.Characters
{
    /// <summary>
    /// Отряд/подразделение.
    /// </summary>
    public class Squad
    {
        List<Character> characters = new List<Character>();
        List<Order> orders = new List<Order>();
        public void Add(Character character)
        {
            characters.Add(character);
        }
        public void Remove(Character character)
        {
            characters.Remove(character);
        }
        public Character GetCharacter(int n)
        {
            return characters[n];
        }
        public int Count()
        {
            return characters.Count;
        }
        public void AddOrder(Order order)
        {
            orders.Add(order);
        }
    }
}
