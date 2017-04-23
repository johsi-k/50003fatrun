using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// For character changing in lobby
public class CharacterChange : MonoBehaviour {
    public RawImage rawimage;
    public Texture[] rawimages;
    private int counter = 0;
	
	// Update is called once per frame
	void Update () {
        rawimage.texture = rawimages[counter];
	}
    public void toggle_right()
    {
        counter = (counter + 1) % rawimages.Length;
    }
    public void toggle_left()
    {
        if (counter == 0)
        {
            counter = rawimages.Length - 1;
        }
        else {
            counter = (counter - 1);
        }
    }
}
