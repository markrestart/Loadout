using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLibrary : MonoBehaviour
{
    public static PrefabLibrary instance;
    public Dictionary<ProjectileType, GameObject> projectiles = new Dictionary<ProjectileType, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null){
            instance = this;
        }
        else{
            Destroy(this);
        }
    }

    
}
