using SuperNet.Netcode.Transport;
using SuperNet.Unity.Core;
using SuperNet.Netcode.Util;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;
using System;

namespace SuperNet.Unity.Components {

	/// <summary>
	/// Manages spawnable network prefabs.
	/// </summary>
	[AddComponentMenu("SuperNet/NetworkSpawner")]
	[HelpURL("https://superversus.com/netcode/api/SuperNet.Unity.Components.NetworkSpawner.html")]
	public sealed class NetworkSpawner : NetworkComponent, INetworkAuthoritative {

		/// <summary>
		/// The prefab to clone when spawning.
		/// </summary>
		[FormerlySerializedAs("Prefab")]
		[Tooltip("The prefab to clone when spawning.")]
		public NetworkPrefab Prefab;

		/// <summary>
		/// The parent transform to spawn under.
		/// </summary>
		[FormerlySerializedAs("Parent")]
		[Tooltip("The parent transform to spawn under.")]
		public Transform Parent;

		/// <summary>
		/// Network channel to use.
		/// </summary>
		[FormerlySerializedAs("SyncChannel")]
		[Tooltip("Network channel to use.")]
		public byte SyncChannel;

		/// <summary>
		/// True if we can spawn and despawn. Ignored if unlocked.
		/// <para>If this is true at start, prespawned instances will automatically spawn.</para>
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("Authority")]
		[Tooltip("True if we can spawn and despawn. Ignored if unlocked.")]
		private bool Authority;

		/// <summary>
		/// True if only authority can spawn. Set to false to let anybody spawn.
		/// </summary>
		[FormerlySerializedAs("Locked")]
		[Tooltip("True if only authority can spawn. Set to false to let anybody spawn.")]
		public bool Locked;

		/// <summary>
		/// True if we can spawn and despawn.
		/// </summary>
		public bool IsAuthority => Authority;

		// Resources
		private Dictionary<NetworkIdentity, SpawnedPrefab> PrefabsSpawned;
		private NetworkPrefab[] PrefabsSpawnedArray;
		private NetworkPrefab[] PrefabsPrespawned;
		private object AuthorityLock;
		private Peer AuthorityPeer;
		
		private class SpawnedPrefab {
			public NetworkIdentity NetworkID;
			public NetworkPrefab Instance;
			public int? PrespawnIndex;
		}

		private void Reset() {
			ResetNetworkID();
			Prefab = null;
			Parent = transform;
			SyncChannel = 0;
			Authority = true;
			Locked = false;
		}

		private void Awake() {

			// Initialize
			PrefabsSpawned = new Dictionary<NetworkIdentity, SpawnedPrefab>();
			PrefabsSpawnedArray = new NetworkPrefab[0];
			AuthorityLock = new object();
			AuthorityPeer = null;

			// Save and disable all pre-spawned instances
			Transform parent = Parent == null ? transform : Parent;
			PrefabsPrespawned = new NetworkPrefab[parent.childCount];
			for (int index = 0; index < PrefabsPrespawned.Length; index++) {
				Transform child = parent.GetChild(index);
				if (child == null) continue;
				NetworkPrefab instance = child.GetComponent<NetworkPrefab>();
				if (instance == null) continue;
				PrefabsPrespawned[index] = instance;
				instance.gameObject.SetActive(false);
			}

		}

		protected override void Start() {

			// Spawn all pre-spawned prefabs
			if (Authority) {
				SpawnAllPrespawns();
			}

			// Register spawner
			base.Start();

		}

		/// <summary>
		/// Get all spawned instances as a list.
		/// </summary>
		/// <returns>All spawned instances.</returns>
		public IReadOnlyList<NetworkPrefab> GetSpawnedPrefabs() {
			return PrefabsSpawnedArray;
		}

		/// <summary>
		/// Spawn a network instance and notify the network.
		/// </summary>
		/// <param name="relativeToSpawner">True to position the new object relative to the spawner.</param>
		/// <returns>The instantiated clone.</returns>
		public NetworkPrefab Spawn(bool relativeToSpawner = true) {
			if (relativeToSpawner) {
				NetworkPrefab prefab = Instantiate(Prefab, transform.position, transform.rotation, Parent);
				SpawnLocal(prefab, null, true);
				return prefab;
			} else {
				NetworkPrefab prefab = Instantiate(Prefab, Parent, false);
				SpawnLocal(prefab, null, true);
				return prefab;
			}
		}

