using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderPulse.Characters
{
    public enum ActionContext : byte
    {
        TakeItem = 0,
        HoldComrade = 1,
        CaptureEnemy = 2,
        UseDevice = 3,
    }
}
