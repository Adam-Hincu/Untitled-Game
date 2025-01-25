using System.Collections.Generic;
using UnityEngine;

public class GunBobbing : MonoBehaviour
{
	private Rigidbody rb;
	private Vector3 startPos;
	private List<Vector3> velHistory;
	private Vector3 desiredBob;
	private float xBob = 0.12f;
	private float yBob = 0.08f;
	private float zBob = 0.1f;
	private float bobSpeed = 0.45f;
	public float intensityModifier = 0.75f;

	public PlayerMovement playerMovement;

	private Vector3 currentVelocity = Vector3.zero;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		startPos = transform.localPosition;
		velHistory = new List<Vector3>();

	}

	private void Update()
	{
		bool isWalking = false;

		if (playerMovement != null)
		{
			isWalking = playerMovement.isWalking;
		}

		if (isWalking)
		{
			MovementBob();
		}
		else
		{
			SmoothStop();
		}
	}

	private void MovementBob()
	{
		if (rb && Mathf.Abs(rb.linearVelocity.magnitude) >= 4f)
		{
			desiredBob = Vector3.zero;
			return;
		}

		float x = Mathf.PingPong(Time.time * bobSpeed, xBob) - xBob / 2f;
		float y = Mathf.PingPong(Time.time * bobSpeed, yBob) - yBob / 2f;
		float z = Mathf.PingPong(Time.time * bobSpeed, zBob) - zBob / 2f;
		desiredBob = new Vector3(x, y, z) * intensityModifier;
		Vector3 bobPosition = startPos + desiredBob;
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, bobPosition, ref currentVelocity, 0.1f);
	}

	private void SmoothStop()
	{
		Vector3 bobPosition = startPos;
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, bobPosition, ref currentVelocity, 0.1f);
	}
}
