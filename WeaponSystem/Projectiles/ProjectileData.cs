using Unity.Netcode;
using UnityEngine;

namespace Balla
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "Weapon System/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        public float projectileSpeed;
        public float gravityMultiplier;
        public float maxDamage;
        public float minDamage;
        public AnimationCurve damageOverLife;
        public float maxLifetime;
        public float drag;
        public float radius;
        public bool cannotBounceOnEntity;
        public int maxBounces;
        public float bounciness;
        public ExplosionData explosionData;
    }
}
