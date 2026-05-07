using Balla.Core;
using Balla.Equipment;
using Balla.Gameplay.Player;
using Balla.UI;
using UnityEngine;

namespace Balla
{
    /// <summary>
    /// Attach this component to the player.
    /// </summary>
    public class PlayerUIBridge : BallaScript
    {
        public PlayerController controller;
        public PlayerEquipment equipment;

        protected override void AfterFrame()
        {
            if (PlayerUI.Instance == null)
                return;


            if (equipment.CurrentWeapon != null)
            {
                PlayerUI.Instance.SetCrosshair(equipment.CurrentWeapon.CrosshairSize);

                //if(equipment.CurrentWeapon.useCharge || equipment.CurrentWeapon.useHeat)
                //    PlayerUI.Instance.UpdateBars(equipment.CurrentWeapon.CurrentCharge, 
                //        equipment.CurrentWeapon.HeatLevel.lerp, 
                //        equipment.CurrentWeapon.isOverheated);


                PlayerUI.Instance.UpdateBars(equipment.CurrentWeapon);

            }

            PlayerUI.Instance.UpdateHealth(equipment.entity.HealthLerp);
        }
    }
}
