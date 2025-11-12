using Balla.Gameplay;
using Balla.Projectile;
using System.Collections.Generic;
using UnityEngine;

namespace Balla.Core
{
    public class TracerPool
    {
        public TracerPool(HitscanData data)
        {
            this.data = data;
            aliveTracers = new();
            ID = nextID;
            nextID++;
        }

        public HashSet<int> aliveTracers;

        public List<HitscanTracer> Tracers { get; private set; }
        static int nextID = 0;
        public int ID;
        protected ushort nextProjectileID;
        public HitscanData data;
        public HitscanTracer GetSingleTracer()
        {
            HitscanTracer t = null;
            for (int i = 0; i < Tracers.Count; i++)
            {
                if (!Tracers[i].alive)
                {
                    t = Tracers[i];
                    aliveTracers.Add(i);
                    break;
                }
            }
            if (t == null)
            {
                //Create new ones, because we still have capacity.
                CreateTracers(out HitscanTracer[] spawnedTracers);
                //Then pull the first one from this.
                t = spawnedTracers[0];
            }
            return t;
        }

        /// <summary>
        /// Retrieves or creates N projectiles.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public HitscanTracer[] GetMultipleProjectiles(int count)
        {
            //Create an array with capacity for the request
            HitscanTracer[] tracers = new HitscanTracer[count];
            //Then check if we have enough projectiles without needing to make more.
            //If this part evaluates to true, then we should make more projectiles.
            int index = 0;
            if (Tracers.Count < count)
            {
                //Whilst we have less un-used projectiles than we need, 
                while (Tracers.Count - ProjectileManager.activeCount < count)
                {
                    CreateTracers(out HitscanTracer[] tracersOut);
                    for (int i = 0; i < ProjectileManager.Instance.projectilesInChunk && index < count; i++, index++)
                    {
                        tracers[index] = tracersOut[i];
                    }
                }
            }
            return tracers;
        }
        public void CreateTracers(out HitscanTracer[] tracersOut)
        {
            tracersOut = new HitscanTracer[ProjectileManager.Instance.projectilesInChunk];
            //then instantiate another chunk of projectiles
            for (int i = 0; i < tracersOut.Length; i++)
            {
                tracersOut[i] = Object.Instantiate(data.tracerObject, Vector3.zero, Quaternion.identity);
                tracersOut[i].poolID = nextProjectileID;
                tracersOut[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
                nextProjectileID++;
            }
            Tracers.AddRange(tracersOut);
        }
        internal void CreateStartTracers(int traceCount = 100)
        {
            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            HitscanTracer[] tracers = new HitscanTracer[traceCount];
            for (int i = 0; i < traceCount; i++)
            {
                tracers[i] = Object.Instantiate(data.tracerObject);
            }
            for (int i = 0; i < tracers.Length; i++)
            {
                nextProjectileID++;
                tracers[i].poolID = nextProjectileID;
            }
            Tracers = new(tracers);
            stopwatch.Stop();
            Debug.Log($"Start Projectile creation took {stopwatch.ElapsedMilliseconds}");
        }

    }
}
