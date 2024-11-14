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
    [SerializeField]
    private bool destroyOnHit = true;
    [SerializeField]
    private bool destroyOnTime = true;

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
        if(!destroyOnTime){
            return;
        }
        timeToLive -= Time.deltaTime;
        if(timeToLive <= 0){
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if(explosionPrefab){
            //Get a quaternition the matches the up axis to the normal of the first contact point
            Quaternion rotation = Quaternion.LookRotation(Vector3.up, Vector3.up);
            Instantiate(explosionPrefab, transform.position, rotation);
            Explosion explosion = explosionPrefab.GetComponent<Explosion>();
            explosion.SetSourceID(sourceID);
        }
    }
}
