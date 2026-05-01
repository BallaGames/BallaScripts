using Balla.Projectile;
using UnityEngine;

namespace Balla
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "Weapon System/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        public Projectile.Projectile projectilePrefab;
        public ushort projectileID;


        public float projectileSpeed;
        public float minChargeSpeedMult = 0.5f;
        public float gravityMultiplier;
        public float maxDamage;
        public float minDamage;
        public float minChargeDamageMult = 0.5f;
        public AnimationCurve damageOverLife;
        public float maxLifetime;
        public float drag;
        public float radius;
        public bool cannotBounceOnEntity;
        public int maxBounces;
        public float bounciness;
        public bool explodeOnExpire;
        public ExplosionData explosionData;

    }
}
