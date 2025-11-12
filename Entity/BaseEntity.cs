using Balla.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Balla.Entity
{
    /// <summary>
    /// An entity is something that can take damage in some way. Every entity should have a rigidbody so that it is easier to access and clear that this object should be an entity. Kinematic status is not important.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseEntity : BallaScript
    {
        /// <summary>
        /// Invoked when this entity is destroyed by another entity.
        /// </summary>
        public Action<BaseEntity, BaseEntity> DestroyedByPlayer;
        internal Rigidbody rb;
        internal ulong entityID = 0;
        public static ulong nextEntityID = 0;
        public static Dictionary<ulong, BaseEntity> EntityIDs;
        [ReadOnly] public float currentHealth;
        [SerializeField] protected float maxHealth;
        protected float HealthPercentage => currentHealth / maxHealth;
        public bool Alive => currentHealth > 0;
        [ReadOnly] public bool diedThisFrame;
        /// <summary>
        /// Subtracts damageTaken from the entity's health. Not all parameters have to be passed.
        /// </summary>
        /// <param name="healthDelta"></param>
        /// <param name="soucePos"></param>
        /// <param name="sourceDir"></param>
        /// <param name="healthSource"></param>
        internal virtual void ModifyHealth(float healthDelta, Vector3 soucePos = default, Vector3 sourceDir = default)
        {
            currentHealth = Mathf.Clamp(currentHealth - healthDelta, 0, maxHealth);
            if(currentHealth <= 0)
            {
                Die();
            }
        }
        protected virtual void Die()
        {
            diedThisFrame = true;
        }
        protected override void Timestep()
        {
            if (diedThisFrame)
            {
                diedThisFrame = false;
            }
            base.Timestep();
        }


        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            OnSpawn();
            currentHealth = maxHealth;
        }
        public virtual void OnSpawn()
        {
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
