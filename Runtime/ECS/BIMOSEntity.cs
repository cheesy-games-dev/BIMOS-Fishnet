using KadenZombie8.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KadenZombie8.BIMOS.Entity
{
    public class BIMOSEntity : MonoBehaviour
    {
        public static Dictionary<ushort, BIMOSEntity> Pool { get; set; } = new();
        public static ushort NextPoolID { get; set; } = 0;
        
        public ushort PoolId { get; set; } = 0;
        public bool Poolee { get; set; } = true;
        public List<BIMOSBody> bodies = new();

        private void Awake() {
            PoolId = 0;
            bodies = GetComponentsInChildren<BIMOSBody>().ToList();
        }

        private void OnEnable() {
            AllocateEntity(this);
        }
        public static void AllocateEntity(BIMOSEntity entity) {
            if (!entity.Poolee || entity.PoolId > 0)
                return;
            entity.PoolId = NextPoolID;
            NextPoolID++;
        }
        public static void DeallocateEntity(ushort id) {
            bool found = Pool.TryGetValue(id, out BIMOSEntity entity);
            if (!found) return;
            Pool.Remove(id);
        }

        private void OnDisable() {
            DeallocateEntity(PoolId);
        }
    }
}
