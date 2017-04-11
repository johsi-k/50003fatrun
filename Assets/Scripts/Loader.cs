using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	// loads the entire game
	public GameObject gameManager;
	// Use this for initialization
	void Awake() {
//		GameTransferObject gto = Instantiate (GameTransferObject);
//		gto.ts = Instantiate(//prefab, Vector2.zero, Quaternion.identity).GetComponent<TileSet>();
		if (GameManager.instance == null)
			Instantiate (gameManager);
	}
}
