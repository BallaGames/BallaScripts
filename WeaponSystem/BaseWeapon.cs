using Balla.Core;
using Balla.Equipment;
using Balla.Projectile;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

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
        protected bool s_attackInput;
        protected bool s_altAttackInput;

        /// <summary>
        /// Used to find out which trigger should be used for the attack animation.
        /// </summary>
        /// <param name="triggerName"></param>
        protected virtual void GetAttackAnimation(out string triggerName)
        {
            triggerName = "null";
        }

        public override void OnSelect(BaseUseable previous)
        {
            base.OnSelect(previous);
        }
        public override void OnDeselect(BaseUseable next)
        {
            base.OnDeselect(next);
        }

        //Set the inputs for the weapon
        internal void SetAttackInput(bool input) => attackInput = input;
        internal void SetAltAttackInput(bool input) => altAttackInput = input;

        [Rpc(SendTo.NotOwner)]
        protected void SendInput_RPC(bool attack, bool altAttack)
        {
            s_attackInput = attack;
            s_altAttackInput = altAttack;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Initialise(true);
        }

        protected virtual void Initialise(bool spawned)
        {

        }
    }
}
