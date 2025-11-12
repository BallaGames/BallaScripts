using Balla.Entity;
using Balla.Gameplay;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Balla.Core
{
    public struct ExplosionRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_pos">Where the explosion occurs.</param>
        /// <param name="_rot">The rotation of the explosion prefab</param>
        /// <param name="_data">the Explosion Data object used for this explosion</param>
        /// <param name="_ID">The Entity ID of the entity causing the explosion.</param>
        /// <param name="chain">Automatically Incrememnted. Pass the old Chain ID.</param>
        public ExplosionRequest(Vector3 _pos, Vector3 _rot, ExplosionData _data, ulong _ID)
        {
            position = _pos;
            rotation = _rot;
            data = _data;
            ID = _ID;
        }
        public ExplosionData data;
        public Vector3 position, rotation;
        public ulong ID;
    }


    public class ExplosionManager : BallaScript
    {
        public static ExplosionManager Instance {  get; private set; }
        public List<ExplosionData> explosions;
        public Dictionary<ExplosionData, Explosion> explosionDict;
        public LayerMask checkMask;
        public LayerMask obstructMask;
        public int maxOverlaps = 16;
        public int requestCounter;
        ExplosionRequest[] requests;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                return;
            }


            explosionDict = new();
            for (int i = 0; i < explosions.Count; i++)
            {
                explosionDict.TryAdd(explosions[i], new Explosion(explosions[i], 10));
            }
        }

        protected override void Timestep()
        {
            base.Timestep();

            if (requestCounter == 0)
                return;
            for (int i = 0; i < requestCounter; i++)
            {
                ExplosionRequest req = requests[i];
                //Do the explosion.
                Explode(req);
            }
            requestCounter = 0;
        }
        public void RequestExplosion(ExplosionData data, Vector3 pos, Vector3 rot, ulong ID)
        {
            requests ??= new ExplosionRequest[128];
            if(requestCounter <= 127)
            {
                requests[requestCounter] = new(pos, rot, data, ID);
                requestCounter++;
            }
        }
        void Explode(ExplosionRequest req)
        {
            Explode(req.data, req.position, req.rotation, req.ID);
        }
        /// <summary>
        /// Performs an explosion at this position
        /// </summary>
        /// <param name="expType">Which explosion to use.</param>
        /// <param name="position"></param>
        /// <param name="sourceEntityID"></param>
        public void Explode(ExplosionData expType, Vector3 position, Vector3 rotation, ulong sourceEntityID)
        {
            SpawnExplosionEffect(explosions.IndexOf(expType), position, Quaternion.Euler(rotation));
            Collider[] cols = new Collider[maxOverlaps];
            int hits = Physics.OverlapSphereNonAlloc(position, expType.radius, cols, checkMask, QueryTriggerInteraction.Ignore);
            Debug.Log($"{hits} objects hit by explosion");
            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    if (cols[i] == null)
                    {
                        continue;
                    }
                    Collider col = cols[i];
                    if (col.attachedRigidbody != null)
                    {
                        if (Physics.Linecast(position, col.attachedRigidbody.worldCenterOfMass, out RaycastHit hit, obstructMask, QueryTriggerInteraction.Ignore))
                        {
                            Debug.DrawLine(position, hit.point, Color.green, 5);
                            float baseDamage = expType.maxDamage * expType.damageFalloff.Evaluate(Mathf.InverseLerp(0, expType.radius, hit.distance));
                            if(hit.rigidbody == null)
                            {
                                //obstruction found
                                if (hit.collider.CompareTag("Destructible"))
                                {
                                    //Hit destructible.
                                    Debug.Log("hit destructible");
                                }
                            }
                            else if (hit.rigidbody == col.attachedRigidbody)
                            {
                                //we hit the correct object.
                                if (hit.rigidbody == BaseEntity.EntityIDs[sourceEntityID].rb)
                                {
                                    //Self damage
                                    //because we hit ourselves and already have our entityID, we can just use that.
                                    BaseEntity.EntityIDs[sourceEntityID].ModifyHealth(baseDamage * expType.selfDamageMult);
                                    Debug.Log("Self-damage from explosion");
                                }
                                else
                                {
                                    //Only entities that are alive should be damaged. Explosives will now only explode if they have NOT just died.
                                    if (QueryHelper.GetEntity(hit.rigidbody, out BaseEntity b) && !b.diedThisFrame)
                                    {
                                        //Hit something with health that was NOT the owner
                                        b.ModifyHealth(baseDamage);
                                        hit.rigidbody.AddExplosionForce(baseDamage * expType.forceMult, position, expType.radius, 0.3f, ForceMode.Impulse);
                                        Debug.Log("Hit non-owner with explosion");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Targets in range but no Line of Sight");
                        }
                    }
                }
            }
        }
        public void SpawnExplosionEffect(int explosionIndex, Vector3 pos, Quaternion rot)
        {
            explosionDict[explosions[explosionIndex]].GetExplosion(pos, rot);
        }
    }
}
