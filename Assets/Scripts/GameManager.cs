using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int maxSlots = 5;
    public float cannonSpeed = 10f;
    public float bubbleDropSpeed = 5f;

    [Header("References")]
    public PixelGrid pixelGrid;
    public CannonManager cannonManager;
    public UIManager uiManager;
    public Transform[] cannonSlots;

    [Header("Current Level")]
    public int currentLevelIndex = 0;
    public LevelData currentLevel;
    public Difficulty currentDifficulty = Difficulty.Easy;

    [Header("Game State")]
    public int score = 0;
    public int movesUsed = 0;
    public bool isGameActive = false;
    public bool isLevelComplete = false;

    // Events
    public delegate void GameEvent();
    public event GameEvent OnLevelComplete;
    public event GameEvent OnGameOver;
    public event GameEvent OnScoreChanged;

    private List<Cannon> cannonsInSlots = new List<Cannon>();
    private int totalPixelsToFill;
    private int pixelsFilled;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadLevel(currentLevelIndex, currentDifficulty);
    }

    public void LoadLevel(int levelIndex, Difficulty difficulty)
    {
        currentLevelIndex = levelIndex;
        currentDifficulty = difficulty;
        
        // Load level data from Resources
        string path = $"Levels/{difficulty}/{levelIndex}";
        TextAsset levelJson = Resources.Load<TextAsset>(path);
        
        if (levelJson != null)
        {
            currentLevel = JsonUtility.FromJson<LevelData>(levelJson.text);
        }
        else
        {
            // Generate a sample level if not found
            currentLevel = LevelGenerator.GenerateSampleLevel(difficulty);
        }

        StartLevel();
    }

    public void StartLevel()
    {
        isGameActive = true;
        isLevelComplete = false;
        score = 0;
        movesUsed = 0;
        pixelsFilled = 0;
        cannonsInSlots.Clear();

        // Initialize the pixel grid
        if (pixelGrid != null)
        {
            pixelGrid.InitializeGrid(currentLevel);
            totalPixelsToFill = pixelGrid.GetTotalColoredPixels();
        }

        // Spawn initial cannons
        if (cannonManager != null)
        {
            cannonManager.SpawnInitialCannons(currentLevel);
        }

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateLevelInfo(currentLevelIndex + 1, currentDifficulty);
            uiManager.UpdateScore(score);
            uiManager.UpdateProgress(0, totalPixelsToFill);
        }
    }

    public bool TryAddCannonToSlot(Cannon cannon)
    {
        if (cannonsInSlots.Count >= maxSlots)
        {
            // Slots are full - fire the cannons!
            FireCannons();
            return false;
        }

        cannonsInSlots.Add(cannon);
        
        // Position cannon in slot
        if (cannonSlots != null && cannonsInSlots.Count <= cannonSlots.Length)
        {
            cannon.MoveToSlot(cannonSlots[cannonsInSlots.Count - 1].position);
        }

        // Check if slots are full
        if (cannonsInSlots.Count >= maxSlots)
        {
            FireCannons();
        }

        return true;
    }

    public void FireCannons()
    {
        if (!isGameActive || cannonsInSlots.Count == 0) return;

        StartCoroutine(FireCannonsSequence());
    }

    private IEnumerator FireCannonsSequence()
    {
        isGameActive = false; // Pause input during firing

        foreach (Cannon cannon in cannonsInSlots)
        {
            // Fire each cannon
            int pixelsHit = pixelGrid.FireCannonAtColor(cannon.cannonColor, cannon.power);
            pixelsFilled += pixelsHit;
            score += pixelsHit * 10;
            movesUsed++;

            // Update UI
            uiManager?.UpdateScore(score);
            uiManager?.UpdateProgress(pixelsFilled, totalPixelsToFill);

            // Animate cannon firing
            yield return StartCoroutine(cannon.FireAnimation());
            
            // Small delay between cannons
            yield return new WaitForSeconds(0.2f);
        }

        // Clear slots
        foreach (Cannon cannon in cannonsInSlots)
        {
            Destroy(cannon.gameObject);
        }
        cannonsInSlots.Clear();

        // Check win condition
        if (pixelsFilled >= totalPixelsToFill)
        {
            LevelComplete();
        }
        else
        {
            isGameActive = true;
            // Spawn new cannons
            cannonManager?.SpawnNewCannons(3);
        }
    }

    private void LevelComplete()
    {
        isLevelComplete = true;
        isGameActive = false;

        // Calculate stars based on moves
        int stars = CalculateStars();
        
        // Save progress
        SaveProgress(stars);

        // Trigger event
        OnLevelComplete?.Invoke();

        // Show UI
        uiManager?.ShowLevelComplete(score, stars, movesUsed);
    }

    private int CalculateStars()
    {
        int optimalMoves = currentLevel.optimalMoves;
        
        if (movesUsed <= optimalMoves)
            return 3;
        else if (movesUsed <= optimalMoves * 1.5f)
            return 2;
        else
            return 1;
    }

    private void SaveProgress(int stars)
    {
        string key = $"Level_{currentDifficulty}_{currentLevelIndex}";
        int previousStars = PlayerPrefs.GetInt(key, 0);
        
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(key, stars);
        }

        // Unlock next level
        int unlockedKey = PlayerPrefs.GetInt($"Unlocked_{currentDifficulty}", 0);
        if (currentLevelIndex >= unlockedKey)
        {
            PlayerPrefs.SetInt($"Unlocked_{currentDifficulty}", currentLevelIndex + 1);
        }

        PlayerPrefs.Save();
    }

    public void NextLevel()
    {
        currentLevelIndex++;
        LoadLevel(currentLevelIndex, currentDifficulty);
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex, currentDifficulty);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        uiManager?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        isGameActive = true;
        Time.timeScale = 1f;
        uiManager?.HidePauseMenu();
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    SuperHard
}
