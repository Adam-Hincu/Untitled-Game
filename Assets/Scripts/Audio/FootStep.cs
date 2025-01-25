using UnityEngine;

public class FootStep : MonoBehaviour
{
	public AudioClip[] footSteps;
	public AudioClip[] wallSteps;
	public float volume;
	private AudioSource audioSource;

	public static FootStep Instance;


	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.volume = volume;
	}

	public void PlayFootStep()
	{
		if(!PlayerMovement.Instance.onWall)
		{
			if (footSteps.Length == 0)
			{
				Debug.LogError("No sound clips assigned!");
				return;
			}

			int randomIndex = Random.Range(0, footSteps.Length);
			AudioClip clip = footSteps[randomIndex];
			audioSource.clip = clip;

			audioSource.Play();
		}
		else
		{
			PlayWallStep();
		}
	}

	public void PlayWallStep()
	{
		if (wallSteps.Length == 0)
		{
			Debug.LogError("No sound clips assigned!");
			return;
		}

		int randomIndex = Random.Range(0, wallSteps.Length);
		AudioClip clip = wallSteps[randomIndex];
		audioSource.clip = clip;

		audioSource.Play();
	}
}
