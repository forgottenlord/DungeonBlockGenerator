using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ThunderPulse.Controllers;
using ThunderPulse.Common;
using ThunderPulse.Items;
using ThunderPulse.Items.Weapons;
using ThunderPulse.Items.Weapons.ColdSteels;
using ThunderPulse.Items.Weapons.Guns;
using ThunderPulse.Skills;
using ThunderPulse.Interfaces;

namespace ThunderPulse.Characters
{
    public abstract class Character : ControlledObject, IIteratable
    {
        /// <summary>
        /// Список костей персонажа.
        /// </summary>
        public Dictionary<KeyWord, Transform> Bones = new Dictionary<KeyWord, Transform>()
        {
            { KeyWord.Character_Bones_Neck, null }, { KeyWord.Character_Bones_Head, null }, { KeyWord.Character_Bones_LowerJaw, null }, { KeyWord.Character_Bones_LeftEye, null }, { KeyWord.Character_Bones_RightEye, null },

            { KeyWord.Character_Bones_LeftBreast, null }, { KeyWord.Character_Bones_LeftShoulder, null }, { KeyWord.Character_Bones_LeftArm, null }, { KeyWord.Character_Bones_LeftHand, null }, { KeyWord.Character_Bones_LeftFist, null },
            { KeyWord.Character_Bones_RightBreast, null }, { KeyWord.Character_Bones_RightShoulder, null }, { KeyWord.Character_Bones_RightArm, null }, { KeyWord.Character_Bones_RightHand, null }, { KeyWord.Character_Bones_RightFist, null },

            { KeyWord.Character_Bones_LowerSpine, null }, { KeyWord.Character_Bones_MiddleSpine, null }, { KeyWord.Character_Bones_UpperSpine, null },

            { KeyWord.Character_Bones_LeftLeg, null }, { KeyWord.Character_Bones_LeftElbow, null }, { KeyWord.Character_Bones_LeftStep, null },
            { KeyWord.Character_Bones_RightLeg, null }, { KeyWord.Character_Bones_RightElbow, null }, { KeyWord.Character_Bones_RightStep, null },
        };
        public Weapon LeftHand;
        public Weapon RightHand;
        private bool isArmed = false;
        public bool IsArmed
        {
            get { return isArmed; }
            set { isArmed = value; }
        }
        public void Start()
        {
            List<Transform> bones = new List<Transform>();
            GetBones(transform, bones);
            Init();
        }
        private List<Transform> GetBones(Transform parent, List<Transform> children)
        {
            for (int n = 0; n < parent.childCount; n++)
            {
                children.Add(parent.GetChild(n));
                GetBones(parent.GetChild(n), children);
            }
            return children;
        }

        public abstract void Init();
        /// <summary>
        /// Привязываем объект к одной из костей объекта.
        /// </summary>
        public virtual void PutObjectInTransform(KeyWord place, Transform objectTransform, Vector3 shift, Vector3 angle)
        {
            if (Bones.ContainsKey(place) && Bones[place] != null)
            {
                objectTransform.SetParent(Bones[place]);
                objectTransform.localPosition = shift;
                objectTransform.localEulerAngles = angle;
            }
        }
        public virtual float Height { get { return 1; } set { } }
        byte level = 0;
        public byte Level
        {
            get { return level; }
        }
        uint experience = 0;
        public uint Experience
        {
            get { return experience; }
            set
            {
                experience = value;
                if (experience > level * level) level++;
            }
            
        }
        void OnSpawn()
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlls"></param>
        /// <param name="ray"></param>
        public override void OnControll(List<KeyWord> controlls, Ray ray)
        {
            /*if (LeftHand.GetKeyWords().Contains(KeyWord.Item_Weapon_TwoHanded))
            {
                if (controlls.Contains(KeyWord.Controlls_RightHandAction))
                {
                    IAlternateUsefull twoHanded = (IAlternateUsefull)LeftHand;
                    if (twoHanded != null)
                    {
                        twoHanded.AlterUse(this);
                    }
                }
            }
            if (controlls.Contains(KeyWord.Controlls_LeftHandAction))
                LeftHand.Use(this);
            if (controlls.Contains(KeyWord.Controlls_RightHandAction))
                RightHand.Use(this);*/
        }

        public void Iterate()
        {

        }
        /// <summary>
        /// Наведение персонажа 
        /// </summary>
        /// <param name="frontSpeed">скорость горизонтального наведения</param>
        /// <param name="sideSpeed">скорость вертикального наведения</param>
        public virtual void Moving(float frontSpeed, float sideSpeed){ }
        
        /// <summary>
        /// Если персонаж перемещается между слоями четырехмерного пространства
        /// </summary>
        /// <param name="Speed">пространственная координата V</param>
		public void Temporating(float Speed)
        { }
        bool IsGrounded { get; set; }
        public virtual void Idle() { }
        public virtual int Health { get; set; }
        public virtual void OnHealthIs0() { }
        public virtual int Mana { get; set; }
        public virtual void OnManaIs0() { }
        public virtual int Stamina { get; set; }
        public virtual void OnStaminaIs0() { }
        public Inventory inventory;
        public Arsenal arsenal;
    }
}
