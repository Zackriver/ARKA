using UnityEngine;
using System.IO;

/// <summary>
/// Complete save/load system using JSON and PlayerPrefs
/// Place in: Assets/Scripts/Systems/SaveSystem.cs
/// </summary>
public class SaveSystem : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static SaveSystem _instance;
    
    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveSystem>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[SaveSystem]");
                    _instance = go.AddComponent<SaveSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    private const string SAVE_FILE_NAME = "savegame.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    
    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log($"[SaveSystem] Save path: {SaveFilePath}");
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SAVE
    // ═══════════════════════════════════════════════════════════════
    
    public void SaveGame()
    {
        try
        {
            SaveData data = CreateSaveData();
            string json = JsonUtility.ToJson(data, true);
            
            File.WriteAllText(SaveFilePath, json);
            
            Debug.Log($"[SaveSystem] Game saved successfully to {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save game: {e.Message}");
        }
    }
    
    private SaveData CreateSaveData()
    {
        SaveData data = new SaveData();
        
        // Game progress
        if (GameManager.Instance != null)
        {
            data.currentLevel = GameManager.Instance.CurrentLevel;
            data.playerScore = GameManager.Instance.PlayerScore;
            data.playerLives = GameManager.Instance.PlayerLives;
        }
        
        // Inventory
        if (PlayerInventory.Instance != null)
        {
            data.inventoryData = PlayerInventory.Instance.GetSaveData();
        }
        
        // High score
        data.highScore = PlayerPrefs.GetInt(Constants.SaveKeys.HIGH_SCORE, 0);
        
        // Timestamp
        data.saveTimestamp = System.DateTime.Now.ToString();
        
        return data;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // LOAD
    // ═══════════════════════════════════════════════════════════════
    
    public bool LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("[SaveSystem] No save file found");
            return false;
        }
        
        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            ApplySaveData(data);
            
            Debug.Log($"[SaveSystem] Game loaded successfully from {SaveFilePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load game: {e.Message}");
            return false;
        }
    }
    
    private void ApplySaveData(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveSystem] Save data is null!");
            return;
        }
        
        // Inventory
        if (PlayerInventory.Instance != null && data.inventoryData != null)
        {
            PlayerInventory.Instance.LoadSaveData(data.inventoryData);
        }
        
        // High score
        PlayerPrefs.SetInt(Constants.SaveKeys.HIGH_SCORE, data.highScore);
        
        Debug.Log($"[SaveSystem] Save data applied (saved at: {data.saveTimestamp})");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITIES
    // ═══════════════════════════════════════════════════════════════
    
    public bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }
    
    public void DeleteSave()
    {
        if (HasSaveFile())
        {
            File.Delete(SaveFilePath);
            Debug.Log("[SaveSystem] Save file deleted");
        }
    }
    
    public void SaveHighScore(int score)
    {
        int currentHighScore = PlayerPrefs.GetInt(Constants.SaveKeys.HIGH_SCORE, 0);
        
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(Constants.SaveKeys.HIGH_SCORE, score);
            PlayerPrefs.Save();
            
            Debug.Log($"[SaveSystem] New high score: {score}");
        }
    }
    
    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(Constants.SaveKeys.HIGH_SCORE, 0);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // AUTO-SAVE
    // ═══════════════════════════════════════════════════════════════
    
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}