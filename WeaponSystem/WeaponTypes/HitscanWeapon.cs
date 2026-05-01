using Balla.Gameplay;
using UnityEngine;

namespace Balla.Equipment
{
    public class HitscanWeapon : RangedFireModule
    {
        public HitscanData hitscanData;
        public int dataIndex = -1;
        internal override Vector3 FirePoint => HitscanManager.FireFromMuzzle ? base.MuzzlePoint : weapon.holder.firearmShootPoint.position;

        internal override void Fire(Vector3 pos, Vector3 dir)
        {
            base.Fire(pos, dir);
            HitscanManager.Instance.RequestHitscan(this, dir);
        }
        internal override void PreFire()
        {
            base.PreFire();
            if(dataIndex == -1)
            {
                dataIndex = HitscanManager.Instance.hitscanData.IndexOf(hitscanData);
            }
            FireSimulation();
            PostFire();
        }
    }
}
