using UnityEngine;

public static class SpriteGenerator
{
    public static Sprite CreateCircleSprite(int size = 64, Color? color = null)
    {
        Color fillColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size);
        
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance < radius - 1)
                {
                    texture.SetPixel(x, y, fillColor);
                }
                else if (distance < radius)
                {
                    // Anti-aliased edge
                    float alpha = 1f - (distance - (radius - 1));
                    Color edgeColor = fillColor;
                    edgeColor.a *= alpha;
                    texture.SetPixel(x, y, edgeColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    public static Sprite CreateSquareSprite(int size = 64, Color? color = null)
    {
        Color fillColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                texture.SetPixel(x, y, fillColor);
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    public static Sprite CreateDiamondSprite(int size = 64, Color? color = null)
    {
        Color fillColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size);
        
        float half = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - half);
                float dy = Mathf.Abs(y - half);
                
                if (dx + dy < half)
                {
                    texture.SetPixel(x, y, fillColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    public static Sprite CreateGlowSprite(int size = 64, Color? color = null)
    {
        Color fillColor = color ?? Color.white;
        Texture2D texture = new Texture2D(size, size);
        
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDist = distance / radius;
                
                if (normalizedDist <= 1f)
                {
                    // Gradient from center (full alpha) to edge (zero alpha)
                    float alpha = 1f - normalizedDist;
                    alpha = alpha * alpha; // Quadratic falloff for softer glow
                    
                    Color glowColor = fillColor;
                    glowColor.a = alpha * fillColor.a;
                    texture.SetPixel(x, y, glowColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}