using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;

public class ConnectionUI_Manager : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private TMPro.TMP_InputField roomCodeInput;
    [SerializeField]
    private Draft_Manager draftManager;
    [SerializeField]
    private TMPro.TMP_Text roomCodeText;

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
    public async void HostGame()
    {
        var roomCode = await StartHostWithRelay();
        if (!string.IsNullOrEmpty(roomCode)){
            Message_System.LocalMessage("Join code: " + roomCode);
            draftManager.enabled = true;
            roomCodeText.text = "Join code: " + roomCode;
            gameObject.SetActive(false);
        }
    }

    // Join a game using Unity Netcode for GameObjects
    public async void JoinGame()
    {
        var connected = await StartClientWithRelay();
        if (connected){
            draftManager.enabled = true;
            gameObject.SetActive(false);
        }
    }

    public async Task<string> StartHostWithRelay()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Unity.Services.Relay.Models.Allocation allocation = await RelayService.Instance.CreateAllocationAsync(CONSTANTS.MAX_PLAYERS);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay()
    {
        var joinCode = roomCodeInput.text;
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
