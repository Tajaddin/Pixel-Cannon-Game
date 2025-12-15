using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneSetup : MonoBehaviour
{
    [Header("Setup Options")]
    public bool setupGameScene = true;
    public bool setupMainMenu = false;

    [Header("References (Auto-filled)")]
    public GameManager gameManager;
    public PixelGrid pixelGrid;
    public CannonManager cannonManager;
    public UIManager uiManager;

#if UNITY_EDITOR
    [ContextMenu("Setup Game Scene")]
    public void SetupScene()
    {
        if (setupGameScene)
            CreateGameScene();
        else if (setupMainMenu)
            CreateMainMenuScene();
    }

    private void CreateGameScene()
    {
        // --- MANAGERS (Must be Root Objects for DontDestroyOnLoad) ---
        
        // Game Manager
        if (FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
        }
        else
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        // Audio Manager
        if (FindFirstObjectByType<AudioManager>() == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }

        // Ad Manager
        if (FindFirstObjectByType<AdManager>() == null)
        {
            GameObject adObj = new GameObject("AdManager");
            adObj.AddComponent<AdManager>();
        }

        // --- GAMEPLAY OBJECTS (Can be organized) ---
        GameObject gameWorld = new GameObject("--- GAME WORLD ---");

        // Cannon Manager
        CreateCannonManager(gameWorld.transform);

        // Pixel Grid
        CreatePixelGrid(gameWorld.transform);

        // UI Manager (Canvas usually stays at root or handles its own scaling)
        GameObject uiObj = new GameObject("UIManager");
        uiManager = uiObj.AddComponent<UIManager>();

        // Create UI Canvas
        CreateGameUI();

        // Cameras
        CreateCameras();
        
        Debug.Log("Game Scene Setup Complete!");
    }

    private void CreateGameUI()
    {
        // Check if Canvas already exists to avoid duplicates
        if (FindFirstObjectByType<Canvas>() != null) return;

        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Event System
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Add UIManager reference to the Canvas
        if (uiManager != null)
        {
            uiManager.transform.SetParent(canvasObj.transform, false);
            
            // Create essential UI holders for UIManager
            CreatePanel(canvasObj.transform, "HUD_Panel");
            CreatePanel(canvasObj.transform, "Pause_Panel").SetActive(false);
            CreatePanel(canvasObj.transform, "GameOver_Panel").SetActive(false);
        }
    }

    private GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private void CreateCannonManager(Transform parent)
    {
        GameObject cmObj = new GameObject("CannonManager");
        cmObj.transform.parent = parent;
        cannonManager = cmObj.AddComponent<CannonManager>();
        
        // Create Spawn Area
        GameObject spawnArea = new GameObject("SpawnArea");
        spawnArea.transform.parent = cmObj.transform;
        spawnArea.transform.localPosition = new Vector3(0, -4, 0); // Position at bottom
        cannonManager.spawnArea = spawnArea.transform;
    }

    private void CreatePixelGrid(Transform parent)
    {
        GameObject pgObj = new GameObject("PixelGrid");
        pgObj.transform.parent = parent;
        pgObj.transform.position = new Vector3(0, 2, 0); // Position slightly up
        pixelGrid = pgObj.AddComponent<PixelGrid>();
    }

    private void CreateCameras()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            mainCam.tag = "MainCamera";
        }
        
        // FIX: Ensure AudioListener exists
        if (mainCam.GetComponent<AudioListener>() == null)
        {
            mainCam.gameObject.AddComponent<AudioListener>();
        }

        mainCam.orthographic = true;
        mainCam.orthographicSize = 10f;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // Dark blue-ish gray
        mainCam.transform.position = new Vector3(0, 0, -10);
    }

    private void CreateMainMenuScene()
    {
        // Managers (Must be root)
        if (FindFirstObjectByType<AudioManager>() == null) new GameObject("AudioManager").AddComponent<AudioManager>();
        if (FindFirstObjectByType<AdManager>() == null) new GameObject("AdManager").AddComponent<AdManager>();

        CreateMainMenuUI();
        CreateCameras();
        
        Debug.Log("Main Menu Setup Complete!");
    }

    private void CreateMainMenuUI()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        MainMenu mainMenu = canvasObj.AddComponent<MainMenu>();
        
        // Create basic panels
        mainMenu.mainPanel = CreatePanel(canvasObj.transform, "MainPanel");
        
        // Create visual elements
        CreateUIElement(mainMenu.mainPanel, "PlayButton", "PLAY", 0);
        CreateUIElement(mainMenu.mainPanel, "SettingsButton", "SETTINGS", -100);
    }

    private void CreateUIElement(GameObject parent, string name, string text, float yOffset)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.SetParent(parent.transform, false);
        btnRect.sizeDelta = new Vector2(400, 100);
        btnRect.anchoredPosition = new Vector2(0, yOffset);

        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.6f, 1f);

        btnObj.AddComponent<UnityEngine.UI.Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = 48;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneSetup))]
public class SceneSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneSetup setup = (SceneSetup)target;

        GUILayout.Space(20);

        if (GUILayout.Button("1. SETUP GAME SCENE", GUILayout.Height(40)))
        {
            setup.setupGameScene = true;
            setup.setupMainMenu = false;
            setup.SetupScene();
        }

        if (GUILayout.Button("2. SETUP MENU SCENE", GUILayout.Height(40)))
        {
            setup.setupGameScene = false;
            setup.setupMainMenu = true;
            setup.SetupScene();
        }
    }
}
#endif