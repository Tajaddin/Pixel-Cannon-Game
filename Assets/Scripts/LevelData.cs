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

/// <summary>
/// Enhanced Level Generator with procedural patterns for 1000+ levels
/// </summary>
public static class LevelGenerator
{
    // Pixel art patterns stored as 2D arrays (0 = empty, 1+ = color index)
    private static readonly Dictionary<string, int[,]> Patterns = new Dictionary<string, int[,]>();

    static LevelGenerator()
    {
        InitializePatterns();
    }

    private static void InitializePatterns()
    {
        // Heart
        Patterns["heart"] = new int[,]
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

        // Star
        Patterns["star"] = new int[,]
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

        // Smiley
        Patterns["smiley"] = new int[,]
        {
            {0,0,0,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0},
            {0,1,1,2,1,1,1,2,1,1,0},
            {1,1,1,2,1,1,1,2,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1},
            {1,1,3,1,1,1,1,1,3,1,1},
            {0,1,1,3,1,1,1,3,1,1,0},
            {0,0,1,1,3,3,3,1,1,0,0},
            {0,0,0,1,1,1,1,1,0,0,0}
        };

        // Diamond
        Patterns["diamond"] = new int[,]
        {
            {0,0,0,0,0,1,0,0,0,0,0},
            {0,0,0,0,1,1,1,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,1},
            {0,1,1,1,1,1,1,1,1,1,0},
            {0,0,1,1,1,1,1,1,1,0,0},
            {0,0,0,1,1,1,1,1,0,0,0},
            {0,0,0,0,1,1,1,0,0,0,0},
            {0,0,0,0,0,1,0,0,0,0,0}
        };

        // Cat face
        Patterns["cat"] = new int[,]
        {
            {1,0,0,0,0,0,0,0,0,0,1},
            {1,1,0,0,0,0,0,0,0,1,1},
            {1,1,1,1,1,1,1,1,1,1,1},
            {1,1,2,1,1,1,1,1,2,1,1},
            {1,1,2,1,1,1,1,1,2,1,1},
            {1,1,1,1,1,3,1,1,1,1,1},
            {1,1,1,1,3,3,3,1,1,1,1},
            {1,1,4,1,1,1,1,1,4,1,1},
            {1,1,1,4,1,1,1,4,1,1,1},
            {0,1,1,1,1,1,1,1,1,1,0}
        };

        // House
        Patterns["house"] = new int[,]
        {
            {0,0,0,0,0,1,0,0,0,0,0},
            {0,0,0,0,1,1,1,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,1},
            {2,2,2,2,2,2,2,2,2,2,2},
            {2,2,3,3,2,2,2,4,4,2,2},
            {2,2,3,3,2,2,2,4,4,2,2},
            {2,2,3,3,2,2,2,4,4,2,2},
            {2,2,3,3,2,2,2,4,4,2,2}
        };

        // Flower
        Patterns["flower"] = new int[,]
        {
            {0,0,0,1,1,1,0,0,0},
            {0,1,1,1,2,1,1,1,0},
            {0,1,2,1,2,1,2,1,0},
            {1,1,1,2,2,2,1,1,1},
            {1,2,1,2,3,2,1,2,1},
            {1,1,1,2,2,2,1,1,1},
            {0,1,2,1,4,1,2,1,0},
            {0,1,1,1,4,1,1,1,0},
            {0,0,0,1,4,1,0,0,0},
            {0,0,0,1,4,1,0,0,0},
            {0,0,5,5,4,5,5,0,0}
        };

        // Tree
        Patterns["tree"] = new int[,]
        {
            {0,0,0,0,1,0,0,0,0},
            {0,0,0,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1},
            {0,0,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1},
            {0,0,0,2,2,2,0,0,0},
            {0,0,0,2,2,2,0,0,0},
            {0,0,0,2,2,2,0,0,0}
        };

        // Moon
        Patterns["moon"] = new int[,]
        {
            {0,0,0,1,1,1,1,0,0,0,0},
            {0,0,1,1,1,1,1,1,0,0,0},
            {0,1,1,1,1,1,0,0,0,0,0},
            {1,1,1,1,1,0,0,0,0,0,0},
            {1,1,1,1,0,0,0,0,0,0,0},
            {1,1,1,1,0,0,0,0,0,0,0},
            {1,1,1,1,1,0,0,0,0,0,0},
            {0,1,1,1,1,1,0,0,0,0,0},
            {0,0,1,1,1,1,1,1,0,0,0},
            {0,0,0,1,1,1,1,0,0,0,0}
        };

        // Sun
        Patterns["sun"] = new int[,]
        {
            {0,0,0,0,1,0,0,0,0},
            {0,1,0,0,1,0,0,1,0},
            {0,0,1,0,1,0,1,0,0},
            {0,0,0,2,2,2,0,0,0},
            {1,1,1,2,2,2,1,1,1},
            {0,0,0,2,2,2,0,0,0},
            {0,0,1,0,1,0,1,0,0},
            {0,1,0,0,1,0,0,1,0},
            {0,0,0,0,1,0,0,0,0}
        };

        // Rocket
        Patterns["rocket"] = new int[,]
        {
            {0,0,0,0,1,0,0,0,0},
            {0,0,0,1,1,1,0,0,0},
            {0,0,0,1,1,1,0,0,0},
            {0,0,1,1,2,1,1,0,0},
            {0,0,1,1,2,1,1,0,0},
            {0,0,1,1,1,1,1,0,0},
            {0,0,1,1,1,1,1,0,0},
            {0,3,1,1,1,1,1,3,0},
            {3,3,1,1,1,1,1,3,3},
            {0,0,0,4,0,4,0,0,0},
            {0,0,4,4,0,4,4,0,0}
        };

        // Butterfly
        Patterns["butterfly"] = new int[,]
        {
            {1,1,0,0,0,0,0,1,1},
            {1,2,1,0,0,0,1,2,1},
            {1,1,2,1,0,1,2,1,1},
            {0,1,1,1,3,1,1,1,0},
            {0,0,1,1,3,1,1,0,0},
            {0,1,1,1,3,1,1,1,0},
            {1,1,2,1,0,1,2,1,1},
            {1,2,1,0,0,0,1,2,1},
            {1,1,0,0,0,0,0,1,1}
        };

        // Apple
        Patterns["apple"] = new int[,]
        {
            {0,0,0,0,2,0,0,0,0},
            {0,0,0,2,2,0,0,0,0},
            {0,0,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1},
            {0,1,1,1,1,1,1,1,0},
            {0,0,1,1,1,1,1,0,0}
        };

        // Fish
        Patterns["fish"] = new int[,]
        {
            {0,0,0,0,1,1,0,0,0,0,0},
            {0,0,1,1,1,1,1,1,0,0,0},
            {1,1,1,2,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,2,1,1,1,1,1,1,0},
            {0,0,1,1,1,1,1,1,0,0,0},
            {0,0,0,0,1,1,0,0,0,0,0}
        };

        // Crown
        Patterns["crown"] = new int[,]
        {
            {0,1,0,0,0,1,0,0,0,1,0},
            {0,1,1,0,0,1,0,0,1,1,0},
            {0,1,1,1,0,1,0,1,1,1,0},
            {0,1,1,1,1,1,1,1,1,1,0},
            {0,1,1,2,1,1,1,2,1,1,0},
            {0,1,1,1,1,2,1,1,1,1,0},
            {0,1,1,1,1,1,1,1,1,1,0}
        };

        // Mushroom
        Patterns["mushroom"] = new int[,]
        {
            {0,0,0,1,1,1,0,0,0},
            {0,0,1,1,2,1,1,0,0},
            {0,1,1,2,2,2,1,1,0},
            {1,1,1,1,2,1,1,1,1},
            {1,2,1,1,1,1,1,2,1},
            {0,0,0,3,3,3,0,0,0},
            {0,0,0,3,3,3,0,0,0},
            {0,0,3,3,3,3,3,0,0}
        };

        // Ghost
        Patterns["ghost"] = new int[,]
        {
            {0,0,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,0},
            {1,1,2,1,1,1,2,1,1},
            {1,1,2,1,1,1,2,1,1},
            {1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1},
            {1,0,1,0,1,0,1,0,1}
        };

        // Paw print
        Patterns["paw"] = new int[,]
        {
            {0,1,1,0,0,1,1,0},
            {0,1,1,0,0,1,1,0},
            {1,1,0,0,0,0,1,1},
            {1,1,0,0,0,0,1,1},
            {0,0,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,0},
            {0,1,1,1,1,1,1,0},
            {0,0,1,1,1,1,0,0}
        };

        // Umbrella
        Patterns["umbrella"] = new int[,]
        {
            {0,0,0,1,1,1,0,0,0},
            {0,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1},
            {0,0,0,0,2,0,0,0,0},
            {0,0,0,0,2,0,0,0,0},
            {0,0,0,0,2,0,0,0,0},
            {0,0,0,0,2,0,0,0,0},
            {0,0,0,2,2,0,0,0,0}
        };
    }

