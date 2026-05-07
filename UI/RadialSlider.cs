using Balla.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Balla
{
    [RequireComponent(typeof(Image)), ExecuteAlways()]
    public class RadialSlider : MonoBehaviour
    {
        [Range(0, 1)]
        public float fillAmount;
        float lastFill;

        public int fillAmountID = -999;
        Image r;
        public MaterialPropertyBlock mpb;

        private void Start()
        {
            fillAmountID = Shader.PropertyToID("_FillAmount");
        }

        private void LateUpdate()
        {
            if(lastFill != fillAmount)
            {
                fillAmount = Mathf.Clamp01(fillAmount);
                r.material.SetFloat(fillAmountID, fillAmount);
                lastFill = fillAmount;
            }
        }

        public void Show(bool b)
        {
            r.enabled = b;
        }

        private void OnValidate()
        {
            if(r == null)
            {
                r = GetComponent<Image>();
            }

            if (fillAmountID < 0)
            {
                fillAmountID = Shader.PropertyToID("_FillAmount");
            }
        }
    }
}
