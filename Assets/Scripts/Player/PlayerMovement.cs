// PlayerMovement
using System;
using UnityEngine;
using EZCameraShake;

public class PlayerMovement : MonoBehaviour
{
	#region public-variables

	[Header("Assignables")]
	//Assignables
	public Transform playerCam;
	public Transform orientation;
	public Transform ground; // New reference for ground position
	public GameObject playerSmokeFx;
	public GameObject jumpSmokeFx; // New particle effect for jumping
	public GameObject footstepParticleFx; // New field for footstep particles
	public Rigidbody rb;
	public CapsuleCollider playerCollider;
	public GameObject playerModel; // Reference to the actual player model

	[Space(10)]

	public LayerMask whatIsGround;
	public LayerMask whatIsWallrunnable;
	public LayerMask whatIsCeiling; // New layer mask for ceiling checks

	[Header("MovementSettings")]

	//Movement Settings 
	public float sensitivity = 50f;
	public float moveSpeed = 4500f;
	public float walkSpeed = 20f;
	public float runSpeed = 10f;
	public bool grounded;
	public bool onWall;

	#endregion

	#region private-floats

	//Private Floats
	private float wallRunGravity = 1f;
	private float maxSlopeAngle = 35f;
	private float wallRunRotation;
    private float slideSlowdown = 0.2f;
	private float actualWallRotation;
	private float wallRotationVel;
	private float desiredX;
	private float xRotation;
	private float sensMultiplier = 1f;
	private float jumpCooldown = 0.25f;
	private float jumpForce = 550f;
	private float fallSpeed;
	private float playerHeight;
	private float distance;
	private float x;
	private float y;

	[Header("Vault Settings")]
	public float eyeHeight = 1.6f;
	public float vaultCheckDistance = 1.3f;
	public float vaultHeightCheck = 3f;
	public float ceilingCheckHeight = 2f;
	public float vaultForwardBoost = 0.4f;
	public bool showVaultDebug = true;
	public float vaultRadius = 0.3f; // Added for ceiling check radius

	#endregion

	#region private-bools

	//Private bools
	private bool readyToJump;
	private bool jumping;
	private bool sprinting;
    private bool crouching;
	private bool wallRunning;
    private bool cancelling;
	private bool readyToWallrun = true;
	private bool surfing;
	private bool cancellingGrounded;
	private bool cancellingWall;
	private bool cancellingSurf;

	[HideInInspector] public bool isWalking;

	#endregion

	#region private-vectors

	//Private Vector3's
	private Vector3 normalVector;
	private Vector3 wallNormalVector;
	private Vector3 lastMoveSpeed;

	#endregion

	public static PlayerMovement Instance;

	#region Common-functions

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		readyToJump = true;
		wallNormalVector = Vector3.up;
		playerHeight = playerCollider.bounds.size.y;

