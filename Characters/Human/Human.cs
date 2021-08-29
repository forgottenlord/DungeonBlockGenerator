using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ThunderPulse.Characters.Combatant;
using ThunderPulse.Items;
using ThunderPulse.Interfaces;
using ThunderPulse.Common;

namespace ThunderPulse.Characters
{
    [KeyWords(KeyWord.isHuman)]
	public class Human : Character, ICombatant
    {
        CapsuleCollider MainCollider;
        Rigidbody RgB;
        public float MaxSpeed = 12f;
        float Speed = 3f;
        List<Item> leftHand; int currentLeftItem;
        List<Item> rightHand; int currentRightItem;
        public Transform skeletonMain;
        public static float MinimalSpeed = 0.01f;
		public static float MaximumSpeed = 5f;
        public List<DitzelGames.FastIK.FastIKFabric> fabrics = new List<DitzelGames.FastIK.FastIKFabric>();

        public override void Init()
        {
            skeletonMain = transform.GetChild(0);
            keyWords.Add(KeyWord.Item);
            keyWords.Add(KeyWord.Item_Dress);
            MainCollider = Common.GeneratedObject.SetComponent<CapsuleCollider>(gameObject);
            RgB = gameObject.AddComponent<Rigidbody>();
            RgB.constraints = RigidbodyConstraints.FreezeRotation;// | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

            Height = 2.7f;
            //CameraGO.AddComponent<CameraReplacementShader>();
            
        }

        public override void OnControll(List<KeyWord> controlls, Ray ray)
        {
            base.OnControll(controlls, ray);
            if (controlls.Contains(KeyWord.Controlls_MoveFront))
            {
                Vector3 speed = skeletonMain.up * (Speed) * Time.deltaTime;
                if (controlls.Contains(KeyWord.Controlls_Sprint))
                {
                    speed *= 3;
                }

                transform.position += speed;
            }
            if (controlls.Contains(KeyWord.Controlls_MoveBack))
                transform.position -= skeletonMain.up * (Speed) * Time.deltaTime;
            if (controlls.Contains(KeyWord.Controlls_MoveRight))
                transform.position -= skeletonMain.right * (Speed) * Time.deltaTime;
            if (controlls.Contains(KeyWord.Controlls_MoveLeft))
                transform.position += skeletonMain.right * (Speed) * Time.deltaTime;
            if (controlls.Contains(KeyWord.Controlls_Jump))
                RgB.AddForce(transform.up, ForceMode.VelocityChange);
            if (controlls.Contains(KeyWord.Controlls_LeftHandAction))
                leftHand[currentLeftItem].Use(this);
            if (controlls.Contains(KeyWord.Controlls_RightHandAction))
                rightHand[currentRightItem].Use(this);
            ///Нажали другую клавишу, персонаж взмахнул правым оружием.
            if (controlls.Contains(KeyWord.Controlls_RightHandChange))
            {
                currentRightItem++;
                if (currentRightItem >= rightHand.Count) currentRightItem = 0;
            }
            ///Нажали одну клавишу, персонаж взмахнул левым оружием.
            if (controlls.Contains(KeyWord.Controlls_LeftHandChange))
            {
                currentLeftItem++;
                if (currentLeftItem >= leftHand.Count) currentLeftItem = 0;
            }

            if (controlls.Contains(KeyWord.Controlls_ContextAction))
            {
                this.PutObjectInTransform(KeyWord.Character_Bones_LeftFist, GameObject.Find("ShotGun").AddComponent<ThunderPulse.Items.Weapons.Guns.OneHandedGun>().transform,
                    new Vector3(), new Vector3());
            }
            if (controlls.Contains(KeyWord.Order_Suicide))
            {
                Destroy();
            }
            if (controlls.Contains(KeyWord.Order_PoseStandUpper))
            {
                if (skeletonMain.localPosition.y < -0.1f)
                    skeletonMain.localPosition += new Vector3(0, .05f, 0);
            }
            if (controlls.Contains(KeyWord.Order_PoseStandLower))
            {
                if (skeletonMain.localPosition.y > -0.7933118f)
                    skeletonMain.localPosition += new Vector3(0, -.05f, 0);
            }
        }
        /// <summary>
        /// рост персонажа
        /// </summary>
        public override float Height
        {
            get
            {
                if (MainCollider != null)
                    return MainCollider.height;
                else
                    return 2.0f;
            }
            set { MainCollider.height = value; MainCollider.radius = 0.2f * value; }
        }
        public override void Idle()
        {

        }
		public void OnSpawn()
        {
        }
        public override void Moving(float frontSpeed, float sideSpeed)
        {

        }
        public override void Targeting(float HSpeed, float VSpeed)
        {
            skeletonMain.localEulerAngles += new Vector3(0, HSpeed, 0);
            //transform.localEulerAngles += new Vector3(-VSpeed, 0, 0);
        }
        bool isGrounded;
        public bool IsGrounded
        {
            get { return isGrounded; }
            set { isGrounded = value; }
        }
		public void Laying(float Impulse) { }
		private int health;
		private int maxHealth;
		public override int Health
		{
			get { return health; }
			set
			{
				health += value;
				if (health < 0)
				{
					OnHealthIs0();
					return;
				}
				if (health > maxHealth) health = maxHealth;
			}
		}
		public override void OnHealthIs0()
		{
		}
		private int stamina;
		private int maxStamina;
		public override int Stamina
		{
			get { return stamina; }
			set {
				stamina += value;
				if (health < 0)
				{
					OnStaminaIs0();
					return;
				}
				if (stamina > maxStamina) stamina = maxStamina;
			}
		}
		public override void OnStaminaIs0()
		{
        }
    }
}
