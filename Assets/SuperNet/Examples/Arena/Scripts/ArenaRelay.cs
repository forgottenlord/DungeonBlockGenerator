using SuperNet.Netcode.Transport;
using SuperNet.Netcode.Util;
using SuperNet.Unity.Core;
using System;
using System.Net;
using UnityEngine;

namespace SuperNet.Examples.Arena {

	public class ArenaRelay : MonoBehaviour {

		// Scene objects
		public ArenaMenu Menu;
		
		// Resources
		private Peer Peer = null;
		private bool RequestList = false;
		private IPEndPoint ServerToAdd = null;
		private IPEndPoint ServerToNotify = null;
		
		private void Reset() {
			Menu = transform.Find("/Canvas").GetComponent<ArenaMenu>();
		}

		public void Connect(IPEndPoint address) {

			// If address is null, disconnect
			if (address == null) {
				Disconnect();
				return;
			}

			// Check if already connected
			if (Peer != null && !Peer.Disposed) {
				if (Peer.Remote.Equals(address)) {
					// Already connected to the same address, just return
					return;
				} else {
					// Connected to a different address, disconnect
					Disconnect();
				}
			}

			// Log
			Debug.Log("[Arena] [Relay] Connecting to relay " + address + ".");

			// Create and register events
			PeerEvents events = new PeerEvents();
			events.OnConnect += OnRelayConnect;
			events.OnDisconnect += OnRelayDisconnect;
			events.OnReceive += OnRelayReceive;

			// Start connecting
			Peer = Menu.Host.Connect(address, events, null, true);

		}

		public void Disconnect() {

			// Disconnect peer and shutdown host
			if (Peer != null) {
				Debug.Log("[Arena] [Relay] Disconnecting from relay.");
				Peer.Disconnect();
				Peer = null;
			}

			// Clear pending actions
			ServerToAdd = null;
			ServerToNotify = null;
			RequestList = false;

			// Disconnecting from the relay removes the server from the list if added

		}

		public void RequestServerList() {

			// Check if already connected
			if (Peer.Connected) {

				// Already connected, just log and send message
				Debug.Log("[Arena] [Relay] Requesting server list.");
				Peer.Send(new RelayServerListRequest());
				RequestList = false;

			} else {

				// Not yet connected, wait for the connection to be accepted
				RequestList = true;

			}

		}

		public void NotifyServer(IPEndPoint server) {

			// This method is called by the menu whenever it starts connecting to a server
			// It notifies the relay which then notifies the server
			// If the server then starts connecting back to the client a P2P connection is established
			// This allows server to accept connections even if they don't have ports open

			// Check if already connected
			if (Peer.Connected) {

				// Already connected, just log and send message
				Debug.Log("[Arena] [Relay] Sending connection request.");
				Peer.Send(new RelayConnect() { Address = server.ToString() });
				ServerToNotify = null;

			} else {

				// Not yet connected, wait for the connection to be accepted
				ServerToNotify = server;

			}

		}

		public void AddServer(IPEndPoint local) {

			// Check if already connected
			if (Peer.Connected) {

				// Already connected, just log and send message
				Debug.Log("[Arena] [Relay] Adding to server list.");
				Peer.Send(new RelayServerListAdd() { Address = local.ToString() });
				ServerToAdd = null;

			} else {

				// Not yet connected, wait for the connection to be accepted
				ServerToAdd = local;

			}

		}

		private void OnRelayConnect(Peer peer) {
			NetworkManager.Run(() => {

				// Called when a connection to the relay is established

				// Log
				Debug.Log("[Arena] [Relay] Connection to relay " + peer.Remote + " established.");

				// All peers are automatically registered by NetworkHost because AutoRegister is enabled
				// A registered peer is used for syncronizing components by sending and receiving component messages
				// This peer however is used to only communicate with the relay so we can unregister it
				NetworkManager.Unregister(peer);

				// Add the server to the server list
				if (ServerToAdd != null) {
					Debug.Log("[Arena] [Relay] Adding to server list.");
					peer.Send(new RelayServerListAdd() { Address = ServerToAdd.ToString() });
					ServerToAdd = null;
				}

				// Request server list
				if (RequestList) {
					Debug.Log("[Arena] [Relay] Requesting server list.");
					peer.Send(new RelayServerListRequest());
					RequestList = false;
				}

				// Notify server
				if (ServerToNotify != null) {
					Debug.Log("[Arena] [Relay] Sending connection request.");
					peer.Send(new RelayConnect() { Address = ServerToNotify.ToString() });
					ServerToNotify = null;
				}

			});
		}

		private void OnRelayDisconnect(Peer peer, Reader message, DisconnectReason reason, Exception exception) {
			NetworkManager.Run(() => {

				// Called when a connection to relay terminates

				// Ignore if we requested the disconnect
				if (reason == DisconnectReason.Disconnected) return;

				// Log
				Debug.Log("[Arena] [Relay] Disconnected from relay: " + reason);

				// If not the relay peer, do nothing
				if (Peer != peer) {
					return;
				}

				// Shutdown connection
				Disconnect();

				// Clear out the server list
				Menu.ServerListDeleteAll("No connection");

				// If already in the menu do nothing
				if (Menu.MenuCanvas.activeSelf) {
					return;
				}

				// Show error
				Menu.OpenCanvasError("Connection to relay failed.");

			});
		}

		private void OnRelayReceive(Peer peer, Reader message, MessageReceived info) {

			// Called when a relay sends a message

			// Convert channel to message type and process based on type
			RelayMessageType type = (RelayMessageType)info.Channel;
			switch (type) {
				case RelayMessageType.ServerListResponse:
					// Server list received from relay
					OnRelayReceiveList(peer, message);
					break;
				case RelayMessageType.Connect:
					// A new connection request received
					OnRelayReceiveConnect(message);
					break;
			}

		}

		private void OnRelayReceiveList(Peer peer, Reader reader) {

			// Read the message
			RelayServerListResponse message = new RelayServerListResponse();
			message.Read(reader);

			// Log
			Debug.Log("[Arena] [Relay] Received " + message.Servers.Count + " servers.");

			// Send list to menu
			NetworkManager.Run(() => Menu.ServerListCreate(message, peer.Remote));

		}

		private void OnRelayReceiveConnect(Reader reader) {

			// This method just starts connecting to the received address
			// The address is received from the relay when a P2P connection is requested
			// If the server and the client both start connecting to eachother at the same time
			// Then a P2P connection is established

			// Read the message
			RelayConnect message = new RelayConnect();
			message.Read(reader);

			// Parse address
			IPEndPoint address = IPResolver.TryParse(message.Address);
			if (address == null) {
				Debug.LogWarning("[Arena] [Relay] Connection address " + message.Address + " is invalid.");
				return;
			}

			// Log
			Debug.Log("[Arena] [Relay] Received " + address + " connection request from relay.");

			// Start connecting to establish a P2P connection
			Menu.Host.Connect(address);

		}

	}

}
