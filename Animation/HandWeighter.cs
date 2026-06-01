using System.Collections.Generic;
using UnityEngine;

namespace Balla
{
    public class HandWeighter : MonoBehaviour
    {
        public float lHandWeight;
        public float rHandWeight;

        public Transform weightDummyBone;

        private void Update()
        {
            lHandWeight = weightDummyBone.localPosition.x * -1;
            rHandWeight = weightDummyBone.localPosition.z;
        }
    }
}
