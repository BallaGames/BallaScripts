using UnityEngine;
using UnityEngine.VFX;

namespace Balla
{
    [CreateAssetMenu(fileName = "ExplosionData", menuName = "Weapon System/ExplosionData")]
    public class ExplosionData : ScriptableObject
    {
        public ExplosionEffect explosionPrefab;
        public float radius;
        public float maxDamage, selfDamageMult, enviroDamageMult;
        public AnimationCurve damageFalloff;
    }
}
