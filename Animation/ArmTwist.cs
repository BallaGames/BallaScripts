using Balla.Core;
using UnityEngine;

namespace Balla
{
    /// <summary>
    /// Use this if your forearm twist bones are not children of your forearm bone. might also work if they are.
    /// This one does make some assumptions though, such as the root bone and forearm bone having the same parent OR the rootbone being a child of the forearm.
    /// </summary>
    public class ArmTwist : BallaScript
    {
        public enum Axis
        {
            x = 0,
            y = 1,
            z = 2,
        }


        [System.Serializable]
        public struct Bone
        {
            public Transform bone;
            public float weight;
            public Axis axis;
        }

        public Bone[] bones;

        public Transform forearm;
        public Transform rootBone;
        public Axis rootAxis;
        public float rootWeight;
        public Transform endBone;

        [ReadOnly] public bool isChild;

        protected override void OnEnable()
        {
            base.OnEnable();

            isChild = rootBone.parent == forearm;
        }

        //We process it after the frame to make sure anything like recoil is already done at this point. Should work, right? If not, we'll delay it slightly somehow.
        protected override void AfterFrame()
        {
            if (!isChild)
            {
                switch (rootAxis)
                {
                    case Axis.x:
                        rootBone.localRotation = forearm.localRotation * Quaternion.Euler(endBone.localEulerAngles.x * rootWeight, 0, 0);
                        break;
                    case Axis.y:
                        rootBone.localRotation = forearm.localRotation * Quaternion.Euler(0, endBone.localEulerAngles.y * rootWeight, 0);
                        break;
                    case Axis.z:
                        rootBone.localRotation = forearm.localRotation * Quaternion.Euler(0, 0, endBone.localEulerAngles.z * rootWeight);
                        break;
                    default:
                        break;
                }
            }
            for (int i = 0; i < bones.Length; i++)
            {
                ref Bone b = ref bones[i];
                switch (b.axis)
                {
                    case Axis.x:
                        b.bone.localRotation = Quaternion.Euler(endBone.localEulerAngles.x * b.weight, 0, 0);
                        break;
                    case Axis.y:
                        b.bone.localRotation = Quaternion.Euler(0, endBone.localEulerAngles.y * b.weight, 0);
                        break;
                    case Axis.z:
                        b.bone.localRotation = Quaternion.Euler(0, 0, endBone.localEulerAngles.z * b.weight);
                        break;
                    default:
                        Debug.Log("Defaulting on rotation axis");
                        break;
                }
            }
        }
    }
}
