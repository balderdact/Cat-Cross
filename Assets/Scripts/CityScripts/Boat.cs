using UnityEngine;
using System.Collections;

/** Represents a boat. */
public class Boat : MovingObject {

	/** Set the cat's parent to the boat if it enters its collider. */
	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Player") {
			other.transform.parent.SetParent (transform);
		}
	}

	/** Unparents the cat if it leaves the boat. */
	void OnTriggerExit2D (Collider2D other) {
		if (other.tag == "Player") {
			transform.DetachChildren ();
		}
	}

}
