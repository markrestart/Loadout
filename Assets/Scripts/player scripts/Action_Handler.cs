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
    [SerializeField]
    public List<GameObject> weaponModels = new List<GameObject>();
    private float damageMultiplier = 1;

    public float DamageMultiplier { get => damageMultiplier; set => damageMultiplier = value; }
    private float fireRateMultiplier = 1;
    public float FireRateMultiplier { get => fireRateMultiplier; set {
        fireRateMultiplier = value;
        foreach(Data_Equipment equipment in playerManager.Equipments){
            equipment.setActivationRateMultiplier(value);
        }
    } }
    
    private float abilityCooldownMultiplier = 1;
    public float AbilityCooldownMultiplier { get => abilityCooldownMultiplier; set {
        abilityCooldownMultiplier = value;
        foreach(Data_Ability ability in playerManager.Abilities){
            ability.SetCooldownMultiplier(value);
        }
     } }

    private void Awake()
    {
        playerManager = GetComponent<Player_Manager>();
    }

    // Start is called before the first frame update
    public void Ready()
    {
        SyncAbilityRpc(0);
        SyncEquipmentRpc(0);
    }

    public void Unready()
    {
        SyncAbilityRpc(CONSTANTS.NULL_ID);
        SyncEquipmentRpc(CONSTANTS.NULL_ID);
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
                //TODO: Some abilities may not need to be sent as an rpc(Like movement abilities)
                ActivateAbilityRpc();
            }
        }

        if(Input.GetButtonDown("CycleEquipment")){
            //Get the next equipment in the list, or the first if at the end
            do{
                activeEquipment = playerManager.Equipments[(playerManager.Equipments.IndexOf(activeEquipment) + 1) % playerManager.Equipments.Count];
            }while(activeEquipment.ammoType != AmmoType.NA && playerManager.AmmoCount(activeEquipment.ammoType) == 0 && activeEquipment.CurrentAmmo == 0);
            
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
            SyncWeaponAfterUseRpc(activeEquipment.CurrentAmmo, playerManager.AmmoCount(activeEquipment.ammoType));
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SyncWeaponAfterUseRpc(int currentAmmo, int playerAmmo)
    {
        if(activeEquipment != null)
        {
            activeEquipment.SetCurrentAmmo(currentAmmo);
            playerManager.SetAmmo(activeEquipment.ammoType, playerAmmo);
        }
    }

    [Rpc(SendTo.Everyone)]
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
        if(playerManager.Equipments.Count == 0 || equipmentIndex == CONSTANTS.NULL_ID)
        {
            activeEquipment = null;
            return;
        }

        activeEquipment = playerManager.Equipments[equipmentIndex];
        for(int i = 0; i < weaponModels.Count; i++)
        {
            weaponModels[i].SetActive(i == (int)playerManager.Equipments[equipmentIndex].equipmentModel);
        }
        PlayEquipAnimation();
    }

    [Rpc(SendTo.Everyone)]
    public void SyncAbilityRpc(ushort abilityIndex)
    {
        if(playerManager.Abilities.Count == 0 || abilityIndex == CONSTANTS.NULL_ID)
        {
            activeAbility = null;
            return;
        }
        
        activeAbility = playerManager.Abilities[abilityIndex];
    }


    [Rpc(SendTo.Server)]
    public void FireProjectileRpc(float damage, ProjectileType projectile, EquipmentAVEffect avEffect)
    {
        var sourceID = playerManager.NetworkObject.OwnerClientId;
        var projectileObj = Instantiate(PrefabLibrary.instance.Projectiles[projectile], firePoint.position, firePoint.rotation);
        projectileObj.GetComponent<Projectile>().SetDamage(damage * damageMultiplier);
        projectileObj.GetComponent<Projectile>().SetSourceID(sourceID);
        projectileObj.GetComponent<NetworkObject>().Spawn();

        PlayFireAnimationRpc();
        AVFXRpc(avEffect, false, firePoint.position);
    }

    [Rpc(SendTo.Server)]
    public void FireHitscanRpc(float damage, EquipmentAVEffect avEffect)
    {
        bool isHit = false;
        var sourceID = playerManager.NetworkObject.OwnerClientId;
        RaycastHit hit;
        if(Physics.Raycast(firePoint.position, firePoint.forward, out hit)){
            //If the raycast hits something with a Damage_Handler, deal damage
            if(hit.collider.GetComponent<Damage_Handler>()){
                hit.collider.GetComponent<Damage_Handler>().TakeDamage(damage * damageMultiplier, sourceID);
                isHit = true;
            }
        }

        PlayFireAnimationRpc();
        AVFXRpc(avEffect, isHit, hit.point);
    }

    [Rpc(SendTo.Server)]
    public void MeleeAttackRpc(float damage, EquipmentAVEffect avEffect)
    {
        bool hit = false;
        var sourceID = playerManager.NetworkObject.OwnerClientId;
        Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, 1.5f);
        foreach(Collider hitCollider in hitColliders){
            if(hitCollider.transform != firePoint.parent.parent.parent && hitCollider.GetComponent<Damage_Handler>()){
                hitCollider.GetComponent<Damage_Handler>().TakeDamage(damage * (playerManager.Archetype != null ? playerManager.Archetype.meleeModifier : 1), sourceID);
                hit = true;
            }
        }

        PlayFireAnimationRpc();
        AVFXRpc(avEffect, hit, hitColliders.Length > 0 ? hitColliders[0].transform.position : firePoint.position);
    }

    [Rpc(SendTo.Everyone)]
    public void AVFXRpc(EquipmentAVEffect avEffect, bool isHit, Vector3 hitPoint){
        if(avEffect == EquipmentAVEffect.NA){
            return;
        }
        var fxObj = Instantiate(PrefabLibrary.instance.AVEffects[avEffect], hitPoint, Quaternion.identity);
        fxObj.GetComponent<AVEffect>().StartEffect(isHit, firePoint.position);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayFireAnimationRpc()
    {
        if(weaponModels[(int)activeEquipment.equipmentModel].GetComponent<Animation>() != null)
        {
            var animator = weaponModels[(int)activeEquipment.equipmentModel].GetComponent<Animation>();
            animator.clip = animator.GetClip($"{animator.gameObject.name}Fire");
            animator.Play();
            if(activeEquipment.fireSound != null){
                playerManager.WeaponSound.clip = activeEquipment.fireSound;
                playerManager.WeaponSound.Play();
            }
        }
    }

    private void PlayEquipAnimation(){
        if(weaponModels[(int)activeEquipment.equipmentModel].GetComponent<Animation>() != null)
        {
            var animator = weaponModels[(int)activeEquipment.equipmentModel].GetComponent<Animation>();
            animator.clip = animator.GetClip($"{animator.gameObject.name}Equip");
            animator.Play();
            if(activeEquipment.equipSound != null){
                playerManager.WeaponSound.clip = activeEquipment.equipSound;
                playerManager.WeaponSound.Play();
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ReloadRpc(){
        if(weaponModels[(int)activeEquipment.equipmentModel].GetComponent<Animation>() != null)
        {
            var animator = weaponModels[(int)activeEquipment.equipmentModel].GetComponent<Animation>();
            animator.clip = animator.GetClip($"{animator.gameObject.name}Reload");
            animator.Play();
            if(activeEquipment.reloadSound != null){
                playerManager.WeaponSound.clip = activeEquipment.reloadSound;
                playerManager.WeaponSound.Play();
            }
        }
    }
}
