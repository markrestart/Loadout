using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamworksManager : MonoBehaviour
{
    public static SteamworksManager Instance;
    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null){
            Instance = this;
        }else{
            Destroy(this);
        }

        try
        {
            Steamworks.SteamClient.Init( CONSTANTS.STEAM_APP_ID );
            Debug.Log("Steamworks initialized");
        }
        catch ( System.Exception e )
        {
            Debug.Log("Steamworks failed to initialize");
            // Something went wrong - it's one of these:
            //
            //     Steam is closed?
            //     Can't find steam_api dll?
            //     Don't have permission to play app?
            //
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Steamworks.SteamClient.IsValid){
            Steamworks.SteamClient.RunCallbacks();
        }
    }

    private void OnApplicationQuit() {
        Steamworks.SteamClient.Shutdown();
    }
}
