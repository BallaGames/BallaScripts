using Balla.Core;
using Balla.Entity;
using Balla.Equipment;
using Balla.Gameplay.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Balla.Gameplay
{
    public class NetPlayer : BallaNetScript
    {
        public static Dictionary<ulong, NetPlayer> PlayerIDs;
        /// <summary>
        /// Retrieves the a <see cref="NetPlayer"/> from the PlayerID
        /// </summary>
        /// <param name="playerID">The ID of the player you're trying to get the object for</param>
        /// <returns></returns>
        public static NetPlayer GetPlayerByID(ulong playerID) => PlayerIDs[playerID];

        public PlayerEquipment equipment;
        public PlayerController controller;



        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            PlayerIDs ??= new Dictionary<ulong, NetPlayer>();
            PlayerIDs.Add(OwnerClientId, this);
        }
        public override void OnNetworkPreDespawn()
        {
            PlayerIDs.Remove(OwnerClientId);
        }
    }
}
