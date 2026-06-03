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
        public CanvasGroup crossshairCG;
        public Vector2 crosshairSize;
        Vector2 ch_size;
        public float ch_lerpSpeed;
        bool lastReloading;
        public RadialSlider reloadTimeSlider;

        public void WeaponSwitched(BaseWeapon newWeapon)
        {
            chargeBarRoot.SetActive(newWeapon.useCharge);
            heatBarRoot.SetActive(newWeapon.useHeat);
        }
        public void UpdateBars(BaseWeapon weapon)
        {
            heatBar.fillAmount = weapon.HeatLevel.lerp;
            chargeBar.fillAmount = weapon.CurrentCharge;
            if(lastOverheat != weapon.isOverheated)
            {
                lastOverheat = weapon.isOverheated;
                if(!weapon.isOverheated)
                    heatBar.color = defaultColour;
            }
            if (weapon.isOverheated)
            {
                overheatCycle = Mathf.Repeat(overheatCycle + (Delta * overheatCycleSpeed), 1);
                heatBar.color = overheatColour.Evaluate(overheatCycle);
            }
            if(reloadTimeSlider != null)
            {
                if(lastReloading != weapon.IsReloading)
                {
                    reloadTimeSlider.Show(weapon.IsReloading);
                    lastReloading = weapon.IsReloading;
                }
                reloadTimeSlider.fillAmount = weapon.ReloadTimeRatio;
            }
        }
        public void UpdateHealth(float lerp)
        {
            healthSlider.value = lerp;
        }
        public void SetCrosshair(Vector2 size, float aim)
        {
            ch_size = Vector2.Lerp(ch_size, size, ch_lerpSpeed * Time.deltaTime);
            crossshairCG.alpha = aim;
            crosshairTransform.sizeDelta = ch_size;
        }
    }
}
