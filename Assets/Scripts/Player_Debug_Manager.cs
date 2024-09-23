using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Debug_Manager : MonoBehaviour
{
    public Player_Manager playerManager;
    public Player_Movement_Controller playerMovementController;
    public Action_Handler actionHandler;
    public Reserves_Controller reservesController;
    public PlayerUI_Manager playerUIManager;
    public GameObject debugUI;
    public TMPro.TextMeshProUGUI debugText;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F12)){
            debugUI.SetActive(!debugUI.activeSelf);
        }
        if(Input.GetKeyDown(KeyCode.F1)){
            playerManager.TakeDamage(20, playerManager.NetworkObject.OwnerClientId);
        }
        if(debugUI.activeSelf){
            debugText.text = "";
            foreach(ulong id in Rounds_Manager.Instance.PlayerScores.Keys){
                debugText.text += $"{Rounds_Manager.Instance.PlayerNames[id]}({id}): {Rounds_Manager.Instance.PlayerScores[id]} - {Rounds_Manager.Instance.PlayersAlive[id]}\n";
            }
        }
    }
}
