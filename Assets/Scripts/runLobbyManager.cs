using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
//using UnityEngine.WSA;


public class runLobbyManager : NetworkLobbyManager {
    public Text targetText;
    public GameObject mainCamera;
    private Button soundOn;
    public Text mytext = null;
    int counter = 1;

    // tracks if lobby players are ready on the client and saves local player state
    private List<runLobbyPlayer> lobbyPlayers = new List<runLobbyPlayer>();

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
    }



    public void onCreateClicked() // bound to button which creates a match for lobby to be visible; initiated by host
    {
        this.StartMatchMaker();
        matchMaker.CreateMatch("gameName", 4, true, "matchPassword", "", "", 0, 0, OnMatchCreate); // dummy parameters
       
    }

    public void onGoClicked()  // bound to button which enables player to join a listed match
    {
        this.StartMatchMaker();
        print("attempting to join a listed match");
        matchMaker.ListMatches(0, 5, "", false, 0, 0, OnMatchList);
    }


    // callback that happens when NetworkMatch.ListMatches request has been processed on server
    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        base.OnMatchList(success, extendedInfo, matchList);
        if (success)
        {
            if (matchList.Count != 0)
            {
                foreach (MatchInfoSnapshot snapshot in matchList)
                {
                    if (snapshot.currentSize < snapshot.maxSize)
                    {
                        Debug.Log("requested match/list of matches was returned, attempting to join");
                        matchMaker.JoinMatch(snapshot.networkId, "matchPassword", "", "", 0, 0, OnMatchJoined);
                        break;
                    }
                }
            }
        }
    }


    // callback that happens when NetworkMatch.CreateMatch request has been processed on the server
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        Debug.Log("OnMatchCreate");

        if (success) {
            state = NetworkState.InLobby;
            mainCamera.GetComponent<Menu>().RandomMatchWaitingRoom();  // throws UI from loading screen to waitingRoomCanvas
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
        mytext.text = Network.player.ipAddress+"\n client connected "+counter;
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
    }


    // updates list of lobby players 
    // removing objects which have become null (player disconnects)
    private void UpdateLobbyPlayers()
    {
        if (lobbyPlayers.Count != numPlayers)
        {
            lobbyPlayers = lobbyPlayers.Where(p => p!=null).ToList();
        }
    }


    public override void OnLobbyServerPlayersReady()
    {
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
        base.OnServerAddPlayer(conn, playerControllerId);
        if (SceneManager.GetActiveScene().name == "Game")
        {
            Vector3 spawn = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
            GameObject gamePlayerInstance = Instantiate(gamePlayerPrefab, spawn, Quaternion.identity);
            // gamePlayer customisation goes here
            NetworkServer.DestroyPlayersForConnection(conn);
            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance, 0);
        }
    }
}