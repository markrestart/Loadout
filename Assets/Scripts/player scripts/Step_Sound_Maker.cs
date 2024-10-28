using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step_Sound_Maker : MonoBehaviour
{
	[SerializeField]
    private float stepRate = 0.5f;
    [SerializeField]
    private float minStepDistance = 0.1f;
    [SerializeField]
    private AudioSource audioSource;
    
    private Vector3 lastStepPosition;
    private float stepCoolDown;

 
	// Update is called once per frame
	void Update () {
		stepCoolDown -= Time.deltaTime;
		if ((Vector3.Distance(lastStepPosition, transform.position) > minStepDistance) && stepCoolDown < 0f){
			audioSource.pitch = 1f + Random.Range (-0.2f, 0.2f);
			audioSource.Play();
			stepCoolDown = stepRate;
            lastStepPosition = transform.position;
		}
	}
}