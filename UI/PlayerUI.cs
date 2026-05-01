using Balla.Core;
using Balla.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace Balla.UI
{
    public class PlayerUI : BallaScript
    {
        public static PlayerUI Instance;
        [Header("Charge and Heat Bar")]
        public GameObject chargeBarRoot;
        public GameObject heatBarRoot;
        public Image chargeBar, heatBar;

        public float overheatCycleSpeed;
        float overheatCycle;
        public Gradient overheatColour;
        public Color defaultColour;
        bool lastOverheat;

        [Header("Health Bar")]
        public Slider healthSlider;

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

            heatBar.color = defaultColour;
        }

        public RectTransform crosshairTransform;
        public Vector2 crosshairSize;
        Vector2 ch_size;
        public float ch_lerpSpeed;
        

        public void WeaponSwitched(BaseWeapon newWeapon)
        {
            chargeBarRoot.SetActive(newWeapon.useCharge);
            heatBarRoot.SetActive(newWeapon.useHeat);
        }
        public void UpdateBars(float charge, float heat, bool isOverheating)
        {
            heatBar.fillAmount = heat;
            chargeBar.fillAmount = charge;
            if(lastOverheat != isOverheating)
            {
                lastOverheat = isOverheating;
                if(!isOverheating)
                    heatBar.color = defaultColour;
            }
            if (isOverheating)
            {
                overheatCycle = Mathf.Repeat(overheatCycle + (Delta * overheatCycleSpeed), 1);
                heatBar.color = overheatColour.Evaluate(overheatCycle);
            }
        }
        public void UpdateHealth(float lerp)
        {
            healthSlider.value = lerp;
        }
        public void SetCrosshair(Vector2 size)
        {
            ch_size = Vector2.Lerp(ch_size, size, ch_lerpSpeed * Time.deltaTime);
            crosshairTransform.sizeDelta = ch_size;
        }
    }
}