		/// <summary>
		/// Spawn a network instance and notify the network.
		/// </summary>
		/// <param name="position">Position for the new object.</param>
		/// <param name="rotation">Orientation of the new object.</param>
		/// <param name="relativeToParent">True to position the new object relative to spawn parent.</param>
		/// <returns>The instantiated clone.</returns>
		public NetworkPrefab Spawn(Vector3 position, Quaternion rotation, bool relativeToParent = false) {
			Transform parent = Parent;
			if (parent == null) {
				NetworkPrefab prefab = Instantiate(Prefab, position, rotation);
				SpawnLocal(prefab, null, true);
				return prefab;
			} else if (relativeToParent) {
				NetworkPrefab prefab = Instantiate(Prefab, parent.position + position, parent.rotation * rotation, parent);
				SpawnLocal(prefab, null, true);
				return prefab;
			} else {
				NetworkPrefab prefab = Instantiate(Prefab, position, rotation, parent);
				SpawnLocal(prefab, null, true);
				return prefab;
			}
		}

		/// <summary>
		/// Spawn an already instantiated prefab.
		/// </summary>
		/// <param name="instance">Instantiated prefab.</param>
		public void Spawn(NetworkPrefab instance) {

			// Validate
			if (instance == null) {
				throw new ArgumentNullException(nameof(instance), "Instance is null");
			}

			// Warn if instance is already spawned on a different spawner
			NetworkSpawner spawner = instance.Spawner;
			if (spawner != null && spawner != this && NetworkManager.LogExceptions) {
				Debug.LogWarning(string.Format(
					"[SuperNet] [NetworkSpawner - {0}] Respawning on a different spawner.",
					NetworkID, spawner.NetworkID
				), instance);
			}

			// Warn if instance has an incorrect parent
			if (instance.transform.parent != Parent && NetworkManager.LogExceptions) {
				Debug.LogWarning(string.Format(
					"[SuperNet] [NetworkSpawner - {0}] Instance is not parented to correct transform.",
					NetworkID
				), instance);
			}

			// Warn if spawning is not allowed
			if (Locked && !Authority && NetworkManager.LogExceptions) {
				Debug.LogWarning(string.Format(
					"[SuperNet] [NetworkSpawner - {0}] Instance spawned on a locked spawner without authority.",
					NetworkID
				), instance);
			}

			// Spawn instance
			SpawnLocal(instance, null, true);

		}

		/// <summary>
		/// Despawn an instantiated instance and optionally destroy it.
		/// </summary>
		/// <param name="instance">Instance to despawn.</param>
		/// <param name="destroy">True to also destroy the instance.</param>
		/// <returns>True if despawned, false if not.</returns>
		public bool Despawn(NetworkPrefab instance, bool destroy = true) {

			if (instance == null) {

				// Provided instance is null, already despawned
				return true;

			} else if (instance.Spawner != this) {

				// Instance is not spawned on this spawner
				return false;

			} else if (Locked && !Authority) {

				// Spawner is locked
				return false;

			} else if (!Locked && instance.Spawnee != null) {

				// Spawner is unlocked but instance was spawned by someone else
				return false;

			}

			// Despawn the instance
			DespawnLocal(instance, true, destroy);
			return true;

		}

		internal void OnPrefabDestroy(NetworkPrefab instance) {

			// Warn if despawning is not allowed
			if (Locked && !Authority && NetworkManager.LogExceptions) {
				Debug.LogWarning(string.Format(
					"[SuperNet] [NetworkSpawner - {0}] Instance was destroyed on a locked spawner without authority.",
					NetworkID
				), instance);
			}

			// Warn if destroying an instance spawned by someone else
			Peer spawnee = instance.Spawnee;
			if (!Locked && spawnee != null) {
				Debug.LogWarning(string.Format(
					"[SuperNet] [NetworkSpawner - {0}] Instance spawned by {1} was destroyed without authority.",
					NetworkID, spawnee.Remote
				), instance);
			}

			// Despawn instance
			if (Locked) {
				DespawnLocal(instance, Authority, false);
			} else {
				DespawnLocal(instance, spawnee == null, false);
			}

		}

		private void SpawnAllPrespawns() {
			for (int index = 0; index < PrefabsPrespawned.Length; index++) {
				NetworkPrefab instance = Interlocked.Exchange(ref PrefabsPrespawned[index], null);
				if (instance != null) {
					SpawnLocal(instance, index, false);
				}
			}
		}

