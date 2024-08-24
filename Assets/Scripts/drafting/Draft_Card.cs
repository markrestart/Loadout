using System;
using System.Collections.Generic;
using Unity.Netcode;

public class Draft_Card
{
    private DraftCardType type;
    private Data_Equipment equipment;
    private Data_Archetype archetype;
    private Data_Ability ability;
    private AmmoType ammoType;
    private int ammoAmount;
    private int armor;

    public Data_Equipment Equipment { get => equipment; }
    public Data_Archetype Archetype { get => archetype; }
    public Data_Ability Ability { get => ability; }
    public AmmoType AmmoType { get => ammoType; }
    public int AmmoAmount { get => ammoAmount; }
    public int Armor { get => armor; }

    //An equaility operator for Draft_Card so that we can compare them in the Draft_Manager
    public static bool operator ==(Draft_Card a, Draft_Card b){
        if(a.EType != b.EType){
            return false;
        }
        switch(a.EType){
            case DraftCardType.Equipment:
                return a.Equipment.equipmentName == b.Equipment.equipmentName;
            case DraftCardType.Ability:
                return a.Ability.abilityName == b.Ability.abilityName;
            case DraftCardType.Ammo:
                return a.AmmoType == b.AmmoType && a.AmmoAmount == b.AmmoAmount;
            case DraftCardType.Archetype:
                return a.Archetype.archetypeName == b.Archetype.archetypeName;
            case DraftCardType.Armor:
                return a.Armor == b.Armor;
            default:
                return false;
        }
    }

    public static bool operator !=(Draft_Card a, Draft_Card b){
        return !(a == b);
    }


    public Draft_Card(Data_Equipment equipment){
        this.type = DraftCardType.Equipment;
        this.equipment = equipment;
        this.ability = null;
        this.ammoType = AmmoType.NA;
        this.ammoAmount = 0;
        this.armor = 0;
        this.archetype = null;
    }

    public Draft_Card(Data_Ability ability){
        this.type = DraftCardType.Ability;
        this.equipment = null;
        this.ability = ability;
        this.ammoType = AmmoType.NA;
        this.ammoAmount = 0;
        this.armor = 0;
        this.archetype = null;
    }

    public Draft_Card(Tuple<AmmoType, int> ammo){
        this.type = DraftCardType.Ammo;
        this.equipment = null;
        this.ability = null;
        this.ammoType = ammo.Item1;
        this.ammoAmount = ammo.Item2;
        this.armor = 0;
        this.archetype = null;
    }

    public Draft_Card(Data_Archetype archetype){
        this.type = DraftCardType.Archetype;
        this.equipment = null;
        this.ability = null;
        this.ammoType = AmmoType.NA;
        this.ammoAmount = 0;
        this.armor = 0;
        this.archetype = archetype;
    }

    public Draft_Card(int armor){
        this.type = DraftCardType.Armor;
        this.equipment = null;
        this.ability = null;
        this.ammoType = AmmoType.NA;
        this.ammoAmount = 0;
        this.armor = armor;
        this.archetype = null;
    }

    public string Name { 
        get {
            switch(type){
                case DraftCardType.Equipment:
                    return equipment.equipmentName;
                case DraftCardType.Ability:
                    return ability.abilityName;
                case DraftCardType.Ammo:
                    return ammoType.ToString();
                case DraftCardType.Archetype:
                    return archetype.archetypeName;
                case DraftCardType.Armor:
                    return "Armor";
                default:
                    return "";
            }
        } 
    }

    public DraftCardType EType { get => type; }
    public string Type { 
        get {
            switch(type){
                case DraftCardType.Equipment:
                    return "Equipment";
                case DraftCardType.Ability:
                    return "Ability";
                case DraftCardType.Ammo:
                    return "Ammo";
                case DraftCardType.Archetype:
                    return "Archetype";
                case DraftCardType.Armor:
                    return "Armor";
                default:
                    return "";
            }
        } 
    }

    public string Description { 
        get {
            switch(type){
                case DraftCardType.Equipment:
                    return equipment.description;
                case DraftCardType.Ability:
                    return ability.description;
                case DraftCardType.Ammo:
                    return $"{ammoAmount.ToString()} ammo of type {ammoType.ToString()}";
                case DraftCardType.Archetype:
                    return archetype.description;
                case DraftCardType.Armor:
                    return $"Armor that provides protection from ${armor} damage";
                default:
                    return "";
            }
        } 
    }

    //TODO: these hardcoded values should be replaced with a more dynamic system
    public float Weight { 
        get {
            switch(type){
                case DraftCardType.Equipment:
                    return equipment.weight;
                case DraftCardType.Ammo:
                    switch(ammoType){
                        case AmmoType.Bullet:
                            return ammoAmount * 0.1f;
                        case AmmoType.Shell:
                            return ammoAmount *  0.2f;
                        case AmmoType.Rocket:
                            return ammoAmount * 5f;
                        case AmmoType.Arrow:
                            return ammoAmount * 0.01f;
                        default:
                            return 0;
                    }
                case DraftCardType.Armor:
                    return armor * 2f;
                default:
                    return 0;
            }
        } 
    }
}

public enum DraftCardType{
    Equipment,
    Ability,
    Ammo,
    Archetype,
    Armor
}