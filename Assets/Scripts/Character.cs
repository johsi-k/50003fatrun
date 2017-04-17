using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class Character : NetworkBehaviour {


	private float health = 1f;
	private float healthDropRate = 0.025f;
	private Image healthContent;
	private Text healthText;
    private AudioSource audio;
    public AudioClip applecrunch;
    public AudioClip jumpSound;
    public AudioClip poopSound;
    public AudioClip shieldSound;
    public AudioClip deathSound;
    public AudioClip burpSound;
    
	// amount of fats: from 0-1
	[SyncVar]
	private float fatLevel = 1f;
	// how much fats to drop per sec
	private float fatDropRate = 0.1f;
	private Image fatContent;
	private Text fatText;

	// whether fruit is eaten
    [SyncVar]
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
    private UnityEngine.Object deathLock = new UnityEngine.Object();

    // server side death check
    [SyncVar]
    private bool isDead = false;
    [SyncVar]
    private bool isDyingAnimation = false;
    private float deathDuration = 1.5f;
    private Vector2 deathPosition;

    // if the game is done for the player
    [SyncVar]
    private bool isFinished = false;
    [SyncVar]
    // if character has shield, will not eat fast food
    private bool isShield = false;

    //Syncvars
    [SyncVar]
	private float moveSpeedSync;
	[SyncVar]
	private float jumpForceSync;
	[SyncVar]
	private float animSpeedSync;
	[SyncVar]
	private float scaleSync;

	private Rigidbody2D rb;
	private Collider2D collider;
	private Animator anim;
	private UnityEngine.Object boostPopUp;
	private Canvas canvas;
    private SpriteRenderer spriteRenderer;

	public float moveSpeed;
	public float jumpForce;
	private bool isJump;

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

    private UnityEngine.Object shitBullet;
    private UnityEngine.Object shield;
    private UnityEngine.Object tornado;


    void Awake () {
		rb = GetComponent<Rigidbody2D> ();
		collider = GetComponent<Collider2D> ();
		anim = GetComponent<Animator> ();
        spriteRenderer = GetComponent<SpriteRenderer>();
		currentState = runStateHash;
//		currentSize = sizes [0];
		rchresults = new RaycastHit2D[3];
        shitBullet = Resources.Load("Prefabs/Food/JunkFood/Dung");
        shield = Resources.Load("Prefabs/Others/Shield");
        tornado = Resources.Load("Prefabs/Others/Tornado");
        BoostPopUpController.Initialize ();
		PlusPopUpController.Initialize ();
		IsWinPopUpController.Initialize ();
        audio = GetComponent<AudioSource>();
    }

	// Use this for initialization
	void Start () {

        DontDestroyOnLoad(gameObject);

		// prevents character from experiencing rotation due to gravity
		rb.freezeRotation = true;

		// used to nudge the character a little if it gets stuck for unknown reasons
		lastPosition = (Vector2) transform.position - Vector2.left;

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

	}

	// Reduces the fat level based on the fat drop rate
	void ReduceFatLevel() {
		if (fatLevel > 0) {
			fatLevel -= Time.deltaTime * fatDropRate;
		} 
	}

	// returns true if fat level > 0
	bool HasFatLevelChanged() {
		if (fatLevel > 0) {
			return true;
		}
		return false;
	}

	// Reduces the health based on the health drop rate
	void ReduceHealth() {
		if (health > 0) {
			health -= Time.deltaTime * healthDropRate;
		}
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
	//void NudgeCharacter() {
	//	Vector2 currentPosition = transform.position;

	//	if (currentPosition.Equals (lastPosition)) {
	//		transform.position = currentPosition + new Vector2 (2, 0);;
	//	}

	//	lastPosition = currentPosition;
	//}

	//void FixedUpdate() {
	//	if (!isLocalPlayer) {
	//		return;
	//	}
	//	NudgeCharacter ();
	//}

	// Update is called once per frame
	void Update () {
//		int sizeIndex = GetSizeIndexFromLevel ();
//		Size newSize = sizes [sizeIndex];
//		bool hasChangedSize = newSize != currentSize;
		// substitute stats if newSize != currentSize
//		if (hasChangedSize) {
//			currentSize = newSize;
//			UpdateStatsWithSize ();
//		}
        if (isFinished)
        {
            moveSpeed = 0;
            CmdJump();
            return;
        }

		if (isServer) {
			ReduceFatLevel ();

			if (HasFatLevelChanged ()) {
				// lock so that if food is eaten, moveSpeed will not be changed by this
				lock (isMoveSpeedControlledLock) {
					if (!isMoveSpeedControlled) // if ms not controlled by food
				moveSpeedSync = Mathf.Lerp (35f, 5f, fatLevel);
				}
				// same logic
				lock (isJumpForceControlledLock) {
					if (!isJumpForceControlled) // if jf not controlled by food
				jumpForceSync = Mathf.Lerp (50f, 15f, fatLevel);
				}
				lock (isAnimSpeedControlledLock) {
					if (!isAnimSpeedControlled) // if as not controlled by food
				animSpeedSync = Mathf.Lerp (2.0f, 0.5f, fatLevel);
				}
				lock (isScaleControlledLock) {
					if (!isScaleControlled) {// if as not controlled by food
						scaleSync = Mathf.Lerp (1.0f, 3f, fatLevel);
					}
				}
				// rb.AddForce (new Vector2 (0, -2), ForceMode2D.Impulse);
			}
		}

	
		moveSpeed = moveSpeedSync;
		jumpForce = jumpForceSync;
		anim.speed = animSpeedSync;
		transform.localScale = new Vector3 (scaleSync, transform.localScale.y, transform.localScale.z);
        if (isDyingAnimation)
        {
            moveSpeed = 0;
            jumpForce = 0;
            anim.speed = 0;
            if (spriteRenderer.color.a > 0)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g,
                                                    spriteRenderer.color.b, spriteRenderer.color.a - (Time.deltaTime / deathDuration));
            }
        }
		float ms = moveSpeed;
		float msy;
		if (isJump) {
			msy = jumpForce;
			isJump = false;
		} else {
			msy = rb.velocity.y;
		}
		rb.velocity = new Vector2 (ms, msy);
			

		// ONLY THE REST ARE LOCAL
		if (isLocalPlayer) {
			ReduceHealth ();

            if (health <= 0)
            {
                // die
                if (!isDead)
                {
                    audio.PlayOneShot(deathSound, 1F);
                    Debug.Log("DIE");
                    CmdDie();
                }
                return;
            }

            // Change fat bar content and text
            fatContent.fillAmount = fatLevel;
			fatText.text = (int)(100 * fatLevel) + "%";

			// Change health bar content and text
			healthContent.fillAmount = health;
			healthText.text = (int)(100 * health) + "%";

			//	Debug.DrawLine(transform.position, new Vector2(0, collider.bounds.extents.y), Color.green);
			grounded = collider.Raycast (Vector2.down, rchresults, (float)(collider.bounds.extents.y + 0.3)) > 0;
			sideGrounded = collider.Raycast (Vector2.right + Vector2.down, rchresults, (float)(collider.bounds.extents.y + 1.0)) > 0;
			grounded = grounded || sideGrounded;

				
			if (!grounded) {
				notGroundedCount++;
			} else {
				notGroundedCount = 0;
			}


			/////////////////
			// Player Control
			/////////////////

			// state machine for animation
			int currentState = anim.GetCurrentAnimatorStateInfo (0).shortNameHash;
			if (currentState == runStateHash) {
				if (Input.GetKeyDown (KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began)) {
                    audio.PlayOneShot(jumpSound,1F);
                    CmdTornadoCoroutine(5.0f);
                    CmdJump();
					ChangeJump (true);
					CmdChangeJump (true);
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
		
	[Command]
	void CmdJump() {
		RpcJump ();
	}

	[ClientRpc]
	void RpcJump() {
		isJump = true;
	}
		
	[Command]
	void CmdAddFats(float fats) {
		// need synchronity
		fatLevel += fats;
		if (fatLevel > 1.0f) {
			fatLevel = 1.0f;
		} else if (fatLevel < 0f)
        {
            fatLevel = 0f;
        }
	}

	void AddHealth(float hp) {
		// need synchronity
		health += hp;
		if (health > 1.0f) {
			health = 1.0f;
		} else if (health < 0f) {
			health = 0f;
		}
	}

    [Command]
    void CmdDie()
    {
        lock (deathLock)
        {
            if (!isDead)
            {
                RpcDie();
                isDead = true;
                isDyingAnimation = true;
                StartCoroutine(RebornCoroutine());
            }
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        deathPosition = transform.position;
    }

    // Revive character if dead
    IEnumerator RebornCoroutine()
    {
        // wait until reincarnation
        yield return new WaitForSecondsRealtime (deathDuration);
        {
            // revive if death
            isDyingAnimation = false;
            isDead = false;
            RpcReborn();
        }
        yield break;

    }

    [ClientRpc]
    void RpcReborn()
    {
        // return character to position where it died
        transform.position = deathPosition;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1.0f);
        if (isLocalPlayer)
        {
            health = 1f;
        }
    }

    [Command]
	void CmdSpeedUpCoroutine(float seconds) {
        lock (isBoostedLock)
        {
            if (!isBoosted)
            {
                StartCoroutine(SpeedUpCoroutine(seconds));
                isBoosted = true;
                RpcCreateBoostPopUp("FOOD BURST!");
            }
        }
        
	}
		
	IEnumerator SpeedUpCoroutine(float seconds) {
		float prevAnimSpeed;
		float prevMoveSpeed;
		lock(isAnimSpeedControlledLock) {
			isAnimSpeedControlled = true;
			prevAnimSpeed = animSpeedSync;
			animSpeedSync = 4.0f;
		}
		lock(isMoveSpeedControlledLock) {
			isMoveSpeedControlled = true;
			prevMoveSpeed = moveSpeedSync;
			moveSpeedSync = 60.0f;
		}
		yield return new WaitForSecondsRealtime (seconds);
		lock (isAnimSpeedControlledLock) {
			isAnimSpeedControlled = false;
			animSpeedSync = prevAnimSpeed;
		}
		lock (isMoveSpeedControlledLock) {
			isMoveSpeedControlled = false;
			moveSpeedSync = prevMoveSpeed;
		}
		lock (isBoostedLock) {
			Debug.Log ("Lock gone " + System.DateTime.Now);
			isBoosted = false;
		}
	}

    [Command]
    void CmdJumpForceCoroutine(float seconds)
    {
        lock (isBoostedLock)
        {
            if (!isBoosted)
            {
                StartCoroutine(JumpForceCoroutine(seconds));
                isBoosted = true;
                RpcCreateBoostPopUp("SUPER JUMP!");
            }
        }

    }

    IEnumerator JumpForceCoroutine(float seconds) {
		float prevJumpForce;
		lock(isJumpForceControlledLock) {
			isJumpForceControlled = true;
			prevJumpForce = jumpForceSync;
			jumpForceSync = 100.0f;
		}
		yield return new WaitForSecondsRealtime (seconds);
		lock (isJumpForceControlledLock) {
			isJumpForceControlled = false;
			jumpForceSync = prevJumpForce;
		}
		lock (isBoostedLock) {
			Debug.Log ("Lock gone " + System.DateTime.Now);
			isBoosted = false;
		}
	}

    [Command]
    void CmdShieldCoroutine(float seconds)
    {
        lock (isBoostedLock)
        {
            if (!isBoosted)
            {
                StartCoroutine(ShieldCoroutine(seconds));
                isBoosted = true;
                RpcCreateBoostPopUp("SAY NO TO FAST FOOD!");
            }
        }

    }

    IEnumerator ShieldCoroutine(float seconds)
    {
        isShield = true;
        GameObject sh = Instantiate(shield, Vector2.zero, Quaternion.identity) as GameObject;
        sh.transform.SetParent(transform);
        sh.transform.localPosition = Vector2.zero;
        NetworkServer.Spawn(sh);
        yield return new WaitForSecondsRealtime(seconds);
        isShield = false;
        Destroy(sh);
        lock (isBoostedLock)
        {
            isBoosted = false;
        }
    }

    [Command]
    void CmdTornadoCoroutine(float seconds)
    {
        lock (isBoostedLock)
        {
            if (!isBoosted)
            {
                StartCoroutine(TornadoCoroutine(seconds));
                isBoosted = true;
                RpcCreateBoostPopUp("TORNADOES");
            }
        }

    }

    IEnumerator TornadoCoroutine(float seconds)
    {
        GameObject tor = Instantiate(tornado, Vector2.zero, Quaternion.identity) as GameObject;
        tor.transform.position = transform.position;
        tor.GetComponent<TornadoDestroy>().SetFollow(transform, new Vector2(10, 0));
        GameObject tor2 = Instantiate(tornado, Vector2.zero, Quaternion.identity) as GameObject;
        tor2.transform.position = transform.position;
        tor2.GetComponent<TornadoDestroy>().SetFollow(transform, new Vector2(-10, 0));
        NetworkServer.Spawn(tor);
        NetworkServer.Spawn(tor2);
        yield return new WaitForSecondsRealtime(seconds);
        Destroy(tor);
        Destroy(tor2);
        lock (isBoostedLock)
        {
            isBoosted = false;
        }
    }

    [Command]
    void CmdTeleport()
    {
        RpcTeleport();
        RpcCreateBoostPopUp("TELEPORTTTTT!");
    }

    [ClientRpc]
    void RpcTeleport()
    {
        transform.position = new Vector2(transform.position.x + 100, transform.position.y + 100);
    }

    [Command]
    void CmdBulletCoroutine(float seconds)
    {
        lock (isBoostedLock)
        {
            if (!isBoosted)
            {
                StartCoroutine(BulletCoroutine(seconds));
                isBoosted = true;
                RpcCreateBoostPopUp("SHIT FEST!");
            }
        }
        
    }
   
    IEnumerator BulletCoroutine(float seconds)
    {
        // Set up bullet on server
        float startTime = Time.time;
        float shitInterval = 0.35f;
        while (Time.time - startTime < seconds)
        {
            GameObject bullet = Instantiate(shitBullet, new Vector2(transform.position.x - 5, transform.position.y + 5), Quaternion.identity) as GameObject;
            bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(-10, 5);

            // spawn bullet on client, custom spawn handler will be called
            NetworkServer.Spawn(bullet);
            audio.PlayOneShot(poopSound, 1F);

            // when the bullet is destroyed on the server it wil automatically be destroyed on clients
            StartCoroutine(Destroy(bullet, 5.0f));
            yield return new WaitForSecondsRealtime(shitInterval);
        }
        lock (isBoostedLock)
        {
            isBoosted = false;
        }
        yield break;
    }

    public IEnumerator Destroy(GameObject go, float timer)
    {
        yield return new WaitForSeconds(timer);
        NetworkServer.UnSpawn(go);
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
			OnFruitContact (other);
		} else if (other.tag.Contains ("JunkFood")) {
			OnJunkFoodContact (other);
		} else if (other.tag.Contains ("Endpoint")) {
			CmdCheckWinCondition ();
        }
	}

	[Command]
	void CmdCheckWinCondition() {
		GameObject winObject = GameObject.Find ("CheckWinObject");
        Debug.Log("finding win object");
		bool isWin = winObject.GetComponent<WinConditionCheck> ().CheckIsWin();
        isFinished = true;
		RpcIsWinMessage (isWin);
	}

	[ClientRpc]
	void RpcIsWinMessage(bool isWin) {
        if (!isLocalPlayer)
            return;
		if (isWin)
			IsWinPopUpController.createIsWinPopUp ();
		else
			IsWinPopUpController.createIsLostPopUp ();
        GameObject.Find("Canvas").GetComponent<EndGameHandler>().showEndPanel();  // sets end panel to active
        // EndGameHandler script is set as component of Canvas rather than ReturntoMenu panel because panel is inactive by default

    }

	void OnJunkFoodContact(Collider2D other) {
		// add health and fats
		// if hamburger, grow more fats
        if (isShield)
        {
            return;
        }
        audio.PlayOneShot(burpSound, 1F);
		if (other.tag.Contains ("Hamburger")) {
			AddHealth (0.01f);
			CmdAddFats (1.0f);
			if (isLocalPlayer) {
				PlusPopUpController.createHealthPlusPopUp ("+1% hp");
				PlusPopUpController.createFatsPlusPopUp ("+100% fats");
				BoostPopUpController.createBoostPopUp2 ("DISGUSTING!");
			}
		} else if (other.tag.Contains ("Dung")) {
			AddHealth (-0.2f);
			CmdAddFats (0.15f);
            if (isLocalPlayer) {
				PlusPopUpController.createHealthPlusPopUp ("-25% hp");
				PlusPopUpController.createFatsPlusPopUp ("+10% fats");
				BoostPopUpController.createBoostPopUp2 ("EWW GROSS!");
			}
		} else {
			AddHealth (0.05f);
			CmdAddFats (0.5f);
            if (isLocalPlayer) {
				PlusPopUpController.createHealthPlusPopUp ("+5% hp");
				PlusPopUpController.createFatsPlusPopUp ("+25% fats");
			}
		}

		Destroy (other.gameObject);

	}
    
	void OnFruitContact(Collider2D other) {
        // add health and fats
        if (other.tag.Contains("Grapes"))
        {
            AddHealth(0.8f);
            if (isLocalPlayer)
            {
                PlusPopUpController.createHealthPlusPopUp("+80% hp");
                PlusPopUpController.createFatsPlusPopUp("+0% fats");
                BoostPopUpController.createBoostPopUp("HEALTHY GRAPES!");
                audio.PlayOneShot(applecrunch, 1F);
            }
        }
        else if (other.tag.Contains("Banana"))
        {
            AddHealth(0.15f);
            CmdAddFats(-0.8f);
            if (isLocalPlayer)
            {
                PlusPopUpController.createHealthPlusPopUp("+15% hp");
                PlusPopUpController.createFatsPlusPopUp("-80% fats");
                BoostPopUpController.createBoostPopUp("TIME TO SLIM DOWN!");
                audio.PlayOneShot(applecrunch, 1F);
            }
        }
        else
        {
            AddHealth(0.2f);
            CmdAddFats(0.05f);
            if (isLocalPlayer)
            {
                PlusPopUpController.createHealthPlusPopUp("+25% hp");
                PlusPopUpController.createFatsPlusPopUp("+5% fats");
                audio.PlayOneShot(applecrunch, 1F);
            }
        }
		Destroy (other.gameObject);

        if (!isBoosted)
        {
            if (other.tag.Contains("Cherry"))
            {
                // gain lock on state
                CmdSpeedUpCoroutine(3.0f);
            }
            else if (other.tag.Contains("Apple"))
            {
                // gain lock on state
                CmdJumpForceCoroutine(5.0f);
            }
            else if (other.tag.Contains("Melon"))
            {
                CmdBulletCoroutine(5.0f);
            }
            else if (other.tag.Contains("Strawberry"))
            {
                audio.PlayOneShot(shieldSound, 1F);
                CmdShieldCoroutine(5.0f);
            }
            else if (other.tag.Contains("Pineapple"))
            {
                CmdTornadoCoroutine(5.0f);
            }
        }
    }

    [ClientRpc]
    void RpcCreateBoostPopUp(string title)
    {
        if (!isLocalPlayer)
            return;
        BoostPopUpController.createBoostPopUp(title);
    }
}
