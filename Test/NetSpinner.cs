using UnityEngine;
using Balla.Core;
namespace Balla
{
    public class NetSpinner : BallaNetScript
    {
        public Vector3 axis;
        public float speed;

        protected override void Timestep()
        {
            if(IsOwner)
                transform.Rotate(Delta * speed * axis);
        }
    }
}
