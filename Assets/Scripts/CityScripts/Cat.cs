using UnityEngine;
using System.Collections;

/** Handles our played-controlled cat character. */
public class Cat : MonoBehaviour {

	/**********************
	 * Movement Variables *
	 **********************/
	
	/** Time it will take cat to move, in seconds. */
	public float moveTime = 0.1f;
	/** Used to make movement more efficient. */
	private float inverseMoveTime;
	/** Indicates which direction the cat is facing. */
	private string direction = "Down";
	/** True if a jump trigger has been called for the current jump. */
	private bool jumpTriggerCalled = false;


	/**************
	 * Components *
	 **************/

	/** Layer on which collision will be checked. */
	public LayerMask blockingLayer;
	/** The game objects that represent different directions of the cat. */
	public GameObject rightFace;
	public GameObject leftFace;
	public GameObject upFace;
	public GameObject downFace;
	/** The colliders for each of our directions. */
	private PolygonCollider2D rightCollider;
	private PolygonCollider2D leftCollider;
	private BoxCollider2D upCollider;
	private BoxCollider2D downCollider;
	/** The Animator component attached to this object. */
	private Animator animator;
	/** The Rigidbody2D component attached to this object. */
	private Rigidbody2D rb2D;



	/*******************
	 * Event Functions *
	 *******************/

	/** Initialization. */
	void Start() {
		//Set up components.
		rb2D = GetComponent <Rigidbody2D> ();
		animator = GetComponent <Animator> ();
		rightCollider = rightFace.GetComponent <PolygonCollider2D> ();
		leftCollider = leftFace.GetComponent <PolygonCollider2D> ();
		upCollider = upFace.GetComponent <BoxCollider2D> ();
		downCollider = downFace.GetComponent <BoxCollider2D> ();
		
		//By storing the reciprocal of the move time, 
		//we can use it by multiplying instead of dividing,
		//this is more efficient.
		inverseMoveTime = 1f / moveTime;
	}

	
	/** Update is called every frame.
	 *  Handles the player-controlled movement of the cat. */
	void Update() {			
		//If the player is jumping, animate and exit the function.
		if (!GameManager.canMove) {
			if (!jumpTriggerCalled) {
				AnimateJump ();
				jumpTriggerCalled = true;
			}
			return;
		}

		//Reset jumpTriggerCalled.
		jumpTriggerCalled = false;

		//Get input from the input manager.
		int horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
		int vertical = (int) (Input.GetAxisRaw ("Vertical"));
		
		//Check if moving horizontally, if so set vertical to zero.
		if (horizontal != 0) vertical = 0;

		//Check if we have a non-zero value for horizontal or vertical.
		if (horizontal != 0 || vertical != 0) {
			//Store start position to move from, based on objects current transform position.
			Vector2 start = transform.position;
			
			//Calculate end position based on the input.
			Vector2 end = start + new Vector2 (horizontal, vertical);

			//Disable the current collider so that linecast doesn't hit this object's own collider.
			ToggleCollider (false);

			//Cast a line from start point to end point checking collision on blockingLayer.
			RaycastHit2D hit = Physics2D.Linecast (start, end, blockingLayer);

			//Re-enable the collider after linecasting.
			ToggleCollider (true);

			//Start SmoothMovement co-routine passing in the Vector2 end as destination.
			if (hit.transform == null) {
				StartCoroutine (SmoothMovement (end));
			}

			//Player should not be able to move for a bit after moving.
			GameManager.canMove = false;

			//Change the sprites and colliders of the cat accordingly.
			SetDirection (horizontal, vertical);
		}		
	}


	/** To handle different cases of what object we bump into. */
	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "LifeTaker") {
			Debug.Break ();
		} else if (other.tag == "Hazard") {
			//other.GetComponentInChildren<Animator>().SetTrigger("Splash");
			Destroy (gameObject, 0.1f);
		}
	}



	/********************
	 * Helper Functions *
	 ********************/

	/** Animates the jump. */
	void AnimateJump () {
		switch (direction) {
		case "Right":
			animator.SetTrigger("RightJump");
			break;
		case "Left":
			animator.SetTrigger("LeftJump");
			break;
		case "Up":
			animator.SetTrigger("UpJump");
			break;
		case "Down":
			animator.SetTrigger("DownJump");
			break;
		default:
			break;
		}
	}


	/** Toggles the current collider with VALUE. */
	void ToggleCollider (bool value) {
		switch (direction) {
		case "Right":
			rightCollider.enabled = value;
			break;
		case "Left":
			leftCollider.enabled = value;
			break;
		case "Up":
			upCollider.enabled = value;
			break;
		case "Down":
			downCollider.enabled = value;
			break;
		default:
			break;
		}
	}


	/** Sets the cat's direction animation and colliders to the current direction.
	 *  Also sets the direction to the current one. */
	void SetDirection (int horizontal, int vertical) {
		if (horizontal > 0) {
			upFace.SetActive(false);
			downFace.SetActive(false);
			leftFace.SetActive(false);
			rightFace.SetActive (true);
			direction = "Right";
		} else if (horizontal < 0) {
			upFace.SetActive(false);
			downFace.SetActive(false);
			rightFace.SetActive(false);
			leftFace.SetActive (true);
			direction = "Left";
		} else if (vertical > 0) {
			downFace.SetActive(false);
			rightFace.SetActive(false);
			leftFace.SetActive (false);
			upFace.SetActive(true);
			direction = "Up";
		} else if (vertical < 0) {
			upFace.SetActive(false);
			rightFace.SetActive(false);
			leftFace.SetActive (false);
			downFace.SetActive(true);
			direction = "Down";
		}
	}
	
	
	/** Coroutine for moving the cat from one space to next.
	 *  takes a parameter END to specify where to move to. */
	IEnumerator SmoothMovement (Vector3 end) {
		//Calculate the remaining distance to move based on
		//the square magnitude of the difference between current position and end parameter. 
		//Square magnitude is used instead of magnitude because it's computationally cheaper.
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		
		//While that distance is greater than a very small amount (Epsilon, almost zero):
		while (sqrRemainingDistance > float.Epsilon) {
			//Find a new position proportionally closer to the end, based on the moveTime
			Vector3 newPostion
				= Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			
			//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
			rb2D.MovePosition (newPostion);
			
			//Recalculate the remaining distance after moving.
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			//Return and loop until sqrRemainingDistance is close enough to zero to end the function
			yield return null;
		}
	}


}
