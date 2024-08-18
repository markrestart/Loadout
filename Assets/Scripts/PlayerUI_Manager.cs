using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI_Manager : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI ammoText;
    [SerializeField]
    private TMPro.TextMeshProUGUI ReloadText;
    [SerializeField]
    private TMPro.TextMeshProUGUI healthText;

    private Player_Manager playerManager;
    private Action_Handler actionHandler;
    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<Player_Manager>();
        actionHandler = GetComponent<Action_Handler>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerManager.IsReady){
            return;
        }
        if(actionHandler.ActiveEquipment != null && actionHandler.ActiveEquipment.ammoType != AmmoType.NA){
        ammoText.text = actionHandler.ActiveEquipment.CurrentAmmo.ToString();
        ReloadText.text = playerManager.AmmoCount(actionHandler.ActiveEquipment.ammoType).ToString();
        }
        healthText.text = playerManager.Health.ToString();
    }
}
