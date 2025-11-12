using Balla.Core;
using UnityEngine;

namespace Balla.Entity
{
    public class Breakable : BaseEntity
    {
        public GameObject breakPrefab;
        public ExplosionData explosionData;
        internal override void ModifyHealth(float healthDelta, Vector3 soucePos = default, Vector3 sourceDir = default)
        {
            base.ModifyHealth(healthDelta, soucePos, sourceDir);
        }
        protected override void Die()
        {
            base.Die();
            Destroy(gameObject);
            if(explosionData != null)
            {
                ExplosionManager.Instance.RequestExplosion(explosionData, transform.position, transform.eulerAngles, entityID);
            }
        }
    }
}
