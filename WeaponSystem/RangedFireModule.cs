using Balla.Equipment;
using Balla.Gameplay;
using UnityEngine;

namespace Balla
{
    public class RangedFireModule : MonoBehaviour
    {
        protected RangedWeapon weapon;
        internal virtual Vector3 MuzzlePoint => weapon.MuzzlePoint;
        internal virtual Vector3 FirePoint => weapon.MuzzlePoint;
        internal virtual RangedWeapon Weapon => weapon;
        private void Awake()
        {
            weapon = weapon != null ? weapon : GetComponent<RangedWeapon>();
        }

        internal virtual void Fire(Vector3 pos, Vector3 rot)
        {

        }

        internal virtual void PreFire()
        {

        }
        internal virtual void PostFire()
        {

        }
        internal virtual void FireSimulation()
        {

        }
    }
}
