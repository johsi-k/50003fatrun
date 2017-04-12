using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour {

    public RawImage[] dots = new RawImage[3];
    private int countdown = 100;
   
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //crossFadeMethod();
        trailingDotMethod();

	}

    void crossFadeMethod()
    {
        while (countdown > 0)
        {
            foreach (RawImage dot in dots)
            {
                dot.CrossFadeAlpha(0, 1, true);
            }
            --countdown;
        }

    }

    void trailingDotMethod()
    {
        if(countdown>75)
        {
            dots[0].gameObject.SetActive(false);
            dots[1].gameObject.SetActive(false);
            dots[2].gameObject.SetActive(false);
        } else if (countdown > 50)
        {
            dots[0].gameObject.SetActive(true);
            dots[1].gameObject.SetActive(false);
            dots[2].gameObject.SetActive(false);

        } else if (countdown > 25)
        {
            dots[0].gameObject.SetActive(true);
            dots[1].gameObject.SetActive(true);
            dots[2].gameObject.SetActive(false);
        } else
        {
            dots[0].gameObject.SetActive(true);
            dots[1].gameObject.SetActive(true);
            dots[2].gameObject.SetActive(true);
        }

        countdown--;

        if(countdown<=0)
        {
            countdown = 100;
        }
    }
}