		private void SpawnLocal(NetworkPrefab instance, int? index, bool send) {
			Run(() => {

				// Check if instance is already spawned
				NetworkIdentity existingID = instance.NetworkID;
				PrefabsSpawned.TryGetValue(existingID, out SpawnedPrefab found);
				if (found != null && found.Instance == instance) {
					return;
				}

				// Activate and reparent instance
				instance.gameObject.SetActive(true);
				instance.transform.parent = Parent;

				// Initialize
				NetworkIdentity registerID = instance.OnSpawnLocal(this);

				// Save the instance
				PrefabsSpawned[registerID] = new SpawnedPrefab() {
					NetworkID = registerID,
					Instance = instance,
					PrespawnIndex = index,
				};

				// Update array
				UpdateSpawnedArray();

				// Send spawn message and state
				if (send) {
					SendNetworkMessage(new NetworkMessageSpawn(
						registerID,
						instance.transform.localPosition,
						instance.transform.localRotation,
						SyncChannel
					));
				}

			});
		}

		private void SpawnRemote(Peer peer, NetworkIdentity networkID, Vector3 position, Quaternion rotation, int? index) {
			Run(() => {

				// Check if instance is already spawned
				PrefabsSpawned.TryGetValue(networkID, out SpawnedPrefab found);
				if (found != null && found.Instance != null) {
					return;
				}

				// Parent and instance
				Transform parent = Parent;

				// Find prespawned instance
				NetworkPrefab instance = null;
				if (index != null && index >= 0 && index < PrefabsPrespawned.Length) {
					instance = Interlocked.Exchange(ref PrefabsPrespawned[index.Value], null);
				}

				// If no prespawned instanced found, instantiate
				if (instance == null) {
					if (parent == null) {
						instance = Instantiate(Prefab, position, rotation);
					} else {
						instance = Instantiate(Prefab, parent.position + position, parent.rotation * rotation, parent);
					}
				}
				
				// Activate and reparent instance
				instance.gameObject.SetActive(true);
				instance.transform.parent = parent;

				// Initialize
				instance.OnSpawnRemote(this, peer, networkID);

				// Save instance
				PrefabsSpawned[networkID] = new SpawnedPrefab() {
					NetworkID = networkID,
					Instance = instance,
					PrespawnIndex = index,
				};

				// Update array
				UpdateSpawnedArray();

			});
		}

		private void DespawnLocal(NetworkPrefab instance, bool send, bool destroy) {
			Run(() => {

				// If instance is not spawned, do nothing
				NetworkIdentity networkID = instance.NetworkID;
				PrefabsSpawned.TryGetValue(networkID, out SpawnedPrefab found);
				if (found == null || found.Instance != instance) {
					return;
				}

				// Remove the instance and update array
				PrefabsSpawned.Remove(networkID);
				UpdateSpawnedArray();

				// Send despawn message
				if (send) {
					SendNetworkMessage(new NetworkMessageDespawn(networkID, SyncChannel));
				}

				// Destroy the instance
				if (destroy) {
					Destroy(instance.gameObject);
				}

			});
		}

		private void DespawnRemote(Peer peer, NetworkIdentity networkID) {
			Run(() => {

				// Check if instance exists
				PrefabsSpawned.TryGetValue(networkID, out SpawnedPrefab found);
				if (found == null || found.Instance == null) {
					return;
				}

				// Check if instance is owned by the peer
				Peer spawnee = found.Instance.Spawnee;
				if (!Locked && spawnee != peer) {
					if (spawnee == null) {
						if (NetworkManager.LogExceptions) {
							Debug.LogWarning(string.Format(
								"[SuperNet] [NetworkSpawner - {0}] Recieved despawn from {1} but we spawned it. Ignoring.",
								NetworkID, peer.Remote
							), found.Instance);
						}
					} else {
						if (NetworkManager.LogExceptions) {
							Debug.LogWarning(string.Format(
								"[SuperNet] [NetworkSpawner - {0}] Recieved despawn from {1} but {2} spawned it. Ignoring.",
								NetworkID, peer.Remote, spawnee.Remote
							), found.Instance);
						}
					}
					return;
				}

				// Remove the instance and update array
				PrefabsSpawned.Remove(networkID);
				UpdateSpawnedArray();

				// Destroy the instance
				NetworkPrefab instance = found.Instance;
				Destroy(instance.gameObject);

			});
		}

