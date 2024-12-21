using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
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
    [SerializeField]
    private bool destroyOnTrigger = true;
    [SerializeField]
    private float timeToSet = 0.0f;
    private bool isSet = false;

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
        if(!IsOwner){
            return;
        }
        if(!destroyOnHit){
            return;
        }
        if(other.gameObject.GetComponent<Damage_Handler>()){
            other.gameObject.GetComponent<Damage_Handler>().TakeDamage(damage, sourceID);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        if(!IsOwner){
            return;
        }
        if(!destroyOnTrigger || isSet == false){
            return;
        }
        if(other.gameObject.GetComponent<Damage_Handler>()){
            Destroy(gameObject);
        }
    }

    private void Update() {
        if(!IsOwner){
            return;
        }
        if(destroyOnTime){
            timeToLive -= Time.deltaTime;
            if(timeToLive <= 0){
                Destroy(gameObject);
            }
        }

        if(timeToSet > 0){
            timeToSet -= Time.deltaTime;
            if(timeToSet <= 0){
                isSet = true;
            }
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();
        if(explosionPrefab){
            //Get a quaternition the matches the up axis to the normal of the first contact point
            Quaternion rotation = Quaternion.LookRotation(Vector3.up, Vector3.up);
            Instantiate(explosionPrefab, transform.position, rotation);
            Explosion explosion = explosionPrefab.GetComponent<Explosion>();
            explosion.SetSourceID(sourceID);
        }
    }
}
