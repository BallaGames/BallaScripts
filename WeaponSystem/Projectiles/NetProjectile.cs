using Balla.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Balla.Projectile
{
    public class NetProjectile : BallaScript
    {

        internal ulong owner;
        [SerializeField] internal bool alive;
        
        [SerializeField] internal VisualEffect vfx;
        [SerializeField] internal MeshFilter meshFilter;
        [SerializeField] internal MeshRenderer meshRenderer;

        internal float LifeLerp => Mathf.InverseLerp(0, maxLife, currLife);
        [SerializeField] internal float maxLife, currLife;
        internal Vector3 velocity;
        internal Vector3 direction;
        internal float speed;
        internal Vector3 gravity;
        internal int bounces;
        internal Vector3 targetPos;
        [SerializeField] protected Renderer r;
        [SerializeField] internal ProjectileData data;
        internal int globalID = 0, poolID = 0;
        public static int NextGlobalID;

        public static Dictionary<int, NetProjectile> GlobalIDs;

        protected override void OnEnable()
        {
            
        }
        protected override void OnDisable()
        {
            
        }
        protected override void Timestep()
        {
            if (alive)
            {
                transform.forward = targetPos - transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, ProjectileManager.Instance.projectileLerpSpeed * Delta);
            }
        }
        private void Awake()
        {
            GlobalIDs ??= new Dictionary<int, NetProjectile>();

            if (GlobalIDs.TryAdd(NextGlobalID, this))
            {
                globalID = NextGlobalID;
                NextGlobalID++;
            }
            r.enabled = false;
        }
        /// <summary>
        /// Prepares the projectile for simulation on the server.
        /// </summary>
        /// <param name="simPos"></param>
        /// <param name="startPos"></param>
        /// <param name="forward"></param>
        /// <param name="entity"></param>
        /// <param name="dataIndex"></param>
        internal void SimSetup(Vector3 simPos, Vector3 startPos, Vector3 forward, ulong entity)
        {
            owner = entity;
            alive = true;
            currLife = 0;
            maxLife = data.maxLifetime;
            velocity = forward * data.projectileSpeed;
            gravity = Physics.gravity * data.gravityMultiplier;
            bounces = data.maxBounces;
            //Remember to initialise both the current position AND target position to the place we fire from.
            transform.position = startPos;
            targetPos = simPos;
            GameCore.Subscribe(this);
            r.enabled = true;
            if (vfx != null)
            {
                vfx.Play();
            }
            alive = true;
        }

        public void Terminate()
        {
            alive = false;
            ProjectileManager.activeCount--;
            if(vfx != null)
            {
                vfx.Stop();
            }
            r.enabled = false;
            Debug.Log("terminated projectile @ " + Time.time);
            GameCore.Unsubscribe(this);
        }
    }
}
