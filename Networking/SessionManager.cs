using Balla.Core;
using Balla.Gameplay;
using UnityEngine;

namespace Balla
{
    public class SessionManager : BallaNetScript
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            QueryHelper.Initialise();
        }
    }
}
