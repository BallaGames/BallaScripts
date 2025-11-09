using UnityEngine;

namespace Balla.Equipment
{
    public class HitscanWeapon : RangedWeapon
    {
        protected override void Fire(Vector3 pos, Vector3 dir)
        {
            base.Fire(pos, dir);
        }
        protected override void PreFire()
        {
            base.PreFire();
            PostFire();
        }
    }
}
