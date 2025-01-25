using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;

	public static AudioManager Instance;

	private void Awake()
	{
		Instance = this;

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;

			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.isLooping;
		}
	}

	public void Play(string name, bool isLooping)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);

		s.source.loop = isLooping;
		s.source.Play();
	}

	public void Play(string name, float volume, float pitch, bool isLooping)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);

		s.source.volume = volume;
		s.source.pitch = pitch;
		s.source.loop = isLooping;

		s.source.Play();
	}

	public void Stop(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		s.source.Stop();
	}

	public bool IsPlaying(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		return s.source.isPlaying;
	}

	public void PlayRandom(string[] soundNames)
	{
		if (soundNames.Length == 0)
		{
			Debug.LogWarning("No sound names assigned.");
			return;
		}

		int randomIndex = UnityEngine.Random.Range(0, soundNames.Length);
		string randomSoundName = soundNames[randomIndex];

		Play(randomSoundName, false);
	}

	public void PlayWithRandomPitch(string name, bool isLooping)
	{
		PlayWithRandomPitch(name, 0.8f, 1.2f, isLooping);
	}

	public void PlayWithRandomPitch(string name, float minPitch, float maxPitch, bool isLooping)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		
		s.source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
		s.source.loop = isLooping;
		
		s.source.Play();
	}
}
