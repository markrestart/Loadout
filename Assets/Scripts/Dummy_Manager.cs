using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Dummy_Manager : NetworkBehaviour, ITakes_Damage
{
    [SerializeField]
    private float damageTaken;
    // UI text to display damage taken
    [SerializeField]
    private TMPro.TextMeshProUGUI damageText;
    public void TakeDamage(float damage)
    {
        Debug.Log("Dummy Damage: " + damage);
        TakeDamageRpc(damage);
    }

    [Rpc(SendTo.Everyone)]
    public void TakeDamageRpc(float damage)
    {
        Debug.Log("Dummy Damage rpc: " + damage);
        damageTaken += damage;
        damageText.text = $"+{damage} = {damageTaken}";
    }

}
