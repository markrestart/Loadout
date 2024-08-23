using Unity.Netcode;
using UnityEngine;

public class ConnectionUI_Manager : MonoBehaviour
{
    // Host a game using Unity Netcode for GameObjects
    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
    }

    // Join a game using Unity Netcode for GameObjects
    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
    }
}
