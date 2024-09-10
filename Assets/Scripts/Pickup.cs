using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private float points;
    private Dictionary<ulong, float> playersContactStartTimes = new Dictionary<ulong, float>();
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private Collider triggerCollider;

    // Update is called once per frame
    void Update()
    {
        bool wasPickedUp = points <= 0;
        points += Time.deltaTime;
        if(wasPickedUp && points > 0){
            meshRenderer.enabled = true;
            triggerCollider.enabled = true;
        }

        ulong bestPlayerID = 999;
        float bestTime = Time.time;
        // Check if any player has been in contact for 5 seconds
        foreach(ulong playerID in playersContactStartTimes.Keys){
            if(playersContactStartTimes[playerID] < bestTime){
                bestTime = playersContactStartTimes[playerID];
                bestPlayerID = playerID;
            }
        }
        if(bestTime <= Time.time - 5){
            playersContactStartTimes.Clear();
            DoPickup(bestPlayerID);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.GetComponent<Player_Manager>()){
            playersContactStartTimes.Add(other.gameObject.GetComponent<NetworkObject>().OwnerClientId, Time.time);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.GetComponent<Player_Manager>()){
            playersContactStartTimes.Remove(other.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void DoPickup(ulong playerID){
        Rounds_Manager.Instance.AddScoreRpc(playerID, points);
        points = Random.Range(-25, -10);
        meshRenderer.enabled = false;
        triggerCollider.enabled = false;
    }
}
