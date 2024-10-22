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
    public EquipmentModel equipmentModel;
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
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip equipSound;
    public EquipmentAVEffect avEffect = EquipmentAVEffect.NA;

    private float lastActivationTime;
    private float lastReloadTime;
    private float activationRateMultiplier = 1;

    public void setActivationRateMultiplier(float multiplier){
        activationRateMultiplier = multiplier;
    }

        //Base use for weapons. this will handle most ranged weapons and melee weapons
    public void Use(Transform firePoint, Player_Manager playerManager, Action_Handler action_Handler, bool isNewPress) {
        if(Time.time - lastActivationTime < activationRate * activationRateMultiplier){
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
            if(reloadTime > 0){
                action_Handler.ReloadRpc();
            }
            return;
        }
        lastActivationTime = Time.time;
        if(equipmentType == EquipmentType.MeleeWeapon){
            //Deal damage to all enemies in a radius
            action_Handler.MeleeAttackRpc(damage, avEffect);
        }
        else if(isHitscan){
            currentAmmo--;
            action_Handler.FireHitscanRpc(damage, avEffect);
        }else{
            currentAmmo--;
            action_Handler.FireProjectileRpc(damage, projectile, avEffect);
        }
    }
    public void Ready() {
        lastActivationTime = Time.time;
        lastReloadTime = Time.time;
        currentAmmo = magazineSize;
    }

    public void SetCurrentAmmo(int ammo){
        currentAmmo = ammo;
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
        this.equipmentModel = scriptableEquipment.equipmentModel;
        this.fireSound = scriptableEquipment.fireSound;
        this.reloadSound = scriptableEquipment.reloadSound;
        this.equipSound = scriptableEquipment.equipSound;
        this.avEffect = scriptableEquipment.avEffect;
    }
}
