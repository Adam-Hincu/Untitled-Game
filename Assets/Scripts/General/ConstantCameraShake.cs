using UnityEngine;
using EZCameraShake;
using System.Collections;

public class ConstantCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("How intense the shake is")]
    [SerializeField] private float magnitude = 2f;
    [Tooltip("How rough or smooth the shake is")]
    [SerializeField] private float roughness = 2f;
    [Tooltip("Time to reach full shake intensity")]
    [SerializeField] private float fadeInTime = 0.1f;
    [Tooltip("Time to fade out the shake")]
    [SerializeField] private float fadeOutTime = 0.1f;

    [Header("Influence Settings")]
    [Tooltip("How much shake affects position (X,Y,Z)")]
    [SerializeField] private Vector3 positionInfluence = new Vector3(0.15f, 0.15f, 0f);
    [Tooltip("How much shake affects rotation (X,Y,Z)")]
    [SerializeField] private Vector3 rotationInfluence = new Vector3(1f, 1f, 1f);

    [Header("Runtime Settings")]
    [Tooltip("Start shaking automatically on enable")]
    [SerializeField] private bool shakeOnStart = true;

    private CameraShakeInstance shakeInstance;
    private float currentMagnitude = 0f;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (shakeOnStart)
        {
            StartShaking();
        }
    }

    private void OnDisable()
    {
        StopShaking();
    }

    /// <summary>
    /// Starts the camera shake with current settings
    /// </summary>
    public void StartShaking()
    {
        // Stop any existing shake first
        StopShaking();

        // Start the continuous camera shake
        shakeInstance = CameraShaker.Instance.StartShake(magnitude, roughness, fadeInTime);
        
        // Set the position and rotation influence
        CameraShaker.Instance.DefaultPosInfluence = positionInfluence;
        CameraShaker.Instance.DefaultRotInfluence = rotationInfluence;
    }

    /// <summary>
    /// Stops the current camera shake with a fade out
    /// </summary>
    public void StopShaking()
    {
        if (shakeInstance != null)
        {
            shakeInstance.StartFadeOut(fadeOutTime);
            shakeInstance = null;
        }
    }

    private IEnumerator FadeInShake(float duration)
    {
        float elapsedTime = 0f;
        float startMagnitude = currentMagnitude;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentMagnitude = Mathf.Lerp(startMagnitude, magnitude, elapsedTime / duration);
            
            if (shakeInstance != null)
            {
                shakeInstance.Magnitude = currentMagnitude;
            }
            
            yield return null;
        }
        
        currentMagnitude = magnitude;
        if (shakeInstance != null)
        {
            shakeInstance.Magnitude = magnitude;
        }
    }

    private IEnumerator FadeOutShake(float duration)
    {
        float elapsedTime = 0f;
        float startMagnitude = currentMagnitude;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentMagnitude = Mathf.Lerp(startMagnitude, 0f, elapsedTime / duration);
            
            if (shakeInstance != null)
            {
                shakeInstance.Magnitude = currentMagnitude;
            }
            
            yield return null;
        }
        
        currentMagnitude = 0f;
        StopShaking();
    }

    public void StartShakingWithFade(float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Start the continuous camera shake if not already started
        if (shakeInstance == null)
        {
            shakeInstance = CameraShaker.Instance.StartShake(0f, roughness, 0f);
            CameraShaker.Instance.DefaultPosInfluence = positionInfluence;
            CameraShaker.Instance.DefaultRotInfluence = rotationInfluence;
        }

        fadeCoroutine = StartCoroutine(FadeInShake(fadeDuration));
    }

    public void StopShakingWithFade(float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (shakeInstance != null)
        {
            fadeCoroutine = StartCoroutine(FadeOutShake(fadeDuration));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
