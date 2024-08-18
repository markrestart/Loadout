using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy_Manager : MonoBehaviour, ITakes_Damage
{
    [SerializeField]
    private float damageTaken;
    // UI text to display damage taken
    [SerializeField]
    private TMPro.TextMeshProUGUI damageText;
    public void TakeDamage(float damage)
    {
        damageTaken += damage;
        damageText.text = $"+{damage} = {damageTaken}";
    }

}