		CameraBobbing();
	}

	private void FixedUpdate()
	{
		FootSteps();
		FooleySound();
	}

	private void Update()
	{
		MyInput();
		Look();
		WallRunning();
		Movement();

		fallSpeed = rb.linearVelocity.y;
		lastMoveSpeed = XZVector(rb.linearVelocity);
	}

	#endregion

	private void MyInput()
	{
		x = Input.GetAxisRaw("Horizontal");
		y = Input.GetAxisRaw("Vertical");
		jumping = Input.GetButton("Jump");
		crouching = Input.GetKey(KeyCode.LeftControl);

		isWalking = (x != 0 || y != 0) && !jumping && !crouching && grounded;

		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			StartCrouch();
		}
		if (Input.GetKeyUp(KeyCode.LeftControl))
		{
			StopCrouch();
		}
	}

	#region other

	public static Vector3 XZVector(Vector3 v)
	{
		return new Vector3(v.x, 0f, v.z);
	}

	private void FootSteps()
	{
		if (!crouching && (grounded || wallRunning))
		{
			float num = 1.2f;
			float num2 = rb.linearVelocity.magnitude;

			if (num2 > 20f)
			{
				num2 = 20f;
			}

			distance += num2;

			if (distance > 300f / num)
			{
				FootStep.Instance.PlayFootStep();
				
				// Spawn footstep particles at ground level, keeping rotation
				if (footstepParticleFx != null)
				{
					Vector3 particlePosition = new Vector3(
						playerModel.transform.position.x,
						ground.position.y,
						playerModel.transform.position.z
					);
					GameObject footstepParticles = Instantiate(footstepParticleFx, particlePosition, Quaternion.LookRotation(Vector3.up));
					Destroy(footstepParticles, 2f);
				}
				
				distance = 0f;
			}
		}
	}

	void FooleySound()
	{
		if (rb.linearVelocity.magnitude > 55)
		{
			if (!AudioManager.Instance.IsPlaying("fooley"))
			{
				AudioManager.Instance.Play("fooley", rb.linearVelocity.magnitude / 100f, 1, false);
			}
		}
		else
		{
			AudioManager.Instance.Stop("fooley");
		}
	}

	#endregion

	#region Movement

	private void StartCrouch()
	{
		float num = 400f;
		playerModel.transform.localScale = new Vector3(1f, 0.5f, 1f);
		playerModel.transform.position = new Vector3(playerModel.transform.position.x, playerModel.transform.position.y - 0.5f, playerModel.transform.position.z);
		if (rb.linearVelocity.magnitude > 0.1f && grounded)
		{
			rb.AddForce(orientation.transform.forward * num);
		}
	}

	private void StopCrouch()
	{
		playerModel.transform.localScale = new Vector3(1f, 1.5f, 1f);
		playerModel.transform.position = new Vector3(playerModel.transform.position.x, playerModel.transform.position.y + 0.5f, playerModel.transform.position.z);
	}

	private void Jump()
	{
		if ((grounded || wallRunning || surfing) && readyToJump)
		{
			Vector3 velocity = rb.linearVelocity;
			readyToJump = false;

			// Spawn jump particles at ground level, keeping rotation
			Vector3 particlePosition = new Vector3(
				playerModel.transform.position.x,
				ground.position.y,
				playerModel.transform.position.z
			);
			GameObject jumpParticles = Instantiate(jumpSmokeFx, particlePosition, Quaternion.LookRotation(Vector3.up));
			Destroy(jumpParticles, 2f);

			rb.AddForce(Vector2.up * jumpForce * 1.5f);
			rb.AddForce(normalVector * jumpForce * 0.5f);

			if (rb.linearVelocity.y < 0.5f)
			{
				rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
			}
			else if (rb.linearVelocity.y > 0f)
			{
				rb.linearVelocity = new Vector3(velocity.x, velocity.y / 2f, velocity.z);
			}

			if (wallRunning)
			{
				// Wall jump particles code remains unchanged since it already uses LookRotation
				Vector3 wallJumpPosition = new Vector3(
					playerModel.transform.position.x + wallNormalVector.x * 0.5f,
					ground.position.y,
					playerModel.transform.position.z + wallNormalVector.z * 0.5f
				);
				GameObject wallJumpParticles = Instantiate(jumpSmokeFx, wallJumpPosition, Quaternion.LookRotation(wallNormalVector));
				Destroy(wallJumpParticles, 2f);

				rb.AddForce(wallNormalVector * jumpForce * 3f);
			}

			Invoke("ResetJump", jumpCooldown);

			if (wallRunning)
			{
				wallRunning = false;
			}

			AudioManager.Instance.PlayWithRandomPitch("jump", 0.85f, 1.1f, false);
		}
	}

	private void Movement()
	{
		Vector2 magnitude = FindVelRelativeToLook();
		CounterMovement(x, y, magnitude);

		rb.AddForce(Vector3.down * Time.deltaTime * 10f);

		float magnitudeX = magnitude.x;
		float magnitudeY = magnitude.y;
		float forceMultiplierX = 1f;
		float forceMultiplierY = 1f;
		float speed = walkSpeed;

		if (readyToJump && jumping)
		{
			Jump();
		}
		if (sprinting)
		{
			speed = runSpeed;
		}
		if (crouching && grounded && readyToJump)
		{
			rb.AddForce(Vector3.down * Time.deltaTime * 3000f);
			return;
		}

		#region Calculations

		if (x > 0f && magnitudeX > speed)
		{
			x = 0f;
		}
		if (x < 0f && magnitudeX < -speed)
		{
			x = 0f;
		}
		if (y > 0f && magnitudeY > speed)
		{
			y = 0f;
		}
		if (y < 0f && magnitudeY < -speed)
		{
			y = 0f;
		}
		if (!grounded)
		{
			forceMultiplierX = 0.5f;
			forceMultiplierY = 0.5f;
		}
		if (grounded && crouching)
		{
			forceMultiplierY = 0f;
		}
		if (wallRunning)
		{
			forceMultiplierY = 0.3f;
			forceMultiplierX = 0.3f;
		}
		if (surfing)
		{
			forceMultiplierX = 0.7f;
			forceMultiplierY = 0.3f;
		}

		#endregion

		rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * forceMultiplierX * forceMultiplierY);
		rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * forceMultiplierX);
	}


	private void CounterMovement(float inputX, float inputY, Vector2 velocityMagnitude)
	{
		if (!grounded || jumping)
		{
			return;
		}

		float decelerationFactor = 0.16f;
		float threshold = 0.01f;

		if (crouching)
		{
			rb.AddForce(moveSpeed * Time.deltaTime * -rb.linearVelocity.normalized * slideSlowdown);
			return;
		}

		if ((Mathf.Abs(velocityMagnitude.x) > threshold && Mathf.Abs(inputX) < 0.05f) || (velocityMagnitude.x < 0f - threshold && inputX > 0f) || (velocityMagnitude.x > threshold && inputX < 0f))
		{
			rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * (0f - velocityMagnitude.x) * decelerationFactor);
		}

		if ((Mathf.Abs(velocityMagnitude.y) > threshold && Mathf.Abs(inputY) < 0.05f) || (velocityMagnitude.y < 0f - threshold && inputY > 0f) || (velocityMagnitude.y > threshold && inputY < 0f))
		{
			rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * (0f - velocityMagnitude.y) * decelerationFactor);
		}

		if (Mathf.Sqrt(Mathf.Pow(rb.linearVelocity.x, 2f) + Mathf.Pow(rb.linearVelocity.z, 2f)) > walkSpeed)
		{
			float currentYVelocity = rb.linearVelocity.y;
			Vector3 horizontalVelocity = rb.linearVelocity.normalized * walkSpeed;
			rb.linearVelocity = new Vector3(horizontalVelocity.x, currentYVelocity, horizontalVelocity.z);
		}
	}

	#endregion

	#region Camera
	private void Look()
	{
		float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
		float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

		desiredX = playerCam.transform.localRotation.eulerAngles.y + mouseX;
		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		FindWallRunRotation();

		actualWallRotation = Mathf.SmoothDamp(actualWallRotation, wallRunRotation, ref wallRotationVel, 0.2f);

		playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, actualWallRotation);
		orientation.transform.localRotation = Quaternion.Euler(0f, desiredX, 0f);
	}

	private void CameraBobbing()
	{
		float amount = rb.linearVelocity.magnitude / 9f;
		CameraShaker.Instance.ShakeOnce(amount, 0.1f * amount, 0.25f, 0.2f);

		Invoke("CameraBobbing", 0.2f);
	}

	#endregion

	#region WallRuning

	public Vector2 FindVelRelativeToLook()
	{
		float currentRotation = orientation.transform.eulerAngles.y;
		float targetRotation = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;
		float rotationDifference = Mathf.DeltaAngle(currentRotation, targetRotation);
		float complementaryAngle = 90f - rotationDifference;
		float velocityMagnitude = rb.linearVelocity.magnitude;

		float velocityY = velocityMagnitude * Mathf.Cos(rotationDifference * Mathf.Deg2Rad);
		float velocityX = velocityMagnitude * Mathf.Cos(complementaryAngle * Mathf.Deg2Rad);

		return new Vector2(velocityX, velocityY);
	}


	private void FindWallRunRotation()
	{
		float currentRotation = playerCam.transform.rotation.eulerAngles.y;
		float targetRotation = Vector3.SignedAngle(Vector3.forward, wallNormalVector, Vector3.up);
		float rotationDifference = Mathf.DeltaAngle(currentRotation, targetRotation);

		bool isCloseToWallRotation = Mathf.Abs(wallRunRotation) < 4f && y > 0f && Mathf.Abs(x) < 0.1f;
		bool isFarFromWallRotation = Mathf.Abs(wallRunRotation) > 22f && y < 0f && Mathf.Abs(x) < 0.1f;

		wallRunRotation = -(rotationDifference / 90f) * 15f;

		if (!wallRunning)
		{
			wallRunRotation = 0f;
			return;
		}

		if (!readyToWallrun)
		{
			return;
		}

		if (isCloseToWallRotation || isFarFromWallRotation)
		{
			if (!cancelling)
			{
				cancelling = true;
				CancelInvoke("CancelWallrun");
				Invoke("CancelWallrun", 0.2f);
			}
		}
		else
		{
			cancelling = false;
			CancelInvoke("CancelWallrun");
		}
	}



	private void CancelWallrun()
	{
		Invoke("GetReadyToWallrun", 0.1f);

		rb.AddForce(wallNormalVector * 600f);

		readyToWallrun = false;

		AudioManager.Instance.Play("land", false);
	}

	private void GetReadyToWallrun()
	{
		readyToWallrun = true;
	}

	private void WallRunning()
	{
		if (wallRunning)
		{
			rb.AddForce(-wallNormalVector * Time.deltaTime * moveSpeed);
			rb.AddForce(Vector3.up * Time.deltaTime * rb.mass * 100f * wallRunGravity);
		}
	}

	private void StartWallRun(Vector3 normal)
	{
		if (!grounded && readyToWallrun)
		{
			wallNormalVector = normal;
			float num = 20f;
			if (!wallRunning)
			{
				rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
				rb.AddForce(Vector3.up * num, ForceMode.Impulse);
			}
			wallRunning = true;
		}
	}
	#endregion

	#region Collision-Detection

	private void OnCollisionEnter(Collision other)
	{
		Vector3 normal = other.contacts[0].normal;

		if (IsFloor(normal))
		{
			if(!crouching)
			{
				MoveCamera.instance.BobOnce(new Vector3(0f, fallSpeed, 0f));
			}
			else
			{
				MoveCamera.instance.BobOnce(new Vector3(0f, fallSpeed / 2, 0f));
			}

			if (fallSpeed < -15f)
			{
				// Spawn landing particles at ground level, keeping rotation
				Vector3 particlePosition = new Vector3(
					playerModel.transform.position.x,
					ground.position.y,
					playerModel.transform.position.z
				);
				GameObject playerSmoke = Instantiate(playerSmokeFx, particlePosition, Quaternion.LookRotation(transform.position - particlePosition));
				Destroy(playerSmoke, 2f);
			}
		}

		if (IsWall(normal))
		{
			TryVault(normal);
		}
	}

	private void OnCollisionStay(Collision other)
	{
		int objectLayer = other.gameObject.layer;

		float cancellationDelay = 3f;


		if ((int)whatIsGround != ((int)whatIsGround | (1 << objectLayer)))
		{
			return;
		}

		for (int i = 0; i < other.contactCount; i++)
		{
			Vector3 contactNormal = other.contacts[i].normal;

			if (IsFloor(contactNormal))
			{
				if (wallRunning)
				{
					wallRunning = false;
				}
				if (!grounded && crouching)
				{
					AudioManager.Instance.Play("start slide", false);
					AudioManager.Instance.Play("slide", false);
				}

				grounded = true;
				normalVector = contactNormal;
				cancellingGrounded = false;
				CancelInvoke("StopGrounded");
			}

			if (IsWall(contactNormal) && objectLayer == LayerMask.NameToLayer("Ground"))
			{
				if (!onWall)
				{
					AudioManager.Instance.Play("start slide", false);
					AudioManager.Instance.Play("slide", false);
				}

				StartWallRun(contactNormal);
				onWall = true;
				cancellingWall = false;
				CancelInvoke("StopWall");
			}

			if (IsSurf(contactNormal))
			{
				surfing = true;
				cancellingSurf = false;
				CancelInvoke("StopSurf");
			}

			IsRoof(contactNormal);
		}

		if (!cancellingGrounded)
		{
			cancellingGrounded = true;
			Invoke("StopGrounded", Time.deltaTime * cancellationDelay);
		}

		if (!cancellingWall)
		{
			cancellingWall = true;
			Invoke("StopWall", Time.deltaTime * cancellationDelay);
		}

		if (!cancellingSurf)
		{
			cancellingSurf = true;
			Invoke("StopSurf", Time.deltaTime * cancellationDelay);
		}
	}

	private void TryVault(Vector3 wallNormal)
	{
		Vector3 moveDirection = lastMoveSpeed.normalized;
		Vector3 rayStart = playerModel.transform.position + Vector3.up * eyeHeight;

		// Debug all raycasts
		if (showVaultDebug)
		{
			// Forward check (Blue)
			Debug.DrawLine(rayStart, rayStart + moveDirection * vaultCheckDistance, Color.blue, 0.5f);
			// Ground check (Green)
			Debug.DrawLine(rayStart + moveDirection * vaultCheckDistance, 
						  rayStart + moveDirection * vaultCheckDistance + Vector3.down * vaultHeightCheck, 
						  Color.green, 0.5f);
			// Ceiling check (Red)
			Debug.DrawLine(rayStart, rayStart + Vector3.up * ceilingCheckHeight, Color.red, 0.5f);
		}

		// Forward check
		if (Physics.Raycast(rayStart, moveDirection, vaultCheckDistance, whatIsGround))
		{
			return;
		}

		// Ground check
		RaycastHit groundHit;
		Vector3 vaultCheckPosition = rayStart + moveDirection * vaultCheckDistance;
		if (!Physics.Raycast(vaultCheckPosition, Vector3.down, out groundHit, vaultHeightCheck, whatIsGround))
		{
			return;
		}

		// Simple ceiling check using SphereCast
		if (Physics.SphereCast(rayStart, vaultRadius, Vector3.up, out RaycastHit ceilingHit, ceilingCheckHeight, whatIsCeiling))
		{
			if (showVaultDebug)
			{
				// Show where we hit the ceiling
				Debug.DrawLine(ceilingHit.point, ceilingHit.point + Vector3.up * 0.5f, Color.red, 2f);
				Debug.DrawLine(ceilingHit.point + Vector3.right * vaultRadius, ceilingHit.point - Vector3.right * vaultRadius, Color.red, 2f);
				Debug.DrawLine(ceilingHit.point + Vector3.forward * vaultRadius, ceilingHit.point - Vector3.forward * vaultRadius, Color.red, 2f);
			}
			return;
		}

		// Calculate vault destination
		Vector3 vaultDestination = groundHit.point + Vector3.up * playerHeight * 0.5f;

		// Perform vault
		MoveCamera.instance.vaultOffset += playerModel.transform.position - vaultDestination;
		playerModel.transform.position = vaultDestination;
		rb.linearVelocity = lastMoveSpeed * vaultForwardBoost;
	}
	#endregion

	#region Getter-Functions

	private bool IsFloor(Vector3 v)
	{
		return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
	}

	private bool IsSurf(Vector3 v)
	{
		float num = Vector3.Angle(Vector3.up, v);
		if (num < 89f)
		{
			return num > maxSlopeAngle;
		}
		return false;
	}

	private bool IsWall(Vector3 v)
	{
		return Math.Abs(90f - Vector3.Angle(Vector3.up, v)) < 0.1f;
	}

	private bool IsRoof(Vector3 v)
	{
		return v.y == -1f;
	}

	private void ResetJump()
	{
		readyToJump = true;
	}

	private void StopGrounded()
	{
		grounded = false;
	}

	private void StopWall()
	{
		onWall = false;
		wallRunning = false;
	}

	private void StopSurf()
	{
		surfing = false;
	}

	public Vector3 GetVelocity()
	{
		return rb.linearVelocity;
	}

	public float GetFallSpeed()
	{
		return rb.linearVelocity.y;
	}

	public Collider GetPlayerCollider()
	{
		return playerCollider;
	}

	public Transform GetPlayerCamTransform()
	{
		return playerCam.transform;
	}

	public bool IsCrouching()
	{
		return crouching;
	}

	public Rigidbody GetRb()
	{
		return rb;
	}
	#endregion

	private void OnDrawGizmos()
	{
		if (!showVaultDebug || playerModel == null) return;

		Vector3 moveDir = Application.isPlaying ? lastMoveSpeed.normalized : playerModel.transform.forward;
		Vector3 rayStart = playerModel.transform.position + Vector3.up * eyeHeight;
		Vector3 vaultCheckPosition = rayStart + moveDir * vaultCheckDistance;

		// Forward check visualization (Blue)
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(rayStart, 0.1f);
		Gizmos.DrawLine(rayStart, vaultCheckPosition);
		Gizmos.DrawWireSphere(vaultCheckPosition, 0.1f);

		// Ground check visualization (Green)
		Gizmos.color = Color.green;
		Vector3 groundCheckEnd = vaultCheckPosition + Vector3.down * vaultHeightCheck;
		Gizmos.DrawLine(vaultCheckPosition, groundCheckEnd);
		Gizmos.DrawWireSphere(groundCheckEnd, 0.1f);

		// Ceiling check visualization (Red)
		Gizmos.color = Color.red;
		Vector3 ceilingCheckEnd = rayStart + Vector3.up * ceilingCheckHeight;
		
		// Draw the ceiling check volume
		float segments = 8;
		float angleStep = 360f / segments;
		
		// Draw the bottom circle
		for (int i = 0; i < segments; i++)
		{
			float angle = i * angleStep * Mathf.Deg2Rad;
			float nextAngle = ((i + 1) % segments) * angleStep * Mathf.Deg2Rad;
			
			Vector3 start = rayStart + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * vaultRadius;
			Vector3 end = rayStart + new Vector3(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle)) * vaultRadius;
			
			Gizmos.DrawLine(start, end);
		}
		
		// Draw the top circle
		for (int i = 0; i < segments; i++)
		{
			float angle = i * angleStep * Mathf.Deg2Rad;
			float nextAngle = ((i + 1) % segments) * angleStep * Mathf.Deg2Rad;
			
			Vector3 start = ceilingCheckEnd + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * vaultRadius;
			Vector3 end = ceilingCheckEnd + new Vector3(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle)) * vaultRadius;
			
			Gizmos.DrawLine(start, end);
		}
		
		// Draw the connecting lines
		for (int i = 0; i < segments; i++)
		{
			float angle = i * angleStep * Mathf.Deg2Rad;
			Vector3 bottom = rayStart + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * vaultRadius;
			Vector3 top = ceilingCheckEnd + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * vaultRadius;
			
			Gizmos.DrawLine(bottom, top);
		}
	}
}
