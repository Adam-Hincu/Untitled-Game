using UnityEngine;
using System.IO;

public class SteamAppIDChecker : MonoBehaviour
{
    [SerializeField] private uint steamAppID = 480; // Default App ID, can be changed in inspector

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        string rootPath = Directory.GetCurrentDirectory();
        string filePath = Path.Combine(rootPath, "steam_appid.txt");

        // Try to get the Steam App ID from any existing SteamAppIDChecker component
        var checker = GameObject.FindObjectOfType<SteamAppIDChecker>();
        uint appId = checker != null ? checker.steamAppID : 480;

        if (!File.Exists(filePath))
        {
            try
            {
                File.WriteAllText(filePath, appId.ToString());
            }
            catch (System.Exception)
            {
                // Silently fail if we can't create the file
            }
        }
    }
} 