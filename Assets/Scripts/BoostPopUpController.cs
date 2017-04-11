using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostPopUpController : MonoBehaviour {

	private static GameObject canvas;
	private static Object boostPopUp;

	public static void Initialize() {
		if (!canvas) {
			canvas = GameObject.Find ("Canvas");
			boostPopUp = Resources.Load ("Prefabs/Others/BoostPopUp");
			Debug.Log (boostPopUp);
		}
	}

	public static void createBoostPopUp(string text) {
		GameObject instance = Instantiate (boostPopUp) as GameObject;
		AnimatorClipInfo[] info = instance.GetComponent<Animator> ().GetCurrentAnimatorClipInfo (0);
		Destroy (instance, info [0].clip.length); // destroys instance when animation is complete
		instance.transform.SetParent (canvas.transform, false);
		instance.GetComponent<Text> ().text = text;
	}
}
