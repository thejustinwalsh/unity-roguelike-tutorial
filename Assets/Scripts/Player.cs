using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject
{
	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1.0f;
	public float restartGameOverDelay = 10.0f;
	public Text foodText;

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private bool isCompletingAction = false;
	private Animator animator;
	private int food;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
	private Vector2 touchOrigin = -Vector2.one;
#endif

	protected override void Start ()
	{
		animator = GetComponent<Animator>();
		food = GameManager.instance.playerFoodPoints;
		foodText.text = string.Format("Food: {0}", food);

		base.Start();	
	}

	private void OnDisable()
	{
		GameManager.instance.playerFoodPoints = food;
	}
	
	void Update ()
	{
		if (!GameManager.instance.playersTurn || this.isMoving || isCompletingAction) return;

		int horizontal = 0, vertical = 0;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
		horizontal = (int) Input.GetAxisRaw("Horizontal");
		vertical = (horizontal == 0) ? (int) Input.GetAxisRaw("Vertical") : 0;
 #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		if (Input.touchCount > 0) {
			Touch touch = Input.touches[0];
			if (touch.phase == TouchPhase.Began) {
				touchOrigin = touch.position;
			}
			else if (touch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				Vector2 touchEnd = touch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -1;
				if (Mathf.Abs(x) > Mathf.Abs(y)) {
					horizontal = x > 0 ? 1 : -1;
				}
				else {
					vertical = y > 0 ? 1 : -1;
				}
			}
		}		
#endif

		if (horizontal != 0 || vertical != 0) {
			StartCoroutine(MovePlayer(horizontal, vertical));
		}
	}

	IEnumerator MovePlayer(int xDir, int yDir)
	{
		bool turnAction = false;
		if (AttemptMove(xDir, yDir)) {
			while (this.isMoving) { yield return null; }
			turnAction = true;
		}
		else if (isCompletingAction) {
			yield return new WaitForSeconds(moveTime);
			isCompletingAction = false;
			turnAction = true;
		}

		if (turnAction) GameManager.instance.playersTurn = false;
	}

	protected override bool AttemptMove(int xDir, int yDir)
	{
		bool didMove = false;

		if (didMove = base.AttemptMove(xDir, yDir)) {
			--food;
			foodText.text = string.Format("Food: {0}", food);

			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
		}

		CheckIfGameOver();

		return didMove;
	}

	protected override void OnCantMove(Transform transform)
	{
		Wall hitWall = transform.GetComponent<Wall>();
		if (hitWall) {
			isCompletingAction = true;

			--food;
			foodText.text = string.Format("Food: {0}", food);

			hitWall.DamageWall(wallDamage);
			animator.SetTrigger("PlayerChop");
		}

		/*
		Enemy hitEnemy = transform.GetComponent<Enemy>();
		if (hitEnemy) {
			isCompletingAction = true;

			--food;
			foodText.text = string.Format("Food: {0}", food);

			animator.SetTrigger("PlayerChop");
		}
		*/
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Exit") {
			Invoke("NextLevel", restartLevelDelay);
			enabled = false;
		}
		else if (other.tag == "Food") {
			food += pointsPerFood;
			foodText.text = string.Format("+{0} Food: {1}", pointsPerFood, food);

			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

			other.gameObject.SetActive(false);
		}
		else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = string.Format("+{0} Food: {1}", pointsPerSoda, food);

			SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

			other.gameObject.SetActive(false);
		}
	}

	public void LoseFood(int loss)
	{
		animator.SetTrigger("PlayerHit");
		food -= loss;
		foodText.text = string.Format("-{0} Food: {1}", pointsPerSoda, food);

		CheckIfGameOver();
	}

	private void CheckIfGameOver()
	{
		if (food <= 0) {
			enabled = false;

			SoundManager.instance.PlaySingle(gameOverSound);
			GameManager.instance.GameOver(restartGameOverDelay);
		}
	}

	private void NextLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}
}
