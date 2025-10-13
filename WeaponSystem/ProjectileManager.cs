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
        public static ProjectileManager Instance { get; private set; }
        public static bool FireFromMuzzle => Instance != null && Instance.fireFromMuzzle;
        public LayerMask projectileLayermask;
        [SerializeField] protected bool fireFromMuzzle;
        [SerializeField] internal float projectileLerpSpeed = 10;
        [SerializeField] internal int maxHits;
        #region Projectile Pooling
        //Projectile Pooling
        [SerializeField, Tooltip("The prefab instantiated by the projectile pool")] protected NetProjectile projectilePrefab;
        [SerializeField, Tooltip("How many projectiles are instantiated on start")] protected int startProjectiles = 100;
        [SerializeField, Tooltip("How many projectiles are created when we run out of them in the pool." +
            "\n\"Chunking\" projectile spawns allows for lower constant cpu drain from spawning lots of projectiles repeatedly.")] protected int projectilesInChunk = 25;
        internal static uint ActiveProjectiles;

        public List<NetProjectile> Projectiles { get; private set; }
        public HashSet<int> aliveProjectiles;
        public List<ProjectileData> projectileData;
        public int[] projIndices;

        public NetProjectile GetSingleProjectile(ProjectileWeapon w)
        {
            NetProjectile proj = null;
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
                CreateProjectiles(out NetProjectile[] projectilesSpawned);
                //Then pull the first one from this.
                proj = projectilesSpawned[0];
                aliveProjectiles.Add(Projectiles.IndexOf(proj));
            }
            proj.InitialiseOnServer(fireFromMuzzle ? w.MuzzlePoint : w.holder.firearmShootPoint.position, w.MuzzlePoint, w.holder.firearmShootPoint.forward, w.holder.entity.entityID, w.projDataIndex);
            proj.Initialise_RPC(fireFromMuzzle ? w.MuzzlePoint : w.holder.firearmShootPoint.position, w.holder.entity.entityID, w.projDataIndex);
            return proj;
        }
        /// <summary>
        /// Retrieves or creates N projectiles.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public NetProjectile[] GetMultipleProjectiles(int count)
        {
            //Create an array with capacity for the request
            NetProjectile[] projectiles = new NetProjectile[count];
            //Then check if we have enough projectiles without needing to make more.
            //If this part evaluates to true, then we should make more projectiles.
            int index = 0;
            if (Projectiles.Count < count)
            {
                //Whilst we have less un-used projectiles than we need, 
                while (Projectiles.Count - ActiveProjectiles < count)
                {
                    CreateProjectiles(out NetProjectile[] projectilesSpawned);
                    for (int i = 0; i < projectilesInChunk && index < count; i++, index++)
                    {
                        projectiles[index] = projectilesSpawned[i];
                    }
                }
            }
            return projectiles;
        }
        public void CreateProjectiles(out NetProjectile[] projectilesSpawned)
        {
            projectilesSpawned = new NetProjectile[projectilesInChunk];
            //then instantiate another chunk of projectiles
            for (int i = 0; i < projectilesSpawned.Length; i++)
            {
                projectilesSpawned[i] = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
                projectilesSpawned[i].NetworkObject.Spawn();
            }
            Projectiles.AddRange(projectilesSpawned);
        }
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //Moved the collection creation to Awake, so objects spawned can grab it. these are not synced so this should be fine.
                Projectiles = new();
                aliveProjectiles = new();
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
           
            if (IsServer)
            {
                //The host needs to run the sync logic too, so we'll wrap it in an if-else on IsServer.
                CreateStartProjectiles();
                StartCoroutine(SendProjectileSync());
            }
            else
            {
                //But if we're a non-host client, we want to GET all the projectiles in the scene.
                //We can probably do this quite easily on the projectiles themselves.
                //But we'll leave this here for future reference.
            }
        }
        async void CreateStartProjectiles()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            NetProjectile[] ps;
            ps = await InstantiateAsync(projectilePrefab, startProjectiles);
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].NetworkObject.Spawn();
            }
            stopwatch.Stop();
            Debug.Log($"Start Projectile creation took {stopwatch.ElapsedMilliseconds}. Completed in {System.DateTime.Now.Ticks - _t} from call..");
        }

        protected override void Timestep()
        {
            if (aliveProjectiles.Count == 0)
                return;
            SimulateProjectiles();
        }
        //Projectile Ray stuff
        NativeArray<SpherecastCommand> commands;
        NativeArray<RaycastHit> hits;
        /// <summary>
        /// Simulates every projectile's motion.
        /// </summary>
        protected void SimulateProjectiles()
        {
            commands = new(aliveProjectiles.Count, Allocator.TempJob);
            hits = new(aliveProjectiles.Count * maxHits, Allocator.TempJob);
            QueryParameters qp = new QueryParameters(projectileLayermask, true, QueryTriggerInteraction.UseGlobal, false);
            int counter = 0;
            projIndices = new int[aliveProjectiles.Count];
            BaseEntity[] entities = new BaseEntity[aliveProjectiles.Count];
            foreach (var item in aliveProjectiles)
            {
                projIndices[counter] = item;
                entities[counter] = BaseEntity.EntityIDs[Projectiles[item].owner];
                commands[counter] = new(Projectiles[item].targetPos, Projectiles[item].data.radius, Projectiles[item].velocity.normalized,qp, Projectiles[item].velocity.magnitude * Delta);
                counter++;
            }
            var handle = SpherecastCommand.ScheduleBatch(commands, hits, Mathf.Max(aliveProjectiles.Count / JobsUtility.JobWorkerCount, 1), maxHits);
            handle.Complete();

            bool didHit = false;
            //we have to use a roundabout way of checking for things, since we implement aliveProjectiles via a hashset.
            //we keep a HashSet<int> of all the projectiles that are currently alive, but there's no way to tie the projectile index and the current
            
            for (int x = 0; x < aliveProjectiles.Count; x++)
            {
                //this SHOULD be how we get the current projectile.
                NetProjectile proj = Projectiles[projIndices[x]];
                //The max number of hits we allow.
                int offset = x * maxHits;
                for (int y = 0; y < maxHits; y++)
                {
                    RaycastHit hit = hits[offset + y];
                    if(hit.collider == null)
                    {
                        //We haven't hit anything, and there's never null colliders in the middle.
                        break;
                    }
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
                        if(hit.rigidbody.TryGetComponent(out BaseEntity ent) && ent != entities[x])
                        {
                            ProjectileHit(proj, hit, ent);
                        }
                    }
                    didHit = proj.bounces == -1;
                }
                if (didHit)
                {
                    proj.Terminate();
                    aliveProjectiles.Remove(projIndices[x]);
                }
                else
                {
                    proj.velocity += proj.gravity * Delta;
                    proj.targetPos += proj.velocity * Delta;
                    proj.currLife += Delta;
                    Debug.DrawLine(commands[x].origin, proj.targetPos, Color.red, 1);
                    if (proj.currLife >= proj.maxLife || !proj.alive)
                    {
                        proj.Terminate();
                        aliveProjectiles.Remove(projIndices[x]);
                    }
                }
            }
            hits.Dispose();
            commands.Dispose();
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
            var waitL = new WaitForSeconds(0.1f);
            var waitS = new WaitForSeconds(0.06f);
            while (true)
            {
                //If we have no projectiles, wait till next frame to see if we do.
                if (aliveProjectiles.Count == 0)
                    yield return null;
                //New approach, since we use a hashset
                int synced = 0;
                bool doChunking = aliveProjectiles.Count > 64;
                if (doChunking)
                {
                    ushort[] chunkIDs = new ushort[64];
                    Vector3[] chunkPos = new Vector3[64];
                    foreach (var item in aliveProjectiles)
                    {
                        int index = synced % 64;
                        //We want to use our total sync as the index for this one, since that's the 
                        chunkIDs[index] = Projectiles[item].NetworkBehaviourId;
                        chunkPos[index] = Projectiles[item].transform.position;
                        synced++;
                        if (synced % 64 == 0)
                        {
                            SendProjectilePositions_RPC(chunkIDs, chunkPos);
                            yield return waitL;
                        }
                    }
                }
                else
                {
                    ushort[] IDs = new ushort[aliveProjectiles.Count];
                    Vector3[] pos = new Vector3[aliveProjectiles.Count];
                    foreach (var item in aliveProjectiles)
                    {
                        IDs[synced] = Projectiles[item].NetworkBehaviourId;
                        pos[synced] = Projectiles[item].transform.position;
                        synced++;
                    }
                    SendProjectilePositions_RPC(IDs, pos);
                    yield return waitS;
                }
            }
        }
        [Rpc(SendTo.NotServer)]
        void SendProjectilePositions_RPC(ushort[] IDs, Vector3[] positions)
        {
            Debug.Log($"received sync for {IDs.Length} projectiles");
            for (int i = 0; i < IDs.Length; i++)
            {
                NetProjectile.SetProjectilePosition(IDs[i], positions[i]);
            }
        }
    }
}
