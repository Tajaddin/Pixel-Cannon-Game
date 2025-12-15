using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI progressText;
    public Slider progressSlider;
    public Button pauseButton;
    public Button fireButton;

    [Header("Slot Display")]
    public Transform slotsContainer;
    public Image[] slotImages;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Level Complete Panel")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI movesText;
    public Image[] starImages;
    public Sprite starFilledSprite;
    public Sprite starEmptySprite;
    public Button nextLevelButton;
    public Button replayButton;
    public Button homeButton;

    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Button playButton;
    public Button[] difficultyButtons;

    [Header("Level Select")]
    public GameObject levelSelectPanel;
    public Transform levelButtonsContainer;
    public GameObject levelButtonPrefab;

    [Header("Settings")]
    public GameObject settingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle vibrationToggle;

    void Start()
    {
        SetupButtons();
        HideAllPanels();
    }

    private void SetupButtons()
    {
        // Pause menu buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => GameManager.Instance?.PauseGame());

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());

        if (restartButton != null)
            restartButton.onClick.AddListener(() => {
                Time.timeScale = 1f;
                GameManager.Instance?.RestartLevel();
                HidePauseMenu();
            });

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });

        // Level complete buttons
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() => {
                HideLevelComplete();
                GameManager.Instance?.NextLevel();
            });

        if (replayButton != null)
            replayButton.onClick.AddListener(() => {
                HideLevelComplete();
                GameManager.Instance?.RestartLevel();
            });

        if (homeButton != null)
            homeButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        // Fire button
        if (fireButton != null)
            fireButton.onClick.AddListener(() => GameManager.Instance?.FireCannons());
    }

    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void UpdateLevelInfo(int level, Difficulty difficulty)
    {
        if (levelText != null)
        {
            levelText.text = $"{difficulty} - Level {level}";
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateProgress(int current, int total)
    {
        if (progressText != null)
        {
            int percentage = total > 0 ? (current * 100 / total) : 0;
            progressText.text = $"{percentage}%";
        }

        if (progressSlider != null)
        {
            progressSlider.maxValue = total;
            progressSlider.value = current;
        }
    }

    public void UpdateSlots(int filledSlots, int maxSlots)
    {
        if (slotImages == null) return;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].gameObject.SetActive(i < maxSlots);
                slotImages[i].color = i < filledSlots ? Color.white : new Color(1, 1, 1, 0.3f);
            }
        }
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void ShowLevelComplete(int score, int stars, int moves)
    {
        if (levelCompletePanel == null) return;

        levelCompletePanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Score: {score}";

        if (movesText != null)
            movesText.text = $"Moves: {moves}";

        // Update stars
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = i < stars ? starFilledSprite : starEmptySprite;
                    
                    // Animate stars
                    StartCoroutine(AnimateStar(starImages[i], i, i < stars));
                }
            }
        }
    }

    private IEnumerator AnimateStar(Image star, int index, bool isFilled)
    {
        yield return new WaitForSeconds(index * 0.2f);

        if (!isFilled) yield break;

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 originalScale = star.transform.localScale;

        star.transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out
            
            float scale = t * 1.2f;
            if (t > 0.8f)
                scale = 1.2f - (t - 0.8f) * 1f;
            
            star.transform.localScale = originalScale * scale;
            yield return null;
        }

        star.transform.localScale = originalScale;
    }

    public void HideLevelComplete()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    public void ShowLevelSelect(Difficulty difficulty)
    {
        if (levelSelectPanel == null || levelButtonsContainer == null) return;

        levelSelectPanel.SetActive(true);

        // Clear existing buttons
        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        // Get unlocked level count
        int unlockedLevel = PlayerPrefs.GetInt($"Unlocked_{difficulty}", 0);

        // Create level buttons (showing first 50 levels)
        int levelsToShow = 50;
        for (int i = 0; i < levelsToShow; i++)
        {
            CreateLevelButton(i, difficulty, i <= unlockedLevel);
        }
    }

    private void CreateLevelButton(int levelIndex, Difficulty difficulty, bool isUnlocked)
    {
        GameObject buttonObj;
        
        if (levelButtonPrefab != null)
        {
            buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);
        }
        else
        {
            buttonObj = new GameObject($"Level_{levelIndex + 1}");
            buttonObj.transform.SetParent(levelButtonsContainer);
            
            Image img = buttonObj.AddComponent<Image>();
            img.color = isUnlocked ? Color.white : Color.gray;
            
            Button btn = buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = (levelIndex + 1).ToString();
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUnlocked;
            
            int level = levelIndex; // Capture for lambda
            Difficulty diff = difficulty;
            button.onClick.AddListener(() => {
                levelSelectPanel.SetActive(false);
                GameManager.Instance?.LoadLevel(level, diff);
            });
        }

        // Show stars if completed
        int stars = PlayerPrefs.GetInt($"Level_{difficulty}_{levelIndex}", 0);
        // You can add star display logic here
    }

    public void HideLevelSelect()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    // Settings
    public void ShowSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        // AudioManager.Instance?.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        // AudioManager.Instance?.SetSFXVolume(volume);
    }

    public void SetVibration(bool enabled)
    {
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
    }
}
