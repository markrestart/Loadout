using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField]
    private float damagePerSecond;
    private ulong sourceID;
    [SerializeField]
    private float timeToLive = 5.0f;
    [SerializeField]
    private float explosionForce = 10.0f;
    [SerializeField]
    private float expansionRate = 1.0f;
    private List<Damage_Handler> damageHandlers = new List<Damage_Handler>();

    public void SetSourceID(ulong sourceID)
    {
        this.sourceID = sourceID;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.GetComponent<Damage_Handler>()){
            damageHandlers.Add(other.gameObject.GetComponent<Damage_Handler>());
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.GetComponent<Damage_Handler>()){
            damageHandlers.Remove(other.gameObject.GetComponent<Damage_Handler>());
        }
    }

    private void Update() {
        timeToLive -= Time.deltaTime;
        if(timeToLive <= 0){
            Destroy(gameObject);
        }

        transform.localScale += Vector3.one * expansionRate * Time.deltaTime;

        foreach(Damage_Handler handler in damageHandlers.ToList()){
            handler.TakeDamage(damagePerSecond * Time.deltaTime, sourceID);
            handler.ApplyForce((handler.transform.position - transform.position).normalized * explosionForce * Time.deltaTime);
        }
    }
}
