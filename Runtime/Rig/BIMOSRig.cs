using KadenZombie8.BIMOS.Rig.Movement;
using System;
using UnityEngine;

namespace KadenZombie8.BIMOS.Rig
{
    [DefaultExecutionOrder(-1)]
    public class BIMOSRig : MonoBehaviour
    {
        public static EventHandler<BIMOSRig> OnRigSpawned;
        public static BIMOSRig Instance { get; private set; }

        public ControllerRig ControllerRig;
        public PhysicsRig PhysicsRig;
        public AnimationRig AnimationRig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;
            OnRigSpawned?.Invoke(this, this);
        }
    }
}