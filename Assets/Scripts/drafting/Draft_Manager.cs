using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Draft_Manager : NetworkBehaviour
{
    public List<SO_Equipment> WeaponPool;
    public List<SO_Ability> AbilityPool;
    public List<SO_Archetype> ArchetypePool;
    public List<int> ArmorPool;
    public List<AmmoType> AmmoPool;
    [SerializeField]
    private List<Card_Manager> cardManagers;
    [SerializeField]
    private GameObject startDraftButton;
    [SerializeField]
    private GameObject draftUI;
    [SerializeField]
    private TMPro.TextMeshProUGUI draftedCardsText;

    private Dictionary<ulong, List<Draft_Card>> draftState = new Dictionary<ulong, List<Draft_Card>>();
    private Dictionary<ulong, bool> readyState = new Dictionary<ulong, bool>();
    private List<ulong> players = new List<ulong>();
    private Dictionary<ulong,Reserves_Controller> reservesControllers = new Dictionary<ulong, Reserves_Controller>();
    // Start is called before the first frame update
    public static Draft_Manager Instance;
    void Start()
    {
        draftUI.gameObject.SetActive(true);
        if(!IsServer){
            startDraftButton.gameObject.SetActive(false);
        }
        cardManagers.ForEach(x => x.gameObject.SetActive(false));

        if(IsServer){
            AddPlayerRpc(NetworkManager.Singleton.LocalClientId);
            Message_System.AddMessage("Player has joined the draft. Total players: " + players.Count);
        }

        if(Instance == null){
            Instance = this;
        }else{
            Destroy(this);
        }
    }

    private bool autoDraft = false;
    void Update(){
        if(Input.GetKeyDown(KeyCode.F12)){
            autoDraft = !autoDraft;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer){
            AddPlayerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void AddPlayerRpc(ulong clientId){
        players.Add(clientId);
        readyState.Add(clientId, false);
        //Find the reserves controller for the player
        StartCoroutine(FindReservesController(clientId));
    }

    private IEnumerator FindReservesController(ulong clientId){
        yield return new WaitForSeconds(1);
        var reservesController = FindObjectsOfType<Reserves_Controller>().FirstOrDefault(x => x.OwnerClientId == clientId);
        if(reservesController != null){
            reservesControllers.Add(clientId, reservesController);
        }else{
        StartCoroutine(FindReservesController(clientId));
        }
    }


    public void BeginDraft(){
        if(IsServer){
            RequestNamesRpc();
            var draftCards = GenerateDraftCards(players.Count * (CONSTANTS.PACKS_PER_DRAFT * CONSTANTS.CARDS_PER_PACK));
            draftCards = Shuffler.Shuffle(draftCards);
            for(int i = 0; i < players.Count; i++){
                draftState.Add(players[i], new List<Draft_Card>(draftCards.GetRange(i * (CONSTANTS.PACKS_PER_DRAFT * CONSTANTS.CARDS_PER_PACK), (CONSTANTS.PACKS_PER_DRAFT * CONSTANTS.CARDS_PER_PACK))));
            }
            //TODO: Revisit this. Currently, any archetypes are moved to the front of the draftState list for each player
            foreach(var player in draftState){
                var archetypes = player.Value.Where(x => x.EType == DraftCardType.Archetype).ToList();
                foreach(var archetype in archetypes){
                    player.Value.Remove(archetype);
                    player.Value.Insert(0, archetype);
                }
            }
            //Remove the start draft button
            startDraftButton.gameObject.SetActive(false);
            ConnectionUI_Manager.Instance.CloseLobbyToNewMembers();
            //Display the draft cards
            SendStateAndDisplayDraftRpc(stateToString(draftState));
        }
    }

    [Rpc(SendTo.Everyone)]
    private void RequestNamesRpc(){
        Rounds_Manager.Instance.RegisterNameRpc(NetworkManager.Singleton.LocalClientId, Steamworks.SteamClient.Name);
    }

    [Rpc(SendTo.Everyone)]
    void EndDraftRpc(ulong[] playersList){
        //Put all players into the equiping phase
        foreach(var reservesController in reservesControllers){
            reservesController.Value.EnterEquipPhase(playersList.ToList());
        }
        SetPlayerSkins();

        //Disable the draft manager
        draftUI.gameObject.SetActive(false);
    }

    private void SetPlayerSkins(){
        if(IsServer){
            foreach(var reservesController in reservesControllers){
                var playerID = reservesController.Key;
                var skinIndex = Random.Range(0, 100);
                var colorIndex = (int)playerID;
                SetPlayerSkinsRpc(playerID, skinIndex, colorIndex);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetPlayerSkinsRpc(ulong playerID, int skinIndex, int colorIndex){
        var reservesController = reservesControllers[playerID];
        reservesController.GetComponent<Player_Skin_Manager>().SetSkin(skinIndex, colorIndex);
    }

//TODO: have the draft rotation alternate between clockwise and counter clockwise
    void RotateDraft(){
        if(IsServer){
            //move each player's draftState to the next player
            var temp = draftState[players[0]];
            for(int i = 0; i < players.Count - 1; i++){
                draftState[players[i]] = draftState[players[i + 1]];
            }
            draftState[players[players.Count - 1]] = temp;

            SendStateAndDisplayDraftRpc(stateToString(draftState));
            Message_System.AddMessage($"{draftState[NetworkManager.Singleton.LocalClientId].Count} cards left to draft");
        }
    }

    // When a player selects a card, use an rpc to tell the server to remove it from the draftState. Add it to the player's reserves.

    [Rpc(SendTo.Server)]
    public void SelectCardRpc(ulong player, int[] cardAsArr){
        var card = IntArrayToCard(cardAsArr);
        //Add the card to the player's reserves
        reservesControllers[player].AddToReserves(card);
        //Remove the first instance of the card from the player's draftState using the equality operator
        draftState[player].Remove(draftState[player].First(x => x == card));
        //Set the player's ready state to true
        readyState[player] = true;

        //if all players have selected a card(based on ready state), move to the next round
        if(readyState.Values.All(x => x == true)){
            if(draftState[players[0]].Count == 0){
                EndDraftRpc(players.ToArray());
            }else{
                readyState = readyState.ToDictionary(x => x.Key, x => false);
                RotateDraft();
            }
        }

    }

    private string stateToString(Dictionary<ulong, List<Draft_Card>> state){
        string str = "";
        foreach(var player in state){
            str += player.Key + ":";
            foreach(var card in player.Value){
                int[] cardAsArr = CardToIntArray(card);
                str += cardAsArr[0] + "." + cardAsArr[1] + "." + cardAsArr[2] + ",";
            }
            if(str.Length > 0){
                str = str.Remove(str.Length - 1);
            }
            str += "\n";
        }
        if(str.Length > 0){
            str = str.Remove(str.Length - 1);
        }
        return str;
    }

    private Dictionary<ulong, List<Draft_Card>> stateFromString(string str){
        if(str.Length == 0){
            return new Dictionary<ulong, List<Draft_Card>>();
        }
        var state = new Dictionary<ulong, List<Draft_Card>>();
        var playerStates = str.Split('\n');
        foreach(var playerState in playerStates){
            var playerStateArr = playerState.Split(':');
            var player = ulong.Parse(playerStateArr[0]);
            var cards = playerStateArr[1].Split(',');
            var playerCards = new List<Draft_Card>();
            foreach(var card in cards){
                var cardArr = card.Split('.');
                var cardAsArr = new int[3];
                cardAsArr[0] = int.Parse(cardArr[0]);
                cardAsArr[1] = int.Parse(cardArr[1]);
                cardAsArr[2] = int.Parse(cardArr[2]);
                playerCards.Add(IntArrayToCard(cardAsArr));
            }
            state.Add(player, playerCards);
        }
        return state;
    }

    public void SelectCard(Draft_Card card){
        //Turn off the card managers
        foreach(var cardManager in cardManagers){
            cardManager.gameObject.SetActive(false);
        }

        SelectCardRpc(NetworkManager.Singleton.LocalClientId, CardToIntArray(card));
    }
            
    [Rpc(SendTo.Everyone)]
    private void SendStateAndDisplayDraftRpc(string stateAsString){
        draftState = stateFromString(stateAsString);
        var numCardsToDisplay = draftState[NetworkManager.Singleton.LocalClientId].Count % CONSTANTS.CARDS_PER_PACK;
        if (numCardsToDisplay == 0){
            numCardsToDisplay = CONSTANTS.CARDS_PER_PACK;
        }

        for(int i = 0; i < CONSTANTS.CARDS_PER_PACK; i++){
            if(i >= numCardsToDisplay){
                cardManagers[i].gameObject.SetActive(false);
                continue;
            }
            cardManagers[i].gameObject.SetActive(true);
            cardManagers[i].SetCard(draftState[NetworkManager.Singleton.LocalClientId][i]);
        }

        var drafted = reservesControllers[NetworkManager.Singleton.LocalClientId].Reserves;
        var draftedText = "Drafted: ";
        foreach(var card in drafted){
            draftedText += card.Name + ", ";
        }
        if(draftedText.Length > 0){
            draftedText.Remove(draftedText.Length - 2);
        }
        draftedCardsText.text = draftedText;

        //Testing utilty code
        if(autoDraft){
            SelectCard(draftState[NetworkManager.Singleton.LocalClientId][0]);
        }
    }

    public int[] CardToIntArray(Draft_Card card){
        var arr = new int[3];
        arr[0] = (int)card.EType;
        switch(card.EType){
            case DraftCardType.Equipment:
                arr[1] = (int)WeaponPool.FindIndex(x => x.equipmentName == card.Equipment.equipmentName);
                arr[2] = 0;
                break;
            case DraftCardType.Ability:
                arr[1] = (int)AbilityPool.FindIndex(x => x.abilityName == card.Ability.abilityName);
                arr[2] = 0;
                break;
            case DraftCardType.Archetype:
                arr[1] = (int)ArchetypePool.FindIndex(x => x.archetypeName == card.Archetype.archetypeName);
                arr[2] = 0;
                break;
            case DraftCardType.Ammo:
                arr[1] = (int)card.AmmoType;
                arr[2] = (int)card.AmmoAmount;
                break;
            case DraftCardType.Armor:
                arr[1] = (int)card.Armor;
                arr[2] = 0;
                break;
        }
        return arr;
    }

        public Draft_Card IntArrayToCard(int[] arr){
        DraftCardType type = (DraftCardType)arr[0];
        switch(type){
            case DraftCardType.Equipment:
                return new Draft_Card(new Data_Equipment(WeaponPool[arr[1]]));
            case DraftCardType.Ability:
                return new Draft_Card(new Data_Ability(AbilityPool[arr[1]]));
            case DraftCardType.Archetype:
                return new Draft_Card(new Data_Archetype(ArchetypePool[arr[1]]));
            case DraftCardType.Ammo:
                return new Draft_Card(new System.Tuple<AmmoType, int>((AmmoType)arr[1], arr[2]));
            case DraftCardType.Armor:
                return new Draft_Card(arr[1]);
        }
        return null;
    }

    List<Draft_Card> GenerateDraftCards(int numberOfCards){
        //Generate 50% Weapons, 10% Armor, 10% Ammo, 20% Abilities, 10% Archetypes
        List<Draft_Card> draftCards = new List<Draft_Card>();
        for(int i = 0; i < numberOfCards; i++){
            float percent = (float)i/numberOfCards;
            if(percent <= 0.5f){
                //Weapon
                draftCards.Add(new Draft_Card(new Data_Equipment(WeaponPool[UnityEngine.Random.Range(0, WeaponPool.Count)])));
            }else if(percent <= 0.6f){
                //Armor
                draftCards.Add(new Draft_Card(ArmorPool[UnityEngine.Random.Range(0, ArmorPool.Count)]));
            }else if(percent <= 0.7f){
                //Ammo TODO: Hardcoded ammo amounts should be replaced with a more dynamic system
                var amount = 0;
                List<AmmoType> RestrictedAmmoPool = new List<AmmoType>(AmmoPool);
                foreach(AmmoType a in AmmoPool){
                    //If the ammo type is not on a weapon that has been added to the draft pool, remove it
                    if(!draftCards.Any(x => x.EType == DraftCardType.Equipment && x.Equipment.ammoType == a)){
                        RestrictedAmmoPool.Remove(a);
                    }
                }
                var ammoType = RestrictedAmmoPool[UnityEngine.Random.Range(0, RestrictedAmmoPool.Count)];
                switch(ammoType){
                    case AmmoType.Bullet:
                        amount = UnityEngine.Random.Range(30, 100);
                        break;
                    case AmmoType.Shell:
                        amount = UnityEngine.Random.Range(5, 20);
                        break;
                    case AmmoType.Rocket:
                        amount = UnityEngine.Random.Range(1, 3);
                        break;
                    case AmmoType.Energy:
                        amount = UnityEngine.Random.Range(50, 200);
                        break;
                    case AmmoType.Arrow:
                        amount = UnityEngine.Random.Range(10, 30);
                        break;
                }
                draftCards.Add(new Draft_Card(new System.Tuple<AmmoType, int>(ammoType, amount)));
            }else if(percent <= 0.9f){
                //Ability
                draftCards.Add(new Draft_Card(new Data_Ability(AbilityPool[UnityEngine.Random.Range(0, AbilityPool.Count)])));
            }else{
                //Archetype
                draftCards.Add(new Draft_Card(new Data_Archetype(ArchetypePool[UnityEngine.Random.Range(0, ArchetypePool.Count)])));
            }
        }
        return draftCards;
    }
}

