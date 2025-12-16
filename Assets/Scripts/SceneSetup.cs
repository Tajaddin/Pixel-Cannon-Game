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

        // Create all panels
        mainMenu.mainPanel = CreateMenuPanel(canvasObj.transform, "MainPanel");
        mainMenu.difficultyPanel = CreateMenuPanel(canvasObj.transform, "DifficultyPanel");
        mainMenu.levelSelectPanel = CreateMenuPanel(canvasObj.transform, "LevelSelectPanel");
        mainMenu.settingsPanel = CreateMenuPanel(canvasObj.transform, "SettingsPanel");
        mainMenu.creditsPanel = CreateMenuPanel(canvasObj.transform, "CreditsPanel");

        // Hide all panels except main
        mainMenu.difficultyPanel.SetActive(false);
        mainMenu.levelSelectPanel.SetActive(false);
        mainMenu.settingsPanel.SetActive(false);
        mainMenu.creditsPanel.SetActive(false);

        // Create Main Panel buttons
        mainMenu.playButton = CreateButton(mainMenu.mainPanel.transform, "PlayButton", "PLAY", 100);
        mainMenu.settingsButton = CreateButton(mainMenu.mainPanel.transform, "SettingsButton", "SETTINGS", 0);
        mainMenu.creditsButton = CreateButton(mainMenu.mainPanel.transform, "CreditsButton", "CREDITS", -100);
        mainMenu.quitButton = CreateButton(mainMenu.mainPanel.transform, "QuitButton", "QUIT", -200);

        // Create Difficulty Panel buttons
        CreateLabel(mainMenu.difficultyPanel.transform, "DifficultyTitle", "SELECT DIFFICULTY", 300);
        mainMenu.easyButton = CreateButton(mainMenu.difficultyPanel.transform, "EasyButton", "EASY", 150);
        mainMenu.mediumButton = CreateButton(mainMenu.difficultyPanel.transform, "MediumButton", "MEDIUM", 50);
        mainMenu.hardButton = CreateButton(mainMenu.difficultyPanel.transform, "HardButton", "HARD", -50);
        mainMenu.superHardButton = CreateButton(mainMenu.difficultyPanel.transform, "SuperHardButton", "SUPER HARD", -150);
        mainMenu.backFromDifficultyButton = CreateButton(mainMenu.difficultyPanel.transform, "BackFromDifficultyButton", "BACK", -300);

        // Create Level Select Panel
        CreateLabel(mainMenu.levelSelectPanel.transform, "LevelSelectTitle", "SELECT LEVEL", 400);
        GameObject levelGrid = CreateLevelGrid(mainMenu.levelSelectPanel.transform);
        mainMenu.levelGridContainer = levelGrid.transform;
        mainMenu.backFromLevelSelectButton = CreateButton(mainMenu.levelSelectPanel.transform, "BackFromLevelSelectButton", "BACK", -400);

        // Create Settings Panel
        CreateLabel(mainMenu.settingsPanel.transform, "SettingsTitle", "SETTINGS", 300);
        mainMenu.musicSlider = CreateSlider(mainMenu.settingsPanel.transform, "MusicSlider", "MUSIC", 150);
        mainMenu.sfxSlider = CreateSlider(mainMenu.settingsPanel.transform, "SFXSlider", "SFX", 50);
        mainMenu.vibrationToggle = CreateToggle(mainMenu.settingsPanel.transform, "VibrationToggle", "VIBRATION", -50);
        mainMenu.backFromSettingsButton = CreateButton(mainMenu.settingsPanel.transform, "BackFromSettingsButton", "BACK", -200);

        // Create Credits Panel
        CreateLabel(mainMenu.creditsPanel.transform, "CreditsTitle", "CREDITS", 300);
        CreateLabel(mainMenu.creditsPanel.transform, "CreditsText", "Pixel Cannon Game\n\nMade with Unity", 100);
        CreateButton(mainMenu.creditsPanel.transform, "BackFromCreditsButton", "BACK", -200);
    }

    private GameObject CreateMenuPanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Add semi-transparent background
        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

        return panel;
    }

    private UnityEngine.UI.Button CreateButton(Transform parent, string name, string text, float yOffset)
    {
        GameObject btnObj = new GameObject(name);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.SetParent(parent, false);
        btnRect.sizeDelta = new Vector2(400, 80);
        btnRect.anchoredPosition = new Vector2(0, yOffset);

        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.6f, 1f);

        UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();

        // Setup button colors for interactivity
        var colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.8f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        btn.colors = colors;

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = 36;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btn;
    }

    private void CreateLabel(Transform parent, string name, string text, float yOffset)
    {
        GameObject labelObj = new GameObject(name);
        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(600, 100);
        rect.anchoredPosition = new Vector2(0, yOffset);

        UnityEngine.UI.Text uiText = labelObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = 48;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private UnityEngine.UI.Slider CreateSlider(Transform parent, string name, string label, float yOffset)
    {
        // Container
        GameObject container = new GameObject(name);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.SetParent(parent, false);
        containerRect.sizeDelta = new Vector2(500, 60);
        containerRect.anchoredPosition = new Vector2(0, yOffset);

        // Label
        GameObject labelObj = new GameObject("Label");
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.SetParent(container.transform, false);
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.3f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Text labelText = labelObj.AddComponent<UnityEngine.UI.Text>();
        labelText.text = label;
        labelText.fontSize = 28;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Slider Background
        GameObject bgObj = new GameObject("Background");
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.SetParent(container.transform, false);
        bgRect.anchorMin = new Vector2(0.35f, 0.3f);
        bgRect.anchorMax = new Vector2(1, 0.7f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.SetParent(container.transform, false);
        fillAreaRect.anchorMin = new Vector2(0.35f, 0.3f);
        fillAreaRect.anchorMax = new Vector2(1, 0.7f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.SetParent(fillArea.transform, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image fillImg = fill.AddComponent<UnityEngine.UI.Image>();
        fillImg.color = new Color(0.2f, 0.6f, 1f);

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.SetParent(container.transform, false);
        handleAreaRect.anchorMin = new Vector2(0.35f, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.SetParent(handleArea.transform, false);
        handleRect.sizeDelta = new Vector2(30, 0);
        UnityEngine.UI.Image handleImg = handle.AddComponent<UnityEngine.UI.Image>();
        handleImg.color = Color.white;

        // Slider component
        UnityEngine.UI.Slider slider = container.AddComponent<UnityEngine.UI.Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        return slider;
    }

    private UnityEngine.UI.Toggle CreateToggle(Transform parent, string name, string label, float yOffset)
    {
        GameObject container = new GameObject(name);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.SetParent(parent, false);
        containerRect.sizeDelta = new Vector2(500, 60);
        containerRect.anchoredPosition = new Vector2(0, yOffset);

        // Label
        GameObject labelObj = new GameObject("Label");
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.SetParent(container.transform, false);
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.7f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Text labelText = labelObj.AddComponent<UnityEngine.UI.Text>();
        labelText.text = label;
        labelText.fontSize = 28;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Toggle Background
        GameObject bgObj = new GameObject("Background");
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.SetParent(container.transform, false);
        bgRect.anchorMin = new Vector2(0.8f, 0.2f);
        bgRect.anchorMax = new Vector2(0.95f, 0.8f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Checkmark
        GameObject checkObj = new GameObject("Checkmark");
        RectTransform checkRect = checkObj.AddComponent<RectTransform>();
        checkRect.SetParent(bgObj.transform, false);
        checkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image checkImg = checkObj.AddComponent<UnityEngine.UI.Image>();
        checkImg.color = new Color(0.2f, 0.8f, 0.2f);

        // Toggle component
        UnityEngine.UI.Toggle toggle = container.AddComponent<UnityEngine.UI.Toggle>();
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        toggle.isOn = true;

        return toggle;
    }

    private GameObject CreateLevelGrid(Transform parent)
    {
        GameObject gridObj = new GameObject("LevelGrid");
        RectTransform gridRect = gridObj.AddComponent<RectTransform>();
        gridRect.SetParent(parent, false);
        gridRect.anchorMin = new Vector2(0.1f, 0.2f);
        gridRect.anchorMax = new Vector2(0.9f, 0.8f);
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

        UnityEngine.UI.GridLayoutGroup grid = gridObj.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(10, 10);
        grid.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;

        return gridObj;
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