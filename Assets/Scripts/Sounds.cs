using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sounds : MonoBehaviour
{
    public Text targetText;
    private Button soundOn;
    public Text mytext = null;
    public int counter = 1;
    public bool muteToggle = false;
    public void changeText()
    {
        if (counter % 2 == 1)
        {
            mytext.text = "SOUND ON";
            AudioListener.volume = 1;
        }
        else
        {
            mytext.text = "SOUND OFF";
            AudioListener.volume = 0;
        }
        counter++;
    }

}