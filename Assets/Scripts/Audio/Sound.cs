using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
	[Header("Sound Identity")]
	public string name;

	public AudioClip clip;

	[Header("Audio Properties")]
	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(0.1f, 3f)]
	public float pitch = 1f;

	public bool isLooping = false;

	[Header("Optional")]
	public AudioMixerGroup mixerGroup;

	[HideInInspector]
	public AudioSource source;

	public void Initialize(GameObject gameObject)
	{
		source = gameObject.AddComponent<AudioSource>();
		source.clip = clip;
		source.volume = volume;
		source.pitch = pitch;
		source.loop = isLooping;
		
		if (mixerGroup != null)
		{
			source.outputAudioMixerGroup = mixerGroup;
		}
	}
}
