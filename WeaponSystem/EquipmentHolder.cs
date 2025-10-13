using Balla.Core;
using Balla.Entity;
using UnityEngine;

namespace Balla.Equipment
{
    public class EquipmentHolder : BallaNetScript
    {
        [SerializeField] internal BaseEntity entity;
        [SerializeField] internal Transform firearmShootPoint;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(!TryGetComponent(out entity))
            {
                Debug.LogWarning("no entity on this object!", gameObject);
            }

            foreach (var item in GetComponentsInChildren<BaseEquippable>())
            {
                item.GiveEquippable(this);
            }
        }
    }
}
