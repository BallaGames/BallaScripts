using Balla.Core;
using Balla.Entity;
using Balla.Equipment;
using Balla.Gameplay.Player;
using Balla.UI;
using UnityEngine;

namespace Balla.Equipment
{
    public class EquipmentHolder : BallaScript
    {
        [SerializeField] internal BaseEntity entity;
        [SerializeField] internal Transform firearmShootPoint;
        [ReadOnly, SerializeField] protected Vector3 linearRecoilCurr, angularRecoilCurr;
        protected Vector3 linRecoilTarg, angRecoilTarg, linRecoilMax, angRecoilMax;
        public Transform recoilTransform;
        protected float recoilWaitTime, recoilReturnTime;
        protected bool recoilReturning;
        protected float recoilIntensity;
        /// <summary>
        /// Override this on all children to get the amount by which spread should be influenced.
        /// </summary>
        public virtual (float crouch, float move, bool air) SpreadInfluence => (0, 0, false);
        private void Start()
        {
            if (!TryGetComponent(out entity))
            {
                Debug.LogWarning("no entity on this object!", gameObject);
            }

            foreach (var item in GetComponentsInChildren<BaseEquippable>())
            {
                item.GiveEquippable(this);
            }
            Initialise();
        }
        protected override void OnFrame()
        {
            base.OnFrame();
        }
        protected virtual void Initialise()
        {

        }
        protected virtual void CalculateRecoil()
        {

        }
        public virtual void ReceiveRecoil()
        {

        }
    }
}