		private void UpdateSpawnedArray() {
			int index = 0;
			NetworkPrefab[] array = new NetworkPrefab[PrefabsSpawned.Count];
			foreach (SpawnedPrefab obj in PrefabsSpawned.Values) {
				array[index] = obj.Instance;
				index++;
			}
			Interlocked.Exchange(ref PrefabsSpawnedArray, array);
		}

		void INetworkAuthoritative.OnNetworkAuthorityUpdate(bool authority, HostTimestamp timestamp) {

			// Update authority and resync state
			SetAuthority(authority);

		}

		/// <summary>
		/// Update authority on this component.
		/// </summary>
		/// <param name="authority">Authority to set.</param>
		public void SetAuthority(bool authority) {

			lock (AuthorityLock) {

				// Check if authority changed
				if (Authority == authority) {
					return;
				}

				// Log
				if (NetworkManager.LogEvents) {
					Debug.Log(string.Format(
						"[SuperNet] [NetworkSpawner - {0}] Authority {1}.",
						NetworkID, authority ? "gained" : "lost"
					), this);
				}

				// Update authority
				Authority = authority;

				// Remove authority peer if we are now authority
				if (Authority) {
					Interlocked.Exchange(ref AuthorityPeer, null);
				}

				// Spawn all pending prespawns
				if (Authority) {
					SpawnAllPrespawns();
				}

				// Send or request state
				if (IsRegisteredOnNetwork) {
					foreach (Peer peer in GetNetworkPeers()) {
						SyncNetworkState(peer, false);
					}
				}

			}

		}

		public override void OnNetworkRegister() {

			lock (AuthorityLock) {

				// Send or request state
				foreach (Peer peer in GetNetworkPeers()) {
					SyncNetworkState(peer, false);
				}

			}

		}

		public override void OnNetworkConnect(Peer peer) {

			// Ignore if local peer
			if (Host.IsLocal(peer.Remote)) {
				return;
			}

			// Send or request state
			lock (AuthorityLock) {
				SyncNetworkState(peer, true);
			}

		}

		public override void OnNetworkDisconnect(Peer peer) {

			// Ignore if local peer
			if (Host.IsLocal(peer.Remote)) {
				return;
			}

			lock (AuthorityLock) {

				// Remove authority peer if it disconnected
				Peer previousAuthority = Interlocked.CompareExchange(ref AuthorityPeer, null, peer);

				// Request state from a new authority
				if (Locked && !Authority && previousAuthority == peer) {
					SendNetworkMessage(new NetworkMessageRequest(SyncChannel));
				}

			}

		}

		private void SyncNetworkState(Peer peer, bool requestAuthority) {
			if (Host.IsLocal(peer.Remote)) {
				return;
			} else if (Locked) {
				if (Authority) {
					SendNetworkState(peer);
				} else if (requestAuthority || AuthorityPeer == null) {
					SendNetworkMessage(new NetworkMessageRequest(SyncChannel), peer);
				}
			} else {
				SendNetworkState(peer);
				SendNetworkMessage(new NetworkMessageRequest(SyncChannel), peer);
			}
		}

		private void SendNetworkState(Peer peer = null) {
			Run(() => {

				if (peer == null) {

					// Send all spawns to all peers
					foreach (SpawnedPrefab prefab in PrefabsSpawned.Values) {
						if (prefab.Instance == null) continue;
						if (prefab.PrespawnIndex == null) {
							SendNetworkMessage(new NetworkMessageSpawn(
								prefab.NetworkID,
								prefab.Instance.transform.localPosition,
								prefab.Instance.transform.localRotation,
								SyncChannel
							));
						} else {
							SendNetworkMessage(new NetworkMessagePrespawn(
								prefab.NetworkID,
								prefab.Instance.transform.localPosition,
								prefab.Instance.transform.localRotation,
								prefab.PrespawnIndex.Value,
								SyncChannel
							));
						}
					}

				} else {

					// Send all spawns to the provided peer
					foreach (SpawnedPrefab prefab in PrefabsSpawned.Values) {
						if (prefab.Instance == null) continue;
						if (prefab.PrespawnIndex == null) {
							SendNetworkMessage(new NetworkMessageSpawn(
								prefab.NetworkID,
								prefab.Instance.transform.localPosition,
								prefab.Instance.transform.localRotation,
								SyncChannel
							), peer);
						} else {
							SendNetworkMessage(new NetworkMessagePrespawn(
								prefab.NetworkID,
								prefab.Instance.transform.localPosition,
								prefab.Instance.transform.localRotation,
								prefab.PrespawnIndex.Value,
								SyncChannel
							), peer);
						}
					}

				}

			});
		}

