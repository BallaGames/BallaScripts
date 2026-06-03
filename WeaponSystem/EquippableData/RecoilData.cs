using UnityEngine;

namespace Balla
{
    [CreateAssetMenu(fileName = "RecoilData", menuName = "Weapon System/RecoilData")]
    public class RecoilData : ScriptableObject
    {
        public Vector3 linearForceMin, linearForceMax, angularForceMin, angularForceMax;
        public float linearSpeed, linearSharp, angularSpeed, angularSharp;
        public float recoilReturnTime, recoilReturnSpeed;
        public AnimationCurve linearReturnCurve, angularReturnCurve, linearIntensity, angularIntensity;
        public float intensityClimb, intensityDecay, intensityDecayMult;

        public AnimationCurve camPosIntensity = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve camRotIntensity = AnimationCurve.Linear(0, 1, 1, 0);
        public Vector3 maxCamPos, maxCamRot;
        public Vector3 maxCamPosAdd, minCamPosAdd, maxCamRotAdd, minCamRotAdd;

        public Vector3 aimRecoilPosMult, aimRecoilRotMult;
        public Vector3 aimRecoilPosMultCam, aimRecoilRotMultCam;

        public Quaternion aimRotation = Quaternion.identity;
        public Vector3 aimPositionNegate = Vector3.zero;
        public float aimRotationNegate = 1;

        public float camPosSharp, camRotSharp, camPosDecay, camRotDecay, camRecoilAdd, camRecoilDecay;
    }
}
