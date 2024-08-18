using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "Equipment", order = 0)]
public class Equipment : ScriptableObject {
    public string equipmentName = "Equipment";
    public string description = "This is a description of the equipment.";

    public EquipmentType equipmentType = EquipmentType.RangedWeapon;
    public AmmoType ammoType = AmmoType.NA;
    public int magazineSize = 0;
    private int currentAmmo = 0;
    public int CurrentAmmo { get => currentAmmo; }
    public float reloadTime = 1.0f;
    public float damage = 10.0f;
    public bool isHitscan = true;
    public GameObject projectile;
    public bool isAutomatic = false;
    public float activationRate = 1.0f;
    public float weight = 1.0f;

    private float lastActivationTime = 0.0f;
    private float lastReloadTime = 0.0f;
    private Player_Manager playerManager;

    //Base use for weapons. this will handle most ranged weapons and melee weapons
    //Gadgets will be extended from this class and will have their own use functions
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
            //Spawn projectile
            GameObject newProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation);
            newProjectile.GetComponent<Rigidbody>().AddForce(firePoint.forward * 1000);
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
    MeleeWeapon,
    Gadget
}

public enum AmmoType {
    NA,
    Arrow,
    Bullet,
    Shell,
    Rocket,
    Energy
}