using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

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
    private Player_Manager playerManager;
    [SerializeField]
    private SO_Equipment defaultEquipment;

    private static Dictionary<ulong, bool> readyState = new Dictionary<ulong, bool>();
    private static List<Reserves_Controller> instances = new List<Reserves_Controller>();
    public static List<Reserves_Controller> Instances { get => instances; }

    private void Start() {
        instances.Add(this);
    }

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
            int equipAbilities = 0;
            int maxAbilities = card.Archetype.abilitySlots;
            foreach(Transform child in reserveDisplay.transform){
                Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
                if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Archetype){
                    return false;
                }else if(iCard.IsToggled && iCard.Card.Weight > 0){
                    equipWeight += iCard.Card.Weight;
                    if(equipWeight > totalCarryWeight){
                        return false;
                    }
                }else if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Ability){
                    equipAbilities++;
                    if(equipAbilities >= maxAbilities){
                        return false;
                    }
                }
            }
        }else if(card.EType == DraftCardType.Ability){
            int totalAbilities = 3;
            int equipAbilities = 0;
            foreach(Transform child in reserveDisplay.transform){
                Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
                if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Archetype){
                    totalAbilities = iCard.Card.Archetype.abilitySlots;
                }
            }
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
        int maxAbilities = 3;
        foreach(Transform child in reserveDisplay.transform){
            Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
            if(iCard.IsToggled){
                if(iCard.Card.EType == DraftCardType.Archetype){
                    archetypeCount++;
                    totalCarryWeight = iCard.Card.Archetype.carryWeight;
                    maxAbilities = iCard.Card.Archetype.abilitySlots;
                }else if(iCard.Card.Weight > 0){
                    weightCount += iCard.Card.Weight;
                }else if(iCard.Card.EType == DraftCardType.Ability){
                    abilitiesCount++;
                }
            }
        }
        archetypeCountText.text =   $"{archetypeCount.ToString()}/1";
        weightCountText.text =      $"{weightCount.ToString()}/{totalCarryWeight.ToString()}";
        abilitiesCountText.text =   $"{abilitiesCount.ToString()}/{maxAbilities.ToString()}";
    }

    public void RemoveFromSelected(Draft_Card card){
        if(card.EType != DraftCardType.Archetype){
            return;
        }
        float totalCarryWeight = 100;
        float equipWeight = 0;
        int equipAbilities = 0;
        int maxAbilities = 3;
        foreach(Transform child in reserveDisplay.transform){
            Reserve_Card_Manager iCard = child.GetComponent<Reserve_Card_Manager>();
            if(iCard.IsToggled && iCard.Card.Weight > 0){
                equipWeight += iCard.Card.Weight;
                if(equipWeight > totalCarryWeight){
                    iCard.Deselect();
                    equipWeight -= iCard.Card.Weight;
                }
            }else if(iCard.IsToggled && iCard.Card.EType == DraftCardType.Ability){
                equipAbilities++;
                if(equipAbilities > maxAbilities){
                    iCard.Deselect();
                    equipAbilities--;
                }
            }
        }
    }

    private string reserveDataToString(int[][] reserveData){
        string str = "";
        foreach(var card in reserveData){
            str += card[0] + "." + card[1] + "." + card[2] + ",";
        }
        if(str.Length > 0){
            str = str.Remove(str.Length - 1);
        }
        return str;
    }

    private int[][] reserveDataFromString(string reserveData){
        if(reserveData.Length == 0){
            return new int[0][];
        }
        var cards = reserveData.Split(',');
        var cardData = new List<int[]>();
        foreach(var card in cards){
            var cardParts = card.Split('.');
            cardData.Add(new int[]{int.Parse(cardParts[0]), int.Parse(cardParts[1]), int.Parse(cardParts[2])});
        }
        return cardData.ToArray();
    }

    public void EnterEquipPhase(List<ulong> playerIds)
    {
        //Sync the reserves
        if(IsServer){
            var reserveData = reserves.Select(x => Draft_Manager.Instance.CardToIntArray(x)).ToArray();
            SyncReserveRPC(reserveDataToString(reserveData), playerIds.ToArray());
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

        EquipAndReadyRpc(NetworkManager.LocalClientId, reserveDataToString(equipedCards.Select(x => Draft_Manager.Instance.CardToIntArray(x)).ToArray()));
    }

    public void ResetRound(bool isSurvivor){
        playerManager.Unready();
        if(playerManager.Archetype != null){
            AddToReserves(new Draft_Card(playerManager.Archetype));
        }
        if(isSurvivor){
            foreach(var equipment in playerManager.Equipments){
                AddToReserves(new Draft_Card(equipment));
            }
        }
        playerManager.ClearLoadout();
    }

    [Rpc(SendTo.Everyone)]
    public void EquipAndReadyRpc(ulong player, string cardsAsStr){
        var cardsAsArr = reserveDataFromString(cardsAsStr);
        var equipedCards = cardsAsArr.Select(x => Draft_Manager.Instance.IntArrayToCard(x)).ToArray();
        playerManager.AddEquipment(new Data_Equipment(defaultEquipment));
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
        foreach(var instance in instances){
            instance.playersCountText.text = $"{readyState.Count(x => x.Value)}/{readyState.Count}";
        }

        if(IsServer && readyState.All(x => x.Value)){
            StartRoundRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StartRoundRpc(){
        foreach(var instance in instances){
            instance.StartRoundLocal();
        }
        Rounds_Manager.Instance.StartRound();
    }

    public void StartRoundLocal(){
        if(IsOwner){
            equipMenu.SetActive(false);
            playerManager.Ready();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SyncReserveRPC(string reserveData, ulong[] playerIds){
        var reserveDataArr = reserveDataFromString(reserveData);
        reserves = new List<Draft_Card>();
        foreach(var cardData in reserveDataArr){
            var card = Draft_Manager.Instance.IntArrayToCard(cardData);
            reserves.Add(card);
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
            readyState = playerIds.ToDictionary(x => x, x => false);
            playersCountText.text = $"0/{readyState.Count}";
        }
    }
}
