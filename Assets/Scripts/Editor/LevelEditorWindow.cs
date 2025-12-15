#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// In-editor tool for creating and editing game levels.
/// Access via Window -> Pixel Cannon -> Level Editor
/// </summary>
public class LevelEditorWindow : EditorWindow
{
    // Grid settings
    private int gridWidth = 16;
    private int gridHeight = 16;
    private GameColor[,] grid;
    
    // Selected color
    private GameColor selectedColor = GameColor.Red;
    
    // Level info
    private int levelId = 0;
    private string levelName = "New Level";
    private Difficulty difficulty = Difficulty.Easy;
    
    // Editor state
    private Vector2 scrollPosition;
    private float cellSize = 20f;
    
    // Color palette
    private readonly Dictionary<GameColor, Color> colorPalette = new Dictionary<GameColor, Color>
    {
        { GameColor.Empty, new Color(0.2f, 0.2f, 0.2f) },
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

    [MenuItem("Window/Pixel Cannon/Level Editor")]
    public static void ShowWindow()
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(600, 500);
        window.Show();
    }

    private void OnEnable()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new GameColor[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = GameColor.Empty;
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        
        // Left panel - Tools and Settings
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        DrawToolsPanel();
        EditorGUILayout.EndVertical();
        
        // Right panel - Grid
        EditorGUILayout.BeginVertical();
        DrawGridPanel();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        // Handle input
        HandleInput();
    }

    private void DrawToolsPanel()
    {
        GUILayout.Label("Level Settings", EditorStyles.boldLabel);
        
        levelId = EditorGUILayout.IntField("Level ID", levelId);
        levelName = EditorGUILayout.TextField("Level Name", levelName);
        difficulty = (Difficulty)EditorGUILayout.EnumPopup("Difficulty", difficulty);
        
        EditorGUILayout.Space(10);
        
        // Grid size
        GUILayout.Label("Grid Size", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        int newWidth = EditorGUILayout.IntField("W", gridWidth, GUILayout.Width(60));
        int newHeight = EditorGUILayout.IntField("H", gridHeight, GUILayout.Width(60));
        if (GUILayout.Button("Resize", GUILayout.Width(60)))
        {
            ResizeGrid(newWidth, newHeight);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Color palette
        GUILayout.Label("Color Palette", EditorStyles.boldLabel);
        DrawColorPalette();
        
        EditorGUILayout.Space(10);
        
        // Actions
        GUILayout.Label("Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Clear Grid"))
        {
            if (EditorUtility.DisplayDialog("Clear Grid", "Are you sure?", "Yes", "No"))
            {
                InitializeGrid();
            }
        }
        
        if (GUILayout.Button("Fill with Selected"))
        {
            FillGrid(selectedColor);
        }
        
        EditorGUILayout.Space(10);
        
        // Save/Load
        GUILayout.Label("File Operations", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }
        
        if (GUILayout.Button("Load Level"))
        {
            LoadLevel();
        }
        
        if (GUILayout.Button("Import Image"))
        {
            ImportImage();
        }
        
        EditorGUILayout.Space(10);
        
        // Stats
        GUILayout.Label("Statistics", EditorStyles.boldLabel);
        int pixelCount = CountPixels();
        int colorCount = CountColors();
        EditorGUILayout.LabelField($"Pixels: {pixelCount}");
        EditorGUILayout.LabelField($"Colors: {colorCount}");
        EditorGUILayout.LabelField($"Est. Optimal Moves: {pixelCount / 25 + colorCount}");
    }

    private void DrawColorPalette()
    {
        int colorsPerRow = 4;
        int colorIndex = 0;
        
        foreach (var kvp in colorPalette)
        {
            if (colorIndex % colorsPerRow == 0)
            {
                if (colorIndex > 0) EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            
            GUI.backgroundColor = kvp.Value;
            bool isSelected = selectedColor == kvp.Key;
            string buttonText = isSelected ? "●" : "";
            
            if (GUILayout.Button(buttonText, GUILayout.Width(40), GUILayout.Height(30)))
            {
                selectedColor = kvp.Key;
            }
            
            colorIndex++;
        }
        
        if (colorIndex > 0) EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.LabelField($"Selected: {selectedColor}");
    }

    private void DrawGridPanel()
    {
        GUILayout.Label("Level Grid (Left-click: Draw, Right-click: Erase)", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, 
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        float totalWidth = gridWidth * cellSize;
        float totalHeight = gridHeight * cellSize;
        
        GUILayout.Space(totalHeight + 20);
        
        // Draw grid cells
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Rect cellRect = new Rect(
                    x * cellSize + 10,
                    (gridHeight - 1 - y) * cellSize + 10,
                    cellSize - 1,
                    cellSize - 1
                );
                
                Color cellColor = colorPalette[grid[x, y]];
                EditorGUI.DrawRect(cellRect, cellColor);
                
                // Draw grid lines
                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                Handles.DrawLine(
                    new Vector3(cellRect.x, cellRect.y, 0),
                    new Vector3(cellRect.xMax, cellRect.y, 0)
                );
                Handles.DrawLine(
                    new Vector3(cellRect.x, cellRect.y, 0),
                    new Vector3(cellRect.x, cellRect.yMax, 0)
                );
            }
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void HandleInput()
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
        {
            Vector2 mousePos = e.mousePosition;
            
            // Calculate grid position with scroll offset
            float mouseX = mousePos.x - 210 + scrollPosition.x;
            float mouseY = mousePos.y - 45 + scrollPosition.y;
            
            int gridX = (int)(mouseX / cellSize);
            int gridY = gridHeight - 1 - (int)(mouseY / cellSize);
            
            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                if (e.button == 0) // Left click - draw
                {
                    grid[gridX, gridY] = selectedColor;
                    Repaint();
                }
                else if (e.button == 1) // Right click - erase
                {
                    grid[gridX, gridY] = GameColor.Empty;
                    Repaint();
                }
            }
        }
    }

    private void ResizeGrid(int newWidth, int newHeight)
    {
        newWidth = Mathf.Clamp(newWidth, 4, 64);
        newHeight = Mathf.Clamp(newHeight, 4, 64);
        
        GameColor[,] newGrid = new GameColor[newWidth, newHeight];
        
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                if (x < gridWidth && y < gridHeight)
                    newGrid[x, y] = grid[x, y];
                else
                    newGrid[x, y] = GameColor.Empty;
            }
        }
        
