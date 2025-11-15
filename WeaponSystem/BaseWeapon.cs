using Balla.Equipment;
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
        public Vector3 idleOffset;
        public Quaternion idleRotation;

        protected bool attackInput, altAttackInput;
        protected bool s_attackInput;
        protected bool s_altAttackInput;

        public Vector2 crosshairScaling;
        public virtual Vector2 CrosshairSize
        {
            get
            {
                return crosshairScaling;
            }
        }

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
    }
}
