using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage_Handler : MonoBehaviour
{
    [SerializeField]
    private ITakes_Damage damageTaker;
    public void TakeDamage(float damage)
    {
        damageTaker.TakeDamage(damage);
    }
    private void Start() {
        damageTaker = GetComponent<ITakes_Damage>();
    }
}
