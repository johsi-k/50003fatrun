using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine;
using UnityEngine.UI;

public class Networking : MonoBehaviour
{
    public GameObject manager;
    public Text yourIP;
    public InputField inputIP;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //yourIP.text = "Your IP:"+getIP();
        //yourIP.text = "Your IP:" + Network.player.ipAddress;
    }

    public void clickStartHost()
    {
        NetworkManager mana = manager.GetComponent<NetworkManager>();
        mana.networkPort = 7777;
        mana.StartHost();
    }

    public void clickStartClient()
    {
        NetworkManager mana = manager.GetComponent<NetworkManager>();  // 
        mana.networkAddress = inputIP.text;
        mana.networkPort = 7777;
        mana.StartClient();
    }


}