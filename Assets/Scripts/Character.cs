using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class Character : NetworkBehaviour {


	private float health = 1f;
	private float healthDropRate = 0.01f;
	private Image healthContent;
	private Text healthText;

	// amount of fats: from 0-1
	private float fatLevel = 1f;
	// how much fats to drop per sec
	private float fatDropRate = 0.1f;
	private Image fatContent;
	private Text fatText;

	// whether fruit is eaten
	private bool isBoosted = false;

	// whether these attributes are altered by fruits
	private bool isMoveSpeedControlled = false;
	private bool isJumpForceControlled = false;
	private bool isAnimSpeedControlled = false;
	private bool isScaleControlled = false;

	// Locks for control
	private UnityEngine.Object isBoostedLock = new UnityEngine.Object ();
	private UnityEngine.Object isMoveSpeedControlledLock = new UnityEngine.Object ();
	private UnityEngine.Object isJumpForceControlledLock = new UnityEngine.Object ();
	private UnityEngine.Object isAnimSpeedControlledLock = new UnityEngine.Object ();
	private UnityEngine.Object isScaleControlledLock = new UnityEngine.Object ();

	private Rigidbody2D rb;
	private Collider2D collider;
	private Animator anim;
	private UnityEngine.Object boostPopUp;
	private Canvas canvas;

	public float moveSpeed;
	public float jumpForce;

	private static int runStateHash = Animator.StringToHash ("CatRun");
	private static int jumpStateHash = Animator.StringToHash ("CatJump");
	private static int fallStateHash = Animator.StringToHash ("CatFall");

	// the current animation state
	private int currentState;

	[SerializeField]
	private bool grounded = true;
	[SerializeField]
	private bool sideGrounded = true;
	private int notGroundedCount = 0;
	private Vector2 lastPosition;
	private RaycastHit2D[] rchresults;
	[SerializeField]
	private LayerMask ground;


	void Awake () {
		rb = GetComponent<Rigidbody2D> ();
		collider = GetComponent<Collider2D> ();
		anim = GetComponent<Animator> ();
		currentState = runStateHash;
//		currentSize = sizes [0];
		rchresults = new RaycastHit2D[3];
		BoostPopUpController.Initialize ();
		PlusPopUpController.Initialize ();
	}

	// Use this for initialization
	void Start () {

        DontDestroyOnLoad(gameObject);

		if (!isLocalPlayer) {
			return;
		}

		// links the health and fat text on the canvas to the character
		healthContent = GameObject.Find ("/Canvas/HealthBar/Health").GetComponent<Image> ();
		healthText = GameObject.Find ("/Canvas/HealthBar/HealthText").GetComponent<Text> ();
		fatContent = GameObject.Find ("/Canvas/FatBar/Fats").GetComponent<Image> ();
		fatText = GameObject.Find ("/Canvas/FatBar/FatsText").GetComponent<Text> ();

		// enables the camera to track the player
		Camera.main.GetComponent<CameraTracker>().setTarget(gameObject.transform);
		GameObject.Find ("Background").GetComponent<CameraTracker> ().setTarget (gameObject.transform);

		// prevents character from experiencing rotation due to gravity
		rb.freezeRotation = true;

		// used to nudge the character a little if it gets stuck for unknown reasons
		lastPosition = (Vector2) transform.position - Vector2.left;
	}

	// Reduces the fat level based on the fat drop rate
	bool UpdateFatLevel() {
		// returns true if fat level > 0
		if (fatLevel > 0) {
			fatLevel -= Time.deltaTime * fatDropRate;
			return true;
		} else {
			return false;
		}
	}

	// Reduces the health based on the health drop rate
	bool UpdateHealth() {
		// returns true if health > 0
		if (health > 0) {
			health -= Time.deltaTime * healthDropRate;
			return true;
		}
		return false;
	}

//	int GetSizeIndexFromLevel() {
//		// returns the Size for a given fatLevel, using linear interpolation
//		int index = (int) Mathf.Lerp(sizes.Length, 0, fatLevel);
//		// -1 if fatLevel is min, cos index == sizes.Length does not exist
//		if (index == sizes.Length)
//			index -= 1;
//		return index;
//	}

//	void UpdateStatsWithSize() {
//		// Uses currentSize to update attributes
//		moveSpeed = currentSize.moveSpeed;
//		jumpForce = currentSize.jumpForce;
//		transform.localScale = new Vector3 (currentSize.spriteScale, transform.localScale.y, transform.localScale.z);
//		// push character towards ground since size has transformed, to avoid state change
//		int currentState = anim.GetCurrentAnimatorStateInfo (0).shortNameHash;
//		if (currentState == runStateHash) {
//			
//		}
//
//		anim.speed = currentSize.animationMoveSpeed;
//	}
//		

	// if character is stuck, nudge it a little
	void NudgeCharacter() {
		Vector2 currentPosition = transform.position;

		if (currentPosition.Equals (lastPosition)) {
			transform.position = currentPosition + new Vector2 (2, 0);;
		}

		lastPosition = currentPosition;
	}

	void FixedUpdate() {
		if (!isLocalPlayer) {
			return;
		}
		NudgeCharacter ();
	}

	// Update is called once per frame
	void Update () {
		// Reduce fats
		if (!isLocalPlayer) {
			return;
		}
		bool fatLevelChanged = UpdateFatLevel ();
		fatContent.fillAmount = fatLevel;
		fatText.text = (int)(100 * fatLevel) + "%";

		// Reduce health
		bool healthLevelChanged = UpdateHealth ();
		healthContent.fillAmount = health;
		healthText.text = (int)(100 * health) + "%";

//		int sizeIndex = GetSizeIndexFromLevel ();
//		Size newSize = sizes [sizeIndex];
//		bool hasChangedSize = newSize != currentSize;
		// substitute stats if newSize != currentSize
//		if (hasChangedSize) {
//			currentSize = newSize;
//			UpdateStatsWithSize ();
//		}

		if (fatLevelChanged) {
			// lock so that if food is eaten, moveSpeed will not be changed by this
			lock (isMoveSpeedControlledLock) {
				if (!isMoveSpeedControlled) // if ms not controlled by food
				moveSpeed = Mathf.Lerp (30f, 5f, fatLevel);
			}
			// same logic
			lock (isJumpForceControlledLock) {
				if (!isJumpForceControlled) // if jf not controlled by food
				jumpForce = Mathf.Lerp (50f, 15f, fatLevel);
			}
			lock (isAnimSpeedControlledLock) {
				if (!isAnimSpeedControlled) // if as not controlled by food
				anim.speed = Mathf.Lerp (2.0f, 0.5f, fatLevel);
			}
			lock (isScaleControlledLock) {
				if (!isScaleControlled) {// if as not controlled by food
					float scale = Mathf.Lerp (1.0f, 3f, fatLevel);
					transform.localScale = new Vector3 (scale, transform.localScale.y, transform.localScale.z);
				}
			}
			rb.AddForce (new Vector2 (0, -2), ForceMode2D.Impulse);
		}


//		Debug.DrawLine(transform.position, new Vector2(0, collider.bounds.extents.y), Color.green);
		grounded = collider.Raycast(Vector2.down, rchresults, (float) (collider.bounds.extents.y + 0.3)) > 0;
		sideGrounded = collider.Raycast (Vector2.right + Vector2.down, rchresults, (float)(collider.bounds.extents.y + 1.0)) > 0;
		grounded = grounded || sideGrounded;

		float ms = moveSpeed;


		if (!grounded) {
			notGroundedCount++;
			// ms = ms;
		} else {
			notGroundedCount = 0;
		}
		// movement speed
		rb.velocity = new Vector2 (1f, rb.velocity.y);

		/////////////////
		// Player Control
		/////////////////

		// state machine for animation
		int currentState = anim.GetCurrentAnimatorStateInfo (0).shortNameHash;
		if (currentState == runStateHash) {
			if (Input.GetKeyDown (KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began)) {
				Jump (ms, jumpForce);
				CmdJump (ms, jumpForce);
			}
			// To avoid state change when size changes (Hack)
			// if (!hasChangedSize)
			else if (notGroundedCount >= 10) {
				ChangeJump (true);
				CmdChangeJump (true);
			}
		} else if (currentState == jumpStateHash) {
			ChangeJump (!grounded);
			CmdChangeJump (!grounded);
			ChangeFall (rb.velocity.y <= 0);
			CmdChangeFall (rb.velocity.y <= 0);
		} else if (currentState == fallStateHash) {
			ChangeJump (!grounded);
			CmdChangeJump (!grounded);
		}
	}


	// CHANGE TO FALL ANIMATION
	void ChangeFall(bool isFall) {
		anim.SetBool ("isFall", isFall);
	}

	[Command]
	void CmdChangeFall(bool isFall) {
		RpcChangeFall (isFall);
	}

	[ClientRpc]
	void RpcChangeFall(bool isFall) {
		if (isLocalPlayer)
			return;
		ChangeFall (isFall);
	}

	// CHANGE TO JUMP ANIMATION
	void ChangeJump(bool isJump) {
		anim.SetBool ("isJump", isJump);
	}

	[Command]
	void CmdChangeJump(bool isJump) {
		RpcChangeJump(isJump);
	}

	[ClientRpc]
	void RpcChangeJump(bool isJump) {
		if (isLocalPlayer)
			return;
		ChangeJump(isJump);
	}


	// JUMP AND CHANGE TO JUMP ANIMATION
	void Jump(double ms, double jumpForce) {
		rb.velocity = new Vector2 (ms, jumpForce);
		ChangeJump (true);
	}

	[Command]
	void CmdJump(double ms, double jumpForce) {
		RpcJump(ms, jumpForce);
	}

	[ClientRpc]
	void RpcJump(double ms, double jumpForce) {
		if (isLocalPlayer)
			return;
		Jump (ms, jumpForce);
	}
		

	void AddFats(float fats) {
		// need synchronity
		fatLevel += fats;
		if (fatLevel > 1.0f) {
			fatLevel = 1.0f;
		}
	}

	void AddHealth(float hp) {
		// need synchronity
		health += hp;
		if (health > 1.0f) {
			health = 1.0f;
		}
	}

	IEnumerator SpeedUpCoroutine(float seconds) {
		float prevAnimSpeed;
		float prevMoveSpeed;
		lock(isAnimSpeedControlledLock) {
			isAnimSpeedControlled = true;
			prevAnimSpeed = anim.speed;
			anim.speed = 4.0f;
		}
		lock(isMoveSpeedControlledLock) {
			isMoveSpeedControlled = true;
			prevMoveSpeed = moveSpeed;
			moveSpeed = 50.0f;
		}
		yield return new WaitForSecondsRealtime (seconds);
		lock (isAnimSpeedControlledLock) {
			isAnimSpeedControlled = false;
			anim.speed = prevAnimSpeed;
		}
		lock (isMoveSpeedControlledLock) {
			isMoveSpeedControlled = false;
			moveSpeed = prevMoveSpeed;
		}
		lock (isBoostedLock) {
			Debug.Log ("Lock gone " + System.DateTime.Now);
			isBoosted = false;
		}
	}


	IEnumerator JumpForceCoroutine(float seconds) {
		float prevJumpForce;
		lock(isJumpForceControlledLock) {
			isJumpForceControlled = true;
			prevJumpForce = jumpForce;
			jumpForce = 100.0f;
		}
		yield return new WaitForSecondsRealtime (seconds);
		lock (isJumpForceControlledLock) {
			isJumpForceControlled = false;
			jumpForce = prevJumpForce;
		}
		lock (isBoostedLock) {
			Debug.Log ("Lock gone " + System.DateTime.Now);
			isBoosted = false;
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		// ignore collision from other players
		if (other.gameObject.CompareTag("Player")) {
			Physics2D.IgnoreCollision (other.gameObject.GetComponent<Collider2D>(), collider);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		// fruits
		if (other.tag.Contains ("Fruit")) {
			// add health and fats
			AddHealth(0.2f);
			AddFats (0.05f);
			PlusPopUpController.createHealthPlusPopUp ("+20% hp");
			PlusPopUpController.createFatsPlusPopUp ("+5% fats");
			Destroy (other.gameObject);

			if (other.tag.Contains("Cherry")) {
				// gain lock on state
				lock (isBoostedLock) {
					if (!isBoosted) {
						isBoosted = true;
						BoostPopUpController.createBoostPopUp("FOOD BURST!");
						Debug.Log ("Speed up! " + System.DateTime.Now );
						StartCoroutine(SpeedUpCoroutine(5.0f));
					}
				}
			}
			if (other.tag.Contains("Apple")) {
				// gain lock on state
				lock (isBoostedLock) {
					if (!isBoosted) {
						isBoosted = true;
						BoostPopUpController.createBoostPopUp("SUPER JUMP!");
						Debug.Log ("Jump!" + System.DateTime.Now);
						StartCoroutine(JumpForceCoroutine(5.0f));
					}
				}
			}
		}
	}
}
