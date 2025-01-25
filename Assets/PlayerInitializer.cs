using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerInitializer : MonoBehaviour
{
    [Header("Player Components")]
    public Rigidbody playerRigidbody;
    public GameObject playerCanvas;
    public GameObject playerHolder;
    public MonoBehaviour playerMovement;    // This can be assigned to any movement script that inherits from MonoBehaviour

    [Header("Scene Management")]
    #if UNITY_EDITOR
    [SerializeField] private SceneAsset[] gameplayScenes;
    #endif
    private string[] gameplaySceneNames;

    private void Awake()
    {
        // Convert SceneAssets to scene names
        #if UNITY_EDITOR
        if (gameplayScenes != null)
        {
            gameplaySceneNames = new string[gameplayScenes.Length];
            for (int i = 0; i < gameplayScenes.Length; i++)
            {
                if (gameplayScenes[i] != null)
                    gameplaySceneNames[i] = gameplayScenes[i].name;
            }
        }
        #endif
        
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
        bool isGameplayScene = System.Array.Exists(gameplaySceneNames, sceneName => sceneName == scene.name);
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
