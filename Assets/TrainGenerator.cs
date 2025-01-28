using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrainData
{
    public Vector3 startPosition;    // Position where the locomotive will be placed
    public int trainLength = 3;      // Number of wagons after the locomotive
    public string trainName = "Train"; // Optional name for the train
}

public class TrainGenerator : MonoBehaviour
{
    [Header("Train Configuration")]
    public GameObject locomotivePrefab;  // The locomotive that will always be first
    public GameObject[] wagonPrefabs;    // Array of different wagon prefabs to choose from
    
    [Header("Multiple Trains Setup")]
    public List<TrainData> trainConfigurations = new List<TrainData>();  // List of train configurations
    
    private List<GameObject> activeTrains = new List<GameObject>();  // Keep track of all generated trains

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateAllTrains();
    }

    void Update()
    {
        // Debug: Press L to regenerate all trains
        if (Input.GetKeyDown(KeyCode.L))
        {
            RegenerateAllTrains();
        }
    }

    public void RegenerateAllTrains()
    {
        // Clean up all existing trains
        foreach (GameObject train in activeTrains)
        {
            if (train != null)
            {
                Destroy(train);
            }
        }
        activeTrains.Clear();
        
        // Generate new trains
        GenerateAllTrains();
    }

    private void GenerateAllTrains()
    {
        foreach (TrainData config in trainConfigurations)
        {
            GenerateTrain(config);
        }
    }

    private void GenerateTrain(TrainData config)
    {
        // Create a parent object for the train
        GameObject currentTrain = new GameObject(config.trainName);
        currentTrain.transform.position = config.startPosition;
        activeTrains.Add(currentTrain);

        // Spawn the locomotive first
        GameObject currentPiece = Instantiate(locomotivePrefab, config.startPosition, Quaternion.identity);
        currentPiece.transform.parent = currentTrain.transform;
        TrainPieceController currentController = currentPiece.GetComponent<TrainPieceController>();

        // Keep track of the last piece to connect the next one
        TrainPieceController lastPieceController = currentController;

        // Generate random wagons
        for (int i = 0; i < config.trainLength; i++)
        {
            // Get a random wagon prefab
            GameObject randomWagonPrefab = wagonPrefabs[Random.Range(0, wagonPrefabs.Length)];
            
            // Create position using only X from connection point, keeping original Y and Z
            Vector3 nextPosition = new Vector3(
                lastPieceController.connectionPoint.position.x,
                config.startPosition.y,
                config.startPosition.z
            );
            
            // Spawn the wagon
            GameObject wagon = Instantiate(randomWagonPrefab, nextPosition, Quaternion.identity);
            wagon.transform.parent = currentTrain.transform;
            TrainPieceController wagonController = wagon.GetComponent<TrainPieceController>();

            // Update the last piece reference
            lastPieceController = wagonController;
        }
    }
}
