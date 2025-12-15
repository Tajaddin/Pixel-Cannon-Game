using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelId;
    public string levelName;
    public Difficulty difficulty;
    public int width;
    public int height;
    public int optimalMoves;
    public GameColor[] availableColors;
    public PixelInfo[] pixelData;
}

[System.Serializable]
public class PixelInfo
{
    public int x;
    public int y;
    public GameColor color;

    public PixelInfo(int x, int y, GameColor color)
    {
        this.x = x;
        this.y = y;
        this.color = color;
    }
}

public static class LevelGenerator
{
    // Pre-defined patterns for sample levels
    private static readonly int[,] HeartPattern = new int[,]
    {
        {0,0,1,1,0,0,0,1,1,0,0},
        {0,1,1,1,1,0,1,1,1,1,0},
        {1,1,1,1,1,1,1,1,1,1,1},
        {1,1,1,1,1,1,1,1,1,1,1},
        {1,1,1,1,1,1,1,1,1,1,1},
        {0,1,1,1,1,1,1,1,1,1,0},
        {0,0,1,1,1,1,1,1,1,0,0},
        {0,0,0,1,1,1,1,1,0,0,0},
        {0,0,0,0,1,1,1,0,0,0,0},
        {0,0,0,0,0,1,0,0,0,0,0}
    };

    private static readonly int[,] StarPattern = new int[,]
    {
        {0,0,0,0,0,1,0,0,0,0,0},
        {0,0,0,0,1,1,1,0,0,0,0},
        {0,0,0,0,1,1,1,0,0,0,0},
        {1,1,1,1,1,1,1,1,1,1,1},
        {0,1,1,1,1,1,1,1,1,1,0},
        {0,0,1,1,1,1,1,1,1,0,0},
        {0,0,0,1,1,1,1,1,0,0,0},
        {0,0,1,1,1,0,1,1,1,0,0},
        {0,1,1,1,0,0,0,1,1,1,0},
        {1,1,1,0,0,0,0,0,1,1,1}
    };

