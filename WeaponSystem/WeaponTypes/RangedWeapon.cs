using Balla.Core;
using Balla.Equipment;
using Balla.Projectile;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla.Equipment
{
    public class RangedWeapon : BaseWeapon
    {

        [SerializeField] protected Transform muzzle;
        [SerializeField, Tooltip("How many times this firearm will fire per minute")] protected int roundsPerMinute;
        [SerializeField, ReadOnly, Tooltip("The time between rounds, exposed to help developers.")] protected float timeBetweenRounds;
        [SerializeField, ReadOnly] protected float fireTimeIncrement;
        /// <summary>
        /// How long the weapon has currently waited before being able to fire again.
        /// </summary>
        float fireTimer;
        [SerializeField] protected bool canAutoFire;
        [SerializeField] protected bool usesBurstFire;
        [SerializeField] protected int shotsInBurst;
        [SerializeField] protected float timeBetweenBursts;
        [SerializeField, ReadOnly] protected bool fired;
        protected int burstRoundsFired;
        [SerializeField] protected bool muzzleWhenFiring;
        [SerializeField, ReadOnly] protected bool playingMuzzle;
        public VisualEffect muzzleEffect;
        public Vector3 MuzzlePoint => muzzle != null ? muzzle.position : Vector3.zero;
        protected bool CanFire => fireTimer >= timeBetweenRounds && (canAutoFire || !fired) && (!usesBurstFire || burstRoundsFired == 0);

        protected virtual void CycleLogic()
        {
            if (s_attackInput && CanFire)
            {
                PreFire();
                fired = true;
                //Use modulo to "carry over" fire timer if it exceeds the time between rounds.
                fireTimer %= timeBetweenRounds;
            }
            if (fireTimer < timeBetweenRounds)
            {
                fireTimer += Delta;
            }
        }

        protected override void Initialise(bool spawned)
        {
            base.Initialise(spawned);

            roundsPerMinute = Mathf.Clamp(roundsPerMinute, 0, 3000);
            timeBetweenRounds = 1f / (roundsPerMinute / 60f);
            fireTimeIncrement = timeBetweenRounds * Time.fixedDeltaTime;

        }
        /// <summary>
        /// Runs some logic before shooting and then shoots the weapon.
        /// Effects and audio should be played here on the local client.
        /// </summary>
        protected virtual void PreFire()
        {
            
        }

        protected override void Timestep()
        {
            if (IsOwner)
            {
                if(s_attackInput != attackInput || s_altAttackInput != altAttackInput)
                {
                    SendInput_RPC(attackInput, altAttackInput);
                    s_attackInput = attackInput; 
                    s_altAttackInput = altAttackInput;
                }
            }
            if (muzzleWhenFiring)
            {
                if(playingMuzzle != s_attackInput)
                {
                    if (s_attackInput && CanFire)
                    {
                        muzzleEffect.Play();
                    }
                    else
                    {
                        muzzleEffect.Stop();
                    }
                    playingMuzzle = s_attackInput;
                }
            }
            CycleLogic();
            if (!s_attackInput)
                fired = false;
        }
        /// <summary>
        /// This method is where the actual "fire" code is executed. Override this method for a weapon type's behaviour.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        protected virtual void Fire(Vector3 pos, Vector3 dir)
        {

        }
        protected virtual void PostFire()
        {
            if (muzzleEffect != null && !muzzleWhenFiring)
                muzzleEffect.Play();
               
        }
        protected void FireOnServer(Vector3 pos, Vector3 dir, double clientTime, RpcParams rpcParams = default)
        {
            Debug.Log($"Fired by Client {rpcParams.Receive.SenderClientId}, Local to Server Time Delta is {(NetworkManager.ServerTime.Time - clientTime):0.000000}");
            Fire(pos, dir);
        }


    }
}
