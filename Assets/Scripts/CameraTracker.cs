using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to track the character
public class CameraTracker : MonoBehaviour {

	private Transform playerTransform;

	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position - new Vector3(0, 10, 0);
	}

	// Update is called once per frame
	void LateUpdate () {
		if (playerTransform != null) {
			transform.position = playerTransform.position + offset;
		}
	}

	public void setTarget(Transform target) {
		playerTransform = target;
	}
}