    public static LevelData GenerateSampleLevel(Difficulty difficulty)
    {
        // Use a random level ID for variety
        int seed = Random.Range(0, 1000);
        return GenerateLevel(seed, difficulty);
    }

    public static LevelData GenerateLevel(int levelId, Difficulty difficulty)
    {
        // Use levelId as seed for consistent regeneration
        Random.InitState(levelId + (int)difficulty * 1000);

        LevelData level;

        // Choose generation method based on level ID and difficulty
        int method = levelId % 5;

        switch (method)
        {
            case 0:
                level = GenerateFromPattern(levelId, difficulty);
                break;
            case 1:
                level = GenerateGeometricLevel(levelId, difficulty);
                break;
            case 2:
                level = GenerateMandalaLevel(levelId, difficulty);
                break;
            case 3:
                level = GenerateGradientLevel(levelId, difficulty);
                break;
            case 4:
                level = GenerateMosaicLevel(levelId, difficulty);
                break;
            default:
                level = GenerateFromPattern(levelId, difficulty);
                break;
        }

        // Set difficulty-based parameters
        level.levelId = levelId;
        level.difficulty = difficulty;
        level.availableColors = GetColorsForDifficulty(difficulty, levelId);
        level.optimalMoves = CalculateOptimalMoves(level);

        return level;
    }

