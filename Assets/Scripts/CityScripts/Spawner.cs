using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

/** Spawns game objects. */
public class Spawner : MonoBehaviour {

	/** The game object we want to spawn. */
	public GameObject gameObj;
	/** The direction the objects spawned will go (1 -> right; -1 -> left). */
	public int direction;
	/** The sorting order of the object we're spawning in order to render properly. */
	[HideInInspector] public int sortingOrder;

	/** The speed of the object this spawner will spawn. */
	private float speed;
	/** An array of all possible speeds the objects can move. */
	private float[] speeds = {2f, 2.1f, 2.2f, 2.3f, 2.4f, 2.5f, 2.6f, 2.7f, 2.8f, 2.9f,
		3f, 3.1f, 3.2f, 3.3f, 3.4f, 3.5f};
	/** The spawn time of this spawner. */
	private float spawnTime;
	/** An array of all possible spawn times. */
	private float[] spawnTimes = {2.5f, 2.6f, 2.7f, 2.8f, 2.9f, 3.3f};
	

	/** Initialization. */
	void Start () {
		//Pick a random speed for the object to move at.
		speed = speeds [Random.Range (0, speeds.Length)];

		//Initial spawn.
		Invoke ("Spawn", 0);
	}
	

	/** Does the actual spawning. */
	void Spawn () {
		//Spawns a new object and sets its speed, direction, and sorting order.
		GameObject obj
			= (GameObject) Instantiate (gameObj, transform.position, transform.rotation);
		obj.GetComponent <MovingObject> ().speed = speed;
		obj.GetComponent <MovingObject> ().direction = direction;
		obj.GetComponent <SpriteRenderer> ().sortingOrder = sortingOrder;

		//Pick a random spawn time.
		spawnTime = spawnTimes [Random.Range (0, spawnTimes.Length)];

		//Spawn in spawnTime seconds.
		Invoke ("Spawn", spawnTime);
	}

}
