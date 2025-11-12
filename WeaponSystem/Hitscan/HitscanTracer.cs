using Balla.Core;
using Balla.Gameplay;
using UnityEngine;

namespace Balla
{
    public class HitscanTracer : BallaScript
    {
        public bool alive;
        public HitscanData data;
        public Vector3 start, end;
        
        public ulong poolID;
        public float increment;
        public TrailRenderer trail;
        float life;
        public void Setup(Vector3 startPoint, Vector3 endPoint)
        {
            life = 0;
            start = startPoint;
            transform.position = start;
            end = endPoint;
            trail.Clear();
            trail.emitting = true;
            alive = true;
            increment = data.tracerSpeed / Vector3.Distance(start, end);
        }
        public void Terminate()
        {
            alive = false;
            trail.emitting = false;
        }
        protected override void Timestep()
        {
            base.Timestep();
            if (alive)
            {
                if (life >= 1)
                {
                    Terminate();
                }
                life += Time.fixedDeltaTime * increment;
                transform.position = Vector3.Lerp(start, end, life);
            }
        }
    }
}
