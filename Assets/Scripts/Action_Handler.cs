using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Action_Handler : NetworkBehaviour
{
    [SerializeField]
    private Transform firePoint;
    private Player_Manager playerManager;

    private Data_Equipment activeEquipment;
    public Data_Equipment ActiveEquipment { get => activeEquipment; }
    private Data_Ability activeAbility;

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

    public void Unready()
    {
        activeEquipment = null;
        activeAbility = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner){
            return;
        }
        
        if(!playerManager.IsReady){
            return;
        }
        if(Input.GetButton("Fire1"))
        {
            if(activeEquipment != null)
            {
                activeEquipment.Use(firePoint, playerManager, Input.GetButtonDown("Fire1"));
            }
        }

        if(Input.GetButtonDown("Ability")){
            if(activeAbility != null)
            {
                activeAbility.Activate();
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
