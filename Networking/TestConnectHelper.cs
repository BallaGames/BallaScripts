using Unity.Netcode;
using UnityEngine;

namespace Lunar
{
    public class TestConnectHelper : MonoBehaviour
    {

        bool isClient, isServer;

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            
            if(!isClient && !isServer)
            {
                if (GUILayout.Button("Start host"))
                {
                    isClient = isServer = NetworkManager.Singleton.StartHost();
                }
                if(GUILayout.Button("start client"))
                {
                    isClient = NetworkManager.Singleton.StartClient();
                }
                if(GUILayout.Button("start server"))
                {
                    isServer = NetworkManager.Singleton.StartServer();
                }
            }
            else
            {
                if (GUILayout.Button("Shutdown"))
                {
                    NetworkManager.Singleton.Shutdown();
                    isClient = isServer = false;
                }
            }
            GUILayout.EndVertical();
        }
    }
}
