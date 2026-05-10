using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla.Equipment
{
    public class RangedWeapon : BaseWeapon
    {

        [SerializeField] protected Transform muzzle;
        [SerializeField, Tooltip("How many times this firearm will fire per minute")] protected int roundsPerMinute;
        [SerializeField, ReadOnly, Tooltip("The time between rounds, exposed to help developers.")] protected float timeBetweenRounds;
        /// <summary>
        /// How long the weapon has currently waited before being able to fire again.
        /// </summary>
        float fireTimer;
        public int shotsPerAttack = 1;
        [SerializeField] protected bool canAutoFire;
        [SerializeField] protected bool usesBurstFire;
        [SerializeField] protected int shotsInBurst;
        [SerializeField] protected bool doingBurstFire;
        [SerializeField] protected float timeBetweenBursts;
        [SerializeField, ReadOnly] protected bool fired;
        protected int burstRoundsFired;
        [SerializeField] protected bool muzzleWhenFiring;
        [SerializeField, ReadOnly] protected bool playingMuzzle;
        public VisualEffect muzzleEffect;
        public Vector3 MuzzlePoint => muzzle != null ? muzzle.position : Vector3.zero;

        public RecoilData recoilData;

        [SerializeField, ReadOnly] internal float currentSpread;
        [SerializeField, ReadOnly] internal float aimSpreadModify; 
        public bool useSpread;
        [Tooltip("If true, the weapon will fire each shot an even step apart from each other Currently only works on one axis.")]
        public bool useEvenSpread;
        [SerializeField] protected float baseSpread = 1, crouchSpreadMult = 0.5f, moveSpreadMult = 1.5f, airSpreadMult = 1.6f;
        [SerializeField] protected bool spreadBeforeShot;
        [SerializeField] protected float evenSpreadAmount;
        [SerializeField] protected float maxSpreadAngle;
        [SerializeField] protected float spreadDecay;
        [SerializeField] protected float spreadPerShot;
        [SerializeField] protected Vector2 spreadScale;
        protected override bool CanAttack => base.CanAttack && fireTimer >= timeBetweenRounds 
            && (canAutoFire || !fired) 
            && (!usesBurstFire || burstRoundsFired == 0);
        float spreadModifier;
        public override Vector2 CrosshairSize
        {
            get
            {
                if (!useSpread)
                {
                    return crosshairScaling;
                }
                if (useEvenSpread)
                {
                    return crosshairScaling * new Vector2(1 + evenSpreadAmount, 1);
                }
                else
                {
                    return (1 + currentSpread) * spreadModifier * crosshairScaling;
                }
            }
        }
        private void CalculateSpread()
        {
            var (crouch, move, aim, air) = holder.SpreadInfluence;
            aimAmount = aim;
            spreadModifier = baseSpread * (air ? airSpreadMult : Mathf.Lerp(1, crouchSpreadMult, crouch) * Mathf.Lerp(1, moveSpreadMult, move));
            aimSpreadModify = Mathf.Lerp(1, aimSpreadMultiplier, aim);
        }

        protected virtual void CycleLogic()
        {
            if (s_attackInput)
            {
                if (CanAttack)
                {
                    if (useCharge)
                    {
                        if (mustChargeToFull)
                        {
                            if (!isCharging)
                            {
                                StartCoroutine(ForceCharge());
                            }
                        }
                        else
                        {
                            ModifyCharge(Delta * chargeRate);
                        }
                    }
                    if(!mustChargeToFull && ChargeReady)
                    {
                        PreFire();
                    }
                }
            }
            else
            {
                if (useCharge && !isCharging)
                {
                    ModifyCharge(-Delta * chargeDecay);
                }
            }
            if (fireTimer < timeBetweenRounds)
            {
                if (useCharge)
                {
                    fireTimer += Delta * Mathf.Lerp(fireRateMultAtMinCharge, 1, currentCharge);
                }
                else
                {
                    fireTimer += Delta;
                }

            }
        }

        protected override void InitialiseWeapon(bool spawned)
        {
            base.InitialiseWeapon(spawned);

            roundsPerMinute = Mathf.Clamp(roundsPerMinute, 0, 3000);
            timeBetweenRounds = 1f / (roundsPerMinute / 60f);

            if (fireModule == null)
                fireModule = GetComponent<RangedFireModule>();
        }
        /// <summary>
        /// Runs some logic before shooting and then shoots the weapon.
        /// Effects and audio should be played here on the local client.
        /// </summary>
        protected virtual void PreFire()
        {
            fired = true;
            //Use modulo to "carry over" fire timer if it exceeds the time between rounds.
            if (usesBurstFire)
            {
                if (!doingBurstFire)
                {
                    doingBurstFire = true;
                    StartCoroutine(BurstFire());
                }
            }
            else
            {
                fireTimer %= timeBetweenRounds;
                fireModule.PreFire();
                FireSimulation();
            }
        }

        IEnumerator BurstFire()
        {
            
            while (burstRoundsFired < shotsInBurst)
            {
                FireSimulation();
                burstRoundsFired++;
                yield return new WaitForSeconds(timeBetweenRounds);
            }
            yield return new WaitForSeconds(timeBetweenBursts);
            burstRoundsFired = 0;
            doingBurstFire = false;
            if (canAutoFire)
            {
                fired = false;
            }
        }

        protected override void Timestep()
        {
            base.Timestep();

            CalculateSpread();
            if(s_attackInput != attackInput || s_altAttackInput != altAttackInput)
            {
                s_attackInput = attackInput; 
                s_altAttackInput = altAttackInput;
            }
            if (muzzleWhenFiring)
            {
                if(playingMuzzle != s_attackInput)
                {
                    if (s_attackInput && CanAttack)
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
            {
                if(fired && forceCoolAfterAttack)
                {
                    StartCoroutine(ForceCooldown());
                }
                fired = false;
            }

            if (useSpread)
            {
                currentSpread = Mathf.Clamp(currentSpread - (spreadDecay * Delta), 0, maxSpreadAngle);
            }
            if (useHeat)
            {

                if (isOverheated)
                {
                    currentHeat = Mathf.Max(currentHeat - (overheatDecay * Delta), 0);
                    if(currentHeat <= 0)
                    {
                        isOverheated = false;
                    }
                }
                else
                {
                    currentHeat = Mathf.Max(currentHeat - (heatDecay * Delta), 0);
                }
            }
        }

        /// <summary>
        /// This method is where the actual "fire" code is executed. Override this method for a weapon type's behaviour.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        protected virtual void Fire(Vector3 pos, Vector3 dir)
        {
            fireModule.Fire(pos, dir);
            PostFire();
        }
        protected virtual void PostFire()
        {
            if (muzzleEffect != null && !muzzleWhenFiring)
                muzzleEffect.Play();

            fireModule.PostFire();
        }
        protected virtual void FireSimulation()
        {
            if (useAmmo)
            {
                currentAmmo -= ammoPerAttack;
            }


            if (holder != null)
            {
                holder.ReceiveRecoil();
            }
            if (useSpread)
            {
                if (spreadBeforeShot)
                {
                    if (!useEvenSpread)
                    {
                        currentSpread = Mathf.Min(currentSpread + spreadPerShot, maxSpreadAngle);
                    }
                }

                Vector2 spreadAngle = Vector2.zero;
                for (int i = 0; i < shotsPerAttack; i++)
                {
                    Vector3 dir = Vector3.forward;
                    if (useEvenSpread)
                    {
                        spreadAngle.y = ((Mathf.InverseLerp(0, shotsPerAttack-1, i) * 2) - 1) * evenSpreadAmount * (aimAffectsEvenSpread ? Mathf.Lerp(1, aimSpreadMultiplier, aimAmount) : 1);

                    }
                    else
                    {
                        spreadAngle = (baseSpread + ((currentSpread * maxSpreadAngle)) * spreadModifier * aimSpreadModify) * (UnityEngine.Random.insideUnitCircle * spreadScale);
                    }
                    dir = Quaternion.Euler(spreadAngle.x, spreadAngle.y, 0) * dir;
                    Fire(Vector3.zero, holder.firearmShootPoint.rotation * dir);
                }

                if (!spreadBeforeShot)
                {
                    if (!useEvenSpread)
                    {
                        currentSpread = Mathf.Min(currentSpread + spreadPerShot, maxSpreadAngle);
                    }
                }
            }
            else
            {
                for (int i = 0; i < shotsPerAttack; i++)
                {
                    Fire(Vector3.zero, holder.firearmShootPoint.forward);
                }
            }
            fireModule.FireSimulation();
            if (useHeat)
            {
                currentHeat = Mathf.Min(currentHeat + heatPerAttack, maxHeat);
                if(currentHeat >= maxHeat)
                {
                    isOverheated = true;
                    overheatEffect.Play();
                }
            }

            if(useCharge && dumpChargeOnAttack)
            {
                currentCharge = 0;
                s_attackInput = false;
                fired = false;
            }
        }
        protected override IEnumerator ForceCharge()
        {
            isCharging = true;
            while (currentCharge < 1)
            {
                ModifyCharge(chargeRate * Delta);
                yield return new WaitForFixedUpdate();
            }
            PreFire();
            if (usesBurstFire)
            {
                yield return new WaitUntil(() => doingBurstFire);
            }
            isCharging = false;
            yield break;
        }

        protected virtual void OnValidate()
        {
            //Clamp to upper limit of 3000, after which the time between rounds is less than fixed delta time.
            InitialiseWeapon(false);
        }
    }
}
