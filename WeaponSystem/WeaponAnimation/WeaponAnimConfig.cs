using UnityEngine;

namespace Balla
{
    [CreateAssetMenu(fileName = "WeaponAnimConfig", menuName = "Weapon System/Animation/WeaponAnimConfig")]
    public class WeaponAnimConfig : ScriptableObject
    {
        [Header("Charging"), Tooltip("TRUE: the controller will blend from ChargeClipA to ChargeClipB whilst charging." +
            "\nFALSE: the controller will play a single animation (chargeClipA) which will be held until the weapon is fired.")]
        public bool blendOnCharge;
        public AnimationClip chargeClipA, chargeClipB;
        [Header("Attacks"), Tooltip("TRUE: this weapon's attacks can interrupt the current attack. This behaviour is best for fast-attacking weapons such as machine guns." +
            "\nFALSE: this weapon's attacks can NOT interrupt the current attack animation. This is the intended behaviour for melee weapons.")]
        public bool attackCanInterrupt;
        public AnimationClip attackClipA, attackClipB, attackClipC;

        public AnimationClip reloadStart, reloadFinish;

        [Header("Counted Reload")]
        public AnimationClip maxReload;
        public int maxReloadThreshold = 5;
        public AnimationClip medReload;
        public int medReloadThreshold = 3;
        public AnimationClip minReload;
        public int minReloadThreshold = 1;

        [Header("Instant Reload")]
        public AnimationClip emptyReload;
        public AnimationClip partReload;

        public AnimationClip weaponEquip, weaponSwitch;
    }
}
