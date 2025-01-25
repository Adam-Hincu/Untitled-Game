using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInitializer : MonoBehaviour
{
    [Header("Player Components")]
    public Rigidbody playerRigidbody;
    public GameObject playerCanvas;
    public GameObject playerHolder;
    public MonoBehaviour playerMovement;    // This can be assigned to any movement script that inherits from MonoBehaviour

    [Header("Scene Management")]
    [SerializeField] private string gameplaySceneName;

    private void Awake()
    {
        // Initially disable components
        SetComponentsState(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayScene = scene.name == gameplaySceneName;
        SetComponentsState(isGameplayScene);
    }

    private void SetComponentsState(bool enabled)
    {
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = !enabled;

        if (playerCanvas != null)
            playerCanvas.SetActive(enabled);

        if (playerHolder != null)
            playerHolder.SetActive(enabled);

        if (playerMovement != null)
            playerMovement.enabled = enabled;
    }
}
