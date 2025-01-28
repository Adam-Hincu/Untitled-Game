using UnityEngine;
using System.Collections;

public class TrainGenerator : MonoBehaviour
{
    [Header("Train Configuration")]
    public GameObject locomotivePrefab;  // The locomotive that will always be first
    public GameObject[] wagonPrefabs;    // Array of different wagon prefabs to choose from
    public int trainLength = 3;          // How many wagons to spawn after the locomotive

    private GameObject currentTrain;  // Reference to hold the current train parent
    private bool isGenerating = false;  // Flag to prevent multiple generations at once

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(GenerateTrainCoroutine());
    }

    void Update()
    {
        // Debug: Press L to generate a new train
        if (Input.GetKeyDown(KeyCode.L) && !isGenerating)
        {
            // Clean up old train if it exists
            if (currentTrain != null)
            {
                Destroy(currentTrain);
            }
            StartCoroutine(GenerateTrainCoroutine());
        }
    }

    private IEnumerator GenerateTrainCoroutine()
    {
        isGenerating = true;

        // Create a parent object for the train
        currentTrain = new GameObject("Train");
        currentTrain.transform.position = transform.position;

        // Spawn the locomotive first
        GameObject currentPiece = Instantiate(locomotivePrefab, transform.position, Quaternion.identity);
        currentPiece.transform.parent = currentTrain.transform;
        TrainPieceController currentController = currentPiece.GetComponent<TrainPieceController>();

        // Wait for a frame to ensure transforms are initialized
        yield return new WaitForEndOfFrame();

        // Keep track of the last piece to connect the next one
        TrainPieceController lastPieceController = currentController;

        // Generate random wagons
        for (int i = 0; i < trainLength; i++)
        {
            // Get a random wagon prefab
            GameObject randomWagonPrefab = wagonPrefabs[Random.Range(0, wagonPrefabs.Length)];
            
            // Ensure the last piece's transform is properly updated
            lastPieceController.transform.hasChanged = false;
            yield return new WaitForEndOfFrame();

            // Create position using only X from connection point, keeping original Y and Z
            Vector3 nextPosition = new Vector3(
                lastPieceController.connectionPoint.position.x,
                transform.position.y,
                transform.position.z
            );
            
            // Spawn the wagon
            GameObject wagon = Instantiate(randomWagonPrefab, nextPosition, Quaternion.identity);
            wagon.transform.parent = currentTrain.transform;
            TrainPieceController wagonController = wagon.GetComponent<TrainPieceController>();

            // Wait for the wagon to be properly initialized
            yield return new WaitForEndOfFrame();

            // Update the last piece reference
            lastPieceController = wagonController;
        }

        isGenerating = false;
    }
}
