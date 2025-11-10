using UnityEngine;

namespace Balla
{
    /// <summary>
    /// Spawns the objects necessary for the game to start when the host begins the game
    /// </summary>
    public class GameSpawner : MonoBehaviour
    {
        public GameObject[] objectsToSpawn;

        public void GameStarted()
        {
            for (int i = 0; i < objectsToSpawn.Length; i++)
            {
                Instantiate(objectsToSpawn[i]);
            }
        }
    }
}