    private static readonly int[,] MoonPattern = new int[,]
    {
        {0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0},
        {0,0,0,2,2,2,2,2,2,2,0,0,0,0,0,0},
        {0,0,2,2,2,2,2,2,2,2,2,0,0,0,0,0},
        {0,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0},
        {0,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0},
        {2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0},
        {0,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0},
        {0,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0},
        {0,0,2,2,2,2,2,2,2,2,2,0,0,0,0,0},
        {0,0,0,2,2,2,2,2,2,2,0,0,0,0,0,0},
        {0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
    };

    public static LevelData GenerateSampleLevel(Difficulty difficulty)
    {
        LevelData level = new LevelData();
        level.difficulty = difficulty;

        switch (difficulty)
        {
            case Difficulty.Easy:
                level = GenerateEasyLevel();
                break;
            case Difficulty.Medium:
                level = GenerateMediumLevel();
                break;
            case Difficulty.Hard:
                level = GenerateHardLevel();
                break;
            case Difficulty.SuperHard:
                level = GenerateSuperHardLevel();
                break;
        }

        return level;
    }

    private static LevelData GenerateEasyLevel()
    {
        // Simple heart with one color
        LevelData level = new LevelData();
        level.levelName = "Simple Heart";
        level.difficulty = Difficulty.Easy;
        level.width = 11;
        level.height = 10;
        level.optimalMoves = 3;
        level.availableColors = new GameColor[] { GameColor.Red };

        List<PixelInfo> pixels = new List<PixelInfo>();
        
        for (int y = 0; y < HeartPattern.GetLength(0); y++)
        {
            for (int x = 0; x < HeartPattern.GetLength(1); x++)
            {
                if (HeartPattern[y, x] == 1)
                {
                    pixels.Add(new PixelInfo(x, HeartPattern.GetLength(0) - 1 - y, GameColor.Red));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        return level;
    }

    private static LevelData GenerateMediumLevel()
    {
        // Star with two colors
        LevelData level = new LevelData();
        level.levelName = "Colorful Star";
        level.difficulty = Difficulty.Medium;
        level.width = 11;
        level.height = 10;
        level.optimalMoves = 5;
        level.availableColors = new GameColor[] { GameColor.Yellow, GameColor.Orange };

        List<PixelInfo> pixels = new List<PixelInfo>();
        
        for (int y = 0; y < StarPattern.GetLength(0); y++)
        {
            for (int x = 0; x < StarPattern.GetLength(1); x++)
            {
                if (StarPattern[y, x] == 1)
                {
                    // Alternate colors based on position
                    GameColor color = (x + y) % 2 == 0 ? GameColor.Yellow : GameColor.Orange;
                    pixels.Add(new PixelInfo(x, StarPattern.GetLength(0) - 1 - y, color));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        return level;
    }

    private static LevelData GenerateHardLevel()
    {
        // Moon-like pattern with multiple colors similar to the screenshot
        LevelData level = new LevelData();
        level.levelName = "Crescent Moon";
        level.difficulty = Difficulty.Hard;
        level.width = 16;
        level.height = 16;
        level.optimalMoves = 8;
        level.availableColors = new GameColor[] { GameColor.Cyan, GameColor.White, GameColor.Purple, GameColor.Yellow };

        List<PixelInfo> pixels = new List<PixelInfo>();
        
        // Create a moon-like pattern similar to the image
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                // Create moon shape
                float dx = x - 7;
                float dy = y - 7;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                float dx2 = x - 10;
                float dy2 = y - 7;
                float dist2 = Mathf.Sqrt(dx2 * dx2 + dy2 * dy2);

                if (dist < 7 && dist2 > 5)
                {
                    // Moon body - cyan/white
                    GameColor color = dist < 5 ? GameColor.White : GameColor.Cyan;
                    pixels.Add(new PixelInfo(x, y, color));
                }
            }
        }

        // Add stars (yellow) in the background area
        int[] starX = { 12, 14, 13, 11, 15 };
        int[] starY = { 3, 5, 8, 12, 10 };
        for (int i = 0; i < starX.Length; i++)
        {
            pixels.Add(new PixelInfo(starX[i], starY[i], GameColor.Yellow));
        }

        level.pixelData = pixels.ToArray();
        return level;
    }

    private static LevelData GenerateSuperHardLevel()
    {
        // Complex multi-color pattern
        LevelData level = new LevelData();
        level.levelName = "Galaxy";
        level.difficulty = Difficulty.SuperHard;
        level.width = 20;
        level.height = 20;
        level.optimalMoves = 15;
        level.availableColors = new GameColor[] { 
            GameColor.Purple, GameColor.Blue, GameColor.Cyan, 
            GameColor.Pink, GameColor.Yellow, GameColor.White 
        };

        List<PixelInfo> pixels = new List<PixelInfo>();
        
        // Create spiral galaxy pattern
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                float dx = x - 10;
                float dy = y - 10;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                // Spiral arms
                float spiralValue = Mathf.Sin(angle * 2 + dist * 0.5f);
                
                if (dist < 9 && (spiralValue > 0.3f || dist < 2))
                {
                    GameColor color;
                    if (dist < 2)
                        color = GameColor.White;
                    else if (dist < 4)
                        color = GameColor.Yellow;
                    else if (dist < 6)
                        color = GameColor.Pink;
                    else if (spiralValue > 0.6f)
                        color = GameColor.Cyan;
                    else
                        color = GameColor.Purple;

                    pixels.Add(new PixelInfo(x, y, color));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        return level;
    }

    // Generate level from image (for future use)
    public static LevelData GenerateFromTexture(Texture2D image, Difficulty difficulty)
    {
        LevelData level = new LevelData();
        level.difficulty = difficulty;
        level.width = image.width;
        level.height = image.height;

        List<PixelInfo> pixels = new List<PixelInfo>();
        HashSet<GameColor> usedColors = new HashSet<GameColor>();

        for (int y = 0; y < image.height; y++)
        {
            for (int x = 0; x < image.width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                
                // Skip transparent pixels
                if (pixelColor.a < 0.5f) continue;

                GameColor gameColor = ColorToGameColor(pixelColor);
                if (gameColor != GameColor.Empty)
                {
                    pixels.Add(new PixelInfo(x, y, gameColor));
                    usedColors.Add(gameColor);
                }
            }
        }

        level.pixelData = pixels.ToArray();
        level.availableColors = new GameColor[usedColors.Count];
        usedColors.CopyTo(level.availableColors);

        // Calculate optimal moves
        level.optimalMoves = CalculateOptimalMoves(level);

        return level;
    }

    private static GameColor ColorToGameColor(Color color)
    {
        // Find closest matching game color
        float minDist = float.MaxValue;
        GameColor closest = GameColor.Empty;

        Dictionary<GameColor, Color> colorMap = new Dictionary<GameColor, Color>
        {
            { GameColor.Red, Color.red },
            { GameColor.Blue, new Color(0.2f, 0.6f, 1f) },
            { GameColor.Green, Color.green },
            { GameColor.Yellow, Color.yellow },
            { GameColor.Purple, new Color(0.6f, 0.2f, 0.8f) },
            { GameColor.Orange, new Color(1f, 0.5f, 0f) },
            { GameColor.Pink, new Color(1f, 0.4f, 0.7f) },
            { GameColor.Cyan, Color.cyan },
            { GameColor.White, Color.white },
            { GameColor.Black, new Color(0.1f, 0.1f, 0.1f) }
        };

        foreach (var kvp in colorMap)
        {
            float dist = ColorDistance(color, kvp.Value);
            if (dist < minDist)
            {
                minDist = dist;
                closest = kvp.Key;
            }
        }

        return minDist < 0.5f ? closest : GameColor.Empty;
    }

    private static float ColorDistance(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db);
    }

    private static int CalculateOptimalMoves(LevelData level)
    {
        // Simple calculation based on pixel count and color variety
        int pixelCount = level.pixelData.Length;
        int colorCount = level.availableColors.Length;
        
        // Assume average cannon power of 20
        int avgPower = 20;
        int baseMoves = Mathf.CeilToInt(pixelCount / (float)avgPower);
        
        // Add moves for color switching
        return baseMoves + colorCount;
    }
}
