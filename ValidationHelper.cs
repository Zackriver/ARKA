using UnityEngine;

/// <summary>
/// Helper utilities for data validation and parsing
/// Place in: Assets/Scripts/Utilities/ValidationHelper.cs
/// </summary>
public static class ValidationHelper
{
    // ═══════════════════════════════════════════════════════════════
    // STRING VALIDATION
    // ═══════════════════════════════════════════════════════════════
    
    public static bool IsValidBrickLayoutChar(char c)
    {
        // Valid characters: 0-9, A-Z, space, newline
        if (char.IsWhiteSpace(c)) return true;
        if (char.IsDigit(c)) return true;
        if (char.IsLetter(c) && char.IsUpper(c)) return true;
        
        return false;
    }
    
    public static bool ValidateBrickLayout(string layout, out string error)
    {
        error = "";
        
        if (string.IsNullOrEmpty(layout))
        {
            error = "Layout is empty";
            return false;
        }
        
        foreach (char c in layout)
        {
            if (!IsValidBrickLayoutChar(c))
            {
                error = $"Invalid character: '{c}'";
                return false;
            }
        }
        
        return true;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // NUMBER VALIDATION
    // ═══════════════════════════════════════════════════════════════
    
    public static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }
    
    public static bool IsInRange(float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    
    public static int ClampInt(int value, int min, int max)
    {
        return Mathf.Clamp(value, min, max);
    }
    
    public static float ClampFloat(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }
}