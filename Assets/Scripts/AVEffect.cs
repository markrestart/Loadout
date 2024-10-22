using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AVEffect : MonoBehaviour
{
    [SerializeField]
    private float duration;
    [SerializeField]
    private EquipmentAVEffect type;
    
    public void StartEffect(bool isHit, Vector3 sourcePosition){
        switch(type){
            case EquipmentAVEffect.NA:
                break;
            case EquipmentAVEffect.Impact:
                if(isHit){
                    Destroy(gameObject);
                }else{
                    transform.LookAt(sourcePosition);
                    Destroy(gameObject, duration);
                }
                break;
            case EquipmentAVEffect.Laser:
                StartCoroutine(TrailWalk(sourcePosition, transform.position));
                Destroy(gameObject, duration);
                break;
        }
    }

    private IEnumerator TrailWalk(Vector3 start, Vector3 end){
        transform.position = start;
        yield return null;
        transform.position = end;
    }
}
