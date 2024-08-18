using UnityEngine;

[CreateAssetMenu(fileName = "Ability_Heal", menuName = "Ability_Heal", order = 0)]
public class Ability_Heal : Ability{
    
    public override void Activate() {
        Debug.Log("Activating " + abilityName);
    }

    public override void Deactivate() {
        Debug.Log("Deactivating " + abilityName);
    }

    public override void Update() {
        Debug.Log("Updating " + abilityName);
    }
}
