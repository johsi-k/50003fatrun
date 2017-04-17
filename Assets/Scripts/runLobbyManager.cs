using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;



public class runLobbyManager : NetworkLobbyManager {
    //public Text targetText;
    public GameObject mainCamera;
    private Button soundOn;
    //public Text mytext = null;
    int counter = 1;
    private static System.Random random = new System.Random();
    public string partyID;  // for match CREATION
    public List<NetworkConnection> networkConnectionList = new List<NetworkConnection>();

    // tracks if lobby players are ready on the client and saves local player state
    public List<runLobbyPlayer> lobbyPlayers = new List<runLobbyPlayer>();

    public int expectedPlayers = 99999;

    public enum NetworkState
    {
        Inactive,
        Pregame,
        Connecting,
        InLobby,
        InGame
    }

    public NetworkState state
    {
        get;
        protected set;
    }

    private Action<bool, MatchInfo> m_NextMatchCreatedCallback;
    private Action<bool, MatchInfo> m_NextMatchJoinedCallback;

    // called when we've created a match
    public event Action<bool, MatchInfo> matchCreated; 

    // called when a matchmade game is joined
    public event Action<bool, MatchInfo> matchJoined;


    //public RawImage player1img,player2img,player3img,player4img;
    //public Texture playeron1;

    private void Update()
    {
        UpdateLobbyPlayers();
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }


    public void onCreateClicked() // bound to button which creates a match for lobby to be visible; initiated by host
    {
        this.StartMatchMaker();
        partyID = RandomString(4);
        print("randomstring: " + partyID);
        matchMaker.CreateMatch(partyID, 4, true, "matchPassword", "", "", 0, 0, OnMatchCreate); 
        // partyID is passed into MatchInfoSnapshot.name when match is created
    }

    public void onGoClicked()  // bound to button which enables player to join a listed match
    {
        this.StartMatchMaker();
        print("attempting to join a listed match");
        matchMaker.ListMatches(0, 5, "", false, 0, 0, OnMatchList);
        // MatchInfoSnapshot is only generated here, not when match is created
    }


