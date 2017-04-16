using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FruitThrower : NetworkBehaviour {

	[SerializeField]
	private GameObject[] fruits;
	private float x = 2.0f;
	private float lambda = 1.0f;
	private float unifChance = 0.005f;
	// Use this for initialization
	void Awake () {
	}

	void Start () {
		transform.localPosition = new Vector2 (200, 80);
	}

	bool inverseExponential(float lambda, float x) {
		return Random.Range (0f, 1.0f) < Mathf.Exp (-lambda * x);
	}

	void Update() {
		ThrowFruits ();
	}


	void ThrowFruits() {
		// generator function
		if (Random.Range(0f, 1.0f) < unifChance) {
//			Debug.Log ("Fruit thrown at " + transform.position);
			GameObject fruit = fruits [Random.Range (0, fruits.Length)];
			NetworkServer.Spawn (Instantiate (fruit, transform.position, Quaternion.identity));
//			unifChance *= 0.99f;
		}
//		Vector2 newPosition = (Vector2)transform.position + new Vector2 (1, 0); 
//		transform.position = newPosition;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag("Endpoint")) {
			Destroy(gameObject);
		}
	}
}
