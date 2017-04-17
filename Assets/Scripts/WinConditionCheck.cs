using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WinConditionCheck : NetworkBehaviour {
	private bool isWin = false;
	private Object winLock = new Object();

	public bool CheckIsWin() {
		lock (winLock) {
			if (!isWin) {
				isWin = true;
                // win
				return true;
			} 
            // lost
			return false;
		}
	}
}