    private static LevelData GenerateFromPattern(int levelId, Difficulty difficulty)
    {
        string[] patternNames = new string[] { "heart", "star", "smiley", "diamond", "cat", "house",
            "flower", "tree", "moon", "sun", "rocket", "butterfly", "apple", "fish",
            "crown", "mushroom", "ghost", "paw", "umbrella" };

        string patternName = patternNames[levelId % patternNames.Length];
        int[,] pattern = Patterns[patternName];

        LevelData level = new LevelData();
        level.levelName = patternName.Substring(0, 1).ToUpper() + patternName.Substring(1);
        level.difficulty = difficulty;
        level.width = pattern.GetLength(1);
        level.height = pattern.GetLength(0);

        GameColor[] colorPalette = GetColorsForDifficulty(difficulty, levelId);
        List<PixelInfo> pixels = new List<PixelInfo>();

        for (int y = 0; y < pattern.GetLength(0); y++)
        {
            for (int x = 0; x < pattern.GetLength(1); x++)
            {
                int colorIndex = pattern[y, x];
                if (colorIndex > 0)
                {
                    // Map pattern color index to available colors
                    GameColor color = colorPalette[(colorIndex - 1) % colorPalette.Length];
                    pixels.Add(new PixelInfo(x, pattern.GetLength(0) - 1 - y, color));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        level.availableColors = colorPalette;

        return level;
    }

    private static LevelData GenerateGeometricLevel(int levelId, Difficulty difficulty)
    {
        LevelData level = new LevelData();
        level.levelName = $"Geometric {levelId + 1}";
        level.difficulty = difficulty;

        int size = GetSizeForDifficulty(difficulty);
        level.width = size;
        level.height = size;

        GameColor[] colors = GetColorsForDifficulty(difficulty, levelId);
        List<PixelInfo> pixels = new List<PixelInfo>();

        int shapeType = levelId % 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size / 2f) / (size / 2f);
                float ny = (y - size / 2f) / (size / 2f);
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                bool include = false;
                int colorIndex = 0;

                switch (shapeType)
                {
                    case 0: // Circles
                        include = dist < 1f;
                        colorIndex = (int)(dist * colors.Length);
                        break;
                    case 1: // Squares
                        include = Mathf.Abs(nx) < 0.8f && Mathf.Abs(ny) < 0.8f;
                        colorIndex = (int)((Mathf.Abs(nx) + Mathf.Abs(ny)) * colors.Length / 2);
                        break;
                    case 2: // Cross
                        include = (Mathf.Abs(nx) < 0.3f || Mathf.Abs(ny) < 0.3f) && dist < 0.9f;
                        colorIndex = (x + y) % colors.Length;
                        break;
                    case 3: // Ring
                        include = dist > 0.4f && dist < 0.9f;
                        colorIndex = (int)(dist * colors.Length * 2) % colors.Length;
                        break;
                }

                if (include)
                {
                    colorIndex = Mathf.Clamp(colorIndex, 0, colors.Length - 1);
                    pixels.Add(new PixelInfo(x, y, colors[colorIndex]));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        level.availableColors = colors;

        return level;
    }

    private static LevelData GenerateMandalaLevel(int levelId, Difficulty difficulty)
    {
        LevelData level = new LevelData();
        level.levelName = $"Mandala {levelId + 1}";
        level.difficulty = difficulty;

        int size = GetSizeForDifficulty(difficulty);
        level.width = size;
        level.height = size;

        GameColor[] colors = GetColorsForDifficulty(difficulty, levelId);
        List<PixelInfo> pixels = new List<PixelInfo>();

        int symmetry = 4 + (levelId % 4) * 2; // 4, 6, 8, or 10 fold symmetry
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                // Create radial pattern
                float normalizedAngle = ((angle + Mathf.PI) / (2 * Mathf.PI)) * symmetry;
                float patternValue = Mathf.Sin(normalizedAngle * Mathf.PI) + Mathf.Sin(dist * 0.5f);

                if (dist < center * 0.9f && patternValue > 0.2f)
                {
                    int colorIndex = (int)((dist / center + patternValue) * colors.Length) % colors.Length;
                    pixels.Add(new PixelInfo(x, y, colors[colorIndex]));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        level.availableColors = colors;

        return level;
    }

    private static LevelData GenerateGradientLevel(int levelId, Difficulty difficulty)
    {
        LevelData level = new LevelData();
        level.levelName = $"Gradient {levelId + 1}";
        level.difficulty = difficulty;

        int size = GetSizeForDifficulty(difficulty);
        level.width = size;
        level.height = size;

        GameColor[] colors = GetColorsForDifficulty(difficulty, levelId);
        List<PixelInfo> pixels = new List<PixelInfo>();

        int gradientType = levelId % 3;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float value = 0;

                switch (gradientType)
                {
                    case 0: // Horizontal
                        value = (float)x / size;
                        break;
                    case 1: // Vertical
                        value = (float)y / size;
                        break;
                    case 2: // Diagonal
                        value = (x + y) / (2f * size);
                        break;
                }

                // Create interesting shapes
                float center = size / 2f;
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist < center * 0.85f)
                {
                    int colorIndex = (int)(value * colors.Length);
                    colorIndex = Mathf.Clamp(colorIndex, 0, colors.Length - 1);
                    pixels.Add(new PixelInfo(x, y, colors[colorIndex]));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        level.availableColors = colors;

        return level;
    }

    private static LevelData GenerateMosaicLevel(int levelId, Difficulty difficulty)
    {
        LevelData level = new LevelData();
        level.levelName = $"Mosaic {levelId + 1}";
        level.difficulty = difficulty;

        int size = GetSizeForDifficulty(difficulty);
        level.width = size;
        level.height = size;

        GameColor[] colors = GetColorsForDifficulty(difficulty, levelId);
        List<PixelInfo> pixels = new List<PixelInfo>();

        int tileSize = 2 + (levelId % 3);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int tileX = x / tileSize;
                int tileY = y / tileSize;

                // Create checker pattern with variations
                int colorIndex = (tileX + tileY) % colors.Length;

                // Add some variety based on position within tile
                if ((x % tileSize == 0 || y % tileSize == 0) && colors.Length > 1)
                {
                    colorIndex = (colorIndex + 1) % colors.Length;
                }

                // Create border
                float center = size / 2f;
                float dx = x - center;
                float dy = y - center;
                if (Mathf.Abs(dx) < center * 0.9f && Mathf.Abs(dy) < center * 0.9f)
                {
                    pixels.Add(new PixelInfo(x, y, colors[colorIndex]));
                }
            }
        }

        level.pixelData = pixels.ToArray();
        level.availableColors = colors;

        return level;
    }

    private static int GetSizeForDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return Random.Range(8, 12);
            case Difficulty.Medium: return Random.Range(12, 16);
            case Difficulty.Hard: return Random.Range(16, 20);
            case Difficulty.SuperHard: return Random.Range(20, 24);
            default: return 12;
        }
    }

