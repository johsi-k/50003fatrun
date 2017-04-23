using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class Character : NetworkBehaviour {

    // references the health bar and fat level bar
	private Image healthContent;
	private Text healthText;
    private Image fatContent;
    private Text fatText;

    // All the audio
    private AudioSource audio;
    public AudioClip applecrunch;
    public AudioClip jumpSound;
    public AudioClip poopSound;
    public AudioClip shieldSound;
    public AudioClip deathSound;
    public AudioClip burpSound;
    
	// amount of fats: from 0-1; fat level is controlled by the server object
	[SyncVar]
	private float fatLevel = 1f;
	// how much fats to drop per sec
	private float fatDropRate = 0.1f;
        private float health = 1f;
    private float healthDropRate = 0.025f;

	// whether fruit is eaten; managed by server;
    [SyncVar]
	private bool isBoosted = false;

	// whether these attributes are altered by fruits; managed by server
	private bool isMoveSpeedControlled = false;
	private bool isJumpForceControlled = false;
	private bool isAnimSpeedControlled = false;
	private bool isScaleControlled = false;

	// Locks for control on the server
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
    // position to return to upon death
    private Vector2 deathPosition;

    // if character has shield, will not eat fast food
    [SyncVar]
    private bool isShield = false;

    // whether game has ended
    private bool isEnd = false;

    // Server to clients variable syncing
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

    // Different animation states
	private static int runStateHash = Animator.StringToHash ("CatRun");
	private static int jumpStateHash = Animator.StringToHash ("CatJump");
	private static int fallStateHash = Animator.StringToHash ("CatFall");

	// the current animation state
	private int currentState;

    // check whether object is grounded, for animation change
	[SerializeField]
	private bool grounded = true;
	[SerializeField]
	private bool sideGrounded = true;
    // determines how long the object has been grounded, for animation smoothening
	private int notGroundedCount = 0;
	private RaycastHit2D[] rchresults;
	[SerializeField]
	private LayerMask ground;

    // prefabs for boost animations
    private UnityEngine.Object shitBullet;
    private UnityEngine.Object shield;
    private UnityEngine.Object tornado;


    void Awake () {
		rb = GetComponent<Rigidbody2D> ();
		collider = GetComponent<Collider2D> ();
		anim = GetComponent<Animator> ();
        spriteRenderer = GetComponent<SpriteRenderer>();
		currentState = runStateHash;
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

	// Update is called once per frame
	void Update () {
        // if the game has ended, player should not be able to move
        if (isEnd)
        {
            moveSpeed = 0;
            return;
        }

        // Server-side updating
		if (isServer) {
            // Reduce the fat level and then sync down
			ReduceFatLevel ();

            // If fat level has changed, change the movespeed, jumpforce, animationspeed, and scale
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
			}
		}

        // change the client side values to the values synced down by the server object
		moveSpeed = moveSpeedSync;
		jumpForce = jumpForceSync;
		anim.speed = animSpeedSync;
		transform.localScale = new Vector3 (scaleSync, transform.localScale.y, transform.localScale.z);

        // stops movement if player is dying and fades player away
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

        // set velocity
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
            // reduces health of player
			ReduceHealth ();

            if (health <= 0)
            {
                // die
                if (!isDead)
                {
                    audio.PlayOneShot(deathSound, 1F);
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

            // Checks whether character is grounded
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

    /////////////////////
    // ANIMATION COMMANDS
    /////////////////////

	// CHANGE TO FALL ANIMATION
	void ChangeFall(bool isFall) {
		anim.SetBool ("isFall", isFall);
	}

    // Tells other client objects that the character is falling, through the server
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

    // Tells other client objects that the character is in the air, through the server
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
	
    // Tells other client objects that the character has to jump, through the server
	[Command]
	void CmdJump() {
		RpcJump ();
	}

	[ClientRpc]
	void RpcJump() {
		isJump = true;
	}

    /////////////////////
    // CHANGE FATS AND HEALTH COMMANDS
    /////////////////////
		
    // Change Fat levels, through the server and synced down
	[Command]
	void CmdAddFats(float fats) {
		// need synchronity
		fatLevel += fats;
		if (fatLevel > 1.0f) {
			fatLevel = 1.0f;
		} else if (fatLevel < 0f)
        {
            fatLevel = 0.01f;
        }
	}

    // Change Health Levels
	void AddHealth(float hp) {
		// need synchronity
		health += hp;
		if (health > 1.0f) {
			health = 1.0f;
		} else if (health < 0f) {
			health = 0f;
		}
	}

    /////////////////////
    // DEATH COMMANDS
    /////////////////////
    // Tells the server to tell all clients that the character has died
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

    // Revives character after "deathDuration"
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

    // Tells all clients that the character has reborn
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

    /////////////////////
    // BOOST COMMANDS
    /////////////////////

    // Tells all clients that the character has sped up
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
        // Increase speed and lock modification of speed while boost is applied
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
        // Revert speed to before boost is applied
		lock (isAnimSpeedControlledLock) {
			isAnimSpeedControlled = false;
			animSpeedSync = prevAnimSpeed;
		}
		lock (isMoveSpeedControlledLock) {
			isMoveSpeedControlled = false;
			moveSpeedSync = prevMoveSpeed;
		}
        // unlock boost
		lock (isBoostedLock) {
			isBoosted = false;
		}
	}

    // Tells all clients that the character's jump Force has increased
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
        // Increase jump and lock modification of jump while boost is applied
		lock(isJumpForceControlledLock) {
			isJumpForceControlled = true;
			prevJumpForce = jumpForceSync;
			jumpForceSync = 100.0f;
		}
		yield return new WaitForSecondsRealtime (seconds);
        // Revert jump to before boost is applied
		lock (isJumpForceControlledLock) {
			isJumpForceControlled = false;
			jumpForceSync = prevJumpForce;
		}
        // unlock boost
		lock (isBoostedLock) {
			isBoosted = false;
		}
	}

    // Tells all clients that the character will have the shield animation
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

    // Spawns the shield animation on all clients for a few seconds
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

    // Tells all clients that the character will have the tornado animation
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

     // Spawns the tornado animation on all clients for a few seconds
    IEnumerator TornadoCoroutine(float seconds)
    {
        GameObject tor = Instantiate(tornado, Vector2.zero, Quaternion.identity) as GameObject;
        //tor.transform.position = transform.position;
        //tor.GetComponent<TornadoDestroy>().SetFollow(transform, new Vector2(10, 0));
        tor.transform.SetParent(transform);
        tor.transform.localPosition = new Vector2(10, 5);
        GameObject tor2 = Instantiate(tornado, Vector2.zero, Quaternion.identity) as GameObject;
        //tor2.transform.position = transform.position;
        //tor2.GetComponent<TornadoDestroy>().SetFollow(transform, new Vector2(-10, 0));
        tor2.transform.SetParent(transform);
        tor2.transform.localPosition = new Vector2(-10, 5);
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

    // Produces the shit fest on the character
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
   
    // Generates shit at a time interval of "shitInterval"
    IEnumerator BulletCoroutine(float seconds)
    {
        // Set up bullet on server
        float startTime = Time.time;
        float shitInterval = 0.75f;
        while (Time.time - startTime < seconds)
        {
            GameObject bullet = Instantiate(shitBullet, new Vector2(transform.position.x - 5, transform.position.y + 5), Quaternion.identity) as GameObject;
            bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(-10, 5);

            // spawn bullet on client, custom spawn handler will be called
            NetworkServer.Spawn(bullet);
            audio.PlayOneShot(poopSound, 1F);

            // when the bullet is destroyed on the server it wil automatically be destroyed on clients
            StartCoroutine(DestroyShit(bullet, 5.0f));
            yield return new WaitForSecondsRealtime(shitInterval);
        }
        lock (isBoostedLock)
        {
            isBoosted = false;
        }
        yield break;
    }

    // Destroys shit after "timer" seconds
    public IEnumerator DestroyShit(GameObject go, float timer)
    {
        yield return new WaitForSecondsRealtime(timer);
        NetworkServer.UnSpawn(go);
    }

    // Creates the boost popup on the local client
    [ClientRpc]
    void RpcCreateBoostPopUp(string title)
    {
        if (!isLocalPlayer)
            return;
        BoostPopUpController.createBoostPopUp(title);
    }

    /////////////////////
    // Check Win Condition
    /////////////////////
    [Command]
    void CmdCheckWinCondition() {
        GameObject winObject = GameObject.Find ("CheckWinObject");
        Debug.Log("finding win object");
        bool isWin = winObject.GetComponent<WinConditionCheck> ().CheckIsWin();
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

    /////////////////////
    // Collision Management
    /////////////////////
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
            if (!isEnd)
            {
                CmdCheckWinCondition();
                isEnd = true;
            }
			
        }
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
}
