using Balla.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Balla
{
    public class HandWeighter : BallaScript
    {
        public float lHandWeight;
        public float rHandWeight;

        public Transform charRightHand, weapRightHand, RightHandIKTarget;
        public Transform charLeftHand, weapLeftHand, leftHandIKTarget;


        public Transform weightDummyBone;

        protected override void AfterFrame()
        {
            lHandWeight = weightDummyBone.localPosition.x * -1;
            rHandWeight = weightDummyBone.localPosition.z;

            RightHandIKTarget.SetPositionAndRotation(Vector3.Lerp(charRightHand.position, weapRightHand.position, rHandWeight), 
                Quaternion.Lerp(charRightHand.rotation, weapRightHand.rotation, rHandWeight));
            leftHandIKTarget.SetPositionAndRotation(Vector3.Lerp(charLeftHand.position, weapLeftHand.position, lHandWeight),
                Quaternion.Lerp(charLeftHand.rotation, weapLeftHand.rotation, lHandWeight));
        }
    }
}
