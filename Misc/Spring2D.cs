using System;
using System.Collections.Generic;
using UnityEngine;

namespace Balla.Utils
{
    [System.Serializable]
    public class Spring2D
    {
        public Transform transform;
        public Rigidbody2D rb;
        public Vector2 originOffset, pos, springPos;
        public Vector2 direction;
        public float maxDistance;
        public float stiffness, damper, heightOffset, restLength, travel, length, velocity, springOffset;
        public LayerMask mask;
        public Spring2D()
        {

        }

        public Spring2D(Transform transform, Vector2 direction, float stiffness, float damper,
            float heightOffset, float restLength, float travel, LayerMask mask, Rigidbody2D rb, Vector2 originOffset)
        {
            this.transform = transform;
            this.direction = direction;
            this.stiffness = stiffness;
            this.damper = damper;
            this.heightOffset = heightOffset;
            this.restLength = restLength;
            this.travel = travel;
            this.mask = mask;
            this.rb = rb;
            this.originOffset = originOffset;
        }

        public void UpdateSpring(float delta, out bool hit, out RaycastHit2D hitInfo, out float force)
        {
            maxDistance = restLength + travel;
            
            hitInfo = Physics2D.Raycast(transform.TransformPoint(originOffset), direction, maxDistance, mask);
            hit = hitInfo.collider != null;
            if (hit)
            {
                Debug.DrawLine(transform.TransformPoint(originOffset), hitInfo.point, Color.green, Time.fixedDeltaTime);
                pos = hitInfo.point;
                length = hitInfo.distance - heightOffset;
                springOffset = (restLength - length) / travel;

                velocity = Vector2.Dot(transform.up, rb.GetPointVelocity(springPos));
                force = (stiffness * springOffset) - (damper * velocity);
                springPos = Vector2.up * Mathf.Clamp(restLength - length, -travel, travel);
            }
            else
            {
                Debug.DrawRay(transform.TransformPoint(originOffset), direction * maxDistance, Color.red, Time.fixedDeltaTime);
                force = 0;
                springPos = Vector2.up * restLength;
            }
        }
    }
}
