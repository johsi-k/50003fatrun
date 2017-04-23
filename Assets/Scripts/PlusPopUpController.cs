using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controller for the add health and add fats pop up
public class PlusPopUpController : MonoBehaviour {

	public class RGB {
		public float r;
		public float g;
		public float b;

		public RGB(float r, float g, float b) {
			this.r = r;
			this.g = g;
			this.b = b;
		}
	}
	private static GameObject canvas;
	private static PlusPopUpParent plusPopUp;
	public static RGB fatsColor = new RGB (0f, 0f, 1f);
	public static RGB healthColor = new RGB (0f, 0.95f, 0.97f);

	public static void Initialize() {
		if (!canvas) {
			canvas = GameObject.Find ("PopupCanvas");
			plusPopUp = Resources.Load<PlusPopUpParent> ("Prefabs/Others/PlusPopUpParent");
		}
	}

	public static void createHealthPlusPopUp(string text) {
		createPlusPopUp (text, healthColor, new Vector2(0, 5));
	}

	public static void createFatsPlusPopUp(string text) {
		createPlusPopUp (text, fatsColor, Vector2.zero);
	}

	public static void createPlusPopUp(string text, RGB color, Vector2 offset) {
		PlusPopUpParent instance = Instantiate (plusPopUp);
		instance.transform.SetParent (canvas.transform, false);
		instance.transform.position = (Vector2)instance.transform.position + offset;
		instance.SetColor (new Color (color.r, color.g, color.b));
		instance.SetText (text);


	}
}
