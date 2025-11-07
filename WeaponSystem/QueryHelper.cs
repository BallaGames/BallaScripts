using Balla.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace Balla.Gameplay
{
    public static class QueryHelper
    {
        public static void Initialise()
        {
            cachedColliders?.Clear();
            cachedBodies?.Clear();
            cachedColliders ??= new();
            cachedBodies ??= new();
        }
        public static Dictionary<Collider, BaseEntity> cachedColliders;
        public static Dictionary<Rigidbody, BaseEntity> cachedBodies;
        public static bool GetEntity(Collider col, out BaseEntity ent)
        {
            ent = null;
            return cachedColliders != null && cachedColliders.TryGetValue(col, out ent);
        }
        public static bool GetEntity(Rigidbody rb, out BaseEntity ent)
        {
            if (!cachedBodies.ContainsKey(rb))
            {
                if(rb.TryGetComponent(out ent))
                {
                    cachedBodies.Add(rb, ent);
                    return true;
                }
            }
            ent = cachedBodies[rb];
            return true;
        }
    }
}
