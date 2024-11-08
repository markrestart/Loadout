using Unity.Netcode;
using UnityEngine;

public class Data_Ability
{
    public string abilityName;
    public string description;
    private float cooldown;
    public float Cooldown { get => cooldown * cooldownMultiplier; }
    private float lastActivationTime;
    private float[] values;
    private AbilityActions action;
    public float CooldownRemaining { get => cooldownRemaining(); }
    private float cooldownMultiplier = 1;

    public AudioClip activateSound;
    public AudioClip deactivateSound;

    public Data_Ability(SO_Ability scriptableAbility){
        this.abilityName = scriptableAbility.abilityName;
        this.description = scriptableAbility.description;
        this.cooldown = scriptableAbility.cooldown;
        this.values = scriptableAbility.values;
        this.action = scriptableAbility.action;
        this.activateSound = scriptableAbility.activateSound;
        this.deactivateSound = scriptableAbility.deactivateSound;
    }

    private float cooldownRemaining(){
        var timeSinceLastActivation = Time.time - lastActivationTime;
        if(timeSinceLastActivation < Cooldown){
            return Cooldown - timeSinceLastActivation;
        }
        return 0;
    }

    public void SetCooldownMultiplier(float multiplier){
        cooldownMultiplier = multiplier;
    }

//TODO: Make sure abilities are replicated
    public void Activate(Player_Manager playerManager){
        if(Time.time - lastActivationTime < Cooldown){
            return;
        }
        if(activateSound != null){
            playerManager.AbilitySound.clip = activateSound;
            playerManager.AbilitySound.Play();
        }
        lastActivationTime = Time.time;
        switch(action){
            case AbilityActions.Invisable:
                playerManager.GoInvisable(values[0], deactivateSound);
                break;
            case AbilityActions.Health:
                playerManager.Heal((int)values[0]);
                break;
            case AbilityActions.Teleport:
                playerManager.GoToSpawnPoint();
                break;
            case AbilityActions.Shield:
                playerManager.Shield(values[0], values[1], deactivateSound);
                break;
            case AbilityActions.FireRateBoost:
                playerManager.FireRateBoost(values[0], values[1], deactivateSound);
                break;
            case AbilityActions.DamageBoost:
                playerManager.DamageBoost(values[0], values[1], deactivateSound);
                break;
            case AbilityActions.Jump:
                playerManager.AbilityJump(values[0]);
                break;
            
        }
    }
}
