using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reserve_Card_Manager : MonoBehaviour
{
    private Draft_Card card;
    public Draft_Card Card { get => card; set {
        card = value;
        nameText.text = card.Name;
        typeText.text = card.Type;
        descriptionText.text = card.Description;
        weightText.text = card.Weight > 0 ? card.Weight.ToString() : "";
     } }
    private bool isToggled;
    public bool IsToggled { get => isToggled; }

    [SerializeField]
    private TMPro.TextMeshProUGUI nameText;
    [SerializeField]
    private TMPro.TextMeshProUGUI typeText;
    [SerializeField]
    private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField]
    private TMPro.TextMeshProUGUI weightText;

    private Reserves_Controller reservesController;
    public Reserves_Controller ReservesController { get => reservesController; set => reservesController = value; }

    public void toggleCard()
    {
        isToggled = !isToggled;

        if(isToggled)
        {
            isToggled = false;
            isToggled = reservesController.TryAddToSelected(card);
        }
        else
        {
            reservesController.RemoveFromSelected(card);
        }

        transform.GetComponent<UnityEngine.UI.Image>().color = isToggled ? Color.gray: new Color(0.3186487f, 0.3396226f, 0.2803489f);
        reservesController.UpdateEquipTexts();
    }

    public void Deselect()
    {
        isToggled = false;
        transform.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3186487f, 0.3396226f, 0.2803489f);
    }
}
