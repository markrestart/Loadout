using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Manager : MonoBehaviour, ITakes_Damage
{

    [SerializeField]
    private Data_Archetype archetype;
    [SerializeField]
    private List<Data_Ability> abilities;
    [SerializeField]
    private List<Data_Equipment> equipments;
    [SerializeField]
    private Dictionary<AmmoType, int> ammos = new Dictionary<AmmoType, int>();

    private float playerHealth;
    public float Health { get => playerHealth; }

    public List<Data_Ability> Abilities { get => abilities; }
    public List<Data_Equipment> Equipments { get => equipments; }
    public Data_Archetype Archetype { get => archetype; }
    public void SetArchetype(Data_Archetype archetype)
    {
        this.archetype = archetype;
    }
    public void AddAbility(Data_Ability ability)
    {
        abilities.Add(ability);
    }
    public void AddEquipment(Data_Equipment equipment)
    {
        equipments.Add(equipment);
    }
    public void RemoveAbility(Data_Ability ability)
    {
        abilities.Remove(ability);
    }
    public void RemoveEquipment(Data_Equipment equipment)
    {
        equipments.Remove(equipment);
    }

    public int AmmoCount(AmmoType ammoType)
    {
        if(ammos.ContainsKey(ammoType))
        {
            return ammos[ammoType];
        }
        return 0;
    }
    public void UseAmmo(AmmoType ammoType, int amount)
    {
        if(ammos.ContainsKey(ammoType))
        {
            ammos[ammoType] -= amount;
        }
        if(ammos[ammoType] <= 0)
        {
            ammos.Remove(ammoType);
        }
    }

    public void TakeDamage(float damage)
    {
        playerHealth -= damage;
        if(playerHealth <= 0)
        {
            //Player is dead
            Debug.Log("Player is dead");
        }
    }

    //TODO: temporary cod eto intialize player until drafting/selection is implemented
    void Start() {
        //TODO: Especially get rid of this, adding ammo to player
        ammos.Add(AmmoType.Bullet, 100);
    }

    public void Ready(){
        isReady = true;

        GetComponent<Action_Handler>().Ready();
        playerHealth = archetype.health;
    }
    private bool isReady = false;
    public bool IsReady { get => isReady; }
}
