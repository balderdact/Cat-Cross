using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/** Manages the layout of the game and assists with player movement. */
public class GameManager : MonoBehaviour {

	/*******************
	 * Layout Variables *
	 *******************/
	
	/** True if the player is able to move. */
	[HideInInspector] public static bool canMove = true;
	/** Delay between each Player turn. */
	public float turnDelay = 0.1f;
	/** The number of rows we'll initialize at the start. */
	public int numStartRows = 8;
	/** Boolean to check if we're waiting for the player to move. */
	private bool waiting;
	/** The y-coordinate of the next row to be loaded. */
	private static int y = 0;


	/****************************
	 * Non-lethal Row Variables *
	 ****************************/
	
	/** A tile that the player dies on if stepped on. */
	public GameObject hazard;
	/** A row where the player can relax. */
	public GameObject nonLethalFloor;
	/** Obstacles for the nonLethalFloor. */
	public GameObject[] obstacles;
	/** Random x-coordinates to leave open so that the player won't be blocked. */
	private int randomOpening;
	private int randomOpening2;
	private int randomOpening3;


	/**********************
	 * Road Row Variables *
	 **********************/
	
	/** Road tiles. */
	public GameObject singleRowRoadTile;
	public GameObject bottomRowRoadTile;
	/** Array of possible spawners to put. */
	public GameObject[] spawners;
	/** An invisible barrier that blocks the player. */
	public GameObject barrier;
	/** A queue to determine what road tiles to place for consective road rows. */ 
	private Queue <GameObject> roadRowQueue = new Queue <GameObject> ();
	/** True if we can create a road row.
	 *  False if we had just finished making a sequence of them. */
	private bool canCreateRoadRow = true;


	/***********************
	 * Water Row Variables *
	 ***********************/

	/** The water row. */
	public GameObject waterRow;
	/** Boat spawners. */
	public GameObject[] boatSpawners;


	/*******************
	 * Event Functions *
	 *******************/


	/** Sets up the floor tiles to start the game. */
	void Awake () {
		//Initialize our random openings.
		CreateRandomOpenings ();

		//Initialize 8 rows at the start of the game.
		Instantiate (nonLethalFloor, new Vector3 (0, -1, 0f), Quaternion.identity);
		for (; y < numStartRows - 1; y++) {
			CreateRow ();
		}
	}
	

	/** Update is called once per frame. */
	void Update () {
		//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
		if(canMove || waiting)
			//If true, return and do not start Wait coroutine.
			return;
		
		//Start waiting.
		StartCoroutine (Wait ());
	}


	/** Coroutine to handle movement delay. */
	IEnumerator Wait () {
		waiting = true;
		
		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds(turnDelay);
		yield return new WaitForSeconds(turnDelay);
		
		canMove = true;
		waiting = false;
	}


	/** Destroys the bottom most row and creates a new one. */
	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Floor") {
			CreateRow();
			
