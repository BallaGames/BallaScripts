using Balla.Core;
using System;
using UnityEngine;

namespace Balla.Equipment
{
    /// <summary>
    /// Base class for anything that can be equipped to the player.
    /// </summary>
    public abstract class BaseEquippable : BallaNetScript
    {
        //Actions for various things that can be subscribed to.
        //Supplies the gameobject in case its needed.
        public Action<BaseEquippable> equipped, unequipped, used, stopped;

        [SerializeField, ReadOnly] internal EquipmentHolder holder;
        public virtual void GiveEquippable(EquipmentHolder target)
        {
            holder = target;
        }

        public virtual void OnEquip()
        {
            equipped?.Invoke(this);
        }
        public virtual void OnUnequip()
        {
            unequipped?.Invoke(this);
        }
        public virtual void OnUse()
        {
            used?.Invoke(this);
        }
        public virtual void OnStop()
        {
            stopped?.Invoke(this);
        }
    }
}
