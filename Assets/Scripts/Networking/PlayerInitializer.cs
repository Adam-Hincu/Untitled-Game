using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerInitializer : NetworkBehaviour
{
    [Header("Player Components")]
    public Rigidbody playerRigidbody;
    public GameObject playerCanvas;
    public GameObject playerHolder;
    public MonoBehaviour playerMovement;    // This can be assigned to any movement script that inherits from MonoBehaviour
    public GameObject playerCamera;         // New field for the camera

    [Header("Scene Management")]
    [SerializeField] private string gameplaySceneName;

    private void Awake()
    {
        // Initially disable components
        SetComponentsState(false);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!isLocalPlayer)
        {
            // If this is not the local player, destroy non-local components
            if (playerRigidbody != null) Destroy(playerRigidbody);
            if (playerCanvas != null) Destroy(playerCanvas.gameObject);
            if (playerMovement != null) Destroy(playerMovement);
            if (playerCamera != null) Destroy(playerCamera);
            
            // For non-local players, we'll manage the holder in OnSceneLoaded
            if (playerHolder != null)
                playerHolder.SetActive(false);
                
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // For the local player, enable/disable based on scene
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayScene = scene.name == gameplaySceneName;
        
        if (isLocalPlayer)
        {
            SetComponentsState(isGameplayScene);
        }
        else if (playerHolder != null)
        {
            // For non-local players, only manage the holder
            playerHolder.SetActive(isGameplayScene);
        }
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

        if (playerCamera != null)
            playerCamera.SetActive(enabled);
    }
}
