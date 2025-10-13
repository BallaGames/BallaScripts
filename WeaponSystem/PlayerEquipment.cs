using Balla.Core;
using UnityEngine;

namespace Balla.Equipment
{
    public class PlayerEquipment : EquipmentHolder
    {
        public BaseWeapon weapon;

        protected override void Timestep()
        {
            
            if(weapon != null)
            {
                weapon.SetAttackInput(Input.attack);
                weapon.SetAltAttackInput(Input.altAttack);
            }
        }
    }
}
