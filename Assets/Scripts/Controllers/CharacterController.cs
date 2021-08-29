using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonAssault.Controllers
{
    public class CharacterController : MonoBehaviour
    {
        GameObject cameraGO;
        Transform cameraContainer;
        private Camera cam;
        public float Sencitivity = 3;
        public List<Character> characters = new List<Character>();
        private Character currentCharacter;
        private int currentCharacterNum;
        public Vector3 cameraShift;
        public void Awake()
        {
            cameraContainer = new GameObject("CameraContainer").transform;
            cameraContainer.parent = transform;
            cameraContainer.localPosition = new Vector3();
            cameraGO = new GameObject("CameraGO");
            cameraGO.transform.parent = cameraContainer;
            cameraContainer.parent = transform;
            cameraContainer.transform.localPosition = cameraShift;
            cam = cameraGO.AddComponent<Camera>();
            if (currentCharacter == null)
            {
                currentCharacter = characters[0];
                currentCharacter.OnTransFormChanged += MoveFollowPlayer;
            }
        }
        public void MoveFollowPlayer(Transform characterTransform)
        {
            transform.position = characterTransform.position + new Vector3(0, cameraShift.y, 0);
        }

        public void Update()
        {
            Targeting(Input.GetAxis("Mouse X") * Sencitivity,
                      Input.GetAxis("Mouse Y") * Sencitivity);
            Vector2 movingVector = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) movingVector += new Vector2(1, 0);
            if (Input.GetKey(KeyCode.S)) movingVector += new Vector2(-1, 0);
            if (Input.GetKey(KeyCode.A)) movingVector += new Vector2(0, -1);
            if (Input.GetKey(KeyCode.D)) movingVector += new Vector2(0, 1);
            if (Input.GetKey(KeyCode.LeftShift)) movingVector *= 3;
            currentCharacter.Moving(movingVector);

            if (Input.GetKey(KeyCode.E)) { currentCharacter.Acting(); }
            if (Input.GetKey(KeyCode.Q)) { SwitchCharacter(); }

            if (Input.GetKey(KeyCode.Space)) currentCharacter.Jumping(true);


            if (Input.GetAxis("Mouse ScrollWheel") > 0) Zoom(1f);
            if (Input.GetAxis("Mouse ScrollWheel") < 0) Zoom(-1f);
        }

        public void SwitchCharacter()
        {
            currentCharacterNum++;
            if (currentCharacterNum >= characters.Count) currentCharacterNum = 0;
            currentCharacter = characters[currentCharacterNum];
        }
        public void Targeting(float HSpeed, float VSpeed)
        {
            transform.localEulerAngles += new Vector3(0, HSpeed, 0);

            //Debug.Log(cameraContainer.localEulerAngles.x - VSpeed);
            float vertical = cameraContainer.localEulerAngles.x - VSpeed;

            cameraContainer.localEulerAngles = new Vector3(vertical, 0, 0);
            //if (Input.GetMouseButton(0))
            //arsenal?.Use(transform.position, new Vector3(vertical, transform.localEulerAngles.y));
            currentCharacter.Targeting(HSpeed, VSpeed);
        }
        public void Zoom(float zoom)
        {
            cameraGO.transform.localPosition += new Vector3(0, 0, zoom);
        }
    }
}
