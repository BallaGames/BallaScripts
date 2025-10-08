using UnityEngine;

namespace Balla.Equipment
{
    /// <summary>
    /// The BaseDataEquip class is primarily going to be used for perks or to modify certain behaviours on the player, such as adding new stats.
    /// </summary>
    public abstract class BaseDataEquip
    {
        protected PlayerEquipment player;
        /// <summary>
        /// If any behaviour requires a reference to the player, do it after base or reference the argument.
        /// </summary>
        /// <param name="player"></param>
        public virtual void AssignData(PlayerEquipment player)
        {
            this.player = player;
        }
        /// <summary>
        /// If any behaviour requires a reference to the player, do it before base or reference the argument.
        /// </summary>
        /// <param name="player"></param>
        public virtual void RemoveData(PlayerEquipment player)
        {
            player = null;
        }
    }
}
