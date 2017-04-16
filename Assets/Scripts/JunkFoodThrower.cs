using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class JunkFoodThrower : NetworkBehaviour {

	[SerializeField]
	private GameObject[] junkFood;
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
		ThrowJunkFood ();
	}
		
	void ThrowJunkFood() {
		// generator function
		if (Random.Range(0f, 1.0f) < unifChance) {
			GameObject jf = junkFood [Random.Range (0, junkFood.Length)];
			NetworkServer.Spawn (Instantiate (jf, transform.position, Quaternion.identity));
			unifChance *= 1.01f;
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
