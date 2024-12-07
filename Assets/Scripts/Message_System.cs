using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Message_System : NetworkBehaviour
{
    public static Message_System Instance;
    private List<string> messageQueue = new List<string>();
    private List<System.Tuple<string, bool, int>> tutorialQueue = new List<System.Tuple<string, bool, int>>();
    private int tutorialID = 0;
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

    private InputAction messageAction;
    [SerializeField]
    private GameObject tutorialUI;
    [SerializeField]
    private TMPro.TextMeshProUGUI tutorialText;
    [SerializeField]
    private TMPro.TextMeshProUGUI clearTutorialText;

    [Rpc(SendTo.Everyone)]
    public void AddMessageRpc(string message){
        messageQueue.Add(message);
    }

    public static void AddMessage(string message){
        if(Instance != null && Instance.IsServer){
            Instance.AddMessageRpc(message);
        }
    }

    public static void LocalMessage(string message){
        if(Instance != null){
            Instance.messageQueue.Add(message);
        }
    }

    public static int AddTutorialMessage(string message, bool clearableByPlayer){
        if(Instance != null){
            int id = Instance.tutorialID++;
            Instance.tutorialQueue.Add(new System.Tuple<string, bool, int>(message, clearableByPlayer, id));
            return id;
        }
        return -1;
    }

    public void ClearTutorialMessage(int id){
        if(Instance != null){
            OnMessageCleared.Invoke(id);
            Instance.tutorialQueue.RemoveAll(x => x.Item3 == id);
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

        messageAction = InputSystem.actions.FindAction("Jump");
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
    
        if(tutorialQueue.Count > 0){
            tutorialUI.SetActive(true);
            if(tutorialQueue[0].Item2){
                clearTutorialText.gameObject.SetActive(true);
            }
            tutorialText.text = tutorialQueue[0].Item1;
            if(messageAction.triggered){
                if(tutorialQueue[0].Item2){
                    OnMessageCleared.Invoke(tutorialQueue[0].Item3);
                    tutorialQueue.RemoveAt(0);
                }
            }
        }else{
            tutorialUI.SetActive(false);
        }
    }

    #region listeners
        public UnityEvent<int> OnMessageCleared = new UnityEvent<int>();
    #endregion
}
