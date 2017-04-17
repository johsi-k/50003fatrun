using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;

    [SyncVar]
    int sprite_selected;

    [SyncVar]
    public bool thisPlayerReady;

    // Use this for initialization
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);

        SetupSelf();
        isThisLocalPlayer = isLocalPlayer;
        thisPlayerReady = readyToBegin;

    }


    void Awake()
    {
       
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

        // tells each lobbyPlayer object to transform itself into a gamePlayer object when game scene is active scene
        if (SceneManager.GetActiveScene().name == "Game")
        {
            CmdownGenerateCharacter();
        }


    }

    void SetupSelf() {
        GameObject container = GameObject.FindGameObjectWithTag("PlayerListPanel");  // PlayerListPanel is child of WaitingRoomCanvas
        transform.SetParent(container.transform); // sets incoming lobby player as child of PlayerListPanel
        if(isLocalPlayer)
        {
            setupLocal();
        } else
        {
            setupOther();
        }
    }

    public void generateMe()
    {
        CmdownGenerateCharacter();
    }

    [Command]
    void CmdownGenerateCharacter()
    {
        //if (SceneManager.GetActiveScene().name == "Game")
            Debug.Log("generating character " + connectionToClient.ToString());
        {
            Vector3 spawn = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
            GameObject gamePlayerInstance;// = Instantiate(prefab, spawn, Quaternion.identity);
            if(sprite_selected == 0)
            {
                gamePlayerInstance = Instantiate(prefab1, spawn, Quaternion.identity);
            } else if(sprite_selected == 1)
            {
                gamePlayerInstance = Instantiate(prefab2, spawn, Quaternion.identity);
            } else
            {
                gamePlayerInstance = Instantiate(prefab3, spawn, Quaternion.identity);
            }

            // gamePlayer customisation goes here

            //NetworkServer.DestroyPlayersForConnection(connectionToClient);
            // connectionToClient: connection that this lobbyPlayer has to the client
            NetworkServer.ReplacePlayerForConnection(connectionToClient, gamePlayerInstance, playerControllerId); // replaces the reference
            Destroy(gameObject); // destroys the original lobbyPlayer object 
            
        }
    }

    void setupLocal()  // if local player
    {
        //(runLobbyManager.singleton as runLobbyManager).ownGenerateCharacter(this.connectionToServer);
        //CmdownGenerateCharacter();
        // makes buttons visible and clickable
        spriteLeft.gameObject.SetActive(true);
        spriteRight.gameObject.SetActive(true);
        readyButton.gameObject.SetActive(true);



        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.GetComponent<Menu>().RandomMatchWaitingRoom();

        // throws UI from LoadingCanvas to WaitingRoomCanvas 
        // this is called in setupLocal() because we know that
        // once localPlayer is set up, all other lobbyplayers are
        // also set up, hence the canvas is ready for viewing

        // (runLobbyManager.singleton as runLobbyManager).someFunction();
        

    }

    void setupOther()  // if not local player
    {
        //try
        spriteLeft.gameObject.SetActive(false);
        spriteRight.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);

    }



    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();
        SetupSelf();

        //checks for GameScene and add player prefab
        string scene = SceneManager.GetActiveScene().name;
        print("At scene " + scene);
        if (scene == "Game")
        {

        }

        // adds player object to list in lobby manager
        (runLobbyManager.singleton as runLobbyManager).AddLobbyPlayer(this);
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


    public override void OnClientExitLobby()
    {
        base.OnClientExitLobby();

    }

}
