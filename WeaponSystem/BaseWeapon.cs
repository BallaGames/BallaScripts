using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla.Equipment
{
    /// <summary>
    /// Base Weapon does not implement any proper logic, but does leave some useful fields to the programmer.
    /// </summary>
    public abstract class BaseWeapon : BaseUseable
    {
        [SerializeField] protected int maxAmmo;
        [SerializeField, ReadOnly] protected int currentAmmo;
        [SerializeField] protected int ammoPerAttack;

        public bool UseCharge => useCharge;
        public float CurrentCharge => currentCharge;
        [SerializeField] internal bool useCharge, mustChargeToFull, dumpChargeOnAttack;
        [SerializeField, ReadOnly] internal bool isCharged, isCharging;
        [SerializeField, ReadOnly] protected float currentCharge;
        [SerializeField] protected float chargeRate, chargeDecay;
        [SerializeField] internal float minCharge, fireRateMultAtMinCharge;

        public bool UseHeat => useHeat;
        public (float curr, float max, float lerp) HeatLevel => (currentHeat, maxHeat, Mathf.InverseLerp(0, maxHeat, currentHeat));
        [SerializeField] internal bool useHeat;
        [SerializeField, ReadOnly] internal float currentHeat;
        [SerializeField, ReadOnly] internal bool isOverheated;
        [SerializeField] internal VisualEffect[] overheatEffect;
        [SerializeField] internal string heatLevelParamName;
        [SerializeField, ReadOnly] internal int heatLevelParamID;
        [SerializeField] internal float maxHeat, heatPerAttack, heatDecay, overheatTime, overheatDecay;

        [SerializeField] internal bool forceCoolAfterAttack;
        [SerializeField, ReadOnly] internal bool forcedCooling; 
        [SerializeField] internal float forcedCoolTime;

        public Vector3 idleOffset;
        public Quaternion idleRotation;

        protected bool attackInput, altAttackInput;
        protected bool s_attackInput;
        protected bool s_altAttackInput;

        public Action OnWeaponFire, OnForceCooldown, OnOverheat;
        public Action WeaponWindupStarted, WeaponWindupComplete;
        public Action OnReloadStart, OnReloadEnd, OnAmmunitionRestored;
        public Action<float> OnChargeHold;
        public Action<int> OnAmmoChanged;

        public Vector2 crosshairScaling;
        public virtual Vector2 CrosshairSize
        {
            get
            {
                return crosshairScaling;
            }
        }
        protected virtual bool CanAttack => !forcedCooling && !isOverheated;
        protected virtual bool ChargeReady => !useCharge || (isCharged && !isCharging);

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
            Show(true);
        }
        public override void OnDeselect(BaseUseable next)
        {
            base.OnDeselect(next);
            SetAttackInput(false);
            SetAltAttackInput(false);
            Show(false);
        }

        //Set the inputs for the weapon
        internal void SetAttackInput(bool input) => attackInput = input;
        internal void SetAltAttackInput(bool input) => altAttackInput = input;

        protected override void Init()
        {
            base.Init();
            InitialiseWeapon(true);
        }

        protected virtual void InitialiseWeapon(bool spawned)
        {

        }
        private void OnValidate()
        {
            InitialiseWeapon(false);
        }

        protected virtual IEnumerator ForceCharge()
        {
            while (currentCharge < 1)
            {
                ModifyCharge(chargeRate * Delta);
                yield return new WaitForFixedUpdate();
            }
        }
        protected virtual IEnumerator ForceCooldown()
        {
            forcedCooling = true;
            if (!isOverheated)
            {
                overheatEffect.Play();
            }
            yield return new WaitForSeconds(forcedCoolTime);
            forcedCooling = false;
        }
        protected void ModifyCharge(float delta)
        {
            currentCharge = Mathf.Clamp01(currentCharge + delta);
            isCharged = currentCharge >= minCharge;
        }
    }
}
