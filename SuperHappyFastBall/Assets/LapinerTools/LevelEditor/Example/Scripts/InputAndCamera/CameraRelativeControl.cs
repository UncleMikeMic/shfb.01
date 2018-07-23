using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class CameraRelativeControl : MonoBehaviour
{
	public Joystick moveJoystick;
	public Joystick rotateJoystick;
	
	public Transform cameraPivot;						// The transform used for camera rotation
	public Transform cameraTransform;					// The actual transform of the camera
	
	public float speed = 5f;							// Ground speed
	public float jumpSpeed = 8f;
	public float inAirMultiplier = 0.25f; 				// Limiter for ground speed while jumping
	public Vector2 rotationSpeed = new Vector2(50, 25);	// Camera rotation speed for each axis

	private CharacterController character;
	private Vector3 velocity;							// Used for continuing momentum while in air
	private bool canJump = true;

	private void Start()
	{
		// Cache component lookup at startup instead of doing this every frame
		character = GetComponent<CharacterController>();	
		
		// Move the character to the correct start position in the level, if one exists
		GameObject spawn = GameObject.Find("PlayerSpawn");
		if (spawn != null)
			transform.position = spawn.transform.position;
	}
	
	private void Update()
	{
		Vector3 movement = cameraTransform.TransformDirection(new Vector3( moveJoystick.position.x, 0, moveJoystick.position.y ) );
		// We only want the camera-space horizontal direction
		movement.y = 0;
		movement.Normalize(); // Adjust magnitude after ignoring vertical movement
		
		// Let's use the largest component of the joystick position for the speed.
		var absJoyPos = new Vector2( Mathf.Abs( moveJoystick.position.x ), Mathf.Abs( moveJoystick.position.y ) );
		movement *= speed * ( ( absJoyPos.x > absJoyPos.y ) ? absJoyPos.x : absJoyPos.y );
		
		// Check for jump
		if ( character.isGrounded )
		{
			if ( !rotateJoystick.IsFingerDown() )
				canJump = true;
			
			if ( canJump && rotateJoystick.tapCount == 2 )
			{
				// Apply the current movement to launch velocity
				velocity = character.velocity;
				velocity.y = jumpSpeed;
				canJump = false;
			}
		}
		else
		{			
			// Apply gravity to our velocity to diminish it over time
			velocity.y += Physics.gravity.y * Time.deltaTime;
			
			// Adjust additional movement while in-air
			movement.x *= inAirMultiplier;
			movement.z *= inAirMultiplier;
		}
		
		movement += velocity;
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		
		// Actually move the character
		if (character.enabled)
		{
			character.Move( movement );
		}
		
		if ( character.isGrounded )
			// Remove any persistent velocity after landing
			velocity = Vector3.zero;
		
		// Face the character to match with where she is moving
		FaceMovementDirection();	
		
		// Scale joystick input with rotation speed
		Vector2 camRotation = rotateJoystick.position;
		camRotation.x *= rotationSpeed.x;
		camRotation.y *= rotationSpeed.y;
		camRotation *= Time.deltaTime;
		
		// Rotate around the character horizontally in world, but use local space
		// for vertical rotation
		cameraPivot.Rotate( 0, camRotation.x, 0, Space.World );
		cameraPivot.Rotate( camRotation.y, 0, 0 );
	}

	private void FaceMovementDirection()
	{	
		Vector3 horizontalVelocity = character.velocity;
		horizontalVelocity.y = 0; // Ignore vertical movement
		
		// If moving significantly in a new direction, point that character in that direction
		if (horizontalVelocity.magnitude > 0.1)
			transform.forward = horizontalVelocity.normalized;
	}
}
