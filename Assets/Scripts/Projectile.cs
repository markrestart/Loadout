using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float timeToLive = 5.0f;

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }



    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Damage_Handler>())
        {
            other.GetComponent<Damage_Handler>().TakeDamage(damage);
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
