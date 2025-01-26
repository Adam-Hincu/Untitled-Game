using UnityEngine;
using EZCameraShake;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
