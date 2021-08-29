using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderPulse;
using UnityEngine;

namespace ThunderPulse.Characters.Customization
{
    public abstract class Bodypart
    {
        public BodyPartID id;
        public Transform[] bones;
        public SkinnedMeshRenderer SMR;
    }
}
