using Balla.Core;
using Balla.Projectile;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla.Equipment
{
    public class ProjectileWeapon : BaseWeapon
    {

        [SerializeField] protected Transform muzzle;
        public Vector3 MuzzlePoint => muzzle != null ? muzzle.position : Vector3.zero;
        protected bool s_attackInput, s_altAttackInput;
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
        protected bool CanFire => fireTimer >= timeBetweenRounds && (canAutoFire || !fired) && (!usesBurstFire || burstRoundsFired == 0);

        [SerializeField] protected bool muzzleWhenFiring;
        [SerializeField, ReadOnly] protected bool playingMuzzle;
        [SerializeField] internal ProjectileData projectileData;
        internal int projDataIndex = -1;
        public VisualEffect muzzleEffect;

        protected virtual void Initialise(bool spawned)
        {
            roundsPerMinute = Mathf.Clamp(roundsPerMinute, 0, 3000);
            timeBetweenRounds = 1f / (roundsPerMinute / 60f);

        }
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
                    if (s_attackInput)
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
            if (!canAutoFire)
            {
                fired = !s_attackInput;
            }
        }
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
        /// <summary>
        /// Runs some logic before shooting and then shoots the weapon.
        /// Effects and audio should be played here on the local client.
        /// </summary>
        protected virtual void PreFire()
        {
            if (IsServer)
            {
                if (projDataIndex == -1)
                {
                    projDataIndex = ProjectileManager.Instance.projectileData.IndexOf(projectileData);
                }
                FireOnServer(ProjectileManager.FireFromMuzzle ? muzzle.position : holder.firearmShootPoint.position, holder.firearmShootPoint.forward, NetworkManager.LocalTime.Time);
            }
            PostFire();
        }
        /// <summary>
        /// The code executed on the server when firing.
        /// </summary>
        protected virtual void Fire(Vector3 pos, Vector3 dir)
        {
            var p = ProjectileManager.Instance.GetSingleProjectile(this);
            p.transform.position = pos;
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
            if(Physics.Raycast(pos, dir, out RaycastHit hit, 100))
            {
                Debug.DrawLine(pos, hit.point, Color.green, 0.5f);
            }
            else
            {
                Debug.DrawRay(pos, dir * 100, Color.red, 0.5f);
            }
        }



        private void OnValidate()
        {
            //Clamp to upper limit of 3000, after which the time between rounds is less than fixed delta time.
            Initialise(false);
        }
    }
}