		public override bool OnNetworkResend(Peer origin, Peer peer, Reader reader, MessageReceived info) {

			// Dont resend to local connections
			if (Host.IsLocal(peer.Remote)) {
				return false;
			}

			// Read header and decide if message needs to be resent
			Header header = reader.ReadEnum<Header>();
			switch (header) {
				case Header.Request:
				case Header.Spawn:
				case Header.Prespawn:
				case Header.Despawn:
					// If we have authority on a locked spawner, don't resend
					if (Locked && Authority) {
						return false;
					} else {
						return true;
					}
				default:
					return false;
			}

		}

		public override void OnNetworkMessage(Peer peer, Reader reader, MessageReceived info) {

			// Ignore messages from local connections
			if (Host.IsLocal(peer.Remote)) {
				return;
			}

			// Read header and process message
			Header header = reader.ReadEnum<Header>();
			switch (header) {
				case Header.Request:
					OnNetworkMessageRequest(peer);
					break;
				case Header.Spawn:
					OnNetworkMessageSpawn(peer, reader, false);
					break;
				case Header.Prespawn:
					OnNetworkMessageSpawn(peer, reader, true);
					break;
				case Header.Despawn:
					OnNetworkMessageDespawn(peer, reader);
					break;
				default:
					if (NetworkManager.LogExceptions) {
						Debug.LogWarning(string.Format(
							"[SuperNet] [NetworkSpawner - {0}] Received invalid header {0} from {1}. Ignoring.",
							NetworkID, header, peer.Remote
						), this);
					}
					break;
			}

		}

		private void OnNetworkMessageRequest(Peer peer) {

			// Respond with state if we have authority or this is an unlocked spawner
			if (Locked) {
				if (Authority) {
					SendNetworkState(peer);
				}
			} else {
				SendNetworkState(peer);
			}

		}

		private void OnNetworkMessageSpawn(Peer peer, Reader reader, bool prespawn) {

			// Read message
			int? index = null;
			NetworkIdentity networkID = reader.ReadNetworkID();
			Vector3 position = reader.ReadVector3();
			Quaternion rotation = reader.ReadQuaternion();
			if (prespawn) index = reader.ReadInt32();

			// Validate ID
			if (networkID.IsInvalid) {
				if (NetworkManager.LogExceptions) {
					Debug.LogWarning(string.Format(
						"[SuperNet] [NetworkSpawner - {0}] Received spawn for invalid ID from {1}. Ignoring.",
						NetworkID, peer.Remote
					), this);
				}
				return;
			}

			lock (AuthorityLock) {

				// If we have authority and spawner is locked, ignore
				if (Locked && Authority) {
					if (NetworkManager.LogExceptions) {
						Debug.LogWarning(string.Format(
							"[SuperNet] [NetworkSpawner - {0}] Recieved spawn from {1} for ID {2} but we have authority. Ignoring.",
							NetworkID, peer.Remote, networkID
						), this);
					}
					return;
				}

				// Save authority peer
				if (Locked) {
					Interlocked.Exchange(ref AuthorityPeer, peer);
				}

				// Log
				if (NetworkManager.LogEvents) {
					if (index == null) {
						Debug.Log(string.Format(
							"[SuperNet] [NetworkSpawner - {0}] Received spawn for ID {1} from {2}.",
							NetworkID, networkID, peer.Remote
						), this);
					} else {
						Debug.Log(string.Format(
							"[SuperNet] [NetworkSpawner - {0}] Received prespawn for ID {1} with index {2} from {3}.",
							NetworkID, index.Value, networkID, peer.Remote
						), this);
					}
				}

			}

			// Spawn instance
			SpawnRemote(peer, networkID, position, rotation, index);

		}

