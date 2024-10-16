using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    private float points;
    private Dictionary<ulong, float> playersContactStartTimes = new Dictionary<ulong, float>();
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private Collider triggerCollider;
    [SerializeField]
    private GameObject pickupEffect;

    // Update is called once per frame
    void Update()
    {
        if(!Rounds_Manager.Instance.RoundStarted){
            return;
        }
        bool wasPickedUp = points <= 0;
        points += Time.deltaTime;
        if(wasPickedUp && points > 0){
            meshRenderer.enabled = true;
            triggerCollider.enabled = true;
        }

        ulong bestPlayerID = CONSTANTS.NULL_ID;
        float bestTime = Time.time;
        // Check if any player has been in contact for 5 seconds
        foreach(ulong playerID in playersContactStartTimes.Keys){
            if(playersContactStartTimes[playerID] < bestTime){
                bestTime = playersContactStartTimes[playerID];
                bestPlayerID = playerID;
            }
        }
        if(bestTime <= Time.time - CONSTANTS.PICKUP_TIME){
            playersContactStartTimes.Clear();
            DoPickup(bestPlayerID);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(!Rounds_Manager.Instance.RoundStarted){
            return;
        }
        if(other.gameObject.GetComponent<Player_Manager>()){
            playersContactStartTimes.Add(other.gameObject.GetComponent<NetworkObject>().OwnerClientId, Time.time);
        }
        pickupEffect.SetActive(true);
    }

    private void OnTriggerExit(Collider other) {
        if(!Rounds_Manager.Instance.RoundStarted){
            return;
        }
        if(other.gameObject.GetComponent<Player_Manager>()){
            playersContactStartTimes.Remove(other.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
        if(playersContactStartTimes.Count == 0){
            pickupEffect.SetActive(false);
        }
    }

    private void DoPickup(ulong playerID){
        if(!IsServer){
            return;
        }
        Rounds_Manager.Instance.AddScoreRpc(playerID, points);
        SetPickupTimerRpc(Random.Range(-CONSTANTS.PICKUP_RESPAWN_TIME_MAX, -CONSTANTS.PICKUP_RESPAWN_TIME_MIN));
    }

    [Rpc(SendTo.Everyone)]
    public void SetPickupTimerRpc(float points){
        pickupEffect.SetActive(false);
        this.points = points;
        meshRenderer.enabled = false;
        triggerCollider.enabled = false;
    }
}