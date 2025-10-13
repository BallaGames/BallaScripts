using UnityEngine;
using UnityEngine.VFX;

namespace Balla
{
    [CreateAssetMenu(fileName = "ExplosionData", menuName = "Weapon System/ExplosionData")]
    public class ExplosionData : ScriptableObject
    {
        public float radius;
        public VisualEffectAsset effect;
        public float maxDamage, selfDamageMult, enviroDamageMult;
        public AnimationCurve damageFalloff;
    }
}
