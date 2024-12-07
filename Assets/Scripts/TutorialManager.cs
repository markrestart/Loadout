using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public bool isTutorial = false;
    public Button equipReadyButton;
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

    public async void SetupTutorial(){
        await Task.Delay(100);
        var playerManager = GameObject.FindAnyObjectByType<Player_Manager>();
        var reservesController = GameObject.FindAnyObjectByType<Reserves_Controller>();
        equipReadyButton.interactable = false;
        // Setup the tutorial
        Message_System.AddTutorialMessage("Welcome to the LOADOUT!", true);
        Message_System.AddTutorialMessage("This is the tutorial, where you will learn the basics of the game.", true);
        Message_System.AddTutorialMessage("The first thing to know about LOADOUT is the phases.", true);
        Message_System.AddTutorialMessage("The game is split into 3 phases: Draft, Equip, and Eliminate.", true);
        Message_System.AddTutorialMessage("We start here, in the draft phase. Where you pick what Archetypes, Equipment, and Abilities you'll have for the entire match.", true);
        int startID = Message_System.AddTutorialMessage("First, you need to press the Start Draft button.", false);
        // Add a listener to clear the message when the player presses the start draft button
        Draft_Manager.Instance.OnDraftStart.AddListener(() => Message_System.Instance.ClearTutorialMessage(startID));
        int pickID = Message_System.AddTutorialMessage("Try picking one of the items displayed now.", false);
        // Add a listener to clear the message when the player picks an item
        Draft_Manager.Instance.OnDraftPick.AddListener(() => Message_System.Instance.ClearTutorialMessage(pickID));
        Message_System.AddTutorialMessage("Great! You've picked an item. When you pick an item in the draft, it's added to your reserves. You can see below what you've drafted", true);
        Message_System.AddTutorialMessage("In a real game, the items would rotate between players, so you would see new items now.", true);
        int packOneID = Message_System.AddTutorialMessage("For now, keep drafting the all the rest of the items.", false);
        // Add a listener to clear the message when the player picks all the items
        Draft_Manager.Instance.OnPackEnd.AddListener(() => Message_System.Instance.ClearTutorialMessage(packOneID));
        Message_System.AddTutorialMessage("The draft happens with three sets of five items. Each player gets one archetype in there first set, so that's the only one available to you unless other players pass on their archetype.", true);
        int allDraftedID = Message_System.AddTutorialMessage("Keep drafting until you have all the items in all three sets.", false);
        // Add a listener to clear the message when the player picks all the items
        Draft_Manager.Instance.OnDraftEnd.AddListener(() => Message_System.Instance.ClearTutorialMessage(allDraftedID));
        Message_System.AddTutorialMessage("Great! You've drafted all the items. Now we move to the Equip phase.", true);
        Message_System.AddTutorialMessage("In the Equip phase, you'll equip your items to your character. You can equip one Archetype. By default you can equip 100 points of Equipment and 2 Abilities. But that can be changed by your Archetype.", true);
        int equipID = Message_System.AddTutorialMessage("Try equiping a weapon now.", false);
        // Add a listener to clear the message when the player equips an item
        reservesController.OnEquip.AddListener(() => Message_System.Instance.ClearTutorialMessage(equipID));
        int equipAbilityID = Message_System.AddTutorialMessage("Great! You've equipped a weapon. Now equip an ability.", false);
        // Add a listener to clear the message when the player equips an item
        reservesController.OnAbility.AddListener(() => {
            Message_System.Instance.ClearTutorialMessage(equipAbilityID);
            equipReadyButton.interactable = true;
        });
        Message_System.AddTutorialMessage("Great! You've equipped an ability. Add any other Abilities, Equipment, or an Archetype you'd like to try out.", true);
        int equipEndID = Message_System.AddTutorialMessage("When you're ready, press the ready button to move to the Eliminate phase.", false);
        // Add a listener to clear the message when the player presses the ready button
        Rounds_Manager.Instance.OnReady.AddListener(() => Message_System.Instance.ClearTutorialMessage(equipEndID));
        Message_System.AddTutorialMessage("Great! You're ready to move to the Eliminate phase. In this phase, you'll fight other players to be the last one standing.", true);
        Message_System.AddTutorialMessage("You can move with the WASD keys, jump with the space bar, and sprint with the shift key.", true);
        Message_System.AddTutorialMessage("You can aim with the mouse and shoot with the left mouse button.", true);
        Message_System.AddTutorialMessage("You can also use your abilities with the e key.", true);
        Message_System.AddTutorialMessage("You can cycle through weapons with the right mouse button and abilities with the q key.", true);
        int inCoinZoneID = Message_System.AddTutorialMessage("You get points in several ways. The most points come from The Coin! Go to the center of the arena and stand near the coin.", false);
        // Add a listener to clear the message when the player enters the coin zone
        GameObject.FindAnyObjectByType<Pickup>().OnZoneEnter.AddListener(() => Message_System.Instance.ClearTutorialMessage(inCoinZoneID));
        Message_System.AddTutorialMessage("Great! You're in the coin zone. Staying near the coin for five seconds will award you points.", true);
        Message_System.AddTutorialMessage("The number of points the coin awards increases the longer it goes unclaimed.", true);
        Message_System.AddTutorialMessage("The coin will return within a minute after it is claimed.", true);
        Message_System.AddTutorialMessage("You can also get points by eliminating other players, damaging players, and surviving.", true);
        Message_System.AddTutorialMessage("Each round lasts until there is one player left.", true);
        var killPlayerID = Message_System.AddTutorialMessage("I'll kill you now to show what the end of a round looks like.", true);
        // Damage the player
        Message_System.Instance.OnMessageCleared.AddListener((int id) => {
            if(id == killPlayerID){
                playerManager.TakeDamage(10000, playerManager.NetworkObject.OwnerClientId);
            }
        });
        Message_System.AddTutorialMessage("Between rounds, you go back to the Equip phase.", true);
        Message_System.AddTutorialMessage("Any Abilities you brought with you into the round are lost, but your Archetypes remain.", true);
        Message_System.AddTutorialMessage("Armor and extra ammo is also lost", true);
        Message_System.AddTutorialMessage("The survivor of the last round loses their weapons, but all the other players get them back with a full reload.", true);
        Message_System.AddTutorialMessage("That's all for the tutorial. You can press escape to pull up a menu and exit. Good luck with your future LOADOUT endevours!", false);
    }
}
