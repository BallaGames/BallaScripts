using Balla.Core;
using Balla.Equipment;
using Balla.Gameplay.Player;
using UnityEngine;

namespace Balla
{
    public class PlayerAnimator : BallaScript
    {
        public PlayerEquipment pe;
        public PlayerController pc;

        private void Start()
        {
            if(pe == null)
            {
                GetComponent<PlayerEquipment>();
            }
        }
    }
}
