using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Manager : MonoBehaviour, ITakes_Damage
{

    [SerializeField]
    private Data_Archetype archetype;
    [SerializeField]
    private List<Data_Ability> abilities = new List<Data_Ability>();
    [SerializeField]
    private List<Data_Equipment> equipments = new List<Data_Equipment>();
    [SerializeField]
    private Dictionary<AmmoType, int> ammos = new Dictionary<AmmoType, int>();
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

    public void TakeDamage(float damage)
    {
        if(isShielded)
        {
            shieldLimit -= damage;
            if(shieldLimit <= 0)
            {
                isShielded = false;
            }
            return;
        }
        playerHealth -= damage;
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
        GetComponent<MeshRenderer>().enabled = false;
        StartCoroutine(InvisableTimer(time));
    }

    private IEnumerator InvisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        GetComponent<MeshRenderer>().enabled = true;
    }

    private bool isShielded = false;
    private float shieldLimit = 0;
    public void Shield(float time, float limit)
    {
        isShielded = true;
        shieldLimit = limit;
        StartCoroutine(ShieldTimer(time));
    }

    private IEnumerator ShieldTimer(float time)
    {
        yield return new WaitForSeconds(time);
        isShielded = false;
    }

    public void AbilityJump(float force)
    {
        GetComponent<Player_Movement_Controller>().AddForce(Vector3.up * force);
    }


    private void Start() {
        spawnPointsParent = GameObject.Find("<SpawnPoints>").transform;
    }

    public void Ready(){
        isReady = true;

        GetComponent<Action_Handler>().Ready();
        playerHealth = archetype != null ? archetype.health : 100;

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
            foreach(Transform spawnPoint in spawnPointsParent){
                // shpere cast to check if another player is at the spawn point
                if(Physics.SphereCast(spawnPoint.position, 3.0f, Vector3.up, out RaycastHit hit, 0.0f)){
                    if(hit.transform != transform && hit.transform.GetComponent<Player_Manager>()){
                        continue;
                    }
                }
                GoToPosition(spawnPoint.position);
            }
        }
    }

    public void Unready(){
        isReady = false;
        GetComponent<Action_Handler>().Unready();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private bool isReady = false;
    public bool IsReady { get => isReady; }
}
