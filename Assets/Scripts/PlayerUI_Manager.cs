using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
    [SerializeField]
    private UnityEngine.UI.Image damageIndicatorOverlay;
    [SerializeField]
    private GameObject inGameUI;

    private Player_Manager playerManager;
    private Action_Handler actionHandler;
    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<Player_Manager>();
        actionHandler = GetComponent<Action_Handler>();
    }

    public void SetInGameUIActive(bool active){
        inGameUI.SetActive(active);
    }

    private float damageOpacity = 0;
    public void addDamage(float damage){
        damageOpacity += damage;
        if(damageOpacity > CONSTANTS.MAX_DAMAGE_INDICATOR_VALUE){
            damageOpacity = CONSTANTS.MAX_DAMAGE_INDICATOR_VALUE;
        }
        if(damageOpacity < CONSTANTS.MIN_DAMAGE_INDICATOR_VALUE){
            damageOpacity = CONSTANTS.MIN_DAMAGE_INDICATOR_VALUE;
        }
        Debug.Log($"Damage display: {damageOpacity}");
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerManager.IsReady){
            return;
        }

        if(damageOpacity > 1){
            damageOpacity = math.lerp(damageOpacity, 0, CONSTANTS.DAMAGE_INDICATOR_FADE_SPEED * Time.deltaTime);
            damageIndicatorOverlay.color = new Color(1, 0, 0, damageOpacity / CONSTANTS.MAX_DAMAGE_INDICATOR_VALUE * CONSTANTS.MAX_DAMAGE_INDICATOR_OPACITY);
        }else{
            damageIndicatorOverlay.color = new Color(1, 0, 0, 0);
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
