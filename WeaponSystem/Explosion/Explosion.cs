using Balla.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Balla
{
    public class Explosion
    {

        ExplosionData data;
        public List<ExplosionEffect> explosions;
        int nextExplosionIndex;
        public Explosion(ExplosionData data, int capacity)
        {
            this.data = data;
            if(explosions != null && explosions.Count > 0)
            {
                for (int i = 0; i < explosions.Count; i++)
                {
                    if (explosions[i] != null)
                        Object.Destroy(explosions[i].gameObject);
                }
            }
            explosions = new List<ExplosionEffect>(new SizeCollection<ExplosionEffect>(capacity));
            for (int i = 0; i < capacity; i++)
            {
                explosions[i] = Object.Instantiate(data.explosionPrefab);
                explosions[i].gameObject.SetActive(false);
            }
        }

        public void GetExplosion(Vector3 position, Quaternion rotation)
        {
            explosions[nextExplosionIndex].transform.SetPositionAndRotation(position, rotation);
            explosions[nextExplosionIndex].gameObject.SetActive(true);
            explosions[nextExplosionIndex].PlayEffects();
            nextExplosionIndex++;
            nextExplosionIndex %= explosions.Count;
        }
    }
}
