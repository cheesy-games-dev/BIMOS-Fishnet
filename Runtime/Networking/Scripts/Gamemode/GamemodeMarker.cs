using System.Collections.Generic;
using UnityEngine;

namespace KadenZombie8.BIMOS
{
    public class GamemodeMarker : MonoBehaviour
    {
        public static List<GamemodeMarker> Markers { get; set; } = new();

        private void OnEnable() {
            Markers.Add(this);
        }
        private void OnDisable() {
            Markers.Remove(this);
        }

        public static void SpawnPlayer(GamemodePlayer player, int position = -1) {
            TeleportToSpawnPoint(player, Markers.GetRandomItem().transform);
        }

        public static Transform TeleportToSpawnPoint(GamemodePlayer player, Transform spawnPoint) {
            var _player = player.rig;
            _player.PhysicsRig.GrabHandlers.Left.AttemptRelease();
            _player.PhysicsRig.GrabHandlers.Right.AttemptRelease();

            var rigidbodies = spawnPoint.GetComponentsInChildren<Rigidbody>();
            var rootPosition = _player.PhysicsRig.Rigidbodies.LocomotionSphere.position;
            foreach (var rigidbody in rigidbodies) {
                var offset = rigidbody.position - rootPosition; //Calculates the offset between the locoball and the rigidbody
                rigidbody.position = spawnPoint.position + offset; //Sets the rigidbody's position
                rigidbody.transform.position = spawnPoint.position + offset; //Sets the transform's position

                if (rigidbody.isKinematic)
                    continue;

                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            //Update the animation rig's position
            _player.AnimationRig.Transforms.Hips.position += spawnPoint.position - rootPosition;

            //Move the player's animated feet to the new position
            _player.AnimationRig.Feet.TeleportFeet();

            _player.ControllerRig.transform.rotation = spawnPoint.rotation;
            return spawnPoint;
        }
    }
}
