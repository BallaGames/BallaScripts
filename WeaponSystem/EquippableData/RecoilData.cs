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

        public bool addPosition;
        public AnimationCurve addXPos = AnimationCurve.Constant(0, 1, 1), 
            addYPos = AnimationCurve.Constant(0, 1, 1), 
            addZPos = AnimationCurve.Constant(0, 1, 1);
        public bool addRotation;
        public AnimationCurve addXRot = AnimationCurve.Constant(0, 1, 1),
            addYRot = AnimationCurve.Constant(0, 1, 1),
            addZRot = AnimationCurve.Constant(0, 1, 1);

        public AnimationCurve camPosIntensity = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve camRotIntensity = AnimationCurve.Linear(0, 1, 1, 0);
        public Vector3 maxCamPos, maxCamRot;
        public Vector3 maxCamPosAdd, minCamPosAdd, maxCamRotAdd, minCamRotAdd;

        public Vector3 aimRecoilPosMult, aimRecoilRotMult;
        public Vector3 aimRecoilPosMultCam, aimRecoilRotMultCam;


        public float camPosSharp, camRotSharp, camPosDecay, camRotDecay, camRecoilAdd, camRecoilDecay;
    }
}
