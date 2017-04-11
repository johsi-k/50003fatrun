using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTransferObject : MonoBehaviour {
	// loads the gameplay
	protected static GameTransferObject instance;
	public TileSet t;
	// Use this for initialization
	void Awake() {
		if (instance == null)
			instance = this;
		DontDestroyOnLoad (instance);
	}
}
