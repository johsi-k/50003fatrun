using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsWinPopUpController : MonoBehaviour {

	private static GameObject canvas;
	private static Object isWinPopUp;
	private static Object isLostPopUp;

	public static void Initialize() {
		if (!canvas) {
			canvas = GameObject.Find ("PopupCanvas");
			isWinPopUp = Resources.Load ("Prefabs/Others/IsWinPopUp");
			isLostPopUp = Resources.Load ("Prefabs/Others/IsLostPopUp");
		}
	}

	public static void createIsWinPopUp() {
		GameObject instance = Instantiate (isWinPopUp) as GameObject;
		instance.transform.SetParent (canvas.transform, false);
	}

	public static void createIsLostPopUp() {
		GameObject instance = Instantiate (isLostPopUp) as GameObject;
		instance.transform.SetParent (canvas.transform, false);
	}
}
