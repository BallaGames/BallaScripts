using Balla.Equipment;
using System.Collections;
using UnityEngine;

namespace Balla
{
    public class GrappleEquipment : BaseUseable
    {
        [SerializeField, ReadOnly] protected bool grappling;
        [SerializeField, ReadOnly] protected bool movingGrapple;
        [SerializeField, ReadOnly] protected bool grappleHit, isCoolingDown;
        [SerializeField, ReadOnly] protected float cooldown;
        [SerializeField, ReadOnly] protected GameObject hookObject;
        [SerializeField] protected Transform hookTransform;
        public float CooldownLerp => cooldown / lastCooldown;
        [SerializeField] protected GameObject hookPrefab;
        [SerializeField] protected float hookTravelSpeed, hookMaxDistance, hookTimeout, hookPullForce, hookPullDrag, hookViewDirectionBlend;
        [SerializeField] protected float baseCooldown, cooldownPerUnit;
        [SerializeField] protected LayerMask hookLayer;

        [SerializeField, ReadOnly] protected Vector3 hookHitPoint;
        float lastCooldown;

        protected override void Init()
        {
            base.Init();
            //Spawns a new hook for the grapple so that it doesn't 
            hookObject = Instantiate(hookPrefab);
            hookObject.SetActive(false);
        }

        public override void OnUse()
        {
            if (grappling)
            {
                EndGrapple();
            }
            else
            {
                TryGrapple();
            }
        }
        protected void TryGrapple()
        {
            if(Physics.Raycast(holder.firearmShootPoint.position, holder.firearmShootPoint.forward, out RaycastHit hit, hookMaxDistance, hookLayer))
            {
                if(hit.distance > 0.4f)
                {
                    hookHitPoint = hit.point;
                    movingGrapple = true;
                    StartCoroutine(UpdateGrapple());
                }
            }
        }
        protected void EndGrapple()
        {

        }

        IEnumerator UpdateGrapple()
        {
            grappling = true;
            while (movingGrapple)
            {
                yield return null;
            }
            float t = 0;
            while (grappling && t < hookTimeout && Vector3.Distance(holder.transform.position, hookHitPoint) < .8f)
            {
                t += Delta;

                Vector3 force = (Vector3.Lerp(holder.firearmShootPoint.forward, hookHitPoint - holder.transform.position, hookViewDirectionBlend) * hookPullForce) 
                    - (holder.entity.rb.linearVelocity * hookPullDrag);

                holder.entity.rb.AddForce(force);
            }

        }
    }
}
