using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Data_Equipment
{
    public string equipmentName;
    public string description;

    public EquipmentType equipmentType;
    public AmmoType ammoType;
    public int magazineSize;
    private int currentAmmo;
    public int CurrentAmmo { get => currentAmmo; }
    public float reloadTime;
    public float damage;
    public bool isHitscan;
    public ProjectileType projectile;
    public bool isAutomatic;
    public float activationRate;
    public float weight;

    private float lastActivationTime;
    private float lastReloadTime;

        //Base use for weapons. this will handle most ranged weapons and melee weapons
    public void Use(Transform firePoint, Player_Manager playerManager, bool isNewPress) {
        if(Time.time - lastActivationTime < activationRate){
            return;
        }
        if(Time.time - lastReloadTime < reloadTime){
            return;
        } 
        if(!isAutomatic && !isNewPress){
            return;
        }
        if(equipmentType == EquipmentType.RangedWeapon && currentAmmo <= 0){
            Reload(playerManager);
            return;
        }
        lastActivationTime = Time.time;
        if(equipmentType == EquipmentType.MeleeWeapon){
            //Deal damage to all enemies in a radius
            Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, 1.0f);
            foreach(Collider hitCollider in hitColliders){
                if(hitCollider.transform != firePoint.parent.parent.parent && hitCollider.GetComponent<Damage_Handler>()){
                    hitCollider.GetComponent<Damage_Handler>().TakeDamage(damage);
                }
            }
        }
        else if(isHitscan){
            currentAmmo--;
            //Spawn hitscan projectile
            RaycastHit hit;
            if(Physics.Raycast(firePoint.position, firePoint.forward, out hit)){
                //If the raycast hits something with a Damage_Handler, deal damage
                if(hit.collider.GetComponent<Damage_Handler>()){
                    hit.collider.GetComponent<Damage_Handler>().TakeDamage(damage);
                }
            }
        }else{
            //Spawn projectile TODO: Have a projectile manager to grab the prefab from so this can be networked
            GameObject newProjectile = GameObject.Instantiate(PrefabLibrary.instance.projectiles[projectile], firePoint.position, firePoint.rotation);
            newProjectile.GetComponent<Rigidbody>().AddForce(firePoint.forward * 1000);
        }
    }
    public void Ready() {
        lastActivationTime = Time.time;
        lastReloadTime = Time.time;
        currentAmmo = magazineSize;
    }

    public void Reload(Player_Manager playerManager) {
        var ammoNeeded = magazineSize - currentAmmo;
        var ammoCount = playerManager.AmmoCount(ammoType);
        if(ammoCount >= ammoNeeded){
            playerManager.UseAmmo(ammoType, ammoNeeded);
            currentAmmo = magazineSize;
        }else if(ammoCount > 0){
            playerManager.UseAmmo(ammoType, ammoCount);
            currentAmmo += ammoCount;
        }
        lastReloadTime = Time.time;
    }

    public Data_Equipment(SO_Equipment scriptableEquipment){
        this.equipmentName = scriptableEquipment.equipmentName;
        this.description = scriptableEquipment.description;
        this.equipmentType = scriptableEquipment.equipmentType;
        this.ammoType = scriptableEquipment.ammoType;
        this.magazineSize = scriptableEquipment.magazineSize;
        this.currentAmmo = magazineSize;
        this.reloadTime = scriptableEquipment.reloadTime;
        this.damage = scriptableEquipment.damage;
        this.isHitscan = scriptableEquipment.isHitscan;
        this.projectile = scriptableEquipment.projectile;
        this.isAutomatic = scriptableEquipment.isAutomatic;
        this.activationRate = scriptableEquipment.activationRate;
        this.weight = scriptableEquipment.weight;
        this.lastActivationTime = 0.0f;
        this.lastReloadTime = 0.0f;
    }
}
