using UnityEngine;
using System.Collections;

/** The point of view we play the game in. */
public class CameraMovement : MonoBehaviour {

	/** The speed with which the camera will be moving. */
	public float smoothing = 1f;
	/** The default y-axis increment for each frame. */
	public float normalIncrement = 0.3f;
	/** The y-axis increment to follow the player once he reaches a certain point. */
	public float followIncrement = 2f;
	/** The position that the camera will be following. */
	public Transform target;

	/** The starting point of the cat. */
	private Vector3 basePosition = new Vector3 (0, 0, 0);
	/** True if the player has started moving. */
	private bool started = false;

	
	/** Moves the camera. */
	void Update () {
		//Set started to true if the player started moving.
		if (target.transform.position != basePosition) started = true;

		//If the player hasn't moved yet at the start, don't move the camera yet.
		if (started) {
			// Create a postion the camera is aiming for based on the offset from the target.
			Vector3 targetCamPos = transform.position;

			//If the player moves too high, have the camera follow the player.
			//Otherwise, move camera normally.
			if (target.transform.position.y > transform.position.y - 0.5f) {
				targetCamPos.y += followIncrement;
			} else {
				targetCamPos.y += normalIncrement;
			}
			
			//Smoothly interpolate between the camera's current position and it's target position.
			transform.position
				= Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
		}
	}

}
