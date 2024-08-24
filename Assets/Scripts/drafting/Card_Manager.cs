using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_Manager : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI nameText;
    [SerializeField]
    private TMPro.TextMeshProUGUI typeText;
    [SerializeField]
    private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField]
    private TMPro.TextMeshProUGUI weightText;
    [SerializeField]
    private Draft_Manager draftManager;

    private Draft_Card card;
    // Start is called before the first frame update
    public void SetCard(Draft_Card card)
    {
        nameText.text = card.Name;
        typeText.text = card.Type;
        descriptionText.text = card.Description;
        weightText.text = card.Weight > 0 ? card.Weight.ToString() : "";
        this.card = card;
    }

    public void SelectCard()
    {
        draftManager.SelectCard(card);
    }
}
