using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// The object that throws fruit
public class FruitThrower : NetworkBehaviour {

	[SerializeField]
	private GameObject[] fruits;
	private float unifChance = 0.005f;

	void Start () {
		// sets the location of the fruit thrower to be 200 meters on the x-axis and 80 meters on the y-axis away from the character
		transform.localPosition = new Vector2 (200, 80);
	}

	void Update() {
		ThrowFruits ();
	}


	// Throws food with a uniform chance
	void ThrowFruits() {
		if (Random.Range(0f, 1.0f) < unifChance) {
			GameObject fruit = fruits [Random.Range (0, fruits.Length)];
			NetworkServer.Spawn (Instantiate (fruit, transform.position, Quaternion.identity));
		}
	}

	// Gets destroyed when it reaches the endpoint
	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag("Endpoint")) {
			Destroy(gameObject);
		}
	}
}
