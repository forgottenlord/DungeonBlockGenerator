using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderPulse.Characters;
using ThunderPulse.Common;
using ThunderPulse.Interfaces;
using ThunderPulse.Vehicles.StarShips.CustomStarShips;
using ThunderPulse.Transporting;

namespace ThunderPulse.Controllers
{
    /// <summary>
    /// Интерфейс игрока всегда один единственный.
    /// </summary>
    public sealed class Controller : MonoBehaviour, IController
    {
        /// <summary>
        /// Режим управления тактический(KeyWord.ControllType_Tactical) или строительный(KeyWord.ControllType_Customization)
        /// </summary>
        private KeyWord ControllType = KeyWord.ControllType_Tactical;
        
        GameObject cameraGO;
        Transform cameraContainer;
        public Vector3 cameraShift;
        public float Sencitivity = 3;
        public IArsenal arsenal;
        private Camera cam;

        public ControlledObject UnitToControll;
        /// <summary>
        /// Объект находящийся под контролем игрока.
        /// </summary>
        private ControlledObject controlledUnit;
        private ObjectCarrier objectCarrier;
        void Awake()
        {
            Name = "Player";
            cameraContainer = new GameObject("CameraContainer").transform;
            cameraContainer.parent = transform;
            cameraContainer.localPosition = new Vector3();
            cameraGO = new GameObject("CameraGO");
            cameraGO.transform.parent = cameraContainer;
            cameraContainer.parent = transform;
            cameraContainer.transform.localPosition = cameraShift;
            cam = cameraGO.AddComponent<Camera>();
            cam.farClipPlane = 99999;
            arsenal = GetComponent<IArsenal>();

            objectCarrier = GeneratedObject.SetComponent<ObjectCarrier>(gameObject);
        }
        public void SwitchControllType()
        {
            if (ControllType == KeyWord.ControllType_Tactical)
                ControllType = KeyWord.ControllType_Customization;
            else
                ControllType = KeyWord.ControllType_Tactical;
        }

        /// <summary>
        /// Захват контроля над объектом.
        /// </summary>
        /// <typeparam name="UnitType"></typeparam>
        public void Capture(ControlledObject unit)
        {
            Cursor.lockState = CursorLockMode.Locked;
            //transform.localEulerAngles = new Vector3();
            if (unit != null)
            {
                if (unit is Character)
                {
                    Human character = (Human)unit;
                    cameraGO.transform.localPosition = new Vector3(1, 1, -3);
                    transform.localPosition = new Vector3(0, character.Height / 2, 0);
                    controlled.Add(character);
                    controlledUnit = character;
                }
                if (unit is CustomStarShipView)
                {
                    cameraGO.transform.localPosition = new Vector3(0, 0, -100);
                    cameraContainer.transform.localPosition = new Vector3(0, 0, 0);
                }
            }
            //transform.parent = controlledUnit.transform;
            transform.position = controlledUnit.transform.position;
        }
        public string Name
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }

        public List<Interfaces.IControlled> controlled = new List<IControlled>();
        public void Zoom(float zoom)
        {
            cameraGO.transform.localPosition += new Vector3(0, 0, zoom);
        }
        List<KeyWord> controls = new List<KeyWord>();
        public void FixedUpdate()
        {
            if (controlledUnit == null && UnitToControll != null) Capture(UnitToControll);
            if (controlledUnit == null) return;
            transform.position = controlledUnit.transform.position;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            controls.Clear();
            if (controlledUnit is Character)
            {
                Character chara = (Character)controlledUnit;
                if (Input.GetKeyDown(KeyCode.Tilde))
                {
                    if (chara.IsArmed) controls.Add(KeyWord.Order_Disarm);
                    else controls.Add(KeyWord.Order_Arm);
                }
            }

            if (Input.GetKey(KeyCode.W)) controls.Add(KeyWord.Controlls_MoveFront);
            if (Input.GetKey(KeyCode.S)) controls.Add(KeyWord.Controlls_MoveBack);
            if (Input.GetKey(KeyCode.D)) controls.Add(KeyWord.Controlls_MoveRight);
            if (Input.GetKey(KeyCode.A)) controls.Add(KeyWord.Controlls_MoveLeft);
            //if (Input.GetKey(KeyCode.Space)) controls.Add(KeyWord.Controlls_Jump);

            if (cameraGO != null)
            {
                if (objectCarrier.CarriedObjects.Count > 0)
                {
                    objectCarrier.Hold(Input.GetAxis("Mouse ScrollWheel"));
                }
                else
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0) Zoom(1f);
                    if (Input.GetAxis("Mouse ScrollWheel") < 0) Zoom(-1f);
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        objectCarrier.Capture(cam.transform, hitInfo.transform);
                    }
                }
                if (Input.GetKeyUp(KeyCode.T))
                {
                    objectCarrier.Release();
                }
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchControllType();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                controls.Add(KeyWord.Order_Suicide);
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                controls.Add(KeyWord.Order_PoseStandLower);
            }
            if (Input.GetKey(KeyCode.Space))
            {
                controls.Add(KeyWord.Order_PoseStandUpper);
            }
            controls.Add(ControllType);
            if (ControllType == KeyWord.ControllType_Tactical)
            {
                Targeting(Input.GetAxis("Mouse X") * Sencitivity,
                                Input.GetAxis("Mouse Y") * Sencitivity);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ThunderPulse.UI.Menus.CreatorUI.Create<ThunderPulse.UI.Menus.GridSelectorUI>(KeyWord.UI_StarShipModulesSelectorUI);
            }
            //ray.origin = cam.transform.position;
            controlledUnit.OnControll(controls, ray);
        }
        public void Targeting(float HSpeed, float VSpeed)
        {
            transform.localEulerAngles += new Vector3(0, HSpeed, 0);

            //Debug.Log(cameraContainer.localEulerAngles.x - VSpeed);
            float vertical = cameraContainer.localEulerAngles.x - VSpeed;

            cameraContainer.localEulerAngles = new Vector3(vertical, 0, 0);
            if (Input.GetMouseButton(0))
                arsenal?.Use(transform.position, new Vector3(vertical, transform.localEulerAngles.y));
            controlledUnit.Targeting(HSpeed, VSpeed);
        }
        public void Action()
        {

        }
    }
}