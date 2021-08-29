using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Characters.Animations
{
    public class Pose
    {
        public string name;
        public int frameNumber;
        public List<Quaternion> boneRotations = new List<Quaternion>();
    }
}
