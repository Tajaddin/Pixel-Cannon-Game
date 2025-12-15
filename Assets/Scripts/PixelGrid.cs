using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float pixelSize = 0.5f;
    public float spacing = 0.05f;

    [Header("Prefabs")]
    public GameObject pixelPrefab;

    [Header("Colors")]
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color[] availableColors;

    private Pixel[,] pixels;
    private LevelData levelData;
    private Dictionary<GameColor, List<Pixel>> colorGroups = new Dictionary<GameColor, List<Pixel>>();

    public void InitializeGrid(LevelData level)
    {
        levelData = level;
        gridWidth = level.width;
        gridHeight = level.height;

        ClearGrid();
        CreateGrid();
        LoadLevelPixels();
    }

    private void ClearGrid()
    {
        if (pixels != null)
        {
            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    if (pixels[x, y] != null)
                    {
                        Destroy(pixels[x, y].gameObject);
                    }
                }
            }
        }

        pixels = new Pixel[gridWidth, gridHeight];
        colorGroups.Clear();
    }

    private void CreateGrid()
    {
        float totalWidth = gridWidth * (pixelSize + spacing);
        float totalHeight = gridHeight * (pixelSize + spacing);
        Vector3 startPos = transform.position - new Vector3(totalWidth / 2, totalHeight / 2, 0);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = startPos + new Vector3(
                    x * (pixelSize + spacing) + pixelSize / 2,
                    y * (pixelSize + spacing) + pixelSize / 2,
                    0
                );

                GameObject pixelObj;
                if (pixelPrefab != null)
                {
                    pixelObj = Instantiate(pixelPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    pixelObj = CreateDefaultPixel(pos);
                }

                pixelObj.name = $"Pixel_{x}_{y}";
                
                Pixel pixel = pixelObj.GetComponent<Pixel>();
                if (pixel == null)
                {
                    pixel = pixelObj.AddComponent<Pixel>();
                }

                pixel.gridX = x;
                pixel.gridY = y;
                pixel.SetColor(GameColor.Empty, emptyColor);
                
                pixels[x, y] = pixel;
            }
        }
    }

    private GameObject CreateDefaultPixel(Vector3 position)
    {
        GameObject pixel = GameObject.CreatePrimitive(PrimitiveType.Quad);
        pixel.transform.position = position;
        pixel.transform.localScale = new Vector3(pixelSize, pixelSize, 1);
        pixel.transform.parent = transform;
        
        // Remove collider if not needed
        Collider col = pixel.GetComponent<Collider>();
        if (col != null) Destroy(col);

        return pixel;
    }

    private void LoadLevelPixels()
    {
        if (levelData.pixelData == null) return;

        foreach (PixelInfo info in levelData.pixelData)
        {
            if (info.x >= 0 && info.x < gridWidth && info.y >= 0 && info.y < gridHeight)
            {
                Pixel pixel = pixels[info.x, info.y];
                pixel.targetColor = info.color;
                pixel.isFilled = false;
                
                // Set to grayscale version of target color initially
                Color grayVersion = GetGrayscaleColor(info.color);
                pixel.SetDisplayColor(grayVersion);

                // Add to color groups
                if (!colorGroups.ContainsKey(info.color))
                {
                    colorGroups[info.color] = new List<Pixel>();
                }
                colorGroups[info.color].Add(pixel);
            }
        }
    }

    private Color GetGrayscaleColor(GameColor color)
    {
        // Return a darker gray shade based on the color
        float grayValue = 0.3f + ((int)color * 0.05f);
        return new Color(grayValue, grayValue, grayValue, 1f);
    }

    public int GetTotalColoredPixels()
    {
        int total = 0;
        foreach (var group in colorGroups.Values)
        {
            total += group.Count;
        }
        return total;
    }

    public int FireCannonAtColor(GameColor color, int power)
    {
        if (!colorGroups.ContainsKey(color))
            return 0;

        List<Pixel> targetPixels = colorGroups[color];
        int pixelsFilled = 0;
        int pixelsToFill = Mathf.Min(power, targetPixels.Count);

        // Fill pixels from top to bottom, left to right
        List<Pixel> unfilledPixels = targetPixels.FindAll(p => !p.isFilled);
        unfilledPixels.Sort((a, b) => {
            if (a.gridY != b.gridY) return b.gridY.CompareTo(a.gridY); // Top first
            return a.gridX.CompareTo(b.gridX); // Left first
        });

        for (int i = 0; i < pixelsToFill && i < unfilledPixels.Count; i++)
        {
            Pixel pixel = unfilledPixels[i];
            StartCoroutine(FillPixelAnimation(pixel, GetColorValue(color)));
            pixelsFilled++;
        }

        return pixelsFilled;
    }

    private IEnumerator FillPixelAnimation(Pixel pixel, Color targetColor)
    {
        pixel.isFilled = true;
        
        // Animate the fill
        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = pixel.GetCurrentColor();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep
            
            pixel.SetDisplayColor(Color.Lerp(startColor, targetColor, t));
            
            // Scale pop effect
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            pixel.transform.localScale = Vector3.one * pixelSize * scale;
            
            yield return null;
        }

        pixel.SetDisplayColor(targetColor);
        pixel.transform.localScale = Vector3.one * pixelSize;
    }

    public Color GetColorValue(GameColor color)
    {
        switch (color)
        {
            case GameColor.Red: return Color.red;
            case GameColor.Blue: return new Color(0.2f, 0.6f, 1f);
            case GameColor.Green: return Color.green;
            case GameColor.Yellow: return Color.yellow;
            case GameColor.Purple: return new Color(0.6f, 0.2f, 0.8f);
            case GameColor.Orange: return new Color(1f, 0.5f, 0f);
            case GameColor.Pink: return new Color(1f, 0.4f, 0.7f);
            case GameColor.Cyan: return Color.cyan;
            case GameColor.White: return Color.white;
            case GameColor.Black: return new Color(0.1f, 0.1f, 0.1f);
            default: return emptyColor;
        }
    }

    public bool IsComplete()
    {
        foreach (var group in colorGroups.Values)
        {
            foreach (Pixel pixel in group)
            {
                if (!pixel.isFilled)
                    return false;
            }
        }
        return true;
    }

    public List<GameColor> GetRemainingColors()
    {
        List<GameColor> remaining = new List<GameColor>();
        
        foreach (var kvp in colorGroups)
        {
            if (kvp.Value.Exists(p => !p.isFilled))
            {
                remaining.Add(kvp.Key);
            }
        }

        return remaining;
    }
}

public class Pixel : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public GameColor targetColor;
    public bool isFilled;

    private SpriteRenderer spriteRenderer;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetColor(GameColor color, Color displayColor)
    {
        targetColor = color;
        SetDisplayColor(displayColor);
    }

    public void SetDisplayColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        else if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }

    public Color GetCurrentColor()
    {
        if (spriteRenderer != null)
            return spriteRenderer.color;
        else if (meshRenderer != null)
            return meshRenderer.material.color;
        return Color.white;
    }
}

public enum GameColor
{
    Empty = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Yellow = 4,
    Purple = 5,
    Orange = 6,
    Pink = 7,
    Cyan = 8,
    White = 9,
    Black = 10
}
