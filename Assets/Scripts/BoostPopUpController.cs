using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Singleton that manages the Boost Pop Up UI
public class BoostPopUpController : MonoBehaviour {

	private static GameObject canvas;
	private static Object boostPopUp;
	private static Object boostPopUp2;

	// Initialize the Singleton
	public static void Initialize() {
		if (!canvas) {
			canvas = GameObject.Find ("Canvas");
			boostPopUp = Resources.Load ("Prefabs/Others/BoostPopUp");
			boostPopUp2 = Resources.Load ("Prefabs/Others/BoostPopUp2");
		}
	}

	// Create the Boost Pop Up
	public static void createBoostPopUp(string text) {
		GameObject instance = Instantiate (boostPopUp) as GameObject;
		AnimatorClipInfo[] info = instance.GetComponent<Animator> ().GetCurrentAnimatorClipInfo (0);
		Destroy (instance, info [0].clip.length); // destroys instance when animation is complete
		instance.transform.SetParent (canvas.transform, false);
		instance.GetComponent<Text> ().text = text;
	}

	// Create the Boost Pop Up
	public static void createBoostPopUp2(string text) {
		GameObject instance = Instantiate (boostPopUp2) as GameObject;
		AnimatorClipInfo[] info = instance.GetComponent<Animator> ().GetCurrentAnimatorClipInfo (0);
		Destroy (instance, info [0].clip.length); // destroys instance when animation is complete
		instance.transform.SetParent (canvas.transform, false);
		instance.GetComponent<Text> ().text = text;
	}
}
