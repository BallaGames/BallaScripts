using Balla.Entity;
using Balla.Equipment;
using Balla.Projectile;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

namespace Balla.Core
{
    /// <summary>
    /// <br>The Firearm Manager contains fields and methods helpful to or used by Firearms specifically.<br></br>
    /// It exists solely to support the use of firearms and provide some common world-functionality for weapons
    /// whilst also enabling the multithreading approach used for some weapons.</br>
    /// <br>First, the Firearm Manager gives some helpful configs for all weapons in the game to use. More of these will be added later and they will be annotated when added.</br>
    /// <br>The Firearm Manager also gives firearms access to the global projectile pool. Whilst not every weapon will use projectiles, those that do will benefit from this. </br>
    /// <br>It is advised to use hitscan for weapons that would have a much faster projectile, since the projectile process is more performance and bandwidth-intensive.</br>
    /// </summary>
    public class ProjectileManager : BallaNetScript
    {
        public static List<ProjectilePool> pools;
        public static ProjectileManager Instance { get; private set; }
        public static bool FireFromMuzzle => Instance != null && Instance.fireFromMuzzle;
        public LayerMask projectileLayermask;
        [SerializeField] protected bool fireFromMuzzle;
        [SerializeField] internal float projectileLerpSpeed = 10;
        [SerializeField] internal int maxHits;
        #region Projectile Pooling
        //Projectile Pooling
        [SerializeField, Tooltip("How many projectiles are instantiated on start")] protected int startProjectiles = 100;
        [SerializeField, Tooltip("How many projectiles are created when we run out of them in the pool." +
            "\n\"Chunking\" projectile spawns allows for lower constant cpu drain from spawning lots of projectiles repeatedly.")] internal int projectilesInChunk = 25;
        internal static uint activeCount;


        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(250);
            GUILayout.Label("active projectiles: " + activeCount);
            GUILayout.EndVertical();
        }

