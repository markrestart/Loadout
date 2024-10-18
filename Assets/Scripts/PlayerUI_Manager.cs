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
    private UnityEngine.UI.Image hitConfirmIndicator;
    [SerializeField]
    private GameObject inGameUI;
    [SerializeField]
    private DamageIndicatorDetails IndicatorDetails;

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
        if (damage < IndicatorDetails.MIN_DAMAGE_ADD_VALUE){
            damage = IndicatorDetails.MIN_DAMAGE_ADD_VALUE;
        }
        damageOpacity += damage;
        if(damageOpacity > IndicatorDetails.MAX_DAMAGE_INDICATOR_VALUE){
            damageOpacity = IndicatorDetails.MAX_DAMAGE_INDICATOR_VALUE;
        }
        if(damageOpacity < IndicatorDetails.MIN_DAMAGE_INDICATOR_VALUE){
            damageOpacity = IndicatorDetails.MIN_DAMAGE_INDICATOR_VALUE;
        }
    }

    public void DisplayHitConfirm(){
        hitConfirmIndicator.color = new Color(1, 1, 1, 1);
        hitConfirmIndicator.rectTransform.localScale = new Vector3(1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerManager.IsReady){
            return;
        }

        if(damageOpacity > 1){
            damageOpacity = math.lerp(damageOpacity, 0, IndicatorDetails.DAMAGE_INDICATOR_FADE_SPEED * Time.deltaTime);
            damageIndicatorOverlay.color = new Color(1, 0, 0, damageOpacity / IndicatorDetails.MAX_DAMAGE_INDICATOR_VALUE * IndicatorDetails.MAX_DAMAGE_INDICATOR_OPACITY);
        }else{
            damageIndicatorOverlay.color = new Color(1, 0, 0, 0);
        }

        if(hitConfirmIndicator.color.a > 0){
            hitConfirmIndicator.color = new Color(1, 1, 1, hitConfirmIndicator.color.a - Time.deltaTime/IndicatorDetails.HIT_CONFIRM_DISPLAY_TIME);
            float newSize = math.lerp(hitConfirmIndicator.rectTransform.localScale.x, IndicatorDetails.HIT_CONFIRM_MIN_SIZE, Time.deltaTime);
            hitConfirmIndicator.rectTransform.localScale = new Vector3(newSize, newSize, 1);
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

[System.Serializable]
class DamageIndicatorDetails{
    public int MAX_DAMAGE_INDICATOR_VALUE = 135;
    public int MIN_DAMAGE_INDICATOR_VALUE = 20;
    public float MAX_DAMAGE_INDICATOR_OPACITY = .5f;
    public float DAMAGE_INDICATOR_FADE_SPEED = 0.25f;
    public int MIN_DAMAGE_ADD_VALUE = 5;
    public float HIT_CONFIRM_DISPLAY_TIME = .9f;
    public float HIT_CONFIRM_MIN_SIZE = .5f;
}