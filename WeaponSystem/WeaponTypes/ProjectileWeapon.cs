using Balla.Core;
using Balla.Projectile;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla.Equipment
{
    public class ProjectileWeapon : RangedWeapon
    {
        [SerializeField] internal ProjectileData projectileData;
        internal int dataIndex = -1;

        /// <summary>
        /// The code executed on the server when firing.
        /// </summary>
        protected override void Fire(Vector3 pos, Vector3 dir)
        {
            base.Fire(pos, dir);
            var p = ProjectileManager.Instance.GetSingleProjectile(this);
            p.transform.position = pos;
        }
        protected override void PreFire()
        {
            if (IsServer)
            {
                if (dataIndex == -1)
                {
                    dataIndex = ProjectileManager.Instance.projectileData.IndexOf(projectileData);
                }
                FireOnServer(ProjectileManager.FireFromMuzzle ? muzzle.position : holder.firearmShootPoint.position, holder.firearmShootPoint.forward, NetworkManager.LocalTime.Time);
            }
            PostFire();
        }

        private void OnValidate()
        {
            //Clamp to upper limit of 3000, after which the time between rounds is less than fixed delta time.
            Initialise(false);
        }
    }
}
