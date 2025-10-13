using Balla.Core;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Balla.Entity
{
    /// <summary>
    /// An entity is something that can take damage in some way. Every entity should have a rigidbody so that it is easier to access and clear that this object should be an entity. Kinematic status is not important.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseEntity : BallaNetScript
    {
        /// <summary>
        /// Invoked when this entity is destroyed by another entity.
        /// </summary>
        public Action<BaseEntity, BaseEntity> DestroyedByPlayer;
        internal Rigidbody rb;
        internal ulong entityID = 0;
        public static ulong nextEntityID = 0;
        public static Dictionary<ulong, BaseEntity> EntityIDs;
        public NetworkVariable<float> currentHealth = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] protected float maxHealth;
        [SerializeField] protected float HealthPercentage => currentHealth.Value / maxHealth;
       

        /// <summary>
        /// Subtracts damageTaken from the entity's health. Not all parameters have to be passed.
        /// </summary>
        /// <param name="healthDelta"></param>
        /// <param name="soucePos"></param>
        /// <param name="sourceDir"></param>
        /// <param name="healthSource"></param>
        internal virtual void ModifyHealth(float healthDelta, Vector3 soucePos = default, Vector3 sourceDir = default, NetworkBehaviourReference healthSource = default)
        {
            currentHealth.Value = Mathf.Clamp(currentHealth.Value - healthDelta, 0, maxHealth);
        }


        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EntityIDs ??= new Dictionary<ulong, BaseEntity>();
            entityID = nextEntityID;
            EntityIDs.Add(entityID, this);
            nextEntityID++;
        }

        private void OnValidate()
        {
            if(rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }
    }
}
