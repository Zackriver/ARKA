using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio source pooling system to prevent memory leaks
/// Place in: Assets/Scripts/Systems/AudioPool.cs
/// </summary>
public class AudioPool : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static AudioPool _instance;
    
    public static AudioPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioPool>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[AudioPool]");
                    _instance = go.AddComponent<AudioPool>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // POOL SETTINGS
    // ═══════════════════════════════════════════════════════════════
    
    [Header("Pool Settings")]
    [SerializeField] private int poolSize = Constants.Audio.AUDIO_POOL_SIZE;
    
    private Queue<AudioSource> availableSources = new Queue<AudioSource>();
    private List<AudioSource> allSources = new List<AudioSource>();
    private Dictionary<AudioSource, float> activeSourceEndTimes = new Dictionary<AudioSource, float>();
    
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
        
        InitializePool();
    }
    
    private void Update()
    {
        UpdateActiveSources();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            CleanupAllSources();
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════════
    
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewAudioSource();
        }
        
        Debug.Log($"[AudioPool] Initialized with {poolSize} audio sources");
    }
    
    private AudioSource CreateNewAudioSource()
    {
        GameObject sourceObj = new GameObject($"PooledAudioSource_{allSources.Count}");
        sourceObj.transform.SetParent(transform);
        
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        
        allSources.Add(source);
        availableSources.Enqueue(source);
        
        return source;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // PLAY AUDIO
    // ═══════════════════════════════════════════════════════════════
    
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioPool] Attempted to play null AudioClip!");
            return;
        }
        
        AudioSource source = GetAvailableSource();
        
        if (source == null)
        {
            Debug.LogWarning("[AudioPool] No available audio sources!");
            return;
        }
        
        // Configure source
        source.transform.position = position;
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = pitch;
        
        // Play
        source.Play();
        
        // Track end time
        float endTime = Time.time + clip.length / pitch;
        activeSourceEndTimes[source] = endTime;
        
        Debug.Log($"[AudioPool] Playing {clip.name} at {position}");
    }
    
    public void PlayOneShot(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioPool] Attempted to play null AudioClip!");
            return;
        }
        
        AudioSource source = GetAvailableSource();
        
        if (source == null)
        {
            Debug.LogWarning("[AudioPool] No available audio sources!");
            return;
        }
        
        source.transform.position = position;
        source.PlayOneShot(clip, volume);
        
        // Track end time
        float endTime = Time.time + clip.length;
        activeSourceEndTimes[source] = endTime;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SOURCE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    private AudioSource GetAvailableSource()
    {
        // Check for available source
        if (availableSources.Count > 0)
        {
            return availableSources.Dequeue();
        }
        
        // Try to expand pool
        if (allSources.Count < poolSize * 2) // Max 2x expansion
        {
            Debug.Log("[AudioPool] Expanding pool...");
            return CreateNewAudioSource();
        }
        
        // Pool exhausted - force stop oldest source
        Debug.LogWarning("[AudioPool] Pool exhausted, recycling oldest source");
        return ForceRecycleOldestSource();
    }
    
    private AudioSource ForceRecycleOldestSource()
    {
        AudioSource oldest = null;
        float earliestEndTime = float.MaxValue;
        
        foreach (var kvp in activeSourceEndTimes)
        {
            if (kvp.Value < earliestEndTime)
            {
                earliestEndTime = kvp.Value;
                oldest = kvp.Key;
            }
        }
        
        if (oldest != null)
        {
            oldest.Stop();
            ReturnSource(oldest);
            return GetAvailableSource();
        }
        
        return null;
    }
    
    private void ReturnSource(AudioSource source)
    {
        if (source == null) return;
        
        source.clip = null;
        source.Stop();
        
        activeSourceEndTimes.Remove(source);
        
        if (!availableSources.Contains(source))
        {
            availableSources.Enqueue(source);
        }
    }
    
    private void UpdateActiveSources()
    {
        List<AudioSource> toReturn = new List<AudioSource>();
        
        foreach (var kvp in activeSourceEndTimes)
        {
            if (Time.time >= kvp.Value && !kvp.Key.isPlaying)
            {
                toReturn.Add(kvp.Key);
            }
        }
        
        foreach (var source in toReturn)
        {
            ReturnSource(source);
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════════
    
    public void StopAllSounds()
    {
        foreach (var source in allSources)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
        
        activeSourceEndTimes.Clear();
        availableSources.Clear();
        
        foreach (var source in allSources)
        {
            if (source != null)
            {
                availableSources.Enqueue(source);
            }
        }
        
        Debug.Log("[AudioPool] All sounds stopped");
    }
    
    private void CleanupAllSources()
    {
        foreach (var source in allSources)
        {
            if (source != null && source.clip != null)
            {
                source.clip = null;
            }
        }
        
        activeSourceEndTimes.Clear();
        availableSources.Clear();
        allSources.Clear();
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(10, 580, 300, 100));
        GUILayout.Label("=== Audio Pool ===");
        GUILayout.Label($"Total Sources: {allSources.Count}");
        GUILayout.Label($"Available: {availableSources.Count}");
        GUILayout.Label($"Active: {activeSourceEndTimes.Count}");
        GUILayout.EndArea();
    }
}