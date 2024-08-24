using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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

    private static Dictionary<ulong, List<Draft_Card>> draftState = new Dictionary<ulong, List<Draft_Card>>();
    private static Dictionary<ulong, bool> readyState = new Dictionary<ulong, bool>();
    private static List<ulong> players = new List<ulong>();
    private static Dictionary<ulong,Reserves_Controller> reservesControllers = new Dictionary<ulong, Reserves_Controller>();
    // Start is called before the first frame update
    void Start()
    {
        //Disable this instance if it is not on the owner client
        if(OwnerClientId != NetworkManager.Singleton.LocalClientId){
            gameObject.SetActive(false);
        }

        if(IsServer){
            players.Add(OwnerClientId);
            readyState.Add(OwnerClientId, false);
            reservesControllers.Add(OwnerClientId, transform.GetComponent<Reserves_Controller>());
        }else{
            startDraftButton.gameObject.SetActive(false);
        }
        cardManagers.ForEach(x => x.gameObject.SetActive(false));
    }

    public void BeginDraft(){
        if(IsServer){
            var draftCards = GenerateDraftCards(players.Count * 15);
            draftCards = Shuffler.Shuffle(draftCards);
            for(int i = 0; i < players.Count; i++){
                draftState.Add(players[i], new List<Draft_Card>(draftCards.GetRange(i * 15, 15)));
            }
            //Remove the start draft button
            startDraftButton.gameObject.SetActive(false);
            //Display the draft cards
            var draftPacksAsIntArray = draftState.Values.Select(x => x.Select(y => CardToIntArray(y)).ToArray()).ToArray();
            SendStateAndDisplayDraftRpc(draftState.Keys.ToArray(), draftPacksAsIntArray);
        }
    }

    //TODO: This probaly needs to be refactored to specify the local player's objects
    [Rpc(SendTo.Everyone)]
    void EndDraftRpc(ulong[] playersArr){
        //Put all players into the equiping phase
        GetComponent<Reserves_Controller>().EnterEquipPhase(playersArr.ToList());

        //Disable the draft manager
        draftUI.gameObject.SetActive(false);
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

            var draftPacksAsIntArray = draftState.Values.Select(x => x.Select(y => CardToIntArray(y)).ToArray()).ToArray();
            SendStateAndDisplayDraftRpc(draftState.Keys.ToArray(), draftPacksAsIntArray);
        }
    }

    // When a player selects a card, use an rpc to tell the server to remove it from the draftState. Add it to the player's reserves.

    [Rpc(SendTo.Server)]
    public void SelectCardRpc(ulong player, ushort[] cardAsArr){
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
            RotateDraft();
            }
        }

    }

    public void SelectCard(Draft_Card card){
        //Turn off the card managers
        foreach(var cardManager in cardManagers){
            cardManager.gameObject.SetActive(false);
        }

        SelectCardRpc(OwnerClientId, CardToIntArray(card));
    }
            
    [Rpc(SendTo.Everyone)]
    private void SendStateAndDisplayDraftRpc(ulong[] players, ushort[][][] draftPacks){

        var draftPacksAsCards = new List<List<Draft_Card>>();
        foreach(var pack in draftPacks){
            var draftPack = new List<Draft_Card>();
            foreach(var card in pack){
                draftPack.Add(IntArrayToCard(card));
            }
            draftPacksAsCards.Add(draftPack);
        }
        draftState = players.Zip(draftPacksAsCards, (x, y) => new KeyValuePair<ulong, List<Draft_Card>>(x, y)).ToDictionary(x => x.Key, x => x.Value);
        
        var numCardsToDisplay = draftState[OwnerClientId].Count % 5;
        if (numCardsToDisplay == 0){
            numCardsToDisplay = 5;
        }

        for(int i = 0; i < 5; i++){
            if(i >= numCardsToDisplay){
                cardManagers[i].gameObject.SetActive(false);
                continue;
            }
            cardManagers[i].gameObject.SetActive(true);
            cardManagers[i].SetCard(draftState[OwnerClientId][i]);
        }
    }

    public ushort[] CardToIntArray(Draft_Card card){
        var arr = new ushort[3];
        arr[0] = (ushort)card.EType;
        switch(card.EType){
            case DraftCardType.Equipment:
                arr[1] = (ushort)WeaponPool.FindIndex(x => x.equipmentName == card.Equipment.equipmentName);
                arr[2] = 0;
                break;
            case DraftCardType.Ability:
                arr[1] = (ushort)AbilityPool.FindIndex(x => x.abilityName == card.Ability.abilityName);
                arr[2] = 0;
                break;
            case DraftCardType.Archetype:
                arr[1] = (ushort)ArchetypePool.FindIndex(x => x.archetypeName == card.Archetype.archetypeName);
                arr[2] = 0;
                break;
            case DraftCardType.Ammo:
                arr[1] = (ushort)card.AmmoType;
                arr[2] = (ushort)card.AmmoAmount;
                break;
            case DraftCardType.Armor:
                arr[1] = (ushort)card.Armor;
                arr[2] = 0;
                break;
        }
        return arr;
    }

    public Draft_Card IntArrayToCard(ushort[] arr){
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
        //Generate 45% Weapons, 10% Armor, 15% Ammo, 20% Abilities, 10% Archetypes
        List<Draft_Card> draftCards = new List<Draft_Card>();
        for(int i = 0; i < numberOfCards; i++){
            float percent = (float)i/numberOfCards;
            if(percent <= 0.25f){
                //Weapon
                draftCards.Add(new Draft_Card(new Data_Equipment(WeaponPool[UnityEngine.Random.Range(0, WeaponPool.Count)])));
            }else if(percent <= 0.30f){
                //Armor
                draftCards.Add(new Draft_Card(ArmorPool[UnityEngine.Random.Range(0, ArmorPool.Count)]));
            }else if(percent <= 0.45f){
                //Ammo TODO: Hardcoded ammo amounts should be replaced with a more dynamic system
                var amount = 0;
                var ammoType = AmmoPool[UnityEngine.Random.Range(0, AmmoPool.Count)];
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
            }else if(percent <= 0.8f){
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

