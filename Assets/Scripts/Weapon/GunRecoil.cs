using UnityEngine;
using EZCameraShake;

public class GunRecoil : MonoBehaviour
{
	private Vector3 recoilOffset;
	private Vector3 fixedRecoilRotation;
	private Vector3 randomRecoilRotation;
	private Vector3 targetFixedRotation;
	private Vector3 targetRandomRotation;
	private bool isReturning;

	[Header("Recoil Position Settings")]
	[Tooltip("How far back the gun moves when firing")]
	public float recoilKickback = 0.1f;
	[Tooltip("How much the gun moves up when firing")]
	public float recoilVerticalOffset = 0.05f;
	[Tooltip("How much the gun moves sideways when firing")]
	public float recoilHorizontalOffset = 0.02f;
	[Tooltip("Time in seconds for position to return to center")]
	public float positionReturnTime = 0.2f;

	[Header("Fixed Recoil Rotation")]
	[Tooltip("Fixed upward rotation when firing (pitch)")]
	public float fixedRotationX = 5f;
	[Tooltip("Fixed sideways rotation when firing (yaw)")]
	public float fixedRotationY = 2f;
	[Tooltip("Fixed tilt when firing (roll)")]
	public float fixedRotationZ = 1f;
	[Tooltip("Time in seconds to reach maximum fixed recoil")]
	public float fixedRotationSnapTime = 0.05f;
	[Tooltip("Time in seconds for fixed rotation to return to center")]
	public float fixedRotationReturnTime = 0.15f;

	[Header("Random Rotation Variance")]
	[Tooltip("Random rotation variance on X axis (pitch)")]
	public float randomRotationX = 2f;
	[Tooltip("Random rotation variance on Y axis (yaw)")]
	public float randomRotationY = 2f;
	[Tooltip("Random rotation variance on Z axis (roll)")]
	public float randomRotationZ = 3f;
	[Tooltip("Time in seconds to reach random recoil position")]
	public float randomRotationSnapTime = 0.02f;
	[Tooltip("Time in seconds for random rotation to return to center")]
	public float randomRotationReturnTime = 0.1f;

	[Header("Camera Shake Settings")]
	[Tooltip("Enable camera shake effect")]
	public bool enableCameraShake = true;
	[Tooltip("How intense the camera shake is")]
	public float cameraShakeIntensity = 0.2f;
	[Tooltip("How long the camera shake lasts in seconds")]
	public float cameraShakeDuration = 0.1f;
	[Tooltip("How quickly the camera shake fades out in seconds")]
	public float cameraShakeFadeout = 0.5f;

	private void Start()
	{
		// Ensure we have access to the camera shaker
		if (enableCameraShake && CameraShaker.Instance == null)
		{
			Debug.LogWarning("Camera Shaker not found in scene. Camera shake will be disabled.");
			enableCameraShake = false;
		}
	}

	public void Shoot(float recoilMultiplier)
	{
		// Apply position recoil
		Vector3 recoilDir = new Vector3(
			Random.Range(-recoilHorizontalOffset, recoilHorizontalOffset),
			recoilVerticalOffset,
			-recoilKickback
		) * recoilMultiplier;

		recoilOffset += recoilDir;

		// Set target for fixed rotation recoil
		Vector3 fixedRecoil = new Vector3(
			-fixedRotationX, // Negative for upward rotation
			fixedRotationY,
			fixedRotationZ
		) * recoilMultiplier;

		// Set target for random rotation variance
		Vector3 randomRecoil = new Vector3(
			Random.Range(-randomRotationX, randomRotationX),
			Random.Range(-randomRotationY, randomRotationY),
			Random.Range(-randomRotationZ, randomRotationZ)
		) * recoilMultiplier;

		// Update target rotations
		targetFixedRotation += fixedRecoil;
		targetRandomRotation += randomRecoil;
		isReturning = false;

		// Apply camera shake
		if (enableCameraShake && CameraShaker.Instance != null)
		{
			float shakeAmount = cameraShakeIntensity * recoilMultiplier;
			CameraShaker.Instance.ShakeOnce(shakeAmount, cameraShakeFadeout, cameraShakeDuration, cameraShakeDuration);
		}
	}

	private void Update()
	{
		ApplyRecoilEffects();
	}

	private void ApplyRecoilEffects()
	{
		float deltaTime = Time.deltaTime;

		// Calculate speeds based on desired time to reach target
		float positionReturnSpeed = deltaTime / positionReturnTime;
		float fixedRotationSnapSpeed = deltaTime / fixedRotationSnapTime;
		float fixedRotationReturnSpeed = deltaTime / fixedRotationReturnTime;
		float randomRotationSnapSpeed = deltaTime / randomRotationSnapTime;
		float randomRotationReturnSpeed = deltaTime / randomRotationReturnTime;

		// Smoothly return position to origin
		recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, positionReturnSpeed);

		// Move fixed rotation towards target or back to zero
		if (!isReturning)
		{
			fixedRecoilRotation = Vector3.Lerp(fixedRecoilRotation, targetFixedRotation, fixedRotationSnapSpeed);
			randomRecoilRotation = Vector3.Lerp(randomRecoilRotation, targetRandomRotation, randomRotationSnapSpeed);
			isReturning = true;
		}
		else
		{
			targetFixedRotation = Vector3.Lerp(targetFixedRotation, Vector3.zero, fixedRotationReturnSpeed);
			targetRandomRotation = Vector3.Lerp(targetRandomRotation, Vector3.zero, randomRotationReturnSpeed);
			fixedRecoilRotation = Vector3.Lerp(fixedRecoilRotation, targetFixedRotation, fixedRotationSnapSpeed);
			randomRecoilRotation = Vector3.Lerp(randomRecoilRotation, targetRandomRotation, randomRotationSnapSpeed);
		}

		// Apply the combined recoil effects
		transform.localPosition = recoilOffset;
		transform.localRotation = Quaternion.Euler(fixedRecoilRotation + randomRecoilRotation);
	}
}
