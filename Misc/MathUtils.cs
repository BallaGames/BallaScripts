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
        public static Vector3 ScaleComponent(this Vector3 vec, Vector3 scale)
        {
            return new(vec.x * scale.x, vec.y * scale.y, vec.z * scale.z);
        }

        public static Vector3 CurveToVector(AnimationCurve x, AnimationCurve y, AnimationCurve z, float t)
        {
            return new Vector3(x.Evaluate(t), y.Evaluate(t), z.Evaluate(t));
        }
        public static Vector3 CurveToVector(AnimationCurve x, AnimationCurve y, AnimationCurve z, Vector3 t)
        {
            return new Vector3(x.Evaluate(t.x), y.Evaluate(t.y), z.Evaluate(t.z));
        }
    }
}
