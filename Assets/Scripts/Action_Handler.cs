using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Action_Handler : MonoBehaviour
{
    [SerializeField]
    private Transform firePoint;
    private Player_Manager playerManager;

    private Equipment activeEquipment;
    public Equipment ActiveEquipment { get => activeEquipment; }
    private Ability activeAbility;

    private void Awake()
    {
        playerManager = GetComponent<Player_Manager>();
    }

    // Start is called before the first frame update
    public void Ready()
    {
        activeEquipment = playerManager.Equipments.FirstOrDefault();
        activeAbility = playerManager.Abilities.FirstOrDefault();
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerManager.IsReady){
            return;
        }
        if(Input.GetButton("Fire1"))
        {
            if(activeEquipment != null)
            {
                activeEquipment.Use(firePoint, Input.GetButtonDown("Fire1"));
            }
        }

        if(Input.GetButtonDown("Ability")){
            if(activeAbility != null)
            {
                if(activeAbility.IsActive)
                {
                    activeAbility.Deactivate();
                }else{
                    activeAbility.Activate();
                }
            }
        }

        if(Input.GetButtonDown("CycleEquipment")){
            //Get the next equipment in the list, or the first if at the end
            activeEquipment = playerManager.Equipments[(playerManager.Equipments.IndexOf(activeEquipment) + 1) % playerManager.Equipments.Count];
        }

        if(Input.GetButtonDown("CycleAbility")){
            //Get the next ability in the list, or the first if at the end
            activeAbility = playerManager.Abilities[(playerManager.Abilities.IndexOf(activeAbility) + 1) % playerManager.Abilities.Count];
        }
    }
}
