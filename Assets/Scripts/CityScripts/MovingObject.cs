using UnityEngine;
using System.Collections;

/** Represents a deterministically moving object in the game such as a car. */
public class MovingObject : MonoBehaviour {

	/** How fast this object will move. */
	[HideInInspector] public float speed;
	/** The direction this object is moving (1 -> right; -1 -> left). */
	[HideInInspector] public int direction;
		
	/** Update is called once per frame. Moves our object. */
	void Update () {
		//Create a new position to move to.
		Vector2 position = 
			new Vector2(direction, 0) * speed * Time.deltaTime;
		
		//Move to the new position.
		transform.Translate (position.x, position.y, 0);
	}

}
