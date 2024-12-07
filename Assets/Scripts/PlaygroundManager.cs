using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlaygroundManager : NetworkBehaviour
{
    public static PlaygroundManager Instance { get; private set; }
    public bool isPlayground = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Instance == null){
            Instance = this;
        }else{
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupPlayground(){
        Message_System.AddMessage("Setting up playground...");

        // Skip drafting phase
        Draft_Manager.Instance.draftUI.gameObject.SetActive(false);

        // Give the player one of each card
        Reserves_Controller reserves = GameObject.FindAnyObjectByType<Reserves_Controller>();
        List<Draft_Card> cards = new List<Draft_Card>();

        foreach(SO_Archetype c in Draft_Manager.Instance.ArchetypePool){
            reserves.AddToReserves(new Draft_Card(new Data_Archetype(c)));
        }

        foreach(SO_Ability c in Draft_Manager.Instance.AbilityPool){
            reserves.AddToReserves(new Draft_Card(new Data_Ability(c)));
        }

        foreach(SO_Equipment c in Draft_Manager.Instance.WeaponPool){
            reserves.AddToReserves(new Draft_Card(new Data_Equipment(c)));
        }

        foreach(int c in Draft_Manager.Instance.ArmorPool){
            reserves.AddToReserves(new Draft_Card(c));
        }

        foreach(DraftableAmmo c in Draft_Manager.Instance.AmmoPool){
            reserves.AddToReserves(new Draft_Card(new System.Tuple<AmmoType, int>(c.ammoType, c.maxAmount)));
        }

        // Start the game
        if(Steamworks.SteamClient.IsValid){
            Rounds_Manager.Instance.RegisterNameRpc(NetworkManager.Singleton.LocalClientId, Steamworks.SteamClient.Name);
        }else{
            Rounds_Manager.Instance.RegisterNameRpc(NetworkManager.Singleton.LocalClientId, "Player " + NetworkManager.Singleton.LocalClientId);
        }
        reserves.SyncReserves(Draft_Manager.Instance.Players, true);
    }


}
