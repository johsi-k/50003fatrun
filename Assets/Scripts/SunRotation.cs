using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour {

	public float speed = 100;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3(0, 0, -1), speed * Time.deltaTime);
	}
}
