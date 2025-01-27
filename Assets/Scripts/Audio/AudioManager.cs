using UnityEngine;
using System;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;

	public static AudioManager Instance { get; private set; }

	private Dictionary<string, Coroutine> fadeCoroutines = new Dictionary<string, Coroutine>();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
		InitializeSounds();
	}

	private void InitializeSounds()
	{
		foreach (Sound s in sounds)
		{
			s.Initialize(gameObject);
		}
	}

	public void Play(string name, bool isLooping = false)
	{
		Sound sound = GetSound(name);
		if (sound == null) return;

		sound.source.loop = isLooping;
		sound.source.Play();
	}

	public void Play(string name, float volume, float pitch, bool isLooping = false)
	{
		Sound sound = GetSound(name);
		if (sound == null) return;

		sound.source.volume = Mathf.Clamp01(volume);
		sound.source.pitch = pitch;
		sound.source.loop = isLooping;
		sound.source.Play();
	}

	public void Stop(string name)
	{
		Sound sound = GetSound(name);
		if (sound == null) return;

		StopFade(name);
		sound.source.Stop();
	}

	public bool IsPlaying(string name)
	{
		Sound sound = GetSound(name);
		return sound?.source.isPlaying ?? false;
	}

	public void PlayRandom(string[] soundNames)
	{
		if (soundNames == null || soundNames.Length == 0)
		{
			Debug.LogWarning("AudioManager: No sound names provided for random play.");
			return;
		}

		string randomSound = soundNames[UnityEngine.Random.Range(0, soundNames.Length)];
		Play(randomSound);
	}

	public void PlayWithRandomPitch(string name, float minPitch = 0.8f, float maxPitch = 1.2f, bool isLooping = false)
	{
		Sound sound = GetSound(name);
		if (sound == null) return;

		sound.source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
		sound.source.loop = isLooping;
		sound.source.Play();
	}

	#region Audio Fading

	public void FadeIn(string name, float targetVolume, float duration, float startVolume = 0f)
	{
		Sound sound = GetSound(name);
		if (sound == null) return;

		StopFade(name);
		fadeCoroutines[name] = StartCoroutine(FadeCoroutine(sound, startVolume, targetVolume, duration, true));
	}

	public void FadeOut(string name, float duration)
	{
		Sound sound = GetSound(name);
		if (sound == null) return;

		StopFade(name);
		fadeCoroutines[name] = StartCoroutine(FadeCoroutine(sound, sound.source.volume, 0f, duration, false));
	}

	private void StopFade(string name)
	{
		if (fadeCoroutines.TryGetValue(name, out Coroutine coroutine))
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
			}
			fadeCoroutines.Remove(name);
		}
	}

	private IEnumerator FadeCoroutine(Sound sound, float startVolume, float targetVolume, float duration, bool playOnStart)
	{
		if (duration <= 0)
		{
			sound.source.volume = targetVolume;
			if (playOnStart) sound.source.Play();
			else if (targetVolume <= 0) sound.source.Stop();
			yield break;
		}

		sound.source.volume = startVolume;
		if (playOnStart) sound.source.Play();

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			sound.source.volume = Mathf.Lerp(startVolume, targetVolume, t);
			yield return null;
		}

		sound.source.volume = targetVolume;
		if (targetVolume <= 0 && !playOnStart)
		{
			sound.source.Stop();
		}
	}

	#endregion

	private Sound GetSound(string name)
	{
		Sound sound = Array.Find(sounds, s => s.name == name);
		if (sound == null)
		{
			Debug.LogWarning($"AudioManager: Sound '{name}' not found!");
		}
		return sound;
	}
}

