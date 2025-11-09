using Balla.Core;
using UnityEngine;

namespace Balla.Equipment
{
    public class PlayerEquipment : EquipmentHolder
    {
        public BaseWeapon CurrentWeapon => weapons[weaponIndex];
        public int weaponIndex;
        public BaseWeapon[] weapons;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Input.SubscribeToActionPerform(Input.actions.Player.Next, NextWeapon);
            Input.SubscribeToActionPerform(Input.actions.Player.Previous, PreviousWeapon);
            weapons ??= new BaseWeapon[4];
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    if (i == 0)
                        weapons[i].OnSelect(null);
                    else
                        weapons[i].OnDeselect(null);
                }
            }
        }
        void PreviousWeapon()
        {
            SwitchWeapons(false);
        }
        void NextWeapon()
        {
            SwitchWeapons(true);
        }
        void SwitchWeapons(bool next)
        {
            if(weapons.Length <= 1)
            {
                //we have one or less weapons, so we shouldn't try to switch. back out early.
                return;
            }
            //Set the weapon's inputs to false
            var oldWeapon = CurrentWeapon;
            //Increment/Decrement the weapon index and then mod it to prevent it going out of bounds
            weaponIndex += next ? 1 : -1;
            weaponIndex = (int)Mathf.Repeat(weaponIndex, weapons.Length);
            //Then call our select and deselect methods.
            oldWeapon.OnDeselect(CurrentWeapon);
            CurrentWeapon.OnSelect(oldWeapon);
            oldWeapon.OnUnequip();
            CurrentWeapon.OnEquip();
        }

        protected override void Timestep()
        {
           
            if(CurrentWeapon != null)
            {
                CurrentWeapon.SetAttackInput(Input.attack);
                CurrentWeapon.SetAltAttackInput(Input.altAttack);
            }
        }
    }
}
