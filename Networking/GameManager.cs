using Balla.Core;
using Balla.Gameplay;
using UnityEngine;

namespace Balla
{
    public class GameManager : BallaScript
    {
        public static GameManager gameManager;
        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = this;
                QueryHelper.Initialise();

                if (Input)
                {
                    Input.OnPause += PauseReceived;
                }
                Input.SetPause(false);
            }
        }
        void PauseReceived(bool paused)
        {

        }
    }
}
