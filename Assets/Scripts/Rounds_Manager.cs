using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class Rounds_Manager : NetworkBehaviour
{
    public static Rounds_Manager Instance;
    private int currentRound = 0;
    private Dictionary<ulong, float> playerScores = new Dictionary<ulong, float>();
    private Dictionary<ulong, bool> playersAlive = new Dictionary<ulong, bool>();
    [SerializeField]
    private GameObject scoreScreen;
    [SerializeField]
    private TMPro.TextMeshProUGUI scoreText;
    [SerializeField]
    private GameObject quitGameButton;

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

    public void PlayerDeath(ulong playerID){
        playersAlive[playerID] = false;
        AddScoreRpc(playerID, playersAlive.Aggregate(0, (acc, player) => acc + (player.Value ? 1 : 0)) * 25);
        bool allPlayersDead = true;
        foreach(bool isAlive in playersAlive.Values){
            if(isAlive){
                allPlayersDead = false;
                break;
            }
        }
        if(allPlayersDead && IsServer){
            EndroundRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void EndroundRpc(){
        //Display scores
        scoreText.text = "";
        foreach(var playerScore in playerScores){
            scoreText.text += $"Player {playerScore.Key} :  {playerScore.Value}\n";
        }
        scoreScreen.SetActive(true);

        foreach(var reservesController in Reserves_Controller.Instances){
            reservesController.ResetRound(playersAlive[reservesController.NetworkManager.LocalClientId]);
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
        yield return new WaitForSeconds(5);

        //Hide score screen
        scoreScreen.SetActive(false);

        //Go to Equip phase
                //Put all players into the equiping phase
        foreach(var reservesController in Reserves_Controller.Instances){
            reservesController.EnterEquipPhase(playerScores.Keys.ToList());
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StartRoundRpc(){
        currentRound++;
        foreach(ulong playerID in playersAlive.Keys){
            playersAlive[playerID] = true;
        }
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