    private static GameColor[] GetColorsForDifficulty(Difficulty difficulty, int levelId)
    {
        List<GameColor> allColors = new List<GameColor>
        {
            GameColor.Red, GameColor.Blue, GameColor.Green, GameColor.Yellow,
            GameColor.Purple, GameColor.Orange, GameColor.Pink, GameColor.Cyan,
            GameColor.White, GameColor.Black
        };

        // Shuffle based on level ID
        for (int i = 0; i < allColors.Count; i++)
        {
            int j = (i + levelId) % allColors.Count;
            var temp = allColors[i];
            allColors[i] = allColors[j];
            allColors[j] = temp;
        }

        int colorCount;
        switch (difficulty)
        {
            case Difficulty.Easy: colorCount = Random.Range(1, 3); break;
            case Difficulty.Medium: colorCount = Random.Range(2, 4); break;
            case Difficulty.Hard: colorCount = Random.Range(3, 6); break;
            case Difficulty.SuperHard: colorCount = Random.Range(5, 8); break;
            default: colorCount = 2; break;
        }

        return allColors.GetRange(0, colorCount).ToArray();
    }

    public static int CalculateOptimalMoves(LevelData level)
    {
        int pixelCount = level.pixelData.Length;
        int colorCount = level.availableColors.Length;

        // Estimate based on average power and color variety
        int avgPower;
        switch (level.difficulty)
        {
            case Difficulty.Easy: avgPower = 35; break;
            case Difficulty.Medium: avgPower = 27; break;
            case Difficulty.Hard: avgPower = 20; break;
            case Difficulty.SuperHard: avgPower = 12; break;
            default: avgPower = 25; break;
        }

        int baseMoves = Mathf.CeilToInt(pixelCount / (float)avgPower);
        return Mathf.Max(baseMoves + colorCount, 3);
    }

    /// <summary>
    /// Generate level from a texture (for custom pixel art)
    /// </summary>
    public static LevelData GenerateFromTexture(Texture2D image, Difficulty difficulty, string name = "Custom")
    {
        LevelData level = new LevelData();
        level.levelName = name;
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
        level.optimalMoves = CalculateOptimalMoves(level);

        return level;
    }

    private static GameColor ColorToGameColor(Color color)
    {
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

        float minDist = float.MaxValue;
        GameColor closest = GameColor.Empty;

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
}
