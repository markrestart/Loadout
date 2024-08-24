using Unity.Netcode;
using UnityEngine;

public class Data_Ability
{
    public string abilityName;
    public string description;
    public float cooldown;
    private bool lastActivationTime;
    private float[] values;
    private AbilityActions action;

    public Data_Ability(SO_Ability scriptableAbility){
        this.abilityName = scriptableAbility.abilityName;
        this.description = scriptableAbility.description;
        this.cooldown = scriptableAbility.cooldown;
        this.values = scriptableAbility.values;
        this.action = scriptableAbility.action;
    }

    public void Activate(){
        Debug.Log("Activating " + abilityName);
        switch(action){
            case AbilityActions.Invisable:
                break;
            case AbilityActions.Health:
                break;
            case AbilityActions.Teleport:
                break;
            case AbilityActions.Shield:
                break;
        }
    }
}
