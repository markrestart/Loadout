using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;
using Unity.VisualScripting;

public class Rounds_Manager : NetworkBehaviour
{
    public static Rounds_Manager Instance;
    private int currentRound = 0;
    private Dictionary<ulong, float> playerScores = new Dictionary<ulong, float>();
    private Dictionary<ulong, bool> playersAlive = new Dictionary<ulong, bool>();
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    [SerializeField]
    private GameObject scoreScreen;
    [SerializeField]
    private TMPro.TextMeshProUGUI scoreText;
    [SerializeField]
    private GameObject quitGameButton;
    private bool roundStarted = false;
    public bool RoundStarted { get => roundStarted; }
    public Dictionary<ulong, float> PlayerScores { get => playerScores; }
    public Dictionary<ulong, bool> PlayersAlive { get => playersAlive; }
    public Dictionary<ulong, string> PlayerNames { get => playerNames; }

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void RegisterNameRpc(ulong playerID, string playerName){
        playerNames[playerID] = playerName;
        playerScores[playerID] = 0;
        playersAlive[playerID] = true;
    }

    public void PlayerDeath(ulong playerID){
        playersAlive[playerID] = false;
        if(IsServer){
            bool allPlayersDead = true;
            int alivePlayers = 0;
            foreach(bool isAlive in playersAlive.Values){
                if(isAlive){
                    alivePlayers++;
                    if(alivePlayers > 1){
                        allPlayersDead = false;
                    }
                }
            }

            float outlastScore = (playersAlive.Count - alivePlayers - 1) * CONSTANTS.POINTS_PER_OUTLAST;
            AddScoreRpc(playerID, outlastScore);

            //TODO: Can be optimized to check the survivor while checking if all players are dead
            if(allPlayersDead && IsServer){
                ulong survivor = CONSTANTS.NULL_ID;
                foreach(ulong ID in playersAlive.Keys){
                    if(playersAlive[ID]){
                        survivor = ID;
                        break;
                    }
                }
                if(survivor != CONSTANTS.NULL_ID){
                    float survivorScore = CONSTANTS.POINTS_PER_ROUND_WIN;
                    float outlastScoreForSurvivor = (playersAlive.Count - 1) * CONSTANTS.POINTS_PER_OUTLAST;
                    float totalScore = survivorScore + outlastScoreForSurvivor;
                    AddScoreRpc(survivor, totalScore);
                }
                EndroundRpc();
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void EndroundRpc(){
        roundStarted = false;
        //Display scores
        scoreText.text = "";
        foreach(var playerScore in playerScores){
            string playerName = playerNames.ContainsKey(playerScore.Key) ? (playerNames[playerScore.Key].Length > 0 ? playerNames[playerScore.Key] : $"Player {playerScore.Key}") : $"Player {playerScore.Key}x";
            scoreText.text += $"{playerName} :  {playerScore.Value}\n";
        }
        scoreScreen.SetActive(true);

        foreach(var reservesController in Reserves_Controller.Instances){
            reservesController.ResetRound(playersAlive[reservesController.NetworkObject.OwnerClientId]);
        }

        if(currentRound < 3){
            StartCoroutine(EndRoundTimer());
        }
        else{
            quitGameButton.SetActive(true);
        }
    }

    private IEnumerator EndRoundTimer(){
        //Countdown to next round
        yield return new WaitForSeconds(CONSTANTS.END_OF_ROUND_DELAY);

        //Hide score screen
        scoreScreen.SetActive(false);

        //Go to Equip phase
                //Put all players into the equiping phase
        foreach(var reservesController in Reserves_Controller.Instances){
            reservesController.EnterEquipPhase(playerScores.Keys.ToList());
        }
    }

    public void StartRound(){
        roundStarted = true;
        currentRound++;
        playersAlive = playersAlive.ToDictionary(entry => entry.Key, entry => true);
    }

    [Rpc(SendTo.Everyone)]
    public void AddScoreRpc(ulong playerID, float score){
        if(playerScores.ContainsKey(playerID)){
            playerScores[playerID] += score;
        }
        else{
            playerScores.Add(playerID, score);
        }
    }

    public void ExitGame(){
        Application.Quit();
    }
}
