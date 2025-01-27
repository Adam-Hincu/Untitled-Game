using UnityEngine;
using System.Collections;

public class TrainSceneManager : MonoBehaviour
{
    [System.Serializable]
    private class TrainAudioSettings
    {
        [Header("Engine Sound")]
        public string engineSoundName;
        [Range(0f, 1f)]
        public float engineVolume = 1.0f;

        [Header("Wheel Sounds")]
        public string wheelSound1Name;
        [Range(0f, 1f)]
        public float wheelSound1Volume = 1.0f;
        
        public string wheelSound2Name;
        [Range(0f, 1f)]
        public float wheelSound2Volume = 1.0f;
    }

    [System.Serializable]
    private class TrainEffectsSettings
    {
        [Header("Particle Systems")]
        public ParticleSystem smokeSystem;
        public ParticleSystem sparkSystem;

        [Header("Ground Effect")]
        public ScrollingGround scrollingGround;
    }

    [Header("Train Configuration")]
    [SerializeField] private TrainAudioSettings audioSettings;
    [SerializeField] private TrainEffectsSettings effectsSettings;
    
    [Header("General Settings")]
    [SerializeField] private bool startTrainOnStart = false;
    [SerializeField] private float transitionDuration = 1.0f;

    private float initialScrollSpeed;
    private float[] initialEmissionRates;
    private bool isTrainRunning;
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        InitializeEffects();
    }

    private void Start()
    {
        if (startTrainOnStart)
        {
            StartTrain();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !isTrainRunning)
        {
            StartTrain();
        }
        else if (Input.GetKeyDown(KeyCode.N) && isTrainRunning)
        {
            StopTrain();
        }
    }

    private void InitializeEffects()
    {
        // Initialize particle systems
        particleSystems = new ParticleSystem[] 
        {
            effectsSettings.smokeSystem,
            effectsSettings.sparkSystem
        };

        initialEmissionRates = new float[particleSystems.Length];
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] != null)
            {
                initialEmissionRates[i] = particleSystems[i].emission.rateOverTime.constant;
                var emission = particleSystems[i].emission;
                emission.rateOverTime = 0;
            }
        }

        // Initialize scrolling ground
        if (effectsSettings.scrollingGround != null)
        {
            initialScrollSpeed = effectsSettings.scrollingGround.scrollSpeed;
            effectsSettings.scrollingGround.scrollSpeed = 0;
        }
    }

    public void StartTrain()
    {
        if (isTrainRunning) return;
        isTrainRunning = true;

        StopAllCoroutines();
        StartTrainAudio();
        StartTrainEffects();
    }

    public void StopTrain()
    {
        if (!isTrainRunning) return;
        isTrainRunning = false;

        StopAllCoroutines();
        StopTrainAudio();
        StopTrainEffects();
    }

    private void StartTrainAudio()
    {
        AudioManager.Instance.FadeIn(audioSettings.engineSoundName, audioSettings.engineVolume, transitionDuration);
        AudioManager.Instance.FadeIn(audioSettings.wheelSound1Name, audioSettings.wheelSound1Volume, transitionDuration);
        AudioManager.Instance.FadeIn(audioSettings.wheelSound2Name, audioSettings.wheelSound2Volume, transitionDuration);

        var cameraShake = GetComponent<ConstantCameraShake>();
        if (cameraShake != null)
        {
            cameraShake.StartShakingWithFade(transitionDuration);
        }
    }

    private void StopTrainAudio()
    {
        AudioManager.Instance.FadeOut(audioSettings.engineSoundName, transitionDuration);
        AudioManager.Instance.FadeOut(audioSettings.wheelSound1Name, transitionDuration);
        AudioManager.Instance.FadeOut(audioSettings.wheelSound2Name, transitionDuration);

        var cameraShake = GetComponent<ConstantCameraShake>();
        if (cameraShake != null)
        {
            cameraShake.StopShakingWithFade(transitionDuration);
        }
    }

    private void StartTrainEffects()
    {
        // Start particle systems
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] != null)
            {
                StartCoroutine(FadeParticleSystem(particleSystems[i], initialEmissionRates[i], transitionDuration, true));
            }
        }

        // Start scrolling ground
        if (effectsSettings.scrollingGround != null)
        {
            StartCoroutine(FadeScrollingGround(initialScrollSpeed, transitionDuration, true));
        }
    }

    private void StopTrainEffects()
    {
        // Stop particle systems
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] != null)
            {
                StartCoroutine(FadeParticleSystem(particleSystems[i], 0f, transitionDuration, false));
            }
        }

        // Stop scrolling ground
        if (effectsSettings.scrollingGround != null)
        {
            StartCoroutine(FadeScrollingGround(0f, transitionDuration, false));
        }
    }

    private IEnumerator FadeParticleSystem(ParticleSystem ps, float targetRate, float duration, bool playOnStart)
    {
        var emission = ps.emission;
        float startRate = emission.rateOverTime.constant;
        
        if (playOnStart)
        {
            ps.Play();
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            emission.rateOverTime = Mathf.Lerp(startRate, targetRate, t);
            yield return null;
        }

        emission.rateOverTime = targetRate;
        if (!playOnStart && targetRate <= 0)
        {
            ps.Stop();
        }
    }

    private IEnumerator FadeScrollingGround(float targetSpeed, float duration, bool startScrolling)
    {
        if (startScrolling)
        {
            effectsSettings.scrollingGround.StartScrolling();
        }

        float startSpeed = effectsSettings.scrollingGround.scrollSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            effectsSettings.scrollingGround.scrollSpeed = Mathf.Lerp(startSpeed, targetSpeed, t);
            yield return null;
        }

        effectsSettings.scrollingGround.scrollSpeed = targetSpeed;
        if (!startScrolling && targetSpeed <= 0)
        {
            effectsSettings.scrollingGround.StopScrolling();
        }
    }
}
