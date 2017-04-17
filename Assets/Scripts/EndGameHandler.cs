using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameHandler : MonoBehaviour {
    public GameObject endGamePanel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void showEndPanel()
    {
        endGamePanel.SetActive(true);
    }

    public void clickEndGame() // bound to OK button
    {
        Debug.Log("disconnect and return to main");
       ( (runLobbyManager.singleton) as runLobbyManager).StopClient();
    }

}
