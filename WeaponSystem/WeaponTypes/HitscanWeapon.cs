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
            HitscanManager.Instance.RequestHitscan(this, dir);
        }
        protected override void PreFire()
        {
            base.PreFire();
            if(dataIndex == -1)
            {
                dataIndex = HitscanManager.Instance.hitscanData.IndexOf(hitscanData);
            }
            FireSimulation(HitscanManager.FireFromMuzzle ? MuzzlePoint : holder.firearmShootPoint.position);
            PostFire();
        }

        protected override void FireSimulation(Vector3 pos)
        {
            base.FireSimulation(pos);
        }

    }
}
