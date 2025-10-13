using Balla.Core;
using Balla.Entity;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Balla.Projectile
{
    public class NetProjectile : BallaNetScript
    {

        internal ulong owner;
        [SerializeField] internal bool alive;
        internal float LifeLerp => Mathf.InverseLerp(0, maxLife, currLife);
        [SerializeField] internal float maxLife, currLife;
        internal Vector3 velocity;
        internal Vector3 direction;
        internal float speed;
        internal Vector3 gravity;
        internal int bounces;
        [SerializeField] protected Renderer r;
        internal Vector3 targetPos;
        internal ProjectileData data;
        internal ushort ID = 0;

        public static Dictionary<ushort, NetProjectile> ProjectileIDs;

        public static void SetProjectilePosition(ushort ID, Vector3 pos)
        {
            if (ProjectileIDs.ContainsKey(ID))
                ProjectileIDs[ID].ReceiveNewPosition(pos);
            else
                Debug.Log("Failed to get Projectile from ID");
        }

        public void ReceiveNewPosition(Vector3 pos)
        {
            targetPos = pos;
        }
        protected override void Timestep()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, ProjectileManager.Instance.projectileLerpSpeed * Delta);
            if (!IsServer)
            {
                currLife += Delta;
                if (LifeLerp >= 1 && alive)
                {
                    Terminate();
                }
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ProjectileIDs ??= new Dictionary<ushort, NetProjectile>();

            r.enabled = false;
            if(ProjectileIDs.TryAdd(ID, this))
            {
                ID++;
            }
        }
        internal void InitialiseOnServer(Vector3 simPos, Vector3 startPos, Vector3 forward, ulong entity, int dataIndex)
        {
            owner = entity;
            alive = true;
            currLife = 0;
            ProjectileData data = ProjectileManager.Instance.projectileData[dataIndex];
            this.data = data;
            maxLife = data.maxLifetime;
            velocity = forward * data.projectileSpeed;
            gravity = Physics.gravity * data.gravityMultiplier;

            bounces = data.maxBounces;

            //Remember to initialise both the current position AND target position to the place we fire from.
            transform.position = startPos;
            targetPos = simPos;
            r.enabled = true;
        }
        [Rpc(SendTo.ClientsAndHost)]
        internal void Initialise_RPC(Vector3 pos, ulong entity, int dataIndex)
        {
            transform.position = pos;
            r.enabled = true;
            owner = entity;
            alive = true;
            currLife = 0;
            ProjectileData data = ProjectileManager.Instance.projectileData[dataIndex];
            this.data = data;
            maxLife = data.maxLifetime;
            targetPos = pos;
        }

        public void Terminate()
        {
            alive = false;
            r.enabled = false;
            if (IsServer)
            {
                ProjectileManager.ActiveProjectiles--;
            }
        }

    }
}
