using Balla.Core;
using Balla.Entity;
using UnityEngine;

namespace Balla.Equipment
{
    public class EquipmentHolder : BallaScript
    {
        [SerializeField] internal BaseEntity entity;
        [SerializeField] internal Transform firearmShootPoint;

        private void Start()
        {
            if (!TryGetComponent(out entity))
            {
                Debug.LogWarning("no entity on this object!", gameObject);
            }

            foreach (var item in GetComponentsInChildren<BaseEquippable>())
            {
                item.GiveEquippable(this);
            }
            Initialise();
        }
        protected virtual void Initialise()
        {

        }
    }
}
