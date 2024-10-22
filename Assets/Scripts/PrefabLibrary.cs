using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLibrary : MonoBehaviour
{
    [SerializeField]
    private List<ProjectilePrefabListing> projectilePrefabs = new List<ProjectilePrefabListing>();
    [SerializeField]
    private List<avEffectPrefabListing> avEffectPrefabs = new List<avEffectPrefabListing>();
    public static PrefabLibrary instance;
    private Dictionary<ProjectileType, GameObject> projectiles = new Dictionary<ProjectileType, GameObject>();
    public Dictionary<ProjectileType, GameObject> Projectiles { get => projectiles; }
    private Dictionary<EquipmentAVEffect, GameObject> avEffects = new Dictionary<EquipmentAVEffect, GameObject>();
    public Dictionary<EquipmentAVEffect, GameObject> AVEffects { get => avEffects; }
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null){
            instance = this;
            foreach(ProjectilePrefabListing prefabListing in projectilePrefabs){
                projectiles.Add(prefabListing.type, prefabListing.prefab);
            }
            foreach(avEffectPrefabListing prefabListing in avEffectPrefabs){
                avEffects.Add(prefabListing.type, prefabListing.prefab);
            }
        }
        else{
            Destroy(this);
        }
    }

    
}

[System.Serializable]
public struct ProjectilePrefabListing {
    public ProjectileType type;
    public GameObject prefab;
}

[System.Serializable]
public struct avEffectPrefabListing {
    public EquipmentAVEffect type;
    public GameObject prefab;
}