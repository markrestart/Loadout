using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Message_System : NetworkBehaviour
{
    public static Message_System Instance;
    private List<string> messageQueue = new List<string>();
    [SerializeField]
    private TMPro.TextMeshProUGUI messageText;
    [SerializeField]
    private float maxMessageTime = 5f;
    [SerializeField]
    private float minMessageTime = .8f;
    [SerializeField]
    private float messageTimeLossPerQueue = .5f;
    [SerializeField]
    private float messageScaleRate = .5f;
    [SerializeField]
    private float messageYPosRate = .5f;
    private float messageTimer = 0f;
    private bool messageActive = false;
    private float startingScale;
    private float startingYPos;

    [Rpc(SendTo.Everyone)]
    public void AddMessageRpc(string message){
        messageQueue.Add(message);
    }

    public static void AddMessage(string message){
        if(Instance != null && Instance.IsServer){
            Instance.AddMessageRpc(message);
        }
    }

    private void Start() {
        if(Instance == null){
            Instance = this;
        }else{
            Destroy(this);
        }

        messageText.text = "";

        startingScale = messageText.transform.localScale.x;
        startingYPos = messageText.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(messageQueue.Count > 0 && !messageActive){
            messageActive = true;
            messageText.text = messageQueue[0];
            messageQueue.RemoveAt(0);
            messageTimer = maxMessageTime - (messageQueue.Count * messageTimeLossPerQueue);
            if(messageTimer < minMessageTime){
                messageTimer = minMessageTime;
            }
            messageText.transform.localScale = new Vector3(startingScale, startingScale, startingScale);
            messageText.transform.position = new Vector3(messageText.transform.position.x, startingYPos, messageText.transform.position.z);
        }
        if(messageActive){
            messageTimer -= Time.deltaTime;
            if(messageTimer <= 0){
                messageActive = false;
                messageText.text = "";
            }else{
                var newScale = messageText.transform.localScale.x + messageScaleRate * Time.deltaTime;
                messageText.transform.localScale = new Vector3(newScale, newScale, newScale);
                messageText.transform.position = new Vector3(messageText.transform.position.x, messageText.transform.position.y + messageYPosRate * Time.deltaTime, messageText.transform.position.z);
            }
        }
    }
}
