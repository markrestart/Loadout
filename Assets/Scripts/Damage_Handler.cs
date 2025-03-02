using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage_Handler : MonoBehaviour
{
    [SerializeField]
    private ITakes_Damage damageTaker;
    public void TakeDamage(float damage, ulong sourceID)
    {
        damageTaker.TakeDamage(damage, sourceID);
    }

    public void ApplyForce(Vector3 force)
    {
        damageTaker.ApplyForce(force);
    }
    private void Start() {
        damageTaker = GetComponent<ITakes_Damage>();
    }
}
