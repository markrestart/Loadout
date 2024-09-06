using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "Equipment", order = 0)]
public class SO_Equipment : ScriptableObject {
    public string equipmentName = "Equipment";
    public string description = "This is a description of the equipment.";

    public EquipmentType equipmentType = EquipmentType.RangedWeapon;
    public EquipmentModel equipmentModel;
    public AmmoType ammoType = AmmoType.NA;
    public int magazineSize = 0;
    private int currentAmmo = 0;
    public int CurrentAmmo { get => currentAmmo; }
    public float reloadTime = 1.0f;
    public float damage = 10.0f;
    public bool isHitscan = true;
    public ProjectileType projectile = ProjectileType.NA;
    public bool isAutomatic = false;
    public float activationRate = 1.0f;
    public float weight = 1.0f;

    private float lastActivationTime = 0.0f;
    private float lastReloadTime = 0.0f;
    private Player_Manager playerManager;

    //Base use for weapons. this will handle most ranged weapons and melee weapons
    public void Use(Transform firePoint, bool isNewPress) {
        if(Time.time - lastActivationTime < activationRate){
            return;
        }
        if(Time.time - lastReloadTime < reloadTime){
            return;
        } 
        if(!isAutomatic && !isNewPress){
            return;
        }
        if(currentAmmo <= 0){
            Reload();
            return;
        }
        lastActivationTime = Time.time;
        currentAmmo--;
        if(isHitscan){
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
            //GameObject newProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation);
            //newProjectile.GetComponent<Rigidbody>().AddForce(firePoint.forward * 1000);
        }
    }

    public void Ready(Player_Manager player) {
        lastActivationTime = Time.time;
        lastReloadTime = Time.time;
        currentAmmo = magazineSize;
        playerManager = player;
    }

    public void Reload() {
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
}

public enum EquipmentType {
    RangedWeapon,
    MeleeWeapon
}

public enum AmmoType {
    NA,
    Arrow,
    Bullet,
    Shell,
    Rocket,
    Energy
}

public enum ProjectileType {
    NA,
    Granade,
    Rocket,
    Arrow
}

public enum EquipmentModel {
    Base,
    Pistol,
    Rifle,
    Sword,
    RocketLauncher,
    Crossbow,
    LazerGun,
}