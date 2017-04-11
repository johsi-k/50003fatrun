using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitThrower : MonoBehaviour {

	[SerializeField]
	private GameObject[] fruits;
	private float x = 2.0f;
	private float lambda = 1.0f;
	private Rigidbody2D rb;
	// Use this for initialization
	void Awake () {
		rb = GetComponent<Rigidbody2D> ();
	}

	void Start () {
		throwFruits ();
		rb.velocity = new Vector2 (100, 0);
	}

	bool inverseExponential(float lambda, float x) {
		return Random.Range (0f, 1.0f) < Mathf.Exp (-lambda * x);
	}

	void Update() {
		throwFruits ();
	}

	void throwFruits() {
		// generator function
		if (inverseExponential (lambda, x)) {
//			Debug.Log ("Fruit thrown at " + transform.position);
			GameObject fruit = fruits [Random.Range (0, fruits.Length)];
			Instantiate (fruit, transform.position, Quaternion.identity);
			x += 0.01f;
		}
//		Vector2 newPosition = (Vector2)transform.position + new Vector2 (1, 0); 
//		transform.position = newPosition;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag("Endpoint")) {
			Destroy(gameObject);
			Debug.Log("Destroyed");
		}
	}
}
