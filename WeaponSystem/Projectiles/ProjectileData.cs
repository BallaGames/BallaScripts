using Balla.Core;
using Balla.Projectile;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "Weapon System/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        public NetProjectile projectilePrefab;
        public ushort projectileID;


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
        public bool explodeOnExpire;
        public ExplosionData explosionData;


        public VisualEffectAsset vfxAsset;
        public Mesh projectileMesh;
        public Material meshMaterial;
        public Vector3 meshScale, meshPos, meshRotation;
        







    }
}
