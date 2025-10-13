using Unity.Netcode;
using UnityEngine;

namespace Balla
{
    /// <summary>
    /// Spawns the objects necessary for the game to start when the host begins the game
    /// </summary>
    public class GameSpawner : MonoBehaviour
    {
        public NetworkObject[] objectsToSpawn;

        private void Start()
        {
            NetworkManager.Singleton.OnClientStarted += ConnectionStarted;
        }

        private void ConnectionStarted()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            for (int i = 0; i < objectsToSpawn.Length; i++)
            {
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(objectsToSpawn[i]);
            }
        }
    }
}
