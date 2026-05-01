using Balla.Core;
using Balla.Entity;
using Balla.Equipment;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace Balla.Gameplay
{
    public struct HitscanRequest
    {
        public HitscanRequest(HitscanWeapon weapon, Vector3 direction)
        {
            w = weapon;
            startPoint = weapon.FirePoint;
            this.direction = direction;
            distance = weapon.hitscanData.maxRange;
            tracer = HitscanManager.Instance.GetTracer(w);
            charge = w.Weapon.CurrentCharge;
            SetUpTracer();
        }
        private readonly void SetUpTracer()
        {
            tracer.alive = true;
            tracer.start = w.MuzzlePoint;
        }
        public float charge;
        public Vector3 startPoint;
        public Vector3 direction;
        public float distance;
        public HitscanWeapon w;
        public HitscanTracer tracer;
    }
    /// <summary>
    /// Hitscan weapons will make a call to this class to request to fire. at the end of the physics update, all of these shots will be fired simultaneously.<br></br>
    /// This way, we can:
    /// * Batch data when sending
    /// * Multithread raycasts 
    /// * Eliminate situations where both players shoot at the same time, but only one actually logs their hit.
    /// </summary>
    public class HitscanManager : BallaScript
    {
        public static HitscanManager Instance { get; private set; }
        public static List<TracerPool> pools;
        public static bool FireFromMuzzle => Instance != null && Instance.fireFromMuzzle;
        [SerializeField] protected bool fireFromMuzzle;
        public List<HitscanData> hitscanData;
        public LayerMask hitscanMask;
        public int tracersInChunk = 25;
        public int maxHitsPerRay = 16;
        internal static uint activeTracers;
        public int maxCastsPerFrame = 512;
        int requestCounter;
        HitscanRequest[] requests;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                //Initialise everything
                pools = new();
                for (int i = 0; i < hitscanData.Count; i++)
                {
                    pools.Add(new(hitscanData[i]));
                }
                CreateStartTracers();
            }
            else
            {
                enabled = false;
                return;
            }
        }

        void CreateStartTracers()
        {
            for (int i = 0; i < pools.Count; i++)
            {
                pools[i].CreateStartTracers(pools[i].data.tracerStartCount);
            }
        }
        public HitscanTracer GetTracer(HitscanWeapon w)
        {
            return pools[w.dataIndex].GetSingleTracer();
        }
        public void RequestHitscan(HitscanWeapon weapon, Vector3 direction)
        {
            requests ??= new HitscanRequest[maxCastsPerFrame];
            requests[requestCounter] = new(weapon, direction);
            requestCounter++;
        }
        NativeArray<RaycastCommand> commands;
        NativeArray<RaycastHit> hits;
        protected override void Timestep()
        {
            base.Timestep();

            //Lets do our checks and then do the shoot!
            if(requestCounter <= 0)
            {
                //We have no pending requests;
                return;
            }
            //Create the native arrays
            commands = new(requestCounter, Allocator.TempJob);
            hits = new(requestCounter * maxHitsPerRay, Allocator.TempJob);
            //Create some helpful other things we might need
            QueryParameters qp = new(hitscanMask, true);
            BaseEntity[] owners = new BaseEntity[requestCounter];
            for (int i = 0; i < requestCounter; i++)
            {
                HitscanRequest hr = requests[i];
                owners[i] = hr.w.Weapon.holder.entity;
                commands[i] = new(hr.startPoint, hr.direction, qp, hr.distance);
            }
            //Create the job handle and execute it
            Unity.Jobs.JobHandle handle = RaycastCommand.ScheduleBatch(commands, hits,
                Mathf.Max(requestCounter / JobsUtility.JobWorkerCount, 1), maxHitsPerRay);
            handle.Complete();
            for (int x = 0; x < requestCounter; x++)
            {
                float closestValidDistance = -1;
                HitscanRequest req = requests[x];
                int offset = x * maxHitsPerRay;
                bool didHit = false;
                RaycastHit closestHit = hits[0];
                for (int y = 0; y < maxHitsPerRay; y++)
                {
                    RaycastHit hit = hits[offset + y];
                    //If the hit collider is null, we break.
                    //If its null, there's no more recorded hits after this.
                    if (hit.collider == null)
                        break;

                    if(y == 0 || hit.distance <= closestValidDistance)
                    {
                        closestHit = hit;
                        closestValidDistance = hit.distance;
                    }
                }
                if(closestValidDistance != -1)
                {
                    if (closestHit.rigidbody == null)
                    {
                        ProjectileHit(req, closestHit);
                        didHit = true;
                    }
                    else
                    {
                        if (QueryHelper.GetEntity(closestHit.rigidbody, out BaseEntity ent) && ent != owners[x])
                        {
                            ProjectileHit(req, closestHit, ent);
                            didHit = true;
                        }
                    }
                }
                if (didHit)
                {
                    req.tracer.Terminate();
                    req.tracer.Setup(req.w.MuzzlePoint, closestHit.point);
                }
                else
                {
                    req.tracer.Setup(req.w.MuzzlePoint, req.startPoint + (req.direction * req.distance));
                    if(req.w.hitscanData.explosionData != null)
                    {
                        ExplosionManager.Instance.RequestExplosion(req.w.hitscanData.explosionData, req.tracer.end, Vector3.zero, req.w.Weapon.holder.entity.entityID);
                    }
                }
            }
            commands.Dispose();
            hits.Dispose();
            requestCounter = 0;
        }
        public void ProjectileHit(HitscanRequest req, RaycastHit hit, BaseEntity ent = null)
        {
            Debug.DrawLine(req.startPoint, hit.point, Color.green, 1);
            if (ent != null)
            {
                float damage = Mathf.Lerp(req.w.hitscanData.damageAtMinRange,
                    req.w.hitscanData.damageAtMaxRange, req.w.hitscanData.damageFalloff.Evaluate(Mathf.Clamp01(Mathf.InverseLerp(req.w.hitscanData.minRange, req.w.hitscanData.maxRange, hit.distance))));
                ent.rb.AddForceAtPosition(damage * req.w.hitscanData.hitForceMult * req.direction, hit.point, ForceMode.Impulse);

                ent.ModifyHealth(damage, req.startPoint, req.direction);
            }
            if (req.w.hitscanData.explosionData != null)
            {
                ExplosionManager.Instance.RequestExplosion(req.w.hitscanData.explosionData, hit.point + hit.normal, Quaternion.LookRotation(req.direction, hit.normal).eulerAngles, req.w.Weapon.holder.entity.entityID);
                req.tracer.end = hit.point;
            }
        }
    }
}
