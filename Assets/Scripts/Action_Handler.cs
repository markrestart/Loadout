using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Action_Handler : NetworkBehaviour
{
    [SerializeField]
    private Transform firePoint;
    private Player_Manager playerManager;

    private Data_Equipment activeEquipment;
    public Data_Equipment ActiveEquipment { get => activeEquipment; }
    private Data_Ability activeAbility;
    public Data_Ability ActiveAbility { get => activeAbility; }

    private void Awake()
    {
        playerManager = GetComponent<Player_Manager>();
    }

    // Start is called before the first frame update
    public void Ready()
    {
        activeEquipment = playerManager.Equipments.FirstOrDefault();
        activeAbility = playerManager.Abilities.FirstOrDefault();
    }

    public void Unready()
    {
        activeEquipment = null;
        activeAbility = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner){
            return;
        }
        
        if(!playerManager.IsReady){
            return;
        }
        if(Input.GetButton("Fire1"))
        {
            if(activeEquipment != null)
            {
                FireWeaponRpc(Input.GetButtonDown("Fire1"));
            }
        }

        if(Input.GetButtonDown("Ability")){
            if(activeAbility != null)
            {
                ActivateAbilityRpc();
            }
        }

        if(Input.GetButtonDown("CycleEquipment")){
            //Get the next equipment in the list, or the first if at the end
            activeEquipment = playerManager.Equipments[(playerManager.Equipments.IndexOf(activeEquipment) + 1) % playerManager.Equipments.Count];
            SyncEquipmentRpc((ushort)playerManager.Equipments.IndexOf(activeEquipment));
        }

        if(Input.GetButtonDown("CycleAbility")){
            //Get the next ability in the list, or the first if at the end
            if(playerManager.Abilities.Count > 1)
            {
                activeAbility = playerManager.Abilities[(playerManager.Abilities.IndexOf(activeAbility) + 1) % playerManager.Abilities.Count];
                SyncAbilityRpc((ushort)playerManager.Abilities.IndexOf(activeAbility));
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void FireWeaponRpc(bool isNewPress)
    {
        if(activeEquipment != null)
        {
            activeEquipment.Use(firePoint, playerManager, this, isNewPress);
        }
    }

    [Rpc(SendTo.Server)]
    public void ActivateAbilityRpc()
    {
        if(activeAbility != null)
        {
            activeAbility.Activate(playerManager);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SyncEquipmentRpc(ushort equipmentIndex)
    {
        activeEquipment = playerManager.Equipments[equipmentIndex];
    }

    [Rpc(SendTo.Everyone)]
    public void SyncAbilityRpc(ushort abilityIndex)
    {
        activeAbility = playerManager.Abilities[abilityIndex];
    }


    [Rpc(SendTo.Server)]
    public void FireProjectileRpc(ProjectileType projectile, float damage)
    {
        var projectileObj = Instantiate(PrefabLibrary.instance.Projectiles[projectile], firePoint.position, firePoint.rotation);
        projectileObj.GetComponent<Projectile>().SetDamage(damage);
        projectileObj.GetComponent<NetworkObject>().Spawn();
    }

    [Rpc(SendTo.Server)]
    public void FireHitscanRpc(float damage)
    {
        RaycastHit hit;
        if(Physics.Raycast(firePoint.position, firePoint.forward, out hit)){
            //If the raycast hits something with a Damage_Handler, deal damage
            if(hit.collider.GetComponent<Damage_Handler>()){
                hit.collider.GetComponent<Damage_Handler>().TakeDamage(damage);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void MeleeAttackRpc(float damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, 1.0f);
        foreach(Collider hitCollider in hitColliders){
            if(hitCollider.transform != firePoint.parent.parent.parent && hitCollider.GetComponent<Damage_Handler>()){
                hitCollider.GetComponent<Damage_Handler>().TakeDamage(damage);
            }
        }
    }
}
