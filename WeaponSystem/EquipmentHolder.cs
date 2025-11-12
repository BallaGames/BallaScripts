using Balla.Core;
using Balla.Entity;
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
