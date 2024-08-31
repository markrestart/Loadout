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
    [SerializeField]
    private TMPro.TextMeshProUGUI weaponNameText;
    [SerializeField]
    private TMPro.TextMeshProUGUI abilityNameText;
    [SerializeField]
    private TMPro.TextMeshProUGUI abilityCooldownText;

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
            if(actionHandler.ActiveEquipment.CurrentAmmo <= 0){
                ammoText.text = "--";
            }else{
                ammoText.text = actionHandler.ActiveEquipment.CurrentAmmo.ToString();
            }
            if(playerManager.AmmoCount(actionHandler.ActiveEquipment.ammoType) <= 0){
                ReloadText.text = "--";
            }else{
                ReloadText.text = playerManager.AmmoCount(actionHandler.ActiveEquipment.ammoType).ToString();
            }
        }else{
            ammoText.text = "";
            ReloadText.text = "";
        }
        healthText.text = playerManager.Health.ToString();
        if(actionHandler.ActiveEquipment != null){
            weaponNameText.text = actionHandler.ActiveEquipment.equipmentName;
        }else{
            weaponNameText.text = "";
            ammoText.text = "";
            ReloadText.text = "";
        }
        if(actionHandler.ActiveAbility != null){
            abilityNameText.text = actionHandler.ActiveAbility.abilityName;
            var cooldown = actionHandler.ActiveAbility.CooldownRemaining;
            if(cooldown > 0){
                abilityCooldownText.text = cooldown.ToString("F0");
            }else{
                abilityCooldownText.text = "";
            }
        }else{
            abilityNameText.text = "";
            abilityCooldownText.text = "";
        }
    }
}
