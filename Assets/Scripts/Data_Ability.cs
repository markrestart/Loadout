using Unity.Netcode;
using UnityEngine;

public class Data_Ability
{
    public string abilityName;
    public string description;
    public bool isContinuous;
    public float cooldown;
    private float lastActivationTime;
    private bool isActive;
    public bool IsActive { get => isActive; }

    public Data_Ability(SO_Ability scriptableAbility){
        this.abilityName = scriptableAbility.abilityName;
        this.description = scriptableAbility.description;
        this.isContinuous = scriptableAbility.isContinuous;
        this.cooldown = scriptableAbility.cooldown;
        this.lastActivationTime = 0.0f;
        this.isActive = false;
    }

    public void Activate(){
        if(Time.time - lastActivationTime < cooldown){
            return;
        }
        lastActivationTime = Time.time;
        isActive = true;
    }

    public void Deactivate(){
        isActive = false;
    }
}
