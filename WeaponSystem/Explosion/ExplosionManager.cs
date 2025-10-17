using Balla.Core;
using Balla.Entity;
using RootMotion.FinalIK;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Balla.Core
{
    public class ExplosionManager : BallaNetScript
    {
        public static ExplosionManager Instance {  get; private set; }
        public List<ExplosionData> explosions;
        public Dictionary<ExplosionData, Explosion> explosionDict;
        public LayerMask checkMask;
        public LayerMask obstructMask;
        public int maxOverlaps = 16;

        public override void OnNetworkSpawn()
        {
            if(Instance == null)
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
            base.OnNetworkSpawn();
        }

        /// <summary>
        /// Performs an explosion at this position
        /// </summary>
        /// <param name="expType">Which explosion to use.</param>
        /// <param name="position"></param>
        /// <param name="sourceEntityID"></param>
        public void Explode(ExplosionData expType, Vector3 position, Vector3 rotation, ulong sourceEntityID)
        {
            SendExplosion_RPC(explosions.IndexOf(expType), position, Quaternion.Euler(rotation));
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
                            float baseDamage = Mathf.Lerp(expType.maxDamage, 0, expType.damageFalloff.Evaluate(Mathf.InverseLerp(0, expType.radius, hit.distance)));
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
                                    if (col.attachedRigidbody.TryGetComponent(out BaseEntity b))
                                    {
                                        //Hit something with health that was NOT the owner
                                        b.ModifyHealth(baseDamage);
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
        [Rpc(SendTo.ClientsAndHost)]
        public void SendExplosion_RPC(int explosionIndex, Vector3 pos, Quaternion rot)
        {
            explosionDict[explosions[explosionIndex]].GetExplosion(pos, rot);
        }
    }
}
