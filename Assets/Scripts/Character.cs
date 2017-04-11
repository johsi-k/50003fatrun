using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class Character : NetworkBehaviour {

	public float health = 1f;
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

 //	[System.Serializable]
//	private class Size {
//		public float moveSpeed;
//		public float spriteScale;
//		public float jumpForce;
//		public float animationMoveSpeed;
//
//		public Size(float moveSpeed, float jumpForce, float spriteScale, float animMS) {
//			this.moveSpeed = moveSpeed;
//			this.jumpForce = jumpForce;
//			this.spriteScale = spriteScale;
//			this.animationMoveSpeed = animMS;
//		}
//	}
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

	private int currentState;

//	private Size[] sizes = {
//		new Size (5f, 15f, 2.5f, 0.5f),
//		new Size (8f, 18f, 2.35f, 0.6f),
//		new Size (11f, 21f, 2.2f, 0.7f),
//		new Size (14f, 24f, 2.05f, 0.8f),
//		new Size (17f, 27f, 1.9f, 0.9f),
//		new Size (20f, 30f, 1.75f, 1f),
//		new Size (23f, 33f, 1.6f, 1.1f),
//		new Size (26f, 36f, 1.45f, 1.2f),
//		new Size (29f, 39f, 1.3f, 1.3f),
//		new Size (32f, 42f, 1.15f, 1.4f),
//		new Size (35f, 45f, 1f, 1.5f)
//	};
//
//	private Size currentSize;

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

		healthContent = GameObject.Find ("/Canvas/HealthBar/Health").GetComponent<Image> ();
		healthText = GameObject.Find ("/Canvas/HealthBar/HealthText").GetComponent<Text> ();
		fatContent = GameObject.Find ("/Canvas/FatBar/Fats").GetComponent<Image> ();
		fatText = GameObject.Find ("/Canvas/FatBar/FatsText").GetComponent<Text> ();
		Camera.main.GetComponent<CameraTracker>().setTarget(gameObject.transform);
		GameObject.Find ("Background").GetComponent<CameraTracker> ().setTarget (gameObject.transform);
		rb.freezeRotation = true;
		lastPosition = (Vector2) transform.position - Vector2.left; // initialize
//		UpdateStatsWithSize ();
	}

	bool UpdateFatLevel() {
		// returns true if fat level > 0
		if (fatLevel > 0) {
			fatLevel -= Time.deltaTime * fatDropRate;
			return true;
		} else {
			return false;
		}
	}

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

	void FixedUpdate() {
		Vector2 currentPosition = transform.position;

		// if character is stuck, nudge it a little
		if (currentPosition.Equals (lastPosition)) {
			transform.position = currentPosition + new Vector2 (2, 0);;
		}

		lastPosition = currentPosition;
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
//			ms = ms;
		} else {
			notGroundedCount = 0;
		}
		// movement speed
		rb.velocity = new Vector2 (50f, rb.velocity.y);



		// state machine for animation
		int currentState = anim.GetCurrentAnimatorStateInfo (0).shortNameHash;
		if (currentState == runStateHash) {
			if (Input.GetKeyDown (KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began)) {
				rb.velocity = new Vector2 (ms, jumpForce);
				anim.SetBool ("isJump", true);
			}
			// To avoid state change when size changes (Hack)
//			if (!hasChangedSize)
			else if (notGroundedCount >= 10) {
				anim.SetBool ("isJump", true);
			}
		} else if (currentState == jumpStateHash) {
			anim.SetBool ("isJump", !grounded);
			anim.SetBool ("isFall", rb.velocity.y <= 0);
		} else if (currentState == fallStateHash) {
			anim.SetBool ("isJump", !grounded);
		}
	}

	void addFats(float fats) {
		// need synchronity
		fatLevel += fats;
		if (fatLevel > 1.0f) {
			fatLevel = 1.0f;
		}
	}

	void addHealth(float hp) {
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
			addHealth(0.2f);
			addFats (0.05f);
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
