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
            PlayerUI.Instance.crosshairSize = equipment.CurrentWeapon.CrosshairSize;
        }
    }
}
