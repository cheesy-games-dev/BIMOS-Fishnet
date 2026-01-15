using KadenZombie8.BIMOS.Rig;
using KadenZombie8.Pooling;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KadenZombie8.BIMOS.Networking {
    public class NetworkManager : Mirror.NetworkManager {
        public static NetworkManager Singleton {
            get; private set;
        }
        public Dictionary<ushort, BIMOSRig> RigCache {
            get; internal set;
        } = new();
        public bool autoPhysicsRigGrips = true;
        public PoolConfig rigsPoolConfig;
        public PoolConfig entitiesPoolConfig;
        public override void Awake() {
            base.Awake();
            InitializeOnce();
        }

        private void InitializeOnce() {
            SetProperties();
            Register();      
        }

        private void SetProperties() {
            Singleton = this;
            autoCreatePlayer = false;
            playerPrefab = null;
        }

        private void Register() {
            int layer = LayerMask.GetMask("BIMOSRig");
            Physics.IgnoreLayerCollision(layer, layer);
            PoolManager.Instance.RegisterPool(rigsPoolConfig);
            PoolManager.Instance.RegisterPool(entitiesPoolConfig);
            BIMOSRig.OnRigSpawned += RigSpawned;
            RegisterBroadcasts();
        }

        private void RegisterBroadcasts() {
            NetworkServer.RegisterHandler<SpawnRig>(SpawnRigHandler);
            NetworkClient.RegisterHandler<SpawnRig>(SpawnRigClients);
            NetworkServer.RegisterHandler<SyncRig>(SyncRigHandler);
            NetworkClient.RegisterHandler<SyncRig>(SyncRigClients);
        }

        private void SyncRigClients(SyncRig message) {
            if (message.Id == NetworkServer.localConnection.connectionId)
                return;
            if (RigCache.TryGetValue(message.Id, out var rig))
                message.Deserialize(rig);
            else
                SpawnRigClients(new(message.Id));
        }

        private void SyncRigHandler(NetworkConnectionToClient connection, SyncRig message) {
            message.Id = (ushort)connection.connectionId;
            NetworkServer.SendToAll(message);
        }

        private void SpawnRigClients(SpawnRig message) {
            var rig = PoolManager.Instance.AddToPool(rigsPoolConfig.PoolID, BIMOSRig.Instance.gameObject).GetComponent<BIMOSRig>();
            rig.ControllerRig.SetActive(false);
            rig.GetComponents<SettingsMenu>().ToList().ForEach(e => Destroy(e.gameObject));
            rig.GetComponents<EventSystem>().ToList().ForEach(e => Destroy(e.gameObject));
            rig.GetComponents<Camera>().ToList().ForEach(e => Destroy(e));
            if (autoPhysicsRigGrips) {
                rig.AddComponent<PhysicsRigGrips>().physicsRig = rig.PhysicsRig;
            }
            RigCache.Add(message.Id, rig);
        }

        private void SpawnRigHandler(NetworkConnectionToClient connection, SpawnRig message) {
            message.Id = (ushort)connection.connectionId;
            NetworkServer.SendToAll(message);
        }

        private void RigSpawned(object sender, BIMOSRig e) {
            var message = new SpawnRig();
            if (NetworkClient.ready)
                NetworkClient.Send(message);
            else
                NetworkClient.Ready();
        }

        private void FixedUpdate() {
            if(Keyboard.current.tabKey.isPressed) Cursor.lockState = CursorLockMode.None;
            if (!NetworkClient.active)
                return;
            SyncBIMOSRig();
        }

        private void SyncBIMOSRig() {
            var message = new SyncRig();
            message.Serialize(BIMOSRig.Instance);
            if (NetworkClient.ready)
                NetworkClient.Send(message);
            else
                NetworkClient.Ready();
        }
    }

    public struct SpawnRig : NetworkMessage {
        public ushort Id {
            get; set;
        }
        public SpawnRig(ushort id) {
            Id = id;
        }
    }

    public struct SyncRig : NetworkMessage {
        public ushort Id {
            get; set;
        }
        public Vector3[] poses;
        public Quaternion[] rotations;

        public void Serialize(BIMOSRig rig) {
            if (rig == null)
                return;
            poses = new Vector3[4];
            rotations = new Quaternion[4];
            poses[0] = rig.ControllerRig.Transforms.Camera.position;
            poses[1] = rig.ControllerRig.Transforms.LeftController.position;
            poses[2] = rig.ControllerRig.Transforms.RightController.position;
            poses[3] = rig.PhysicsRig.Rigidbodies.Pelvis.position;

            rotations[0] = rig.ControllerRig.Transforms.Camera.rotation;
            rotations[1] = rig.ControllerRig.Transforms.LeftController.rotation;
            rotations[2] = rig.ControllerRig.Transforms.RightController.rotation;
            rotations[3] = rig.PhysicsRig.Rigidbodies.Pelvis.rotation;
        }

        public void Deserialize(BIMOSRig rig) {
            if (rig == null)
                return;
            poses = new Vector3[4];
            rotations = new Quaternion[4];
            float t = 40 * Time.deltaTime;
            rig.ControllerRig.Transforms.Camera.position.Lerp(poses[0], t);
            rig.ControllerRig.Transforms.LeftController.position.Lerp(poses[1], t);
            rig.ControllerRig.Transforms.RightController.position.Lerp(poses[2], t);
            rig.PhysicsRig.Rigidbodies.Pelvis.position.Lerp(poses[3], t);

            rig.ControllerRig.Transforms.Camera.rotation.Lerp(rotations[0], t);
            rig.ControllerRig.Transforms.LeftController.rotation.Lerp(rotations[1], t);
            rig.ControllerRig.Transforms.RightController.rotation.Lerp(rotations[2], t);
            rig.PhysicsRig.Rigidbodies.Pelvis.rotation.Lerp(rotations[3], t);
        }
    }
}
