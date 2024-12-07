using Unity.Netcode;
using UnityEngine;

public class Data_Archetype
{
    public string archetypeName;
    public string description;
    public float walkingSpeedModifier;
    public float runningSpeedModifier;
    public float jumpSpeedModifier;
    public Vector3 hitboxSizeModifier;
    public float meleeModifier;
    public float health;
    public float carryWeight;
    public int abilitySlots;
    public float abilityCooldownModifier;
    public ArchetypeSpecials[] archetypeSpecials;

    public Data_Archetype(SO_Archetype scriptableArchetype){
        this.archetypeName = scriptableArchetype.archetypeName;
        this.description = scriptableArchetype.description;
        this.walkingSpeedModifier = scriptableArchetype.walkingSpeedModifier;
        this.runningSpeedModifier = scriptableArchetype.runningSpeedModifier;
        this.jumpSpeedModifier = scriptableArchetype.jumpSpeedModifier;
        this.hitboxSizeModifier = scriptableArchetype.hitboxSizeModifier;
        this.meleeModifier = scriptableArchetype.meleeModifier;
        this.health = scriptableArchetype.health;
        this.carryWeight = scriptableArchetype.carryWeight;
        this.abilitySlots = scriptableArchetype.abilitySlots;
        this.abilityCooldownModifier = scriptableArchetype.abilityCooldownModifier;
        this.archetypeSpecials = scriptableArchetype.archetypeSpecials;
    }
}
