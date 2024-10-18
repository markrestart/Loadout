using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
    private GameObject playerModel;
    [SerializeField]
    private GameObject playerWeaponModel;
    [SerializeField]
    private List<GameObject> effects;
    private Transform spawnPointsParent;

    private float playerHealth;
    private float armor;
    private float playerMaxHealth = CONSTANTS.DEFAULT_HEALTH;
    public float Health { get => playerHealth + armor; }

    public List<Data_Ability> Abilities { get => abilities; }
    public List<Data_Equipment> Equipments { get => equipments; }
    public Data_Archetype Archetype { get => archetype; }
    [SerializeField]
    private PlayerUI_Manager playerUIManager;

    public void ClearLoadout(){
        archetype = null;
        abilities.Clear();
        equipments.Clear();
        ammos.Clear();

        playerMaxHealth = CONSTANTS.DEFAULT_HEALTH;
        transform.localScale = Vector3.one;
        playerWeaponModel.SetActive(true);
    }
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
        this.armor += armor;
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

    public void TakeDamage(float damage, ulong sourceID)
    {
        TakeDamageRpc(damage, sourceID);
    }

    [Rpc(SendTo.Everyone)]
    public void TakeDamageRpc(float damage, ulong sourceID) 
    {
        //TODO: Add damage indication for shielded and armor damage
        playerUIManager.addDamage(math.ceil(damage));

        //TODO: Re-architect this absolutely disgusting code
        Reserves_Controller.Instances.Where(x => x.OwnerClientId == sourceID).FirstOrDefault().PlayerManager.playerUIManager.DisplayHitConfirm();

        if(isShielded > 0)
        {
            shieldLimit -= damage;
            if(shieldLimit <= 0)
            {
                isShielded = 0;
                effects[3].SetActive(false);
            }
            return;
        }
        if(IsServer){
            Rounds_Manager.Instance.AddScoreRpc(sourceID, damage >= Health ? Health + CONSTANTS.POINTS_PER_ELIMINATION : damage);
        }
        
        if(armor > 0)
        {
            armor -= damage;
            if(armor < 0)
            {
                playerHealth += armor;
                armor = 0;
            }
        }
        else
        {
            playerHealth -= damage;
        }
        if(playerHealth <= 0)
        {
            //Player is dead
            Rounds_Manager.Instance.PlayerDeath(NetworkObject.OwnerClientId);
            SetIsSpecating(true);
        }
    }

    public void SetIsSpecating(bool isSpecating)
    {
        isReady = !isSpecating;
        playerModel.SetActive(!isSpecating);
        playerWeaponModel.SetActive(!isSpecating);
        if(IsOwner){
            GetComponent<PlayerUI_Manager>().SetInGameUIActive(!isSpecating);
        }
        if(isSpecating){
            GoToPosition(new Vector3(0, CONSTANTS.OUT_OF_BOUNDS_HEIGHT, 0));
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
            playerModel.SetActive(false);
        }else{
            effects[0].SetActive(true);
        }
        StartCoroutine(InvisableTimer(time));
    }

    private IEnumerator InvisableTimer(float time)
    {
        if(!IsOwner){
            yield return new WaitForSeconds(time);
            playerModel.SetActive(true);
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
            shieldLimit = 0;
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
            playerModel.SetActive(false);
        }
    }

    public void Ready(){
        isReady = true;

        GetComponent<PlayerUI_Manager>().Ready();


        ReadyRpc();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GoToSpawnPoint();
    }

    [Rpc(SendTo.Everyone)]
    public void ReadyRpc(){
        SetIsSpecating(false);
        playerHealth = archetype != null ? archetype.health : CONSTANTS.DEFAULT_HEALTH;
        GetComponent<Action_Handler>().Ready();
        GetComponent<Action_Handler>().AbilityCooldownMultiplier = archetype != null ? archetype.abilityCooldownModifier : 1;
        foreach(Data_Equipment equipment in equipments){
        equipment.setActivationRateMultiplier(1);
        }
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
