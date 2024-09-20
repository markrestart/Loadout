using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Linq;

public class ConnectionUI_Manager : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private TMPro.TMP_InputField ipInput;

    private static string screenname = "";

    public static void SetScreenName(string name){
        screenname = name;
    }
    public static string GetScreenName(){
        return screenname;
    }

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

    public void IPInput(string ip)
    {
        //remove any whitespace, special, or alphabetic characters
        ip = new string(ip.Where(c => char.IsDigit(c) || c == '.').ToArray());
        ipInput.text = ip;

        //check if the ip is valid
        if(ip.Split('.').Length != 4){
            return;
        }

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip,7777);
    }
}