		private void OnNetworkMessageDespawn(Peer peer, Reader reader) {

			// Read ID
			NetworkIdentity networkID = reader.ReadNetworkID();

			lock (AuthorityLock) {

				// If we have authority and spawner is locked, do nothing
				if (Locked && Authority) {
					if (NetworkManager.LogExceptions) {
						Debug.LogWarning(string.Format(
							"[SuperNet] [NetworkSpawner - {0}] Recieved despawn from {1} for ID {2} but we have authority. Ignoring.",
							NetworkID, peer.Remote, networkID
						), this);
					}
					return;
				}

				// Save authority peer
				if (Locked) {
					Interlocked.Exchange(ref AuthorityPeer, peer);
				}

				// Log
				if (NetworkManager.LogEvents) {
					Debug.Log(string.Format(
						"[SuperNet] [NetworkSpawner - {0}] Received despawn for ID {1} from {2}.",
						NetworkID, networkID, peer.Remote
					), this);
				}

			}

			// Despawn
			DespawnRemote(peer, networkID);

		}

		private enum Header : byte {

			/// <summary>Sent when requesting state from authority.</summary>
			Request = 1,

			/// <summary>Sent whenever an instance is spawned.</summary>
			Spawn = 2,

			/// <summary>Sent whenever a prespawned instance is spawned.</summary>
			Prespawn = 3,

			/// <summary>Sent whenever a instance is despawned.</summary>
			Despawn = 4,

		}

		private struct NetworkMessageRequest : IMessage {

			HostTimestamp IMessage.Timestamp => Host.Now;
			byte IMessage.Channel => Channel;
			bool IMessage.Timed => false;
			bool IMessage.Reliable => true;
			bool IMessage.Ordered => false;
			bool IMessage.Unique => true;
			
			public readonly byte Channel;

			public NetworkMessageRequest(byte channel) {
				Channel = channel;
			}

			void IWritable.Write(Writer writer) {
				writer.WriteEnum(Header.Request);
			}

		}

		private struct NetworkMessageSpawn : IMessage {

			HostTimestamp IMessage.Timestamp => Host.Now;
			byte IMessage.Channel => Channel;
			bool IMessage.Timed => false;
			bool IMessage.Reliable => true;
			bool IMessage.Ordered => false;
			bool IMessage.Unique => true;
			
			public readonly NetworkIdentity Identity;
			public readonly Vector3 Position;
			public readonly Quaternion Rotation;
			public readonly byte Channel;

			public NetworkMessageSpawn(NetworkIdentity identity, Vector3 position, Quaternion rotation, byte channel) {
				Identity = identity;
				Position = position;
				Rotation = rotation;
				Channel = channel;
			}

			void IWritable.Write(Writer writer) {
				writer.WriteEnum(Header.Spawn);
				writer.Write(Identity.Value);
				writer.Write(Position);
				writer.Write(Rotation);
			}

		}

		private struct NetworkMessagePrespawn : IMessage {

			HostTimestamp IMessage.Timestamp => Host.Now;
			byte IMessage.Channel => Channel;
			bool IMessage.Timed => false;
			bool IMessage.Reliable => true;
			bool IMessage.Ordered => false;
			bool IMessage.Unique => true;
			
			public readonly NetworkIdentity Identity;
			public readonly Vector3 Position;
			public readonly Quaternion Rotation;
			public readonly int Index;
			public readonly byte Channel;

			public NetworkMessagePrespawn(NetworkIdentity identity, Vector3 position, Quaternion rotation, int index, byte channel) {
				Identity = identity;
				Position = position;
				Rotation = rotation;
				Index = index;
				Channel = channel;
			}

			void IWritable.Write(Writer writer) {
				writer.WriteEnum(Header.Prespawn);
				writer.Write(Identity.Value);
				writer.Write(Position);
				writer.Write(Rotation);
				writer.Write(Index);
			}

		}

		private struct NetworkMessageDespawn : IMessage {

			HostTimestamp IMessage.Timestamp => Host.Now;
			byte IMessage.Channel => Channel;
			bool IMessage.Timed => false;
			bool IMessage.Reliable => true;
			bool IMessage.Ordered => false;
			bool IMessage.Unique => true;
			
			public readonly NetworkIdentity Identity;
			public readonly byte Channel;

			public NetworkMessageDespawn(NetworkIdentity identity, byte channel) {
				Identity = identity;
				Channel = channel;
			}

			void IWritable.Write(Writer writer) {
				writer.WriteEnum(Header.Despawn);
				writer.Write(Identity.Value);
			}

		}

	}

}
