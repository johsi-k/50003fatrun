using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	// create singleton GameManager
	public static GameManager instance = null;
	public GameObject tileSetObject;
	public GameObject character;
	public GameObject fruitThrower;
	private MapManager mapScript;

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
		Instantiate (fruitThrower, new Vector2 (0, 50), Quaternion.identity);
		TileSet ts = Instantiate(tileSetObject, Vector2.zero, Quaternion.identity).GetComponent<TileSet>();
		mapScript = GetComponent<MapManager> ();
		mapScript.initMap (ts);
	}
	// Use this for initialization
	void Start () {
//		Instantiate (character, new Vector2(0, 10), Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