        public List<ProjectileData> projectileData;
        public static void AddProjectile(NetProjectile proj)
        {
            if (syncProjectiles == null)
            {
                syncProjectiles = new(new SizeCollection<NetProjectile>(1000));
                syncProjectiles[0] = proj;
                Debug.Log($"Created new projectile list with count {syncProjectiles.Count}");
            }
            else
            {
                //If we have an array of sync projectiles already, lets find the first empty OR dead entry
                for (int i = 0; i < syncProjectiles.Count; i++)
                {
                    if (syncProjectiles[i] == null || !syncProjectiles[i].alive)
                    {
                        syncProjectiles[i] = proj;
                        break;
                    }
                }
            }
        }
        public static List<NetProjectile> syncProjectiles;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //Moved the collection creation to Awake, so objects spawned can grab it. these are not synced so this should be fine.
                pools = new();
                Debug.Log($"pool count = {pools.Count} - projectileData count = {projectileData.Count}");
                for (int i = 0; i < projectileData.Count; i++)
                {
                    Debug.Log($"creating pool at index {i}");
                    pools.Add(new(projectileData[i], startProjectiles));
                }

            }
            else
            {
                enabled = false;
                return;
            }
        }
        long _t;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (Instance == null)
            {
                Instance = this;
            }
            if (IsServer)
            {
                //The host needs to run the sync logic too, so we'll wrap it in an if-else on IsServer.
                CreateStartProjectiles();
                //StartCoroutine(SendProjectileSync());
            }
            else
            {
                //But if we're a non-host client, we want to GET all the projectiles in the scene.
                //We can probably do this quite easily on the projectiles themselves.
                //But we'll leave this here for future reference.
            }
        }

        void CreateStartProjectiles()
        {
            for (int i = 0; i < pools.Count; i++)
            {
                pools[i].CreateStartProjectiles(startProjectiles);
            }
        }

        protected override void Timestep()
        {
            if (!IsServer)
                return;

            if (syncProjectiles == null)
            {
                return;
            }
            if(syncProjectiles.Count == 0)
            {
                return;
            }
            SimulateProjectiles();
        }
        //Projectile Ray stuff
        NativeArray<SpherecastCommand> commands;
        NativeArray<RaycastHit> hits;

        public NetProjectile GetSingleProjectile(ProjectileWeapon w)
        {
            NetProjectile proj = pools[w.dataIndex].GetSingleProjectile();
            Vector3 pos = fireFromMuzzle ? w.MuzzlePoint : w.holder.firearmShootPoint.position;
            Vector3 dir = w.holder.firearmShootPoint.forward;
            proj.networkTransform.Teleport(pos, Quaternion.identity, Vector3.one);
            proj.SimSetup(pos, w.MuzzlePoint, dir, w.holder.entity.entityID);
            proj.Initialise_RPC(w.MuzzlePoint);
            activeCount++;
            return proj;
        }

        /// <summary>
        /// Simulates every projectile's motion.
        /// </summary>
        protected void SimulateProjectiles()
        {
            if (activeCount == 0)
            {
                return;
            }
            if (syncProjectiles == null || syncProjectiles.Count == 0)
            {
                return;
            }

            BaseEntity[] entities = new BaseEntity[activeCount];
            commands = new((int)activeCount, Allocator.TempJob);
            hits = new(commands.Length * maxHits, Allocator.TempJob);
            QueryParameters qp = new(projectileLayermask, true, QueryTriggerInteraction.UseGlobal, false);
            int index = 0;
            for (int i = 0; i < syncProjectiles.Count; i++)
            {
                if (syncProjectiles[i] == null || !syncProjectiles[i].alive)
                {
                    continue;
                }
                NetProjectile p = syncProjectiles[i];
                entities[index] = BaseEntity.EntityIDs[p.owner];
                commands[index] = new(p.targetPos, p.data.radius, p.velocity.normalized, qp, p.velocity.magnitude * Delta);
                index++;
            }


            var handle = SpherecastCommand.ScheduleBatch(commands, hits, (int)Mathf.Max(activeCount / JobsUtility.JobWorkerCount, 1), maxHits);
            handle.Complete();

            bool didHit = false;
            index = 0;
            for (int x = 0; x < 1000; x++)
            {
                //this SHOULD be how we get the current projectile.
                NetProjectile proj = syncProjectiles[x];
                if (proj == null)
                    continue;
                //The max number of hits we allow.
                didHit = false;
                int offset = index * maxHits;
                for (int y = 0; y < maxHits; y++)
                {
                    RaycastHit hit = hits[offset + y];
                    if(hit.collider != null)
                    {
                        if(hit.rigidbody == null)
                        {
                            //Do something if we hit a surface, i suppose!
                            //Its automatically NOT our player in this situation
                            ProjectileHit(proj, hit);
                        }
                        else
                        {
                            //And if we hit something with a rigidbody, we should check its entity
                            //If the entity is NOT the entities[x] (this projectile's entity) then we can hit it.
                            //Force recompile now
                            if(hit.rigidbody.TryGetComponent(out BaseEntity ent) && ent != entities[index])
                            {
                                ProjectileHit(proj, hit, ent);
                            }
                        }
                        didHit = proj.bounces == -1;
                    }
                }
                if (didHit)
                {
                    Debug.Log($"ending projectile {index}: hit and no bounces");
                    proj.Terminate();
                }
                else
                {
                    TickProjectile(proj);
                    Debug.DrawLine(commands[index].origin, proj.targetPos, Color.red, 1);
                    if (proj.currLife >= proj.maxLife || !proj.alive)
                    {
                        Debug.Log($"ending projectile {index}: life elapsed");
                        proj.Terminate();
                        if(proj.data.explosionData != null && proj.data.explodeOnExpire)
                        {
                            ExplosionManager.Instance.Explode(proj.data.explosionData, proj.targetPos, Quaternion.LookRotation(proj.transform.forward, proj.transform.up).eulerAngles, proj.owner);
                        }
                    }
                }
                if (!proj.alive)
                {
                    syncProjectiles[x] = null;
                }
                index++;
            }
            hits.Dispose();
            commands.Dispose();
        }
        public void TickProjectile(NetProjectile proj)
        {
            if(proj == null)
            {
                return;
            }
            proj.velocity += (proj.gravity * Delta) - (proj.velocity * (proj.data.drag * Delta));
            proj.targetPos += proj.velocity * Delta;
            proj.currLife += Delta;

        }
        public void ProjectileHit(NetProjectile proj, RaycastHit hit, BaseEntity ent = null)
        {
            Debug.DrawLine(proj.targetPos, hit.point, Color.green, 1);
            if(ent != null)
            {
                ent.rb.AddForceAtPosition(Mathf.Lerp(proj.data.maxDamage, proj.data.minDamage, proj.data.damageOverLife.Evaluate(proj.LifeLerp)) * proj.direction.normalized, hit.point);
                if (proj.data.cannotBounceOnEntity)
                {
                    proj.bounces = -1;
                }
            }
            //this projectile hit something, anyway, so we should move it.
            if (proj.bounces >= 0)
            {
                //project our velocity against our hit normal, invert it and then multiply it by our bounciness.
                proj.velocity = (-Vector3.Project(proj.velocity, hit.normal) * proj.data.bounciness) + Vector3.ProjectOnPlane(proj.velocity, hit.normal);
            }

            proj.bounces--;
            proj.targetPos = hit.point;

            if (proj.bounces < 0)
            {
                ExplosionManager.Instance.Explode(proj.data.explosionData, hit.point + hit.normal * 0.02f, Quaternion.LookRotation(proj.transform.forward, hit.normal).eulerAngles, proj.owner);
            }
        }
        /// <summary>
        /// A coroutine that runs continuously throughout the lifetime of the session, synchronising projectiles.
        /// <para>
        /// <b>Low Projectile Count</b><br></br>
        /// If there are < 64 projectiles, the server will synchronise all projectiles in bulk periodically. <br></br>
        /// This sends a batch of IDs (ushort) and a batch of positions (vector3) to assign as their target position.
        /// </para>
        /// <para>
        /// <b>High Projectile Count</b>
        /// If there are > 64 projectiles, the server "chunks" the projectile synchronisation into blocks of 64,<br></br>
        /// Submitting each "chunk" of data (see above) at a slightly larger interval.
        /// </para>
        /// </summary>
        /// <returns></returns>
        IEnumerator SendProjectileSync()
        {
            //We'll have to rewrite this code later.
            var waitL = new WaitForSeconds(0.1f);
            var waitS = new WaitForSeconds(0.06f);
            //I think the process I'll use instead is iterating over the 
            int spaceLeftInBatch = 64;
            while (true)
            {
                //If we have no projectiles, wait till next frame to see if we do.
                if (activeCount <= 0 || syncProjectiles == null)
                    yield return null;
                else
                {

                    //We're going to convert ALLLL of this into some new magical way of syncing across multiple pools.
                    int synced = 0;
                    bool doChunking = activeCount > 64;
                    if (doChunking)
                    {
                        ushort[] chunkIDs = new ushort[64];
                        Vector3[] chunkPos = new Vector3[64];
                        for (int i = 0, x = 0; i < syncProjectiles.Count && x < activeCount; i++)
                        {
                            if (syncProjectiles[i] == null)
                                continue;
                            NetProjectile item = syncProjectiles[i];
                            int index = synced % 64;
                            chunkIDs[index] = (ushort)item.globalID;
                            chunkPos[index] = item.targetPos;
                            synced++;
                            x++;
                            spaceLeftInBatch--;
                            if (spaceLeftInBatch == 0)
                            {
                                //SendProjectilePositions_RPC(chunkIDs, chunkPos);
                                yield return waitL;
                            }
                        }
                    }
                    else
                    {
                        ushort[] IDs = new ushort[activeCount];
                        Vector3[] pos = new Vector3[activeCount];
                        for (int i = 0, x = 0; i < syncProjectiles.Count && x < activeCount; i++)
                        {
                            if (syncProjectiles[x] == null) continue;
                            IDs[x] = (ushort)syncProjectiles[x].globalID;
                            pos[x] = syncProjectiles[x].targetPos;
                            x++;
                        }
                        //SendProjectilePositions_RPC(IDs, pos);
                        yield return waitS;
                    }
                }
                
            }
        }
    }
}
