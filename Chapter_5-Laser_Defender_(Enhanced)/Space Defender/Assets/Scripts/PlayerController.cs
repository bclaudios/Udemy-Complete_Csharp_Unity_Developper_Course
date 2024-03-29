using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float playerHealth;
	public float playerSpeed;
	public float pallierLvl2, pallierLvl3;
	public float spriteRedTime;

	public GameObject projectile;
	public float lvl1ProjectileSpeed, lvl2ProjectileSpeed, lvl3ProjectileSpeed;
	public float lvl1ProjectileFiringRate, lvl2ProjectileFiringRate, lvl3ProjectileFiringRate;

	public Sprite[] playerSprite;

	public GameObject shieldObject;
	public int shieldCountLvl1, shieldCountLvl2, shieldCountLvl3;
	public int shieldHealth;
	public int shieldCount;
	public AudioClip[] shieldActivationSFX;
	public float shieldActivationSFXVolume;

	public float levelUpSFXVolume;
	public AudioClip levelUpSFX;

	public AudioClip toneSFX;
	public float SFXToneVolume;

	public AudioClip[] hitSound;
	public float SFXHitVolume;

	public GameObject playerExplosion;
	public GameObject alarmSFX;


	private float padding;
	private float xMin, xMax, newX;
	private float yMin, yMax, newY;
	private bool alarmOn;
	private bool lvl2, lvl3;
	private bool shieldActivated;
	private bool rightCanon;
	private GameObject alarm;
	private GameObject shield;
	private AudioSource audio;
	private GameObject laser;
	private LevelManager levelManager;




	void Start() {
		Score.score = 0;
		levelManager = GameObject.FindObjectOfType<LevelManager>();
		shieldCount = shieldCountLvl1;
		rightCanon = true;
		DefineEdges ();
		PlayToneSound ();
	}

	void Update() {
		LvlCheck();
		InputMovement ();
		InputFire ();
		InputShield();
		if (playerHealth == 1 && alarmOn == false) {
			playLowHealthSound();
		}
	}


	void OnTriggerEnter2D (Collider2D collider)
	{
		Destroy (collider.gameObject);
		if (!shieldActivated) {
			playerHealth--;
			if (playerHealth <= 0) {
				Destroy (alarm);
				Instantiate (playerExplosion, transform.position, Quaternion.identity);
				Destroy (gameObject);
			} else {
				PlayHitSound ();
				StartCoroutine (HitColor ());
			}
		}
	}




	void LvlCheck() {
		SpriteRenderer spriteRend = GetComponent<SpriteRenderer>();
		if (Score.score >= pallierLvl2 && lvl2 == false) {
			shieldCount += shieldCountLvl2;
			playerHealth += 2;
			spriteRend.sprite = playerSprite[1];
			gameObject.tag = "Level 2";
			AudioSource.PlayClipAtPoint(levelUpSFX, transform.position, levelUpSFXVolume);
			lvl2 = true;
		}
		if (Score.score >= pallierLvl3 && lvl3 == false) {
			shieldCount += shieldCountLvl3;
			playerHealth += 3;
			spriteRend.sprite = playerSprite[2];
			gameObject.tag = "Level 3";
			AudioSource.PlayClipAtPoint(levelUpSFX, transform.position, levelUpSFXVolume);
			lvl3 = true;
		}
	}




	void DefineEdges() {
		padding = 0.5f;
		float zDistance = transform.position.z - Camera.main.transform.position.z;
		Vector3 leftEdge = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, zDistance));
		Vector3 rightEdge = Camera.main.ViewportToWorldPoint (new Vector3 (1, 0, zDistance));
		Vector3 topEdge = Camera.main.ViewportToWorldPoint (new Vector3 (0, 1, zDistance));
		Vector3 botEdge = Camera.main.ViewportToWorldPoint (new Vector3 (0, 0, zDistance));
		xMin = leftEdge.x + padding;
		xMax = rightEdge.x - padding;
		yMax = topEdge.y - padding;
		yMin = botEdge.y + padding;
	}

	void InputMovement() {
		if (Input.GetKey(KeyCode.Q)) {
			transform.position += Vector3.left * playerSpeed * Time.deltaTime;
		} else if (Input.GetKey(KeyCode.D)) {
			transform.position += Vector3.right * playerSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Z)) {
			transform.position += Vector3.up * playerSpeed * Time.deltaTime;
		} else if (Input.GetKey(KeyCode.S)) {
			transform.position += Vector3.down * playerSpeed * Time.deltaTime;
		}
		newX = Mathf.Clamp (transform.position.x, xMin, xMax);
		newY = Mathf.Clamp (transform.position.y, yMin, yMax);
		transform.position = new Vector3 (newX, newY, transform.position.z);
		RotationMovement();
	}

	void RotationMovement() {
		if (Input.GetKey(KeyCode.Q)) {
			transform.rotation = Quaternion.Euler (0, -40, 0);
		} else if (Input.GetKey(KeyCode.D)) {
			transform.rotation = Quaternion.Euler (0, 40, 0);
		} else {
			transform.rotation = Quaternion.Euler (0, 0, 0);
		}
	}




	void InputFire() {
		if (Input.GetKeyDown (KeyCode.Space)) {
			FireRepartition();
		} else if (Input.GetKeyUp(KeyCode.Space)) {
			CancelInvoke ();
		}
	}

	void FireRepartition() {
		if (tag == "Level 1") {
			InvokeRepeating ("FireLvl1", 0.00001f, lvl1ProjectileFiringRate);
		} else if (tag == "Level 2") {
			InvokeRepeating ("FireLvl2", 0.00001f, lvl2ProjectileFiringRate);
		} else if (tag == "Level 3") {
			InvokeRepeating ("FireLvl3", 0.00001f, lvl3ProjectileFiringRate);
		}
	}


	void FireLvl1() {
		Vector3 laserPadding = new Vector3 (0, 0.2f);
		laser = Instantiate(projectile, (transform.position + laserPadding), Quaternion.identity) as GameObject;
		laser.GetComponent<Rigidbody2D> ().velocity = new Vector3 (0, lvl1ProjectileSpeed);
	}


	void FireLvl2() {
		Vector3 laserPadding = new Vector3 (0.3f,0);
		GameObject leftLaser = Instantiate (projectile, (transform.position - laserPadding), Quaternion.identity) as GameObject;
		leftLaser.GetComponent<Rigidbody2D> ().velocity = new Vector3 (0, lvl2ProjectileSpeed);
		GameObject rightLaser = Instantiate (projectile, (transform.position + laserPadding), Quaternion.identity) as GameObject;
		rightLaser.GetComponent<Rigidbody2D> ().velocity = new Vector3 (0, lvl2ProjectileSpeed);
	}


	void FireLvl3() {
		Vector3 laserPadding = new Vector3 (0.3f, 0);
		if (rightCanon) {
			laser = Instantiate (projectile, (transform.position + laserPadding), Quaternion.identity) as GameObject;
			rightCanon = false;
		} else {
			laser = Instantiate (projectile, (transform.position - laserPadding), Quaternion.identity) as GameObject;
			rightCanon = true;
		}
		laser.GetComponent<Rigidbody2D> ().velocity = new Vector3 (0, lvl3ProjectileSpeed);
	}




	void playLowHealthSound() {
		alarmOn = true;
		alarm = Instantiate(alarmSFX, transform.position, Quaternion.identity) as GameObject;
		if (playerHealth > 1 || playerHealth == 0) {
			alarmOn = false;
			Destroy(alarm);
		}
	}




	void PlayHitSound() {
		int randomIndex = Random.Range(0, hitSound.Length);
		AudioSource.PlayClipAtPoint(hitSound[randomIndex], transform.position, SFXHitVolume);
	}

	IEnumerator HitColor() {
		SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
		PolygonCollider2D polyCollider = this.GetComponent<PolygonCollider2D>();
		polyCollider.enabled = false;
		spriteRenderer.color = new Color (255f, 0f, 0f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 255f, 255f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 0f, 0f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 255f, 255f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 0f, 0f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 255f, 255f, 255f);
		spriteRenderer.color = new Color (255f, 0f, 0f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 255f, 255f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 0f, 0f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 255f, 255f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 0f, 0f, 255f);
		yield return new WaitForSeconds(spriteRedTime);
		spriteRenderer.color = new Color (255f, 255f, 255f, 255f);
		polyCollider.enabled = true;
	}




	void InputShield() {
		if (Input.GetKey(KeyCode.LeftShift) && shieldCount > 0 && shieldActivated == false) {
			shieldHealth = 5;
			shieldCount--;
			shieldActivated = true;
			shield = Instantiate (shieldObject, transform.position, Quaternion.identity) as GameObject;
			shield.transform.parent = transform;
			int randomIndex = Random.Range(0, shieldActivationSFX.Length);
			AudioSource.PlayClipAtPoint (shieldActivationSFX[randomIndex], transform.position, shieldActivationSFXVolume);
		} else if (shieldHealth <= 0) {
			Destroy(shield);
			shieldActivated = false;
		}
	}


	void PlayToneSound() {
		audio = GetComponent<AudioSource> ();
		audio.clip = toneSFX;
		audio.loop = true;
		audio.volume = SFXToneVolume;
		audio.Play();
	}
}
