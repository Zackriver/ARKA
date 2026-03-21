using UnityEngine;
using System;

public static class ProceduralSoundGenerator
{
    private static int sampleRate = 44100;

    // ==================== WAVEFORM GENERATORS ====================
    
    private static float Sin(float frequency, int sample)
    {
        return Mathf.Sin(2f * Mathf.PI * frequency * sample / sampleRate);
    }

    private static float Square(float frequency, int sample)
    {
        return Sin(frequency, sample) >= 0 ? 1f : -1f;
    }

    private static float Triangle(float frequency, int sample)
    {
        float t = (float)sample / sampleRate * frequency;
        return 2f * Mathf.Abs(2f * (t - Mathf.Floor(t + 0.5f))) - 1f;
    }

    private static float Sawtooth(float frequency, int sample)
    {
        float t = (float)sample / sampleRate * frequency;
        return 2f * (t - Mathf.Floor(t)) - 1f;
    }

    private static float Noise()
    {
        return UnityEngine.Random.Range(-1f, 1f);
    }

    // ==================== HELPER FUNCTIONS ====================

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Mathf.Clamp01(t);
    }

    private static float ExpDecay(float t, float decay)
    {
        return Mathf.Exp(-decay * t);
    }

    // Attempt at a slightly different ADSR style
    private static float ADSREnvelope(float t, float attack, float decay, float sustain, float release, float duration)
    {
        float attackEnd = attack;
        float decayEnd = attack + decay;
        float releaseStart = duration - release;

        if (t < attackEnd)
            return t / attack; // Attack: 0 to 1
        else if (t < decayEnd)
            return 1f - ((t - attackEnd) / decay) * (1f - sustain); // Decay: 1 to sustain
        else if (t < releaseStart)
            return sustain; // Sustain
        else
            return sustain * (1f - (t - releaseStart) / release); // Release: sustain to 0
    }

    private static AudioClip CreateClip(string name, float duration, Func<int, float, float> generator)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            samples[i] = Mathf.Clamp(generator(i, t), -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // ==================== GAME SOUNDS ====================
    // Modify the values below to change how each sound plays!

    /// <summary>
    /// Sound when ball hits paddle
    /// </summary>
    public static AudioClip PaddleHit()
    {
        // === MODIFY THESE VALUES ===
        float duration = 0.1f;          // Length in seconds (0.05 - 0.3)
        float startFreq = 300f;         // Starting pitch Hz (100 - 1000)
        float endFreq = 500f;           // Ending pitch Hz (100 - 1000)
        float decaySpeed = 5f;          // How fast volume fades (1 - 20)
        float volume = 0.5f;            // Master volume (0 - 1)
        // ============================

        return CreateClip("PaddleHit", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            float envelope = ExpDecay(t, decaySpeed);
            return Triangle(freq, i) * envelope * volume;
        });
    }

    /// <summary>
    /// Sound when ball hits wall
    /// </summary>
    public static AudioClip WallHit()
    {
        // === MODIFY THESE VALUES ===
        float duration = 0.08f;
        float startFreq = 200f;
        float endFreq = 150f;
        float decaySpeed = 6f;
        float volume = 0.4f;
        // ============================

        return CreateClip("WallHit", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            float envelope = ExpDecay(t, decaySpeed);
            return Sin(freq, i) * envelope * volume;
        });
    }

    /// <summary>
    /// Sound when brick is destroyed
    /// </summary>
    public static AudioClip BrickDestroy()
    {
        // === MODIFY THESE VALUES ===
        float duration = 0.15f;
        float startFreq = 600f;
        float endFreq = 200f;
        float toneDecay = 4f;
        float noiseDecay = 8f;
        float toneVolume = 0.3f;
        float noiseVolume = 0.2f;
        // ============================

        return CreateClip("BrickDestroy", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            float envelope = ExpDecay(t, toneDecay);
            float tone = Square(freq, i) * toneVolume;
            float noise = Noise() * ExpDecay(t, noiseDecay) * noiseVolume;
            return (tone + noise) * envelope;
        });
    }

    /// <summary>
    /// Sound when game starts
    /// </summary>
    public static AudioClip GameStart()
    {
        // === MODIFY THESE VALUES ===
        float duration = 0.5f;
        float startFreq = 200f;
        float endFreq = 400f;
        float decaySpeed = 2f;
        float volume = 0.5f;
        // ============================

        return CreateClip("GameStart", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            return Sin(freq, i) * ExpDecay(t, decaySpeed) * volume;
        });
    }

    /// <summary>
    /// Sound when ball is lost
    /// </summary>
    public static AudioClip BallLost()
    {
        // === MODIFY THESE VALUES ===
        float duration = 0.5f;
        float startFreq = 400f;
        float endFreq = 100f;
        float decaySpeed = 2f;
        float volume = 0.5f;
        // ============================

        return CreateClip("BallLost", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            return Sawtooth(freq, i) * ExpDecay(t, decaySpeed) * volume;
        });
    }

    /// <summary>
    /// Sound when game is over
    /// </summary>
    public static AudioClip GameOver()
    {
        // === MODIFY THESE VALUES ===
        float duration = 1f;
        float startFreq = 200f;
        float endFreq = 50f;
        float decaySpeed = 1f;
        float volume = 0.5f;
        // ============================

        return CreateClip("GameOver", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            return Square(freq, i) * ExpDecay(t, decaySpeed) * volume;
        });
    }

    /// <summary>
    /// Sound when level is completed
    /// </summary>
    public static AudioClip LevelComplete()
    {
        // === MODIFY THESE VALUES ===
        float duration = 0.8f;
        float startFreq = 400f;
        float endFreq = 800f;
        float decaySpeed = 2f;
        float volume = 0.5f;
        // ============================

        return CreateClip("LevelComplete", duration, (i, t) =>
        {
            float freq = Lerp(startFreq, endFreq, t);
            return Triangle(freq, i) * ExpDecay(t, decaySpeed) * volume;
        });
    }

    // ==================== EXTRA SOUND OPTIONS ====================
    // You can add these to your game or replace existing sounds!

    /// <summary>
    /// Retro 8-bit style hit
    /// </summary>
    public static AudioClip RetroBleep()
    {
        return CreateClip("RetroBleep", 0.08f, (i, t) =>
        {
            float freq = 880f; // A5 note
            float envelope = ExpDecay(t, 10f);
            return Square(freq, i) * envelope * 0.3f;
        });
    }

    /// <summary>
    /// Arcade coin sound
    /// </summary>
    public static AudioClip CoinSound()
    {
        return CreateClip("Coin", 0.2f, (i, t) =>
        {
            // Two alternating frequencies for that classic coin sound
            float freq1 = 988f;  // B5
            float freq2 = 1319f; // E6
            float freq = t < 0.5f ? freq1 : freq2;
            float envelope = ExpDecay(t, 4f);
            return Triangle(freq, i) * envelope * 0.4f;
        });
    }

    /// <summary>
    /// Explosion sound
    /// </summary>
    public static AudioClip Explosion()
    {
        return CreateClip("Explosion", 0.4f, (i, t) =>
        {
            float freq = Lerp(150f, 30f, t);
            float noiseAmount = ExpDecay(t, 3f);
            float tone = Sin(freq, i) * 0.3f;
            float noise = Noise() * noiseAmount * 0.7f;
            return (tone + noise) * ExpDecay(t, 2.5f);
        });
    }

    /// <summary>
    /// Power-up collected sound
    /// </summary>
    public static AudioClip PowerUp()
    {
        return CreateClip("PowerUp", 0.3f, (i, t) =>
        {
            // Rising arpeggio effect
            float baseFreq = 400f;
            float freqMultiplier = 1f + t * 2f; // Goes up 2 octaves
            float freq = baseFreq * freqMultiplier;
            float envelope = ExpDecay(t, 3f);
            return Triangle(freq, i) * envelope * 0.4f;
        });
    }

    /// <summary>
    /// Laser/zap sound
    /// </summary>
    public static AudioClip Laser()
    {
        return CreateClip("Laser", 0.15f, (i, t) =>
        {
            float freq = Lerp(1000f, 200f, t);
            float envelope = ExpDecay(t, 5f);
            return Sawtooth(freq, i) * envelope * 0.3f;
        });
    }

    /// <summary>
    /// Soft bounce (alternative paddle hit)
    /// </summary>
    public static AudioClip SoftBounce()
    {
        return CreateClip("SoftBounce", 0.1f, (i, t) =>
        {
            float freq = Lerp(250f, 350f, t);
            float envelope = ExpDecay(t, 8f);
            return Sin(freq, i) * envelope * 0.4f;
        });
    }

    /// <summary>
    /// Hard hit (alternative brick destroy)
    /// </summary>
    public static AudioClip HardHit()
    {
        return CreateClip("HardHit", 0.12f, (i, t) =>
        {
            float freq = Lerp(800f, 300f, t);
            float envelope = ExpDecay(t, 6f);
            float tone = Square(freq, i) * 0.4f;
            float noise = Noise() * ExpDecay(t, 10f) * 0.3f;
            return (tone + noise) * envelope;
        });
    }

    /// <summary>
    /// Victory fanfare (multi-note)
    /// </summary>
    public static AudioClip VictoryFanfare()
    {
        return CreateClip("Victory", 1.0f, (i, t) =>
        {
            // 4 ascending notes
            float[] notes = { 523f, 659f, 784f, 1047f }; // C5, E5, G5, C6
            int noteIndex = Mathf.Min((int)(t * 4), 3);
            float freq = notes[noteIndex];
            
            // Each note has its own mini-envelope
            float noteT = (t * 4) - noteIndex;
            float envelope = ExpDecay(noteT, 4f);
            
            return Triangle(freq, i) * envelope * 0.4f;
        });
    }

    /// <summary>
    /// Defeat/failure sound (multi-note descending)
    /// </summary>
    public static AudioClip DefeatSound()
    {
        return CreateClip("Defeat", 1.2f, (i, t) =>
        {
            // 3 descending notes
            float[] notes = { 392f, 311f, 261f }; // G4, Eb4, C4
            int noteIndex = Mathf.Min((int)(t * 3), 2);
            float freq = notes[noteIndex];
            
            float noteT = (t * 3) - noteIndex;
            float envelope = ExpDecay(noteT, 3f);
            
            return Sawtooth(freq, i) * envelope * 0.4f;
        });
    }
}