using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int baseMaxSlots = 5;
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
    private bool isFirstCompletion = false;

    // Dynamic max slots based on shop purchases
    public int MaxSlots
    {
        get
        {
            int slots = baseMaxSlots;
            if (ShopManager.Instance != null)
            {
                slots = ShopManager.Instance.CurrentMaxSlots;
            }
            return slots;
        }
    }

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
        InitializeManagers();
        LoadLevelFromPlayerPrefs();
    }

    private void InitializeManagers()
    {
        // Ensure essential managers exist
        if (CurrencyManager.Instance == null)
        {
            GameObject currencyObj = new GameObject("CurrencyManager");
            currencyObj.AddComponent<CurrencyManager>();
        }

        if (ShopManager.Instance == null)
        {
            GameObject shopObj = new GameObject("ShopManager");
            shopObj.AddComponent<ShopManager>();
        }

        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }

        // Subscribe to shop events
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnSlotsChanged += OnSlotsChanged;
        }
    }

    private void OnSlotsChanged()
    {
        // Update UI when slots change
        uiManager?.UpdateSlots(cannonsInSlots.Count, MaxSlots);
    }

    private void LoadLevelFromPlayerPrefs()
    {
        // Load selected level and difficulty from PlayerPrefs (set by main menu)
        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        currentDifficulty = (Difficulty)PlayerPrefs.GetInt("SelectedDifficulty", 0);

        LoadLevel(currentLevelIndex, currentDifficulty);
    }

    public void LoadLevel(int levelIndex, Difficulty difficulty)
    {
        currentLevelIndex = levelIndex;
        currentDifficulty = difficulty;

        // Check if this is the first time playing this level
        string key = $"Level_{currentDifficulty}_{currentLevelIndex}";
        isFirstCompletion = PlayerPrefs.GetInt(key, 0) == 0;

        // Load level data from Resources
        string path = $"Levels/{difficulty}/{levelIndex}";
        TextAsset levelJson = Resources.Load<TextAsset>(path);

        if (levelJson != null)
        {
            currentLevel = JsonUtility.FromJson<LevelData>(levelJson.text);
        }
        else
        {
            // Generate a procedural level if not found
            currentLevel = LevelGenerator.GenerateLevel(levelIndex, difficulty);
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
            uiManager.UpdateSlots(0, MaxSlots);
        }

        // Play gameplay music
        AudioManager.Instance?.PlayMusic("Gameplay");
    }

    public bool TryAddCannonToSlot(Cannon cannon)
    {
        if (cannonsInSlots.Count >= MaxSlots)
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

        // Update UI
        uiManager?.UpdateSlots(cannonsInSlots.Count, MaxSlots);

        // Play sound
        AudioManager.Instance?.PlaySFX("SlotFill");

        // Check if slots are full
        if (cannonsInSlots.Count >= MaxSlots)
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

            // Apply coin multiplier from shop
            float coinMultiplier = ShopManager.Instance?.GetCoinMultiplier() ?? 1f;
            int coinBonus = Mathf.RoundToInt(pixelsHit * coinMultiplier);

            // Update UI
            uiManager?.UpdateScore(score);
            uiManager?.UpdateProgress(pixelsFilled, totalPixelsToFill);

            // Play sound for pixel fill
            if (pixelsHit > 0)
            {
                AudioManager.Instance?.PlaySFX("PixelFill");
            }

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

        // Update slots UI
        uiManager?.UpdateSlots(0, MaxSlots);

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

        // Award coins
        AwardCoins(stars);

        // Save progress
        SaveProgress(stars);

        // Play victory music
        AudioManager.Instance?.PlayMusic("Victory");

        // Vibrate
        AudioManager.Instance?.Vibrate();

        // Trigger event
        OnLevelComplete?.Invoke();

        // Show UI
        uiManager?.ShowLevelComplete(score, stars, movesUsed);

        // Show interstitial ad occasionally
        AdManager.Instance?.ShowInterstitialAd();
    }

    private void AwardCoins(int stars)
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AwardLevelCompletion(
                currentDifficulty,
                stars,
                movesUsed,
                currentLevel.optimalMoves,
                isFirstCompletion
            );
        }
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

        // Check if we need to unlock the next difficulty
        if (currentLevelIndex >= 250)
        {
            // Move to next difficulty if available
            if ((int)currentDifficulty < 3)
            {
                currentDifficulty = (Difficulty)((int)currentDifficulty + 1);
                currentLevelIndex = 0;
            }
            else
            {
                // All levels complete!
                currentLevelIndex = 249;
            }
        }

        LoadLevel(currentLevelIndex, currentDifficulty);
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex, currentDifficulty);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
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

    public int GetSlotsUsed()
    {
        return cannonsInSlots.Count;
    }

    public int GetSlotsRemaining()
    {
        return MaxSlots - cannonsInSlots.Count;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnSlotsChanged -= OnSlotsChanged;
        }
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    SuperHard
}
