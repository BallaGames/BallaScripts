using UnityEngine;

namespace Balla
{
    public static class MathUtils
    {
        public static Vector3 Vec3Random(Vector3 min, Vector3 max, bool normalise = false)
        {
            Vector3 vec = new()
            {
                x = Random.Range(min.x, max.x),
                y = Random.Range(min.y, max.y),
                z = Random.Range(min.z, max.z)
            };
            if (normalise)
            {
                vec = vec.magnitude * vec.normalized;
            }
            return vec;
        }
    }
}
