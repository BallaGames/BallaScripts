using Balla.Core;
using UnityEngine;

namespace Balla.UI
{
    public class PlayerUI : BallaScript
    {
        public static PlayerUI Instance;
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public RectTransform crosshairTransform;
        public Vector2 crosshairSize;
        Vector2 ch_size;
        public float ch_lerpSpeed;
        protected override void AfterFrame()
        {
            ch_size = Vector2.Lerp(ch_size, crosshairSize, ch_lerpSpeed * Time.deltaTime);

            crosshairTransform.sizeDelta = ch_size;
        }
    }
}
