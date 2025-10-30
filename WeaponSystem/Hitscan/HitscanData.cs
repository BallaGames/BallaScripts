using UnityEngine;

namespace Balla.Gameplay
{
    /// <summary>
    /// The HitscanData class contains the information required to fire a hitscan weapon.<br></br>
    /// This includes the prefab to instantiate into the pool, the tracer speed and the stats relating to damage.<br></br>
    /// Because of this, even if two weapons have identical visuals, they should have different HitscanData assigned to them.
    /// </summary>
    public class HitscanData : ScriptableObject
    {
        public GameObject tracerObject;
        public float tracerSpeed = 25f;
        public float damageAtMinRange = 15, damageAtMaxRange = 2;
        public float minRange = 15, maxRange = 2;
        public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 0, 1, 1);
        public ExplosionData explosionData;
    }
}
