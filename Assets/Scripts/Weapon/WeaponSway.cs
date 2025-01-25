using System;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
	private Vector3 startPos;
	private Vector3 desiredBob;
	private Vector3 recoilOffset;
	private Vector3 recoilRotation;
	private Vector3 speedBob;

	private float desX;
	private float desY;

	public float gunDrag = 0.2f;
	public float currentGunDragMultiplier = 1f;

	public float swaySpeed = 1f;

	public static WeaponSway Instance { get; private set; }

	private static bool isSwayEnabled = true;

	private Vector2 mouseInputAccumulation;
	private float inputSmoothSpeed = 10f;
	private float inputAccumulationDecay = 5f;

	public static void SetSwayEnabled(bool enabled)
	{
		isSwayEnabled = enabled;
	}

	public void ResetPos()
	{
		// Reset all positional and rotational offsets to zero
		recoilOffset = Vector3.zero;
		desiredBob = Vector3.zero;
		speedBob = Vector3.zero;
		recoilRotation = Vector3.zero;

		// Reset the sway effect
		desX = 0f;
		desY = 0f;

		// Reset the gun's position and rotation to zero
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
	}


	private void Start()
	{
		Instance = this;
		startPos = transform.localPosition;
		// Enable sway by default when the component starts
		SetSwayEnabled(true);
	}

	private void Update()
	{
		if (!isSwayEnabled)
		{
			transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * 15f);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 15f);
			return;
		}

		// Remove swaySpeed from input calculation
		Vector2 currentInput = new Vector2(
			-Input.GetAxis("Mouse X") * gunDrag * currentGunDragMultiplier,
			-Input.GetAxis("Mouse Y") * gunDrag * currentGunDragMultiplier
		);

		// Apply swaySpeed to the decay and smoothing
		mouseInputAccumulation += currentInput;
		mouseInputAccumulation = Vector2.Lerp(mouseInputAccumulation, Vector2.zero, Time.deltaTime * inputAccumulationDecay * swaySpeed);

		// Apply swaySpeed to the smoothing as well
		desX = Mathf.Lerp(desX, mouseInputAccumulation.x, Time.deltaTime * inputSmoothSpeed * swaySpeed);
		desY = Mathf.Lerp(desY, mouseInputAccumulation.y, Time.deltaTime * inputSmoothSpeed * swaySpeed);

		Rotation(new Vector2(desX, desY));

		// Calculate target position
		Vector3 targetPosition = startPos + new Vector3(desX, desY, 0f) + desiredBob + recoilOffset + speedBob;

		// Interpolate towards the target position
		transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 15f);
	}

	private void Rotation(Vector2 offset)
	{
		float num = offset.magnitude * 0.03f;
		if (offset.x < 0f)
		{
			num = -num;
		}

		float y = offset.y;
		Vector3 euler = new Vector3(y: (-offset.x) * 40f, x: y * 80f, z: num * 50f) + recoilRotation;

		try
		{
			if (Time.deltaTime > 0f) // Changed to deltaTime
			{
				// Interpolate rotation using deltaTime for frame rate independence
				transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(euler), Time.deltaTime * 20f);
			}
		}
		catch (Exception)
		{
			// Handle exception if necessary
		}
	}
}