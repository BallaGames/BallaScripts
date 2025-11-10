using Balla.Core;
using Balla.Gameplay;
using UnityEngine;

namespace Balla
{
    public class SessionManager : BallaScript
    {
        private void Awake()
        {
            QueryHelper.Initialise();

        }
    }
}
