using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class runLobbyPlayer : NetworkLobbyPlayer {

    public Button spriteRight;
    public Button spriteLeft;
    public Button readyButton;
    public RawImage spriteRaw;

    public Texture[] spriteList;

    static Color GreyedColor = new Color(200.0f / 255.0f, 200 / 255.0f, 200.0f / 255.0f, 1.0f);
    static Color GreenColor = new Color(0.0f / 255.0f, 250 / 255.0f, 0.0f / 255.0f, 1.0f);
    static Color OrangeColor = new Color(251.0f / 255.0f, 176 / 255.0f, 59.0f / 255.0f, 1.0f);

    public bool isThisLocalPlayer;

    [SyncVar]
    int sprite_selected;

    [SyncVar]
    bool thisPlayerReady;

	// Use this for initialization
	void Start () {
        SetupSelf();
        isThisLocalPlayer = isLocalPlayer;
        thisPlayerReady = readyToBegin;
	}
	
	// Update is called once per frame
	void Update () {
        spriteRaw.texture = spriteList[sprite_selected];
        //CmdToggleReady(readyToBegin);
        if (thisPlayerReady)
        {
            spriteLeft.interactable = false;
            spriteRight.interactable = false;
            GetComponent<Image>().color = GreenColor;
            //SendReadyToBeginMessage();

        }
        else
        {
            spriteLeft.interactable = isLocalPlayer;
            spriteRight.interactable = isLocalPlayer;
            GetComponent<Image>().color = OrangeColor;
            //SendNotReadyToBeginMessage();
        }
    }

    void SetupSelf() {
        GameObject container = GameObject.FindGameObjectWithTag("PlayerListPanel");
        transform.SetParent(container.transform);
        if(isLocalPlayer)
        {
            setupLocal();
        } else
        {
            setupOther();
        }
    }

    void setupLocal()
    {
        spriteLeft.interactable = true;
        spriteRight.interactable = true;
        readyButton.interactable = true;

        //try
        spriteLeft.gameObject.SetActive(true);
        spriteRight.gameObject.SetActive(true);
        readyButton.gameObject.SetActive(true);

    }

    void setupOther()
    {
        spriteLeft.interactable = false;
        spriteRight.interactable = false;
        readyButton.interactable = false;

        //try
        spriteLeft.gameObject.SetActive(false);
        spriteRight.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);

    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();
        SetupSelf();
    }

    public void toggleRight()
    {
        int y = (sprite_selected + 1) % spriteList.Length;
        CmdCharaChange(y);
    }

    public void toggleLeft()
    {
        int y = (sprite_selected - 1);
        if(y<0)
        {
            y = spriteList.Length - 1;
        }
        CmdCharaChange(y);
    }

    [Command] void CmdCharaChange(int i)
    {
        sprite_selected = i;
    }

    public void toggleReady()
    {
        bool readyyy= !readyToBegin;
        CmdToggleReady(readyyy);
        if(readyyy)
        {
            SendReadyToBeginMessage();
        } else
        {
            SendNotReadyToBeginMessage();
        }
    }

    [Command] void CmdToggleReady(bool ready)
    {
        readyToBegin = ready;
        thisPlayerReady = ready;
        OnClientReady(readyToBegin);
    }

}