			//Increment the y-coordinate of the next row to be loaded.
			y++;
		}
		Destroy (other.gameObject);
	}



	/**************************
	 * Row Creation Functions *
	 **************************/


	/** Creates random openings for the player to prevent from being blocked. */
	void CreateRandomOpenings () {
		randomOpening = Random.Range (-4, 5);
		do {
			randomOpening2 = Random.Range (-4, 5);
		} while (randomOpening2 == randomOpening);
		do {
			randomOpening3 = Random.Range (-4, 5);
		} while (randomOpening3 == randomOpening || randomOpening3 == randomOpening2);
	}


	/** Creates a new row. */
	void CreateRow () {
		//Always want a non lethal row at the starting row.
		if (y == 0) {
			CreateNonLethalRow();
			return;
		}

		//If we're still in our road row sequence, make sure we stay there.
		if (roadRowQueue.Count != 0) {
			CreateRoadRow();
			return;
		}

		//Randomly choose between creating different rows.
		int randomChoice = Random.Range (0, 3);
		if (!canCreateRoadRow || randomChoice == 0) {
			CreateNonLethalRow ();
			canCreateRoadRow = true;
		} else if (randomChoice == 1) {
			CreateRoadRow ();
			CreateRandomOpenings ();
		} else {
			CreateWaterRow ();
			CreateRandomOpenings ();
		}
	}


	/** Creates a non-lethal row. */
	void CreateNonLethalRow () {
		//Create a new floor.
		Instantiate (nonLethalFloor, new Vector3 (0, y, 0f), Quaternion.identity);
				
		//Add our side barriers.
		for (int x = -7; x < -4; x++) {
			CreateObstacle (x);
		}
		for (int x = 5; x < 8; x++) {
			CreateObstacle (x);
		}

		//We don't want obstacles on our starting row.
		if (!(y == 0)) {
			//Add obstacles randomly.
			for (int x = -4; x < 5; x++) {
				if (x != randomOpening && x != randomOpening2
				    && x != randomOpening3 && Random.Range (0, 2) == 0) {
					bool obstacleNotCreated = true;
					if (Random.Range (0, 2) == 0) {
						CreateObstacle (x);
						obstacleNotCreated = false;
					}
					if (obstacleNotCreated && Random.Range (0, 10) == 0) {
						Instantiate (hazard, new Vector3 (x, y, 0f), Quaternion.identity);
					}
				}
			}
		}
	}


	/** Creates an obstacle for the non-lethal row at X. */
	void CreateObstacle (int x) {
		//Create a new obstacle tile.
		Instantiate (barrier, new Vector3 (x, y, 0f), Quaternion.identity);

		//Store a random decoration from the decorations array.
		GameObject obstacle = obstacles[Random.Range (0, obstacles.Length)];

		//Decrement the sorting order for all decorations so that previous decorations
		//will render on top of newer ones.
		if (x == -7) {
			for (int i = 0; i < obstacles.Length; i++) {
				SpriteRenderer renderer = obstacles[i].GetComponent <SpriteRenderer> ();
				renderer.sortingOrder = -y;
			}
		}

		//Create the selected decoration.
		Instantiate (obstacle, new Vector3 (x, y, 0f), Quaternion.identity);
	}
	
	
	/** Creates a road row. */
	void CreateRoadRow () {
		//Sets up the queue for the road row sequence.
		if (roadRowQueue.Count == 0) {
			int roadCount = 0;
			for (int i = 1; Random.Range (0, i) == 0; i += Random.Range (0, 3)) roadCount++;
			if (roadCount == 1) {
				roadRowQueue.Enqueue(singleRowRoadTile);
			} else {
				for (int i = roadCount; i >= 1; i--) {
					if (i == roadCount || i != 1) {
						roadRowQueue.Enqueue(bottomRowRoadTile);
					} else {
						roadRowQueue.Enqueue(singleRowRoadTile);
					}
				}
			}
		}

		//Create a new floor.
		Instantiate (roadRowQueue.Dequeue(), new Vector3 (0, y, 0f), Quaternion.identity);

		//If we just instantiated the last row of the sequence, make sure we don't pick
		//a road row sequence to instantiate again immediately.
		if (roadRowQueue.Count == 0) canCreateRoadRow = false;

		//Set up barriers so player can't go out of the screen.
		Instantiate (barrier, new Vector3 (-5, y, 0f), Quaternion.identity);
		Instantiate (barrier, new Vector3 (5, y, 0f), Quaternion.identity);
		
		//Randomly pick a spawner out of the spawner array.
		GameObject spawnerToSpawn = spawners[Random.Range (0, spawners.Length)];

		//Set the spawner's sorting order.
		spawnerToSpawn.GetComponent <Spawner> ().sortingOrder = -y;
		
		//Create the chosen spawner.
		Instantiate (spawnerToSpawn, new Vector3 (spawnerToSpawn.transform.position.x, y, 0f),
		             Quaternion.identity);
	}


	/** Creates a water row. */
	void CreateWaterRow () {
		//Create a new floor.
		Instantiate (waterRow, new Vector3 (0, y, 0f), Quaternion.identity);

		//Randomly pick a spawner out of the spawner array.
		GameObject spawnerToSpawn = boatSpawners[Random.Range (0, boatSpawners.Length)];

		//Create the chosen spawner.
		Instantiate (spawnerToSpawn, new Vector3 (spawnerToSpawn.transform.position.x, y, 0f),
		             Quaternion.identity);
	}

}
