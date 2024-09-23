using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "Archetype", menuName = "Archetype", order = 0)]
public class SO_Archetype : ScriptableObject {
    public string archetypeName = "Archetype";
    public string description = "This is a description of the archetype.";
    public float walkingSpeedModifier = 1.0f;
    public float runningSpeedModifier = 1.0f;
    public float jumpSpeedModifier = 1.0f;
    public Vector3 hitboxSizeModifier = new Vector3(1, 1, 1);
    public float meleeModifier = 1.0f;
    public float health = 100.0f;
    public float carryWeight = 10.0f;
    public int abilitySlots = 1;
    public float abilityCooldownModifier = 1;
}