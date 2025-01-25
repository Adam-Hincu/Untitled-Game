// MoveCamera
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
	public Transform player;

	private Vector3 offset;

	private Vector3 desyncOffset;

	[HideInInspector] public Vector3 vaultOffset;

	private Vector3 desiredBob;

	private Vector3 bobOffset;

	private float bobSpeed = 15f;

	private float bobMultiplier = 1f;

	public Camera mainCam;

	public static MoveCamera instance;

	private void Start()
	{
		instance = this;
		offset = base.transform.position - player.transform.position;
	}

	private void Update()
	{
		UpdateBob();

		base.transform.position = player.transform.position + bobOffset + desyncOffset + vaultOffset + offset;

		vaultOffset = Vector3.Slerp(vaultOffset, Vector3.zero, Time.deltaTime * 7f);
	}

	public void BobOnce(Vector3 bobDirection)
	{
		Vector3 vector = ClampVector(bobDirection * 0.15f, -3f, 3f);
		desiredBob = vector * bobMultiplier;
	}

	private void UpdateBob()
	{
		desiredBob = Vector3.Lerp(desiredBob, Vector3.zero, Time.deltaTime * bobSpeed * 0.5f);
		bobOffset = Vector3.Lerp(bobOffset, desiredBob, Time.deltaTime * bobSpeed);
	}

	private Vector3 ClampVector(Vector3 vec, float min, float max)
	{
		return new Vector3(Mathf.Clamp(vec.x, min, max), Mathf.Clamp(vec.y, min, max), Mathf.Clamp(vec.z, min, max));
	}
}