        gridWidth = newWidth;
        gridHeight = newHeight;
        grid = newGrid;
    }

    private void FillGrid(GameColor color)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = color;
            }
        }
    }

    private int CountPixels()
    {
        int count = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != GameColor.Empty)
                    count++;
            }
        }
        return count;
    }

    private int CountColors()
    {
        HashSet<GameColor> colors = new HashSet<GameColor>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != GameColor.Empty)
                    colors.Add(grid[x, y]);
            }
        }
        return colors.Count;
    }

    private void SaveLevel()
    {
        string difficultyFolder = difficulty.ToString();
        string defaultPath = $"Assets/Resources/Levels/{difficultyFolder}";
        
        if (!Directory.Exists(defaultPath))
        {
            Directory.CreateDirectory(defaultPath);
        }
        
        string path = EditorUtility.SaveFilePanel(
            "Save Level",
            defaultPath,
            $"{levelId}.json",
            "json"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        LevelData levelData = CreateLevelData();
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(path, json);
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Level saved to {path}", "OK");
    }

    private LevelData CreateLevelData()
    {
        LevelData data = new LevelData();
        data.levelId = levelId;
        data.levelName = levelName;
        data.difficulty = difficulty;
        data.width = gridWidth;
        data.height = gridHeight;
        
        List<PixelInfo> pixels = new List<PixelInfo>();
        HashSet<GameColor> colors = new HashSet<GameColor>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != GameColor.Empty)
                {
                    pixels.Add(new PixelInfo(x, y, grid[x, y]));
                    colors.Add(grid[x, y]);
                }
            }
        }
        
        data.pixelData = pixels.ToArray();
        data.availableColors = new GameColor[colors.Count];
        colors.CopyTo(data.availableColors);
        data.optimalMoves = pixels.Count / 25 + colors.Count;
        
        return data;
    }

    private void LoadLevel()
    {
        string path = EditorUtility.OpenFilePanel(
            "Load Level",
            "Assets/Resources/Levels",
            "json"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        string json = File.ReadAllText(path);
        LevelData data = JsonUtility.FromJson<LevelData>(json);
        
        levelId = data.levelId;
        levelName = data.levelName;
        difficulty = data.difficulty;
        gridWidth = data.width;
        gridHeight = data.height;
        
        InitializeGrid();
        
        foreach (PixelInfo pixel in data.pixelData)
        {
            if (pixel.x >= 0 && pixel.x < gridWidth && 
                pixel.y >= 0 && pixel.y < gridHeight)
            {
                grid[pixel.x, pixel.y] = pixel.color;
            }
        }
        
        Repaint();
    }

    private void ImportImage()
    {
        string path = EditorUtility.OpenFilePanel(
            "Import Image",
            "",
            "png,jpg,jpeg"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        byte[] imageData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        
        // Resize to max 32x32
        int maxSize = 32;
        int newWidth = Mathf.Min(texture.width, maxSize);
        int newHeight = Mathf.Min(texture.height, maxSize);
        
        ResizeGrid(newWidth, newHeight);
        
        float scaleX = (float)texture.width / newWidth;
        float scaleY = (float)texture.height / newHeight;
        
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                int sourceX = (int)(x * scaleX);
                int sourceY = (int)(y * scaleY);
                Color pixelColor = texture.GetPixel(sourceX, sourceY);
                
                if (pixelColor.a < 0.5f)
                {
                    grid[x, y] = GameColor.Empty;
                }
                else
                {
                    grid[x, y] = FindClosestColor(pixelColor);
                }
            }
        }
        
        DestroyImmediate(texture);
        Repaint();
    }

    private GameColor FindClosestColor(Color color)
    {
        float minDist = float.MaxValue;
        GameColor closest = GameColor.Empty;
        
        foreach (var kvp in colorPalette)
        {
            if (kvp.Key == GameColor.Empty) continue;
            
            float dist = ColorDistance(color, kvp.Value);
            if (dist < minDist)
            {
                minDist = dist;
                closest = kvp.Key;
            }
        }
        
        return closest;
    }

    private float ColorDistance(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db);
    }
}
#endif