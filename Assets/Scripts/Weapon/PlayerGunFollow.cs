using UnityEngine;

public class PlayerGunFollow : MonoBehaviour
{
	public Rigidbody playerRigidbody; // Reference to the player's Rigidbody
	public Transform cameraTransform; // Reference to the camera's Transform
	public float followSpeed = 0.1f;  // Speed at which the gun follows inverse movement
	public Vector3 resetPosition = Vector3.zero; // Reset position for the gun
	public float xyzMultiplier = 1.0f; // Multiplier for X and Z axes
	public float yMultiplier = 1.0f; // Multiplier for Y axis

	private Vector3 lastPlayerVelocity;

	void Start()
	{
		if (playerRigidbody == null)
		{
			Debug.LogError("Player Rigidbody not assigned!");
		}

		if (cameraTransform == null)
		{
			Debug.LogError("Camera Transform not assigned!");
		}

		// Initialize with player's current velocity
		lastPlayerVelocity = playerRigidbody.linearVelocity;
	}

	void Update()
	{
		// Get the current velocity of the player
		Vector3 playerVelocity = playerRigidbody.linearVelocity;

		// Check if the player's velocity is zero
		if (playerVelocity == Vector3.zero)
		{
			// Reset the gun's position to the initial position (0,0,0)
			transform.localPosition = resetPosition;
		}
		else
		{
			// Inverse the player's velocity
			Vector3 inverseVelocity = -playerVelocity;

			// Apply separate multipliers for Y and XZ axes
			inverseVelocity.x *= xyzMultiplier;
			inverseVelocity.z *= xyzMultiplier;
			inverseVelocity.y *= yMultiplier;

			// Convert inverse velocity to the camera's local space
			Vector3 localInverseVelocity = cameraTransform.InverseTransformDirection(inverseVelocity);

			// Smoothly move the gun based on the local inverse velocity
			transform.localPosition = Vector3.Lerp(transform.localPosition, localInverseVelocity, followSpeed * Time.deltaTime);
		}

		// Store the last velocity for future use (if needed)
		lastPlayerVelocity = playerVelocity;
	}
}
