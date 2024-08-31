using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLibrary : MonoBehaviour
{
    [SerializeField]
    private List<ProjectilePrefabListing> projectilePrefabs = new List<ProjectilePrefabListing>();
    public static PrefabLibrary instance;
    private Dictionary<ProjectileType, GameObject> projectiles = new Dictionary<ProjectileType, GameObject>();
    public Dictionary<ProjectileType, GameObject> Projectiles { get => projectiles; }
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null){
            instance = this;
            foreach(ProjectilePrefabListing prefabListing in projectilePrefabs){
                projectiles.Add(prefabListing.type, prefabListing.prefab);
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