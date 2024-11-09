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
    [SerializeField]
    private GameObject explosionPrefab;

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
        if(explosionPrefab){
            //Get a quaternition the matches the up axis to the normal of the first contact point
            Quaternion rotation = Quaternion.LookRotation(other.contacts[0].normal, Vector3.up);
            Instantiate(explosionPrefab, transform.position, rotation);
            Explosion explosion = explosionPrefab.GetComponent<Explosion>();
            explosion.SetSourceID(sourceID);
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
