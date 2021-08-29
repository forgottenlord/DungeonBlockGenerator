using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonAssault.Controllers
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Character : MonoBehaviour
    {
        public float movingSpeed = 1f;
        public Action<Transform> OnTransFormChanged;
        private Animator animator;
        private CapsuleCollider capsule;
        private Rigidbody rigidbody;
        public void Start()
        {
            animator = GetComponent<Animator>();
            capsule = GetComponent<CapsuleCollider>();
            rigidbody = GetComponent<Rigidbody>();
        }
        public void Acting()
        {

        }
        public void Moving(Vector2 movingVector)
        {
            Vector3 acc = transform.forward * (movingSpeed) * Time.deltaTime * movingVector.x;
            acc += transform.right * (movingSpeed) * Time.deltaTime * movingVector.y;
            transform.position += acc;
            animator.SetFloat("Speed", movingVector.magnitude);


            OnTransFormChanged?.Invoke(transform);
        }
        public void Jumping(bool isJumping)
        {
            transform.position += transform.up * (movingSpeed);
            animator.SetBool("isJumping", isJumping);
            OnTransFormChanged?.Invoke(transform);
        }
        public void Targeting(float HSpeed, float VSpeed)
        {
            transform.localEulerAngles += new Vector3(0, HSpeed, 0);
            //transform.localEulerAngles += new Vector3(-VSpeed, 0, 0);
            OnTransFormChanged?.Invoke(transform);
        }
    }
}
