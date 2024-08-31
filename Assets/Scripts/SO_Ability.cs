using UnityEngine;
[CreateAssetMenu(fileName = "Ability", menuName = "Ability", order = 0)]
public class SO_Ability : ScriptableObject {
    public string abilityName = "Ability";
    public string description = "This is a description of the ability.";
    public AbilityActions action;
    public float[] values;
    public float cooldown = 1.0f;
}

public enum AbilityActions{
    Invisable,
    Health,
    Teleport,
    Shield,
    Jump,
    DamageBoost,
    FireRateBoost,
}