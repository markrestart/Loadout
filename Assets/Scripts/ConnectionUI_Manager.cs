using Unity.Netcode;
using UnityEngine;

public class ConnectionUI_Manager : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    private void Start() {
        panel.SetActive(true);
    }
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
