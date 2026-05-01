using Balla.Projectile;
using System.Collections.Generic;
using UnityEngine;

namespace Balla.Core
{
    public class ProjectilePool
    {
        public ProjectilePool(ProjectileData data, int defaultCount)
        {
            this.data = data;
            aliveProjectiles = new();
            ID = nextID;
            nextID++;
        }

        public HashSet<int> aliveProjectiles;

        public List<Projectile.Projectile> Projectiles { get; private set; }
        static int nextID = 0;
        public int ID;
        protected ushort nextProjectileID;
        public ProjectileData data;
        public Projectile.Projectile GetSingleProjectile()
        {
            Projectile.Projectile proj = null;
            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (!Projectiles[i].alive)
                {
                    proj = Projectiles[i];
                    aliveProjectiles.Add(i);
                    break;
                }
            }
            if (proj == null)
            {
                //Create new ones, because we still have capacity.
                CreateProjectiles(out Projectile.Projectile[] projectilesSpawned);
                //Then pull the first one from this.
                proj = projectilesSpawned[0];
            }
            int index = Projectiles.IndexOf(proj);
            ProjectileManager.AddProjectile(proj);
            return proj;
        }

        /// <summary>
        /// Retrieves or creates N projectiles.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Projectile.Projectile[] GetMultipleProjectiles(int count)
        {
            //Create an array with capacity for the request
            Projectile.Projectile[] projectiles = new Projectile.Projectile[count];
            //Then check if we have enough projectiles without needing to make more.
            //If this part evaluates to true, then we should make more projectiles.
            int index = 0;
            if (Projectiles.Count < count)
            {
                //Whilst we have less un-used projectiles than we need, 
                while (Projectiles.Count - ProjectileManager.activeCount < count)
                {
                    CreateProjectiles(out Projectile.Projectile[] projectilesSpawned);
                    for (int i = 0; i < ProjectileManager.Instance.projectilesInChunk && index < count; i++, index++)
                    {
                        projectiles[index] = projectilesSpawned[i];
                    }
                }
            }
            return projectiles;
        }
        public void CreateProjectiles(out Projectile.Projectile[] projectilesSpawned)
        {
            projectilesSpawned = new Projectile.Projectile[ProjectileManager.Instance.projectilesInChunk];
            //then instantiate another chunk of projectiles
            for (int i = 0; i < projectilesSpawned.Length; i++)
            {
                projectilesSpawned[i] = Object.Instantiate(data.projectilePrefab, Vector3.zero, Quaternion.identity);
                projectilesSpawned[i].poolID = nextProjectileID;
                projectilesSpawned[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                nextProjectileID++;
            }
            Projectiles.AddRange(projectilesSpawned);
        }
        long _t;
        internal void CreateStartProjectiles(int projectileCount = 100)
        {
            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            Projectile.Projectile[] ps = new Projectile.Projectile[projectileCount];
            for (int i = 0; i < projectileCount; i++)
            {
                ps[i] = Object.Instantiate(data.projectilePrefab);
                ps[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
            for (int i = 0; i < ps.Length; i++)
            {
                nextProjectileID++;
                ps[i].poolID = nextProjectileID;
            }
            Projectiles = new(ps);
            stopwatch.Stop();
            Debug.Log($"Start Projectile creation took {stopwatch.ElapsedMilliseconds}. Completed in {System.DateTime.Now.Ticks - _t} from call..");
        }

    }
}
