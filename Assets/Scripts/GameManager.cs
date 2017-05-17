using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;

	public BoardManager boardScript;

	public int startingFoodPoints = 100;
	[HideInInspector] public int playerFoodPoints;

	public float turnDelay = 0.1f;

	[HideInInspector] public bool playersTurn = true;


	private int level = 0;
	private List<Enemy> enemies = new List<Enemy>();
	private bool enemiesMoving = false;


	public float levelStartDelay = 2.0f;
	private Text levelText;
	private GameObject levelImage;
	private bool doingSetup = false;

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		level++;
		InitGame();
	}

	void InitGame()
	{
		boardScript.SetupScene(level);

		enemies.Clear();

		levelImage = GameObject.Find("LevelImage");
		levelImage.SetActive(true);
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		levelText.text = string.Format("Day {0}", level);

		doingSetup = true;
		Invoke("HideLevelImage", levelStartDelay);

	}

	void Awake()
	{
		if (instance == null) { instance = this; } else if (instance != this) { Destroy(gameObject); }
		DontDestroyOnLoad(gameObject);

		boardScript = GetComponent<BoardManager>();

		playerFoodPoints = startingFoodPoints;
	}
	
	void Update()
	{
		if (playersTurn || enemiesMoving || doingSetup) return;
		
		StartCoroutine(MoveEnemies());
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}
	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	public void AddEnemyToList(Enemy script)
	{
		enemies.Add(script);
	}

	IEnumerator MoveEnemies()
	{
		enemiesMoving = true;
		
		bool noEnemyMovement = true;
		foreach(var enemy in enemies) {
			if (enemy.MoveEnemy()) {
				noEnemyMovement &= false;
				while (enemy.isMoving) { yield return null; }
			}
			else {
				yield return null;
			}
		}

		if (enemies.Count == 0 || noEnemyMovement) {
			yield return new WaitForSeconds(turnDelay);
		}

		enemiesMoving = false;

		playersTurn = true;
	}

	void HideLevelImage()
	{
		levelImage.SetActive(false);
		doingSetup = false;
	}

	public void GameOver(float restartDelay)
	{
		enabled = false;

		SoundManager.instance.musicSource.Stop();

		levelImage.SetActive(true);
		levelText.text = string.Format("After {0} days, you starved.", level);

		Invoke("Restart", restartDelay);
	}

	public void Restart()
	{
		enabled = true;
		doingSetup = true;
		level = 0;
		playerFoodPoints = startingFoodPoints;

		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);

		SoundManager.instance.musicSource.Play();
	}
}
