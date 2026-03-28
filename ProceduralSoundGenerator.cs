using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates procedural sound effects with proper cleanup
/// Place in: Assets/Scripts/Systems/ProceduralSoundGenerator.cs
/// </summary>
public class ProceduralSoundGenerator : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // SINGLETON
    // ═══════════════════════════════════════════════════════════════
    
    private static ProceduralSoundGenerator _instance;
    
    public static ProceduralSoundGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ProceduralSoundGenerator>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("[ProceduralSoundGenerator]");
                    _instance = go.AddComponent<ProceduralSoundGenerator>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // AUDIO CLIP CACHE
    // ═══════════════════════════════════════════════════════════════
    
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
    private const int MAX_CACHED_CLIPS = 20;
    
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
    }
    
    private void OnEnable()
    {
        GameEvents.OnPlaySound += HandlePlaySound;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPlaySound -= HandlePlaySound;
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            CleanupAllClips();
            _instance = null;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    private void HandlePlaySound(string soundId, Vector3 position)
    {
        PlaySound(soundId, position);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // SOUND GENERATION
    // ═══════════════════════════════════════════════════════════════
    
    public void PlaySound(string soundId, Vector3 position)
    {
        AudioClip clip = GetOrCreateClip(soundId);
        
        if (clip != null && AudioPool.Instance != null)
        {
            AudioPool.Instance.PlayOneShot(clip, position, Constants.Audio.DEFAULT_VOLUME);
        }
    }
    
    private AudioClip GetOrCreateClip(string soundId)
    {
        // Check cache first
        if (clipCache.ContainsKey(soundId))
        {
            return clipCache[soundId];
        }
        
        // Create new clip
        AudioClip clip = CreateClipForSound(soundId);
        
        if (clip != null)
        {
            // Add to cache
            CacheClip(soundId, clip);
        }
        
        return clip;
    }
    
    private AudioClip CreateClipForSound(string soundId)
    {
        switch (soundId)
        {
            case "BrickHit":
                return GenerateTone(Constants.Audio.BALL_HIT_FREQUENCY, Constants.Audio.SHORT_SOUND_DURATION);
            
            case "BrickBreak":
                return GenerateTone(Constants.Audio.BRICK_BREAK_FREQUENCY, Constants.Audio.MEDIUM_SOUND_DURATION);
            
            case "PowerUpActivate":
            case "PowerUpCollect":
                return GenerateTone(Constants.Audio.POWER_UP_FREQUENCY, Constants.Audio.SHORT_SOUND_DURATION);
            
            case "PowerUpExpire":
                return GenerateTone(Constants.Audio.POWER_UP_FREQUENCY * 0.5f, Constants.Audio.SHORT_SOUND_DURATION);
            
            case "ItemPickup":
                return GenerateTone(660f, Constants.Audio.SHORT_SOUND_DURATION);
            
            case "ItemCollectFail":
                return GenerateTone(220f, Constants.Audio.SHORT_SOUND_DURATION);
            
            case "CraftSuccess":
                return GenerateTone(880f, Constants.Audio.MEDIUM_SOUND_DURATION);
            
            case "CraftFail":
                return GenerateTone(Constants.Audio.GAME_OVER_FREQUENCY, Constants.Audio.SHORT_SOUND_DURATION);
            
            default:
                Debug.LogWarning($"[ProceduralSoundGenerator] Unknown sound ID: {soundId}");
                return GenerateTone(440f, Constants.Audio.SHORT_SOUND_DURATION);
        }
    }
    
    private AudioClip GenerateTone(float frequency, float duration)
    {
        int sampleRate = Constants.Audio.SAMPLE_RATE;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            
            // Generate sine wave
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t);
            
            // Apply envelope (fade in/out)
            float envelope = 1f;
            float fadeTime = 0.01f; // 10ms fade
            int fadeSamples = Mathf.RoundToInt(fadeTime * sampleRate);
            
            if (i < fadeSamples)
            {
                // Fade in
                envelope = (float)i / fadeSamples;
            }
            else if (i > sampleCount - fadeSamples)
            {
                // Fade out
                envelope = (float)(sampleCount - i) / fadeSamples;
            }
            
            samples[i] *= envelope;
        }
        
        AudioClip clip = AudioClip.Create($"Tone_{frequency}Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        
        return clip;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CACHE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════
    
    private void CacheClip(string soundId, AudioClip clip)
    {
        // Check cache size
        if (clipCache.Count >= MAX_CACHED_CLIPS)
        {
            // Remove oldest (first) entry
            var enumerator = clipCache.GetEnumerator();
            if (enumerator.MoveNext())
            {
                string oldestKey = enumerator.Current.Key;
                AudioClip oldClip = clipCache[oldestKey];
                
                clipCache.Remove(oldestKey);
                
                if (oldClip != null)
                {
                    Destroy(oldClip);
                }
            }
        }
        
        clipCache[soundId] = clip;
    }
    
    private void CleanupAllClips()
    {
        foreach (var kvp in clipCache)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        
        clipCache.Clear();
        
        Debug.Log("[ProceduralSoundGenerator] All audio clips cleaned up");
    }
    
    public void ClearCache()
    {
        CleanupAllClips();
        Debug.Log("[ProceduralSoundGenerator] Cache cleared");
    }
    
    // ═══════════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;
        
        GUILayout.BeginArea(new Rect(10, 680, 300, 80));
        GUILayout.Label("=== Sound Generator ===");
        GUILayout.Label($"Cached Clips: {clipCache.Count}/{MAX_CACHED_CLIPS}");
        GUILayout.EndArea();
    }
}