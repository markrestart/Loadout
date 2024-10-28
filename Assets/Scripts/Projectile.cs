using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private ulong sourceID;
    [SerializeField]
    private float timeToLive = 5.0f;
    [SerializeField]
    private float speed = 10.0f;

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetSourceID(ulong sourceID)
    {
        this.sourceID = sourceID;
    }

    private void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.GetComponent<Damage_Handler>()){
            other.gameObject.GetComponent<Damage_Handler>().TakeDamage(damage, sourceID);
        }
        Destroy(gameObject);
    }

    private void Update() {
        timeToLive -= Time.deltaTime;
        if(timeToLive <= 0){
            Destroy(gameObject);
        }
    }
}
