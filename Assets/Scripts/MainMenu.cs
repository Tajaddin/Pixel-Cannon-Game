using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject difficultyPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Main Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button superHardButton;
    public Button backFromDifficultyButton;

    [Header("Level Select")]
    public Transform levelGridContainer;
    public GameObject levelButtonPrefab;
    public TextMeshProUGUI difficultyTitleText;
    public Button backFromLevelSelectButton;

    [Header("Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle vibrationToggle;
    public Button backFromSettingsButton;

    [Header("Visual")]
    public Image logoImage;
    public ParticleSystem backgroundParticles;

    private Difficulty selectedDifficulty;

    void Start()
    {
        // Auto-find UI elements if not assigned in Inspector
        AutoFindUIElements();

        SetupButtons();
        ShowMainPanel();
        LoadSettings();

        // Play background music if you have an audio manager
        // AudioManager.Instance?.PlayMusic("MainMenu");
    }

    private void AutoFindUIElements()
    {
        // Auto-find panels by name
        if (mainPanel == null) mainPanel = FindChildByName("MainPanel");
        if (difficultyPanel == null) difficultyPanel = FindChildByName("DifficultyPanel");
        if (levelSelectPanel == null) levelSelectPanel = FindChildByName("LevelSelectPanel");
        if (settingsPanel == null) settingsPanel = FindChildByName("SettingsPanel");
        if (creditsPanel == null) creditsPanel = FindChildByName("CreditsPanel");

        // Auto-find main buttons by name
        if (playButton == null) playButton = FindButtonByName("PlayButton");
        if (settingsButton == null) settingsButton = FindButtonByName("SettingsButton");
        if (creditsButton == null) creditsButton = FindButtonByName("CreditsButton");
        if (quitButton == null) quitButton = FindButtonByName("QuitButton");

        // Auto-find difficulty buttons
        if (easyButton == null) easyButton = FindButtonByName("EasyButton");
        if (mediumButton == null) mediumButton = FindButtonByName("MediumButton");
        if (hardButton == null) hardButton = FindButtonByName("HardButton");
        if (superHardButton == null) superHardButton = FindButtonByName("SuperHardButton");
        if (backFromDifficultyButton == null) backFromDifficultyButton = FindButtonByName("BackFromDifficultyButton");

        // Auto-find level select elements
        if (backFromLevelSelectButton == null) backFromLevelSelectButton = FindButtonByName("BackFromLevelSelectButton");
        if (levelGridContainer == null)
        {
            GameObject grid = FindChildByName("LevelGrid");
            if (grid != null) levelGridContainer = grid.transform;
        }

        // Auto-find settings elements
        if (backFromSettingsButton == null) backFromSettingsButton = FindButtonByName("BackFromSettingsButton");
        if (musicSlider == null) musicSlider = FindSliderByName("MusicSlider");
        if (sfxSlider == null) sfxSlider = FindSliderByName("SFXSlider");
        if (vibrationToggle == null) vibrationToggle = FindToggleByName("VibrationToggle");
    }

    private GameObject FindChildByName(string name)
    {
        // Search in this object's hierarchy first
        Transform found = FindInChildren(transform, name);
        if (found != null) return found.gameObject;

        // Search entire scene as fallback
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name == name) return obj;
        }
        return null;
    }

    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private Button FindButtonByName(string name)
    {
        GameObject obj = FindChildByName(name);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

    private Slider FindSliderByName(string name)
    {
        GameObject obj = FindChildByName(name);
        return obj != null ? obj.GetComponent<Slider>() : null;
    }

    private Toggle FindToggleByName(string name)
    {
        GameObject obj = FindChildByName(name);
        return obj != null ? obj.GetComponent<Toggle>() : null;
    }

    private void SetupButtons()
    {
        // Main buttons
        if (playButton != null)
            playButton.onClick.AddListener(ShowDifficultyPanel);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettingsPanel);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCreditsPanel);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Difficulty buttons
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));

        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Medium));

        if (hardButton != null)
            hardButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));

        if (superHardButton != null)
            superHardButton.onClick.AddListener(() => SelectDifficulty(Difficulty.SuperHard));

        if (backFromDifficultyButton != null)
            backFromDifficultyButton.onClick.AddListener(ShowMainPanel);

        // Level select
        if (backFromLevelSelectButton != null)
            backFromLevelSelectButton.onClick.AddListener(ShowDifficultyPanel);

        // Settings
        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(ShowMainPanel);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
    }

    private void LoadSettings()
    {
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
    }

    private void ShowMainPanel()
    {
        HideAllPanels();
        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    private void ShowDifficultyPanel()
    {
        HideAllPanels();
        if (difficultyPanel != null)
            difficultyPanel.SetActive(true);

        // Update button states based on unlocks
        UpdateDifficultyButtons();
    }

    private void UpdateDifficultyButtons()
    {
        // Easy is always unlocked
        if (easyButton != null)
            easyButton.interactable = true;

        // Medium unlocks after completing 10 easy levels
        int easyCompleted = GetCompletedLevelCount(Difficulty.Easy);
        if (mediumButton != null)
            mediumButton.interactable = easyCompleted >= 10;

        // Hard unlocks after completing 10 medium levels
        int mediumCompleted = GetCompletedLevelCount(Difficulty.Medium);
        if (hardButton != null)
            hardButton.interactable = mediumCompleted >= 10;

        // Super Hard unlocks after completing 10 hard levels
        int hardCompleted = GetCompletedLevelCount(Difficulty.Hard);
        if (superHardButton != null)
            superHardButton.interactable = hardCompleted >= 10;
    }

    private int GetCompletedLevelCount(Difficulty difficulty)
    {
        int count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (PlayerPrefs.GetInt($"Level_{difficulty}_{i}", 0) > 0)
                count++;
        }
        return count;
    }

    private void SelectDifficulty(Difficulty difficulty)
    {
        selectedDifficulty = difficulty;
        ShowLevelSelect();
    }

    private void ShowLevelSelect()
    {
        HideAllPanels();
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);

        if (difficultyTitleText != null)
            difficultyTitleText.text = selectedDifficulty.ToString().ToUpper();

        PopulateLevelGrid();
    }

    private void PopulateLevelGrid()
    {
        if (levelGridContainer == null) return;

        // Clear existing buttons
        foreach (Transform child in levelGridContainer)
        {
            Destroy(child.gameObject);
        }

        int unlockedLevel = PlayerPrefs.GetInt($"Unlocked_{selectedDifficulty}", 0);
        int totalLevels = 250; // 250 levels per difficulty = 1000 total

        for (int i = 0; i < totalLevels; i++)
        {
            CreateLevelButton(i, i <= unlockedLevel);
        }
    }

    private void CreateLevelButton(int levelIndex, bool isUnlocked)
    {
        GameObject buttonObj;

        if (levelButtonPrefab != null)
        {
            buttonObj = Instantiate(levelButtonPrefab, levelGridContainer);
        }
        else
        {
            // Create a simple button if no prefab
            buttonObj = new GameObject($"Level_{levelIndex + 1}");
            buttonObj.transform.SetParent(levelGridContainer, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);

            Image img = buttonObj.AddComponent<Image>();
            img.color = isUnlocked ? new Color(0.3f, 0.7f, 1f) : new Color(0.5f, 0.5f, 0.5f);

            Button btn = buttonObj.AddComponent<Button>();

            // Level number text
            GameObject textObj = new GameObject("LevelNumber");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.3f);
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = (levelIndex + 1).ToString();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.white;
        }

        // Setup button
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUnlocked;

            int level = levelIndex;
            button.onClick.AddListener(() => StartLevel(level));
        }

        // Show stars
        int stars = PlayerPrefs.GetInt($"Level_{selectedDifficulty}_{levelIndex}", 0);
        Transform starsContainer = buttonObj.transform.Find("Stars");
        if (starsContainer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Transform star = starsContainer.GetChild(i);
                if (star != null)
                {
                    star.gameObject.SetActive(i < stars);
                }
            }
        }
    }

    private void StartLevel(int levelIndex)
    {
        // Store selected level and difficulty
        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        PlayerPrefs.SetInt("SelectedDifficulty", (int)selectedDifficulty);
        PlayerPrefs.Save();

        // Load game scene
        SceneManager.LoadScene("GameScene");
    }

    private void ShowSettingsPanel()
    {
        HideAllPanels();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void ShowCreditsPanel()
    {
        HideAllPanels();
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        // AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        // AudioManager.Instance?.SetSFXVolume(value);
    }

    private void OnVibrationChanged(bool enabled)
    {
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Animation for logo
    void Update()
    {
        if (logoImage != null)
        {
            float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.02f;
            logoImage.transform.localScale = Vector3.one * scale;
        }
    }
}
