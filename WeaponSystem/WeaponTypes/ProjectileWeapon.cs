using Balla.Core;
using UnityEngine;

namespace Balla.Equipment
{
    public class ProjectileWeapon : RangedFireModule
    {
        [SerializeField] internal ProjectileData projectileData;
        internal int dataIndex = -1;
        internal override Vector3 FirePoint => ProjectileManager.FireFromMuzzle ? base.MuzzlePoint : weapon.holder.firearmShootPoint.position;
        /// <summary>
        /// The code executed on the server when firing.
        /// </summary>
        internal override void Fire(Vector3 pos, Vector3 dir)
        {
            base.Fire(pos, dir);
            var p = ProjectileManager.Instance.GetSingleProjectile(this, dir);
            p.transform.position = MuzzlePoint + pos;
            p.charge = weapon.CurrentCharge;
        }
        internal override void PreFire()
        {
            base.PreFire();
            if (dataIndex == -1)
            {
                dataIndex = ProjectileManager.Instance.projectileData.IndexOf(projectileData);
            }
            FireSimulation();
            PostFire();
        }
    }
}