    // callback that happens when NetworkMatch.ListMatches request has been processed on server
    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        base.OnMatchList(success, extendedInfo, matchList);
        if (success)  // list of (possibly non-empty) matches received 
        {
            if (matchList.Count != 0)  // if list is non-empty
            {
            
                string partyIDInputted = GameObject.FindGameObjectWithTag("partyIDInput").GetComponent<InputField>().text;
                foreach (MatchInfoSnapshot snapshot in matchList)
                {
                    print("match name from matchinfosnapshot: " + snapshot.name);

                    // checks if inputted party ID corresponds to a party ID of any of the existing matches
                    // and if match has not reached capacity
                    if (snapshot.name == partyIDInputted && snapshot.currentSize < snapshot.maxSize)
                    {
                        Debug.Log("requested match/list of matches was returned, attempting to join");
                        matchMaker.JoinMatch(snapshot.networkId, "matchPassword", "", "", 0, 0, OnMatchJoined);
                        //break;
                        return;
                    }
                }
            }

            // whether list is empty or not, if JoinMatch() is not called, 
            // player fails to join and returns to JoinPartyCanvas
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Menu>().joinParty();

            // before returning to JoinPartyCanvas, an error message is displayed
            // error panel is first sibling (backmost child of the canvas) under normal circumstances; becomes last sibling (foremost child) when player fails to join
            GameObject.FindGameObjectWithTag("ErrorMessage").GetComponent<RectTransform>().SetAsLastSibling();      
        }
    }


    // callback that happens when NetworkMatch.CreateMatch request has been processed on the server
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        Debug.Log("matched created");


        if (success) {
            state = NetworkState.InLobby;
            Text partyIDText = GameObject.FindGameObjectWithTag("PartyID").GetComponent<Text>();
            partyIDText.text = partyID;
            
        }

        else
        {
            state = NetworkState.Inactive;
        }

    }

    // callback that happens when NetworkMatch.JoinMatch request has been processed on the server
    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        Debug.Log("OnMatchJoined");

        Text partyIDText = GameObject.FindGameObjectWithTag("PartyID").GetComponent<Text>();
        partyIDText.text = "";

        if (success)
        {
            Debug.Log("loggggg");
            state = NetworkState.InLobby;
        }
        else
        {
            state = NetworkState.Pregame;
        }

    }

    public override void OnLobbyClientEnter()
    {
        Debug.Log("entering lobby");
        base.OnLobbyClientEnter();
    }


    public override void OnLobbyClientExit()
    {
        base.OnLobbyClientExit();
    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        base.OnLobbyClientConnect(conn);
        //Toast.Create(null, "Connected");
        //mytext.text = Network.player.ipAddress+"\n client connected "+counter;
        counter++;
        
    }

    
    public void OnLobbyClientDisconnect()
    {
        Debug.Log("Client disconnected");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server stopped");
    }

    // check that all players are ready in lobby
    private bool checkAllReady()
    {
        return lobbyPlayers.All(p => p.thisPlayerReady);
    }

    // adds player to lobby when client enters lobby 
    public void AddLobbyPlayer(runLobbyPlayer player)
    {
        lobbyPlayers.Add(player.GetComponent<runLobbyPlayer>());
        Debug.Log("adding player to lobby");
    }


    // updates list of lobby players 
    private void UpdateLobbyPlayers()
    {
        //if (lobbyPlayers.Count != numPlayers)
        //{
        // removing objects which have become null (player disconnects)
        lobbyPlayers = lobbyPlayers.Where(p => p!=null).ToList();
    
        //}
    }


    public override void OnLobbyServerPlayersReady()
    {
        expectedPlayers = lobbyPlayers.Count;
        foreach(runLobbyPlayer player in lobbyPlayers)
        {
            try
            {
                // detaches runLobbyPlayer from its parent (playerListPanel) and sets new parent to null
                player.transform.SetParent(null, false);
                DontDestroyOnLoad(player.gameObject);  
            }
            catch { }
        }
        base.OnLobbyServerPlayersReady();

    }


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log(conn.ToString());
        networkConnectionList.Add(conn);

        //if (SceneManager.GetActiveScene().name == "Game")
        //{
        //    Vector3 spawn = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
        //    GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab, spawn, Quaternion.identity);

        //    // gamePlayer customisation goes here


        //    NetworkServer.DestroyPlayersForConnection(conn);
        //    NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance, 0);
        //}


        
            //print("length of network connection list: " + networkConnectionList.Count);
            base.OnServerAddPlayer(conn, playerControllerId);
            //ownGenerateCharacter(conn);
        



    }


    public void someFunction() 
    {
        //generate everyone
        if (SceneManager.GetActiveScene().name == "Game" && lobbyPlayers.Count == expectedPlayers)
        {
            Debug.Log("NCcount " + networkConnectionList.Count);
            foreach (runLobbyPlayer player in lobbyPlayers)
            {
                Debug.Log("generating own character");
                //player.generateMe();
                //if (SceneManager.GetActiveScene().name == "Game")
                {
                    Vector3 spawn = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
                    GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab, spawn, Quaternion.identity);

                    // gamePlayer customisation goes here


                    NetworkServer.DestroyPlayersForConnection(player.connectionToClient);
                    NetworkServer.AddPlayerForConnection(player.connectionToClient, gamePlayerInstance, 0);
                }
            }
        }
    }


    public void ownGenerateCharacter(NetworkConnection conn)
    {
        Debug.Log("generating character" + conn.ToString());
      
        if (SceneManager.GetActiveScene().name == "Game")
        {
            Vector3 spawn = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
            GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab, spawn, Quaternion.identity);

            // gamePlayer customisation goes here


            NetworkServer.DestroyPlayersForConnection(conn);
            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance, 0);
        }
    }

    public void OnMatchDestroy(bool success, string extendedInfo)
    {
        
    }

 
    public void OnQuit()
    {
        foreach (runLobbyPlayer player in lobbyPlayers)
        {
            if (player != null)
            {
                if (player.isLocalPlayer)
                {
                   
                    if (player.isServer)  // if host ??? documentation: if object is active on an active server
                    {
                        //player.RemovePlayer();
                        //Debug.Log("removing player");
                      
                        // stop client and server
                        matchMaker.DestroyMatch(matchInfo.networkId, 0, OnMatchDestroy);
                        singleton.StopHost();

                        Debug.Log("Stopping host");
                    }

                    else
                    {
                        singleton.StopClient();
                        Debug.Log("Stopping client");

                        //player.RemovePlayer();
                    }
                }

            }

        }

    }

}