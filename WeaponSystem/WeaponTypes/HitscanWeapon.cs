using Balla.Gameplay;
using UnityEngine;

namespace Balla.Equipment
{
    public class HitscanWeapon : RangedWeapon
    {
        public HitscanData hitscanData;
        public int dataIndex = -1;

        protected override void Fire(Vector3 pos, Vector3 dir)
        {
            base.Fire(pos, dir);
        }
        protected override void PreFire()
        {
            base.PreFire();
            if(dataIndex == -1)
            {
                dataIndex = HitscanManager.Instance.hitscanData.IndexOf(hitscanData);
            }
            FireLogic();
            PostFire();
        }
        void FireLogic()
        {
            HitscanManager.Instance.RequestHitscan(this);
        }
    }
}
