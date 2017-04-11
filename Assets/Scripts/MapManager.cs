using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

	private TileSet ts;
	private Transform map;

	public void initMap(TileSet ts) {
		this.ts = ts;
//		setUpPlatform ();
	}

	public void throwFruits() {
	}


	private void setUpPlatform() {
		map = new GameObject ("Map").transform;
		Vector2 currentPoint = new Vector2 (-10, 0);
		GameObject instance = Instantiate (ts.repeatingPlatform, currentPoint, Quaternion.identity) as GameObject;
		float size = (float) instance.GetComponent<BoxCollider2D> ().bounds.size.x;

//		float size = 5;
		Debug.Log (size);
		Vector2 increment = new Vector2 (size, 0);
		for (int i = 0; i < 10000; i++) {
			currentPoint += increment;
			instance = Instantiate (ts.repeatingPlatform, currentPoint, Quaternion.identity) as GameObject;
			instance.transform.SetParent (map);

		}
	}
	
}
