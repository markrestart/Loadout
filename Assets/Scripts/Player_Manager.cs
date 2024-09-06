using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player_Manager : NetworkBehaviour, ITakes_Damage
{

    [SerializeField]
    private Data_Archetype archetype;
    [SerializeField]
    private List<Data_Ability> abilities = new List<Data_Ability>();
    [SerializeField]
    private List<Data_Equipment> equipments = new List<Data_Equipment>();
    [SerializeField]
    private Dictionary<AmmoType, int> ammos = new Dictionary<AmmoType, int>();
    [SerializeField]
    private MeshRenderer playerModel;
    [SerializeField]
    private List<GameObject> effects;
    private Transform spawnPointsParent;

    private float playerHealth;
    private float playerMaxHealth = 100;
    public float Health { get => playerHealth; }

    public List<Data_Ability> Abilities { get => abilities; }
    public List<Data_Equipment> Equipments { get => equipments; }
    public Data_Archetype Archetype { get => archetype; }
    public void SetArchetype(Data_Archetype archetype)
    {
        this.archetype = archetype;
        playerMaxHealth = archetype.health;
        transform.localScale = archetype.hitboxSizeModifier;
    }
    public void AddAbility(Data_Ability ability)
    {
        abilities.Add(ability);
    }
    public void AddEquipment(Data_Equipment equipment)
    {
        equipments.Add(equipment);
    }
    public void AddAmmo(AmmoType ammoType, int amount)
    {
        if(ammos.ContainsKey(ammoType))
        {
            ammos[ammoType] += amount;
        }
        else
        {
            ammos.Add(ammoType, amount);
        }
    }
    public void AddArmor(int armor)
    {
        playerHealth += armor;
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
        if(ammos.ContainsKey(ammoType) && ammos[ammoType] <= 0)
        {
            ammos.Remove(ammoType);
        }
    }

    public void SetAmmo(AmmoType ammoType, int amount)
    {
        if(ammos.ContainsKey(ammoType))
        {
            ammos[ammoType] = amount;
        }
        else
        {
            ammos.Add(ammoType, amount);
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Player took damage");
        TakeDamageRpc(damage);
    }

    [Rpc(SendTo.Everyone)]
    public void TakeDamageRpc(float damage)
    {
        Debug.Log("Player took damage rpc");
        if(isShielded > 0)
        {
            shieldLimit -= damage;
            if(shieldLimit <= 0)
            {
                isShielded = 0;
            }
            return;
        }
        playerHealth -= damage;
        Debug.Log($"Player {NetworkObject.OwnerClientId} took damage: {damage}. Health: {playerHealth}");
        if(playerHealth <= 0)
        {
            //Player is dead
            Debug.Log("Player is dead");
        }
    }

    public void Heal(float amount)
    {
        if(playerHealth + amount > playerMaxHealth)
        {
            if(playerHealth >= playerMaxHealth)
            {
                return;
            }
            playerHealth = playerMaxHealth;
        }
        else
        {
            playerHealth += amount;
        }
    }

    public void GoInvisable(float time)
    {
        if(!IsOwner){
            playerModel.enabled = false;
        }else{
            effects[0].SetActive(true);
        }
        StartCoroutine(InvisableTimer(time));
    }

    private IEnumerator InvisableTimer(float time)
    {
        if(!IsOwner){
            yield return new WaitForSeconds(time);
            playerModel.enabled = true;
        }else{
            yield return new WaitForSeconds(time);
            effects[0].SetActive(false);
        }
    }

    private int isShielded = 0;
    private float shieldLimit = 0;
    public void Shield(float time, float limit)
    {
        effects[3].SetActive(true);
        isShielded++;
        shieldLimit += limit;
        StartCoroutine(ShieldTimer(time));
    }

    private IEnumerator ShieldTimer(float time)
    {
        yield return new WaitForSeconds(time);
        isShielded--;
        if(isShielded == 0)
        {
            effects[3].SetActive(false);
        }
    }

    public void DamageBoost(float time, float multiplier)
    {
        effects[1].SetActive(true);
        GetComponent<Action_Handler>().DamageMultiplier *= multiplier;
        StartCoroutine(DamageBoostTimer(time, multiplier));
    }

    private IEnumerator DamageBoostTimer(float time, float multiplier)
    {
        yield return new WaitForSeconds(time);
        GetComponent<Action_Handler>().DamageMultiplier /= multiplier;
        if(GetComponent<Action_Handler>().DamageMultiplier == 1)
        {
            effects[1].SetActive(false);
        }
    }

    public void FireRateBoost(float time, float multiplier)
    {
        effects[2].SetActive(true);
        GetComponent<Action_Handler>().FireRateMultiplier *= multiplier;
        StartCoroutine(FireRateBoostTimer(time, multiplier));
    }

    private IEnumerator FireRateBoostTimer(float time, float multiplier)
    {
        yield return new WaitForSeconds(time);
        GetComponent<Action_Handler>().FireRateMultiplier /= multiplier;
        if(GetComponent<Action_Handler>().FireRateMultiplier == 1)
        {
            effects[2].SetActive(false);
        }
    }

    public void AbilityJump(float force)
    {
        GetComponent<Player_Movement_Controller>().AddForce(Vector3.up * force);
    }


    private void Start() {
        spawnPointsParent = GameObject.Find("<SpawnPoints>").transform;
        if(IsOwner){
            playerModel.enabled = false;
        }
    }

    public void Ready(){
        isReady = true;

        GetComponent<PlayerUI_Manager>().SetInGameUIActive(true);

        GetComponent<Action_Handler>().Ready();
        playerHealth = archetype != null ? archetype.health : 100;
        GetComponent<Action_Handler>().AbilityCooldownMultiplier = archetype != null ? archetype.abilityCooldownModifier : 1;
        foreach(Data_Equipment equipment in equipments){
            equipment.setActivationRateMultiplier(1);
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GoToSpawnPoint();
    }

    public void GoToPosition(Vector3 position){
        GetComponent<CharacterController>().enabled = false;
        transform.position = position;
        GetComponent<CharacterController>().enabled = true;
    }

    public void GoToSpawnPoint(){
        // Go to spawn point
        if(spawnPointsParent != null){
            GoToPosition(spawnPointsParent.GetChild((int)NetworkManager.Singleton.LocalClientId).position);
        }
    }

    public void Unready(){
        isReady = false;
        GetComponent<Action_Handler>().Unready();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GetComponent<PlayerUI_Manager>().SetInGameUIActive(false);
    }
    private bool isReady = false;
    public bool IsReady { get => isReady; }
}
