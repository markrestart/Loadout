using Unity.Netcode;
using UnityEngine;

public class Data_Ability
{
    public string abilityName;
    public string description;
    public float cooldown;
    private float lastActivationTime;
    private float[] values;
    private AbilityActions action;
    public float CooldownRemaining { get => cooldownRemaining(); }

    public Data_Ability(SO_Ability scriptableAbility){
        this.abilityName = scriptableAbility.abilityName;
        this.description = scriptableAbility.description;
        this.cooldown = scriptableAbility.cooldown;
        this.values = scriptableAbility.values;
        this.action = scriptableAbility.action;
    }

    private float cooldownRemaining(){
        var timeSinceLastActivation = Time.time - lastActivationTime;
        if(timeSinceLastActivation < cooldown){
            return cooldown - timeSinceLastActivation;
        }
        return 0;
    }

//TODO: Make sure abilities are replicated
    public void Activate(Player_Manager playerManager){
        if(Time.time - lastActivationTime < cooldown){
            return;
        }
        lastActivationTime = Time.time;
        Debug.Log("Activating " + abilityName);
        switch(action){
            case AbilityActions.Invisable:
                playerManager.GoInvisable(values[0]);
                break;
            case AbilityActions.Health:
                playerManager.Heal((int)values[0]);
                break;
            case AbilityActions.Teleport:
                playerManager.GoToSpawnPoint();
                break;
            case AbilityActions.Shield:
                playerManager.Shield(values[0], values[1]);
                break;
            case AbilityActions.FireRateBoost:
                //playerManager.FireRateBoost(values[0], values[1]);
                break;
            case AbilityActions.DamageBoost:
                //playerManager.DamageBoost(values[0], values[1]);
                break;
            case AbilityActions.Jump:
                playerManager.AbilityJump(values[0]);
                break;
            
        }
    }
}
