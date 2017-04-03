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

    //public RawImage player1img,player2img,player3img,player4img;
    //public Texture playeron1;

    private void Update()
    {

    }

    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        base.OnLobbyClientConnect(conn);
        //Toast.Create(null, "Connected");
        mytext.text = Network.player.ipAddress+"\n client connected "+counter;
        counter++;
        
    }


}

