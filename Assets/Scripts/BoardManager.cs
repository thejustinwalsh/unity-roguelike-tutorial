using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
	[Serializable]
	public class Count
	{
		public int min;
		public int max;

		public Count(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
	}

	// Tuning
	public int columns = 8;
	public int rows = 8;
	public Count wallCount = new Count(5, 9);
	public Count foodCount = new Count(1, 5);

	// Tile References
	public GameObject exit;
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] outerWallTiles;
	public GameObject[] foodTiles;
	public GameObject[] enemyTiles;

	private Transform root;
	private List<Vector3> gridPositions = new List<Vector3>();

	void InitializeList()
	{
		gridPositions.Clear();

		for (int x = 1; x < columns - 1; x++)
		{
			for (int y = 1; y < rows - 1; y++)
			{
				gridPositions.Add(new Vector3(x, y, 0.0f));
			}
		}
	}

	void BoardSetup()
	{
		root = new GameObject("Board").transform;

		for (int x = -1; x < columns + 1; x++) {
			for (int y = -1; y < rows + 1; y++) {
				GameObject tile = floorTiles[Random.Range(0, floorTiles.Length)];
				if (x == -1 || x == columns || y == -1 || y == rows) {
					tile = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
				}

				GameObject instance = Instantiate(tile, new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent(root);
			}
		}
	}

	Vector3 RandomPosition()
	{
		int randomIndex = Random.Range(0, gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt(randomIndex);

		return randomPosition;
	}

	void LayoutObjectAtRandom(GameObject[] tileArray, int min, int max)
	{
		int objectCount = Random.Range(min, max + 1);
		for (int i = 0; i < objectCount; i++) {
			Vector3 randomPos = RandomPosition();
			GameObject tile = tileArray[Random.Range(0, tileArray.Length)];
			Instantiate(tile, randomPos, Quaternion.identity);
		}
	}

	public void SetupScene(int level)
	{
		BoardSetup();
		InitializeList();
		
		LayoutObjectAtRandom(wallTiles, wallCount.min, wallCount.max);
		LayoutObjectAtRandom(foodTiles, foodCount.min, foodCount.max);

		int enemyCount = (int)Mathf.Log(level, 2.0f);
		LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

		Instantiate(exit, new Vector3(columns - 1, rows - 1, 0.0f), Quaternion.identity);
	}
}
