using UnityEngine;

namespace Balla
{
    [CreateAssetMenu(fileName = "SwayConfig", menuName = "Scriptable Objects/SwayConfig")]
    public class SwayConfig : ScriptableObject
    {
        public float aimSwayMult, aimMotionMult;
        public Vector3 lookMotionPosScale, lookMotionRotScale;
        public Vector3 movementPosScale, movementRotScale;
        public Vector2 swaySpeed;
        public Vector3 swayPosScale, swayRotScale;
        public float swayMoveSpeedMult;
        public Vector3 swayMovePosScale, swayMoveRotScale;

        public float swayDamping = 0.1f, motionPosDamping = 0.1f, motionRotDamping = 0.1f;
    }
}
