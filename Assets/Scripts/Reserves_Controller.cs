using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class Reserves_Controller : NetworkBehaviour
{
    private List<Draft_Card> reserves = new List<Draft_Card>();
    [SerializeField]
    private GameObject reserveCardPrefab;
    [SerializeField]
    private GameObject equipMenu;
    [SerializeField]
    private GameObject reserveDisplay;
    [SerializeField]
    private TMPro.TextMeshProUGUI archetypeCountText;
    [SerializeField]
    private TMPro.TextMeshProUGUI weightCountText;
    [SerializeField]
    private TMPro.TextMeshProUGUI playersCountText;
    [SerializeField]
    private TMPro.TextMeshProUGUI abilitiesCountText;
    [SerializeField]
    private Draft_Manager draftManager;
    [SerializeField]
    private Player_Manager playerManager;

    private Dictionary<ulong, bool> readyState = new Dictionary<ulong, bool>();

    public List<Draft_Card> GetReserves(){
        return reserves;
    }

    public void AddToReserves(Draft_Card card){
        reserves.Add(card);
    }

    public void RemoveFromReserves(Draft_Card card){
        reserves.Remove(reserves.First(x => x == card));
    }

    public bool TryAddToSelected(Draft_Card card){
        if(card.EType == DraftCardType.Archetype){
            float totalCarryWeight = card.Archetype.carryWeight;
            float equipWeight = 0;
            foreach(Transform child in reserveDisplay.transform){
                Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
                if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Archetype){
                    return false;
                }else if(iCard.IsToggled && iCard.Card.Weight > 0){
                    equipWeight += iCard.Card.Weight;
                    if(equipWeight > totalCarryWeight){
                        return false;
                    }
                }
            }
        }else if(card.EType == DraftCardType.Ability){
            int totalAbilities = 3;
            int equipAbilities = 0;
            foreach(Transform child in reserveDisplay.transform){
                Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
                if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Ability){
                    equipAbilities++;
                    if(equipAbilities >= totalAbilities){
                        return false;
                    }
                }
            }
        }else if(card.EType == DraftCardType.Equipment || card.EType == DraftCardType.Ammo || card.EType == DraftCardType.Armor){
            float totalCarryWeight = 100;
            float equipWeight = card.Weight;
            foreach(Transform child in reserveDisplay.transform){
                Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
                if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Archetype){
                    totalCarryWeight = iCard.Card.Archetype.carryWeight;
                }
            }
            foreach(Transform child in reserveDisplay.transform){
                Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
                if(iCard.IsToggled && iCard.Card.Weight > 0){
                    equipWeight += iCard.Card.Weight;
                    if(equipWeight >= totalCarryWeight){
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void UpdateEquipTexts(){
        int archetypeCount = 0;
        float weightCount = 0;
        int abilitiesCount = 0;
        float totalCarryWeight = 100;
        foreach(Transform child in reserveDisplay.transform){
            Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
            if(iCard.IsToggled){
                if(iCard.Card.EType == DraftCardType.Archetype){
                    archetypeCount++;
                }else if(iCard.Card.Weight > 0){
                    weightCount += iCard.Card.Weight;
                }else if(iCard.Card.EType == DraftCardType.Ability){
                    abilitiesCount++;
                }
            }
        }
        archetypeCountText.text =   $"{archetypeCount.ToString()}/1";
        weightCountText.text =      $"{weightCount.ToString()}/{totalCarryWeight.ToString()}";
        abilitiesCountText.text =   $"{abilitiesCount.ToString()}/3";
    }

    public void RemoveFromSelected(Draft_Card card){
        if(card.EType != DraftCardType.Archetype){
            return;
        }
        float totalCarryWeight = 100;
        float equipWeight = 0;
        foreach(Transform child in reserveDisplay.transform){
            Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
            if(iCard.IsToggled && iCard.Card.Weight > 0){
                equipWeight += iCard.Card.Weight;
                if(equipWeight > totalCarryWeight){
                    iCard.Deselect();
                }
                equipWeight -= iCard.Card.Weight;
            }
        }
    }

    public void EnterEquipPhase(List<ulong> numberOfPlayers)
    {
        //Sync the reserves
        if(IsServer){
            var reserveData = reserves.Select(x => draftManager.CardToIntArray(x)).ToArray();
            SyncReserveRPC(reserveData);
        }

        if(IsOwner){
            //unready the player, free the mouse
            transform.GetComponent<Player_Manager>().Unready();
            //display the equip menu
            equipMenu.SetActive(true);

            //Remove all children from the reserve display
            foreach(Transform child in reserveDisplay.transform){
                Destroy(child.gameObject);
            }
            //display the reserves
            foreach(var card in reserves){
                var cardObj = Instantiate(reserveCardPrefab, reserveDisplay.transform);
                cardObj.GetComponent<Reserve_Card_Manager>().Card = card;
                cardObj.GetComponent<Reserve_Card_Manager>().ReservesController = this;
            }

            //display the player count
            readyState = numberOfPlayers.ToDictionary(x => x, x => false);
            playersCountText.text = $"0/{readyState.Count}";
        }
    }

    public void EquipAndReady(){
        var equipedCards = new List<Draft_Card>();
        foreach(Transform child in reserveDisplay.transform){
            Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
            if(iCard.IsToggled){
                equipedCards.Add(iCard.Card);
            }
        }

        EquipAndReadyRpc(NetworkManager.LocalClientId, equipedCards.Select(x => draftManager.CardToIntArray(x)).ToArray());
    }

    [Rpc(SendTo.Everyone)]
    public void EquipAndReadyRpc(ulong player, ushort[][] cardsAsArr){
        var equipedCards = cardsAsArr.Select(x => draftManager.IntArrayToCard(x)).ToArray();
        foreach(var card in equipedCards){
            switch(card.EType){
                case DraftCardType.Archetype:
                    playerManager.SetArchetype(card.Archetype);
                    break;
                case DraftCardType.Ability:
                    playerManager.AddAbility(card.Ability);
                    break;
                case DraftCardType.Equipment:
                    playerManager.AddEquipment(card.Equipment);
                    break;
                case DraftCardType.Ammo:
                    playerManager.AddAmmo(card.AmmoType, card.AmmoAmount);
                    break;
                case DraftCardType.Armor:
                    playerManager.AddArmor(card.Armor);
                    break;
            }
            reserves.Remove(reserves.First(x => x == card));
        }

        readyState[player] = true;
        playersCountText.text = $"{readyState.Count(x => x.Value)}/{readyState.Count}";

        if(IsServer && readyState.All(x => x.Value)){
            StartRoundRpc();
        }
    }

    //TODO: This needs to be refactored to specify the local player's object
    [Rpc(SendTo.Everyone)]
    public void StartRoundRpc(){
        equipMenu.SetActive(false);
        playerManager.Ready();
    }

    [Rpc(SendTo.Everyone)]
    public void SyncReserveRPC(ushort[][] reserveData){
        reserves = new List<Draft_Card>();
        foreach(var cardData in reserveData){
            var card = draftManager.IntArrayToCard(cardData);
            reserves.Add(card);
        }
    }
}
