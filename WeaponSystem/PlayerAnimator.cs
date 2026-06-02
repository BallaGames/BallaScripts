using Balla.Core;
using Balla.Equipment;
using Balla.Gameplay.Player;
using UnityEngine;

namespace Balla
{
    public class PlayerAnimator : BallaScript
    {
        public PlayerEquipment pe;
        public PlayerController pc;

        public Animator animator;

        private void Start()
        {
            if(pe == null)
            {
                pe = GetComponent<PlayerEquipment>();
            }

            if(pe != null)
            {
                pe.OnReceivedFire += RecievedFire;
                pe.OnReloadStart += StartReload;
                pe.OnReloadEnd += EndReload;
            }
        }

        void RecievedFire()
        {
            animator.SetTrigger("Fire");
            pe.CurrentWeapon.animator.SetTrigger(pe.CurrentWeapon.GetAttackAnimation());
        }
        
        void StartReload()
        {
            string key = pe.CurrentWeapon.CurrentAmmo == 0 ? "EReload" : "PReload";
            animator.SetTrigger(key);
            pe.CurrentWeapon.animator.SetTrigger(key);
        }
        void EndReload()
        {

        }
    }
}
