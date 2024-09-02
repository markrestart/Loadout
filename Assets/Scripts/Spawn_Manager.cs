using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Manager : MonoBehaviour
{
    private int spawnCount;
    public int SpawnCount { get => spawnCount; }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "Player")
        {
            spawnCount--;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Player")
        {
            spawnCount++;
        }
    }
}
