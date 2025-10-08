using Balla.Core;
using Unity.Netcode;
using UnityEngine;

namespace Balla.Equipment
{
    /// <summary>
    /// Base Weapon does not implement any proper logic, but does leave some useful fields to the programmer.
    /// </summary>
    public abstract class BaseWeapon : BaseUseable
    {
        [SerializeField] protected int ammunition;
        [SerializeField] protected int ammoPerAttack;


        protected bool attackInput, altAttackInput;

        /// <summary>
        /// Used to find out which trigger should be used for the attack animation.
        /// </summary>
        /// <param name="triggerName"></param>
        protected virtual void GetAttackAnimation(out string triggerName)
        {
            triggerName = "null";
        }
        //Set the inputs for the weapon
        internal void SetAttackInput(bool input) => attackInput = input;
        internal void SetAltAttackInput(bool input) => altAttackInput = input;
    }
}
