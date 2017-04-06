using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
//using UnityEngine.WSA;

public class runLobbyManager : NetworkLobbyManager {
    public Text targetText;
    private Button soundOn;
    public Text mytext = null;
    int counter = 1;

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

    public void onCreateClicked() // bound to button which creates a match for lobby to be visible
    {
        this.StartMatchMaker();
        matchMaker.CreateMatch("gameName", 4, true, "matchPassword", "", "", 0, 0, OnMatchCreate); // dummy parameters
       
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        Debug.Log("OnMatchCreate");

        if (success) {
            state = NetworkState.InLobby;
        }

        else
        {
            state = NetworkState.Inactive;
        }

        // Fire callback
        if (m_NextMatchCreatedCallback != null)
        {
            m_NextMatchCreatedCallback(success, matchInfo);
            m_NextMatchCreatedCallback = null;
        }

        // Fire event
        if (matchCreated != null)
        {
            matchCreated(success, matchInfo);
        }

    }


    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        Debug.Log("OnMatchJoined");

        if (success)
        {
            state = NetworkState.InLobby;
        }
        else
        {
            state = NetworkState.Pregame;
        }

        // Fire callback
        if (m_NextMatchJoinedCallback != null)
        {
            m_NextMatchJoinedCallback(success, matchInfo);
            m_NextMatchJoinedCallback = null;
        }

        // Fire event
        if (matchJoined != null)
        {
            matchJoined(success, matchInfo); 
        }

    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        base.OnLobbyClientConnect(conn);
        //Toast.Create(null, "Connected");
        mytext.text = Network.player.ipAddress+"\n client connected "+counter;
        counter++;
        
    }


}

