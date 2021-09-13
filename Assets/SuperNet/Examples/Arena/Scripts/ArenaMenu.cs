using SuperNet.Netcode.Transport;
using SuperNet.Netcode.Util;
using SuperNet.Unity.Components;
using SuperNet.Unity.Core;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNet.Examples.Arena {

	public class ArenaMenu : MonoBehaviour {

		// UI objects
		public GameObject MenuCanvas;
		public InputField MenuBottomRelay;
		public Button MenuBottomRefresh;
		public Text MenuLeftStatus;
		public GameObject MenuLeftTemplate;
		public InputField MenuCreatePort;
		public Button MenuCreateLaunch;
		public InputField MenuConnectAddress;
		public Button MenuConnectLaunch;
		public GameObject EscapeCanvas;
		public Button EscapeBackground;
		public Button EscapeResume;
		public Button EscapeSpawnSphere;
		public Button EscapeSpawnCube;
		public Button EscapeSpawnNPC;
		public Button EscapeSpawnCar;
		public Button EscapeExit;
		public GameObject GameCanvas;
		public Text GameStatus;
		public Text GameInfo;
		public GameObject ErrorCanvas;
		public Button ErrorConfirm;
		public Text ErrorStatus;
		public GameObject LoadCanvas;
		public Button LoadCancel;
		public Text LoadStatus;

		// Scene objects
		public NetworkHost Host;
		public ArenaRelay Relay;
		public NetworkSpawner SpawnerPlayers;
		public NetworkSpawner SpawnerSpheres;
		public NetworkSpawner SpawnerCubes;
		public NetworkSpawner SpawnerCars;
		public NetworkSpawner SpawnerNPC;
		public BoxCollider[] Spawns;

		// Client peer
		private Peer Client;

		private void Reset() {
			MenuCanvas = transform.Find("Menu").gameObject;
			MenuBottomRelay = transform.Find("Menu/Bottom/Relay").GetComponent<InputField>();
			MenuBottomRefresh = transform.Find("Menu/Bottom/Refresh").GetComponent<Button>();
			MenuLeftStatus = transform.Find("Menu/Left/Status").GetComponent<Text>();
			MenuLeftTemplate = transform.Find("Menu/Left/Table/Viewport/Content/Template").gameObject;
			MenuCreatePort = transform.Find("Menu/Right/Create/Port/Input").GetComponent<InputField>();
			MenuCreateLaunch = transform.Find("Menu/Right/Create/Launch").GetComponent<Button>();
			MenuConnectAddress = transform.Find("Menu/Right/Connect/Address/Input").GetComponent<InputField>();
			MenuConnectLaunch = transform.Find("Menu/Right/Connect/Launch").GetComponent<Button>();
			EscapeCanvas = transform.Find("Escape").gameObject;
			EscapeBackground = transform.Find("Escape/Background").GetComponent<Button>();
			EscapeResume = transform.Find("Escape/Layout/Resume").GetComponent<Button>();
			EscapeSpawnSphere = transform.Find("Escape/Layout/SpawnSphere").GetComponent<Button>();
			EscapeSpawnCube = transform.Find("Escape/Layout/SpawnCube").GetComponent<Button>();
			EscapeSpawnCar = transform.Find("Escape/Layout/SpawnCar").GetComponent<Button>();
			EscapeSpawnNPC = transform.Find("Escape/Layout/SpawnNPC").GetComponent<Button>();
			EscapeExit = transform.Find("Escape/Layout/Exit").GetComponent<Button>();
			GameCanvas = transform.Find("Game").gameObject;
			GameStatus = transform.Find("Game/Status/Text").GetComponent<Text>();
			GameInfo = transform.Find("Game/Info").GetComponent<Text>();
			ErrorCanvas = transform.Find("Error").gameObject;
			ErrorConfirm = transform.Find("Error/Confirm").GetComponent<Button>();
			ErrorStatus = transform.Find("Error/Status").GetComponent<Text>();
			LoadCanvas = transform.Find("Load").gameObject;
			LoadCancel = transform.Find("Load/Cancel").GetComponent<Button>();
			LoadStatus = transform.Find("Load/Status").GetComponent<Text>();
			Host = FindObjectOfType<NetworkHost>();
			Relay = FindObjectOfType<ArenaRelay>();
			SpawnerPlayers = transform.Find("/Spawners/Players").GetComponent<NetworkSpawner>();
			SpawnerSpheres = transform.Find("/Spawners/Spheres").GetComponent<NetworkSpawner>();
			SpawnerCubes = transform.Find("/Spawners/Cubes").GetComponent<NetworkSpawner>();
			SpawnerCars = transform.Find("/Spawners/Cars").GetComponent<NetworkSpawner>();
			SpawnerNPC = transform.Find("/Spawners/NPC").GetComponent<NetworkSpawner>();
			Spawns = transform.Find("/Spawners/Spawns").GetComponentsInChildren<BoxCollider>();
		}

		private void Start() {

			// Register button listeners
			MenuBottomRefresh.onClick.AddListener(OnClickRelayRefresh);
			MenuCreateLaunch.onClick.AddListener(() => LaunchGame(null, MenuBottomRelay.text, MenuCreatePort.text));
			MenuConnectLaunch.onClick.AddListener(() => LaunchGame(MenuConnectAddress.text, MenuBottomRelay.text, null));
			EscapeBackground.onClick.AddListener(OnClickEscapeBackground);
			EscapeResume.onClick.AddListener(OnClickEscapeResume);
			EscapeSpawnSphere.onClick.AddListener(OnClickEscapeSpawnSphere);
			EscapeSpawnCube.onClick.AddListener(OnClickEscapeSpawnCube);
			EscapeSpawnNPC.onClick.AddListener(OnClickEscapeSpawnNPC);
			EscapeSpawnCar.onClick.AddListener(OnClickEscapeSpawnCar);
			EscapeExit.onClick.AddListener(OnClickEscapeExit);
			ErrorConfirm.onClick.AddListener(OnClickErrorConfirm);
			LoadCancel.onClick.AddListener(OnClickLoadCancel);

			// Register peer events
			Host.PeerEvents.OnConnect += OnPeerConnect;
			Host.PeerEvents.OnDisconnect += OnPeerDisconnect;

			// Clear server list and open menu
			ServerListDeleteAll("No connection");
			OpenCanvasMenu();

		}

		private void Update() {

			// If escape is pressed while in game, open escape menu
			if (GameCanvas.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape)) {
				OpenCanvasEscape();
			}

		}

		private void OnDestroy() {

			// Unregister peer events
			// This is necessary if you use multiple scenes
			Host.PeerEvents.OnConnect -= OnPeerConnect;
			Host.PeerEvents.OnDisconnect -= OnPeerDisconnect;

		}

		private void OpenCanvasMenu() {
			// Open the main menu and close everything else
			MenuCanvas.SetActive(true);
			EscapeCanvas.SetActive(false);
			GameCanvas.SetActive(false);
			ErrorCanvas.SetActive(false);
			LoadCanvas.SetActive(false);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void OpenCanvasEscape() {
			// Open the escape menu and close everything else
			MenuCanvas.SetActive(false);
			EscapeCanvas.SetActive(true);
			GameCanvas.SetActive(false);
			ErrorCanvas.SetActive(false);
			LoadCanvas.SetActive(false);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void OpenCanvasGame() {
			// Open the game ui and close everything else
			MenuCanvas.SetActive(false);
			EscapeCanvas.SetActive(false);
			GameCanvas.SetActive(true);
			ErrorCanvas.SetActive(false);
			LoadCanvas.SetActive(false);
			if (!Host.Listening) {
				GameStatus.text = "Not connected";
			} else if (Client != null) {
				GameStatus.text = "Connected to " + Client.Remote;
			} else {
				GameStatus.text = "Listening on port " + Host.GetBindAddress().Port;
			}
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		public void OpenCanvasError(string error) {
			// Open error screen and close everything else
			MenuCanvas.SetActive(false);
			EscapeCanvas.SetActive(false);
			GameCanvas.SetActive(false);
			ErrorCanvas.SetActive(true);
			LoadCanvas.SetActive(false);
			ErrorStatus.text = error;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void OpenCanvasLoad(string status) {
			// Open loading screen and close everything else
			MenuCanvas.SetActive(false);
			EscapeCanvas.SetActive(false);
			GameCanvas.SetActive(false);
			ErrorCanvas.SetActive(false);
			LoadCanvas.SetActive(true);
			LoadStatus.text = status;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private Vector3 GetRandomSpawnPosition(float height) {
			BoxCollider area = Spawns[UnityEngine.Random.Range(0, Spawns.Length)];
			float x = UnityEngine.Random.Range(area.bounds.min.x, area.bounds.max.x);
			float y = area.bounds.min.y + height;
			float z = UnityEngine.Random.Range(area.bounds.min.z, area.bounds.max.z);
			return new Vector3(x, y, z);
		}

		private Quaternion GetRandomPlayerRotation() {
			return Quaternion.Euler(
				0f,
				UnityEngine.Random.Range(0f, 360f),
				0f
			);
		}

		private Quaternion GetRandomMovableRotation() {
			return Quaternion.Euler(
				UnityEngine.Random.Range(0f, 360f),
				UnityEngine.Random.Range(0f, 360f),
				UnityEngine.Random.Range(0f, 360f)
			);
		}

		public void ServerListDeleteAll(string status) {

			// Hide template
			MenuLeftTemplate.SetActive(false);

			// Delete all template instances
			foreach (Transform child in MenuLeftTemplate.transform.parent) {
				if (child == MenuLeftTemplate.transform) continue;
				Destroy(child.gameObject);
			}

			// Set status
			MenuLeftStatus.text = status;

		}

		public void ServerListCreate(RelayServerListResponse message, IPEndPoint relay) {
			if (message.Servers.Count <= 0) {
				// No servers
				ServerListDeleteAll("No servers");
			} else {
				// Create server rows from template
				ServerListDeleteAll("");
				foreach (string server in message.Servers) {
					GameObject instance = Instantiate(MenuLeftTemplate, MenuLeftTemplate.transform.parent);
					Button connect = instance.transform.Find("Connect").GetComponent<Button>();
					Text address = instance.transform.Find("Address").GetComponent<Text>();
					address.text = server;
					connect.onClick.AddListener(() => LaunchGame(server, relay.ToString(), null));
					instance.SetActive(true);
				}
			}
		}

		private void OnClickRelayRefresh() {
			try {

				// Resolve relay address
				IPEndPoint relay = IPResolver.Resolve(MenuBottomRelay.text);

				// Start host if not started yet
				Host.HostConfiguration.Port = 0;
				Host.Startup(true);

				// Connect to relay
				Relay.Connect(relay);

				// Request server list from the relay
				Relay.RequestServerList();

				// Show notice in the server list
				ServerListDeleteAll("Requesting servers...");

			} catch (Exception exception) {


				// Exception
				Debug.LogException(exception);
				ServerListDeleteAll(exception.Message);

			}
		}

		private void LaunchGame(string clientAddress, string relayAddress, string listenPort) {
			try {

				// Parse port
				bool portEmpty = string.IsNullOrEmpty(listenPort);
				bool portParsed = int.TryParse(listenPort, out int portParseValue);
				int port = portEmpty ? 0 : (portParsed ? portParseValue : -1);
				if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort) {
					OpenCanvasError("Bad listen port");
					return;
				}

				// Move the local player to a random position
				foreach (NetworkPrefab prefab in SpawnerPlayers.GetComponentsInChildren<NetworkPrefab>()) {
					if (prefab == null) continue;
					prefab.transform.position = GetRandomSpawnPosition(2f);
					prefab.transform.rotation = GetRandomPlayerRotation();
				}

				// Restart network host
				Host.Dispose();
				Host.HostConfiguration.Port = port;
				Host.Startup(true);

				// Client
				if (string.IsNullOrWhiteSpace(clientAddress)) {

					// No client provided, just set the client to null
					Client = null;

				} else {

					// Resolve client address and start connecting
					IPEndPoint client = IPResolver.Resolve(clientAddress);
					Client = Host.Connect(client, null, null, true);

				}

				// Relay
				if (!string.IsNullOrWhiteSpace(relayAddress)) {

					// Resolve relay address and start connecting
					IPEndPoint relay = IPResolver.Resolve(relayAddress);
					Relay.Connect(relay);

					if (Client == null) {

						// This is a server, add it to the relay
						IPEndPoint local = Host.GetLocalAddress();
						Relay.AddServer(local);

					} else {

						// This is a client, notify server so it starts connecting back
						Relay.NotifyServer(Client.Remote);

					}

				}

				// Log
				IPEndPoint address = Host.GetBindAddress();
				Debug.Log("[Arena] Game launched on port " + address.Port + ".");

				// Open game or loading screen
				if (Client == null) {
					OpenCanvasGame();
				} else {
					OpenCanvasLoad("Connecting...");
				}

			} catch (Exception exception) {

				// Exception
				Debug.LogException(exception);
				OpenCanvasError(exception.Message);
				
			}
		}

		private void OnPeerConnect(Peer peer) {
			NetworkManager.Run(() => {

				// Ignore if this is not the client peer
				if (Client != peer) {
					return;
				}

				// Log
				Debug.Log("[Arena] Connected to " + peer.Remote + ".");

				// Open game UI
				OpenCanvasGame();

			});
		}

		private void OnPeerDisconnect(Peer peer, Reader reader, DisconnectReason reason, Exception exception) {
			NetworkManager.Run(() => {

				// Ignore if this is not the client peer
				if (Client != peer) {
					return;
				}

				// Log
				Debug.Log("[Arena] Disconnected from " + peer.Remote + ": " + reason);

				// Shutdown connection
				Relay.Disconnect();
				Host.Shutdown();
				Client = null;

				// If already in the menu do nothing
				if (MenuCanvas.activeSelf) {
					return;
				}

				// Show error
				OpenCanvasError("Connection to server failed.");

			});
		}

		private void OnClickEscapeSpawnSphere() {
			Vector3 position = GetRandomSpawnPosition(10f);
			Quaternion rotation = GetRandomMovableRotation();
			SpawnerSpheres.Spawn(position, rotation);
		}

		private void OnClickEscapeSpawnCube() {
			Vector3 position = GetRandomSpawnPosition(10f);
			Quaternion rotation = GetRandomMovableRotation();
			SpawnerCubes.Spawn(position, rotation);
		}

		private void OnClickEscapeSpawnNPC() {
			Vector3 position = GetRandomSpawnPosition(2f);
			Quaternion rotation = GetRandomPlayerRotation();
			SpawnerNPC.Spawn(position, rotation);
		}

		private void OnClickEscapeSpawnCar() {
			Vector3 position = GetRandomSpawnPosition(2f);
			Quaternion rotation = GetRandomPlayerRotation();
			SpawnerCars.Spawn(position, rotation);
		}

		private void OnClickEscapeBackground() {
			// Resume game
			OpenCanvasGame();
		}

		private void OnClickEscapeResume() {
			// Resume game
			OpenCanvasGame();
		}

		private void OnClickEscapeExit() {
			// Shutdown game and open menu
			Relay.Disconnect();
			Host.Shutdown();
			Client = null;
			OpenCanvasMenu();
			DespawnAllObjects();
		}

		private void OnClickErrorConfirm() {
			// Shutdown game and open menu
			Relay.Disconnect();
			Host.Shutdown();
			Client = null;
			OpenCanvasMenu();
			DespawnAllObjects();
		}

		private void OnClickLoadCancel() {
			// Shutdown game and open menu
			Relay.Disconnect();
			Host.Shutdown();
			Client = null;
			OpenCanvasMenu();
			DespawnAllObjects();
		}

		private void DespawnAllObjects() {
			IReadOnlyList<NetworkPrefab> spheres = SpawnerSpheres.GetSpawnedPrefabs();
			foreach (NetworkPrefab prefab in spheres) SpawnerSpheres.Despawn(prefab);
			IReadOnlyList<NetworkPrefab> cubes = SpawnerCubes.GetSpawnedPrefabs();
			foreach (NetworkPrefab prefab in cubes) SpawnerCubes.Despawn(prefab);
			IReadOnlyList<NetworkPrefab> npc = SpawnerNPC.GetSpawnedPrefabs();
			foreach (NetworkPrefab prefab in npc) SpawnerNPC.Despawn(prefab);
			IReadOnlyList<NetworkPrefab> cars = SpawnerCars.GetSpawnedPrefabs();
			foreach (NetworkPrefab prefab in cars) SpawnerCars.Despawn(prefab);
		}

	}

}
