using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Object that throws junk food
public class JunkFoodThrower : NetworkBehaviour {

	[SerializeField]
	private GameObject[] junkFood;
	private float unifChance = 0.005f;

	void Start () {
		// sets the location of the fruit thrower to be 200 meters on the x-axis and 80 meters on the y-axis away from the character
		transform.localPosition = new Vector2 (200, 80);
	}

	void Update() {
		ThrowJunkFood ();
	}
		
	// Throws junk food with a uniform chance. Chance increases at every food throw
	void ThrowJunkFood() {
		if (Random.Range(0f, 1.0f) < unifChance) {
			GameObject jf = junkFood [Random.Range (0, junkFood.Length)];
			NetworkServer.Spawn (Instantiate (jf, transform.position, Quaternion.identity));
			unifChance *= 1.01f;
		}
	}

	// Gets destroyed when it reaches the endpoint
	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag("Endpoint")) {
			Destroy(gameObject);
		}
	}
}
