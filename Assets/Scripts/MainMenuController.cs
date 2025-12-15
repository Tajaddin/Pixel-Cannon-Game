using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Enhanced Main Menu Controller with animations, daily rewards, and shop integration
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject splashPanel;
    public GameObject mainPanel;
    public GameObject difficultyPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;
    public GameObject shopPanel;
    public GameObject dailyRewardPanel;
    public GameObject creditsPanel;

    [Header("Logo & Branding")]
    public Image logoImage;
    public TextMeshProUGUI gameTitle;
    public ParticleSystem backgroundParticles;
    public ParticleSystem logoParticles;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button shopButton;
    public Button dailyRewardButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Currency Display")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public GameObject dailyRewardIndicator; // Shows when daily reward is available

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button superHardButton;
    public Button backFromDifficultyButton;
    public TextMeshProUGUI[] difficultyProgressTexts; // Shows X/250 completed

    [Header("Level Select")]
    public Transform levelGridContainer;
    public GameObject levelButtonPrefab;
    public TextMeshProUGUI difficultyTitleText;
    public Button backFromLevelSelectButton;
    public ScrollRect levelScrollRect;

    [Header("Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle vibrationToggle;
    public Button restorePurchasesButton;
    public Button backFromSettingsButton;

    [Header("Shop UI")]
    public Transform shopItemsContainer;
    public GameObject shopItemPrefab;
    public Button backFromShopButton;
    public TextMeshProUGUI shopCoinsText;
    public TextMeshProUGUI shopGemsText;

    [Header("Daily Reward UI")]
    public Transform dailyRewardContainer;
    public GameObject dailyRewardItemPrefab;
    public Button claimDailyRewardButton;
    public Button closeDailyRewardButton;
    public TextMeshProUGUI streakText;
    public TextMeshProUGUI timeUntilNextText;

    [Header("Animation Settings")]
    public float logoAnimationDuration = 2f;
    public float buttonAppearDelay = 0.1f;
    public AnimationCurve logoScaleCurve;
    public AnimationCurve buttonSlideCurve;

    [Header("Colors")]
    public Color easyColor = new Color(0.2f, 0.8f, 0.2f);
    public Color mediumColor = new Color(0.9f, 0.7f, 0.1f);
    public Color hardColor = new Color(0.9f, 0.3f, 0.1f);
    public Color superHardColor = new Color(0.6f, 0.1f, 0.8f);

    private Difficulty selectedDifficulty;
    private bool isInitialized = false;
    private List<Button> mainMenuButtons = new List<Button>();

    void Start()
    {
        InitializeManagers();
        SetupButtons();
        SetupCurrencyListeners();
        StartCoroutine(ShowSplashSequence());
    }

    private void InitializeManagers()
    {
        // Ensure all managers exist
        if (CurrencyManager.Instance == null)
        {
            GameObject currencyObj = new GameObject("CurrencyManager");
            currencyObj.AddComponent<CurrencyManager>();
        }

        if (DailyRewardManager.Instance == null)
        {
            GameObject dailyObj = new GameObject("DailyRewardManager");
            dailyObj.AddComponent<DailyRewardManager>();
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
    }

    private void SetupButtons()
    {
        // Collect main menu buttons for animation
        if (playButton != null) mainMenuButtons.Add(playButton);
        if (shopButton != null) mainMenuButtons.Add(shopButton);
        if (dailyRewardButton != null) mainMenuButtons.Add(dailyRewardButton);
        if (settingsButton != null) mainMenuButtons.Add(settingsButton);
        if (creditsButton != null) mainMenuButtons.Add(creditsButton);
        if (quitButton != null) mainMenuButtons.Add(quitButton);

        // Main buttons
        playButton?.onClick.AddListener(() => ShowPanel(difficultyPanel));
        shopButton?.onClick.AddListener(() => ShowShopPanel());
        dailyRewardButton?.onClick.AddListener(() => ShowDailyRewardPanel());
        settingsButton?.onClick.AddListener(() => ShowPanel(settingsPanel));
        creditsButton?.onClick.AddListener(() => ShowPanel(creditsPanel));
        quitButton?.onClick.AddListener(QuitGame);

        // Difficulty buttons
        easyButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        mediumButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.Medium));
        hardButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
        superHardButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.SuperHard));
        backFromDifficultyButton?.onClick.AddListener(() => ShowPanel(mainPanel));

        // Level select
        backFromLevelSelectButton?.onClick.AddListener(() => ShowPanel(difficultyPanel));

        // Settings
        backFromSettingsButton?.onClick.AddListener(() => ShowPanel(mainPanel));
        musicSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
        vibrationToggle?.onValueChanged.AddListener(OnVibrationChanged);
        restorePurchasesButton?.onClick.AddListener(() => IAPManager.Instance?.RestorePurchases());

        // Shop
        backFromShopButton?.onClick.AddListener(() => ShowPanel(mainPanel));

        // Daily Reward
        claimDailyRewardButton?.onClick.AddListener(ClaimDailyReward);
        closeDailyRewardButton?.onClick.AddListener(() => ShowPanel(mainPanel));

        LoadSettings();
    }

    private void SetupCurrencyListeners()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
            CurrencyManager.Instance.OnGemsChanged += UpdateGemDisplay;
        }

        UpdateCurrencyDisplays();
    }

    private void UpdateCurrencyDisplays()
    {
        if (CurrencyManager.Instance != null)
        {
            UpdateCoinDisplay(CurrencyManager.Instance.Coins);
            UpdateGemDisplay(CurrencyManager.Instance.Gems);
        }
    }

    private void UpdateCoinDisplay(int coins)
    {
        if (coinsText != null)
            coinsText.text = FormatNumber(coins);
        if (shopCoinsText != null)
            shopCoinsText.text = FormatNumber(coins);
    }

    private void UpdateGemDisplay(int gems)
    {
        if (gemsText != null)
            gemsText.text = FormatNumber(gems);
        if (shopGemsText != null)
            shopGemsText.text = FormatNumber(gems);
    }

    private string FormatNumber(int number)
    {
        if (number >= 1000000)
            return (number / 1000000f).ToString("0.#") + "M";
        if (number >= 1000)
            return (number / 1000f).ToString("0.#") + "K";
        return number.ToString();
    }

    #region Splash & Animation Sequence

    private IEnumerator ShowSplashSequence()
    {
        // Hide all panels
        HideAllPanels();

        // Show splash
        if (splashPanel != null)
        {
            splashPanel.SetActive(true);

            // Animate logo
            if (logoImage != null)
            {
                yield return StartCoroutine(AnimateLogo());
            }

            yield return new WaitForSeconds(0.5f);

            // Fade out splash
            yield return StartCoroutine(FadeOutPanel(splashPanel));
        }

        // Show main menu with animation
        yield return StartCoroutine(ShowMainMenuAnimated());

        isInitialized = true;
    }

    private IEnumerator AnimateLogo()
    {
        if (logoImage == null) yield break;

        logoImage.transform.localScale = Vector3.zero;
        float elapsed = 0f;

        // Start particles
        if (logoParticles != null)
            logoParticles.Play();

        while (elapsed < logoAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / logoAnimationDuration;

            // Use animation curve if available
            float scale = logoScaleCurve != null ? logoScaleCurve.Evaluate(t) : EaseOutBack(t);
            logoImage.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        logoImage.transform.localScale = Vector3.one;

        // Play title sound
        AudioManager.Instance?.PlaySFX("ButtonClick");
    }

    private IEnumerator ShowMainMenuAnimated()
    {
        mainPanel?.SetActive(true);

        // Hide buttons initially
        foreach (Button btn in mainMenuButtons)
        {
            if (btn != null)
            {
                btn.transform.localScale = Vector3.zero;
                btn.interactable = false;
            }
        }

        // Animate buttons one by one
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            if (mainMenuButtons[i] != null)
            {
                StartCoroutine(AnimateButtonAppear(mainMenuButtons[i], i * buttonAppearDelay));
            }
        }

        // Wait for all buttons to appear
        yield return new WaitForSeconds(mainMenuButtons.Count * buttonAppearDelay + 0.3f);

        // Enable buttons
        foreach (Button btn in mainMenuButtons)
        {
            if (btn != null)
                btn.interactable = true;
        }

        // Check for daily reward
        UpdateDailyRewardIndicator();

        // Start background particles
        if (backgroundParticles != null)
            backgroundParticles.Play();

        // Play menu music
        AudioManager.Instance?.PlayMusic("MainMenu");
    }

    private IEnumerator AnimateButtonAppear(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);

        float duration = 0.3f;
        float elapsed = 0f;
        RectTransform rect = button.GetComponent<RectTransform>();
        Vector2 targetPos = rect.anchoredPosition;

        // Start off-screen
        rect.anchoredPosition = targetPos + new Vector2(-500f, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = EaseOutBack(t);

            button.transform.localScale = Vector3.one * eased;
            rect.anchoredPosition = Vector2.Lerp(targetPos + new Vector2(-500f, 0), targetPos, eased);

            yield return null;
        }

        button.transform.localScale = Vector3.one;
        rect.anchoredPosition = targetPos;

        AudioManager.Instance?.PlaySFX("ButtonClick");
    }

    private IEnumerator FadeOutPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }

        panel.SetActive(false);
        canvasGroup.alpha = 1f;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    #endregion

    #region Panel Management

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        if (panel != null)
        {
            panel.SetActive(true);
            StartCoroutine(AnimatePanelIn(panel));
        }
        AudioManager.Instance?.PlaySFX("ButtonClick");
    }

    private void HideAllPanels()
    {
        if (splashPanel != null) splashPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (dailyRewardPanel != null) dailyRewardPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private IEnumerator AnimatePanelIn(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / duration;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    #endregion

    #region Difficulty & Level Selection

    private void SelectDifficulty(Difficulty difficulty)
    {
        selectedDifficulty = difficulty;
        UpdateDifficultyButtons();
        ShowLevelSelectPanel();
    }

    private void UpdateDifficultyButtons()
    {
        // Easy always unlocked
        if (easyButton != null) easyButton.interactable = true;

        // Medium unlocks after 10 easy
        int easyCompleted = GetCompletedLevelCount(Difficulty.Easy);
        if (mediumButton != null)
        {
            mediumButton.interactable = easyCompleted >= 10;
            UpdateDifficultyProgress(0, easyCompleted, 250);
        }

        // Hard unlocks after 10 medium
        int mediumCompleted = GetCompletedLevelCount(Difficulty.Medium);
        if (hardButton != null)
        {
            hardButton.interactable = mediumCompleted >= 10;
            UpdateDifficultyProgress(1, mediumCompleted, 250);
        }

        // Super Hard unlocks after 10 hard
        int hardCompleted = GetCompletedLevelCount(Difficulty.Hard);
        if (superHardButton != null)
        {
            superHardButton.interactable = hardCompleted >= 10;
            UpdateDifficultyProgress(2, hardCompleted, 250);
        }

        // Super Hard progress
        int superHardCompleted = GetCompletedLevelCount(Difficulty.SuperHard);
        UpdateDifficultyProgress(3, superHardCompleted, 250);
    }

    private void UpdateDifficultyProgress(int index, int completed, int total)
    {
        if (difficultyProgressTexts != null && index < difficultyProgressTexts.Length && difficultyProgressTexts[index] != null)
        {
            difficultyProgressTexts[index].text = $"{completed}/{total}";
        }
    }

    private int GetCompletedLevelCount(Difficulty difficulty)
    {
        int count = 0;
        for (int i = 0; i < 250; i++)
        {
            if (PlayerPrefs.GetInt($"Level_{difficulty}_{i}", 0) > 0)
                count++;
        }
        return count;
    }

    private void ShowLevelSelectPanel()
    {
        ShowPanel(levelSelectPanel);

        if (difficultyTitleText != null)
        {
            difficultyTitleText.text = selectedDifficulty.ToString().ToUpper();
            difficultyTitleText.color = GetDifficultyColor(selectedDifficulty);
        }

        PopulateLevelGrid();
    }

    private Color GetDifficultyColor(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.Easy: return easyColor;
            case Difficulty.Medium: return mediumColor;
            case Difficulty.Hard: return hardColor;
            case Difficulty.SuperHard: return superHardColor;
            default: return Color.white;
        }
    }

    private void PopulateLevelGrid()
    {
        if (levelGridContainer == null) return;

        // Clear existing
        foreach (Transform child in levelGridContainer)
        {
            Destroy(child.gameObject);
        }

        int unlockedLevel = PlayerPrefs.GetInt($"Unlocked_{selectedDifficulty}", 0);

        for (int i = 0; i < 250; i++)
        {
            CreateLevelButton(i, i <= unlockedLevel);
        }

        // Scroll to last played level
        if (levelScrollRect != null)
        {
            float normalizedPos = Mathf.Clamp01((float)unlockedLevel / 250f);
            levelScrollRect.verticalNormalizedPosition = 1f - normalizedPos;
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
            buttonObj = CreateDefaultLevelButton(levelIndex, isUnlocked);
        }

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUnlocked;

            int level = levelIndex;
            button.onClick.AddListener(() => StartLevel(level));

            // Set button color based on completion
            int stars = PlayerPrefs.GetInt($"Level_{selectedDifficulty}_{levelIndex}", 0);
            Image btnImage = button.GetComponent<Image>();
            if (btnImage != null)
            {
                if (!isUnlocked)
                    btnImage.color = new Color(0.3f, 0.3f, 0.3f);
                else if (stars > 0)
                    btnImage.color = GetDifficultyColor(selectedDifficulty);
                else
                    btnImage.color = new Color(0.5f, 0.5f, 0.6f);
            }
        }

        // Update level number text
        TextMeshProUGUI levelText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (levelText != null)
        {
            levelText.text = (levelIndex + 1).ToString();
        }
    }

    private GameObject CreateDefaultLevelButton(int levelIndex, bool isUnlocked)
    {
        GameObject buttonObj = new GameObject($"Level_{levelIndex + 1}");
        buttonObj.transform.SetParent(levelGridContainer, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);

        Image img = buttonObj.AddComponent<Image>();
        img.color = isUnlocked ? GetDifficultyColor(selectedDifficulty) : new Color(0.3f, 0.3f, 0.3f);

        buttonObj.AddComponent<Button>();

        // Level number
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = (levelIndex + 1).ToString();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 28;
        text.color = Color.white;

        return buttonObj;
    }

    private void StartLevel(int levelIndex)
    {
        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        PlayerPrefs.SetInt("SelectedDifficulty", (int)selectedDifficulty);
        PlayerPrefs.Save();

        AudioManager.Instance?.PlaySFX("ButtonClick");
        SceneManager.LoadScene("GameScene");
    }

    #endregion

    #region Shop

    private void ShowShopPanel()
    {
        ShowPanel(shopPanel);
        PopulateShop();
        UpdateCurrencyDisplays();
    }

    private void PopulateShop()
    {
        if (shopItemsContainer == null || ShopManager.Instance == null) return;

        // Clear existing
        foreach (Transform child in shopItemsContainer)
        {
            Destroy(child.gameObject);
        }

        List<ShopItem> items = ShopManager.Instance.GetAllItems();
        foreach (ShopItem item in items)
        {
            CreateShopItemUI(item);
        }
    }

    private void CreateShopItemUI(ShopItem item)
    {
        GameObject itemObj;

        if (shopItemPrefab != null)
        {
            itemObj = Instantiate(shopItemPrefab, shopItemsContainer);
        }
        else
        {
            itemObj = CreateDefaultShopItem(item);
        }

        // Set up item UI
        TextMeshProUGUI nameText = itemObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = itemObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = itemObj.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        Button buyButton = itemObj.GetComponentInChildren<Button>();

        if (nameText != null) nameText.text = item.name;
        if (descText != null) descText.text = item.description;
        if (priceText != null)
        {
            switch (item.purchaseType)
            {
                case PurchaseType.CoinTemporary:
                    priceText.text = $"{item.coinPrice} Coins";
                    break;
                case PurchaseType.GemTemporary:
                    priceText.text = $"{item.gemPrice} Gems";
                    break;
                case PurchaseType.RealMoneyPermanent:
                    priceText.text = $"${item.realMoneyPrice:F2}";
                    break;
            }
        }

        if (buyButton != null)
        {
            buyButton.interactable = ShopManager.Instance.CanPurchase(item);
            buyButton.onClick.AddListener(() => PurchaseItem(item));
        }
    }

    private GameObject CreateDefaultShopItem(ShopItem item)
    {
        GameObject itemObj = new GameObject(item.id);
        itemObj.transform.SetParent(shopItemsContainer, false);

        RectTransform rect = itemObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 100);

        Image bg = itemObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.3f);

        // Add layout
        VerticalLayoutGroup layout = itemObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 5;

        // Name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(itemObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = item.name;
        nameText.fontSize = 18;
        nameText.color = Color.white;

        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(itemObj.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = item.description;
        descText.fontSize = 12;
        descText.color = Color.gray;

        // Buy button
        GameObject btnObj = new GameObject("BuyButton");
        btnObj.transform.SetParent(itemObj.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.7f, 0.2f);
        btnObj.AddComponent<Button>();

        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
        priceText.alignment = TextAlignmentOptions.Center;
        priceText.color = Color.white;

        return itemObj;
    }

    private void PurchaseItem(ShopItem item)
    {
        if (ShopManager.Instance.PurchaseItem(item))
        {
            AudioManager.Instance?.PlaySFX("ButtonClick");
            UpdateCurrencyDisplays();
            PopulateShop(); // Refresh to update button states
        }
    }

    #endregion

    #region Daily Rewards

    private void UpdateDailyRewardIndicator()
    {
        if (dailyRewardIndicator != null && DailyRewardManager.Instance != null)
        {
            dailyRewardIndicator.SetActive(DailyRewardManager.Instance.CanClaimToday);
        }
    }

    private void ShowDailyRewardPanel()
    {
        ShowPanel(dailyRewardPanel);
        PopulateDailyRewards();
        UpdateDailyRewardUI();
    }

    private void PopulateDailyRewards()
    {
        if (dailyRewardContainer == null || DailyRewardManager.Instance == null) return;

        // Clear existing
        foreach (Transform child in dailyRewardContainer)
        {
            Destroy(child.gameObject);
        }

        List<DailyReward> rewards = DailyRewardManager.Instance.GetWeeklyRewards();
        int currentDay = DailyRewardManager.Instance.GetCurrentDayInCycle();

        for (int i = 0; i < rewards.Count; i++)
        {
            CreateDailyRewardUI(rewards[i], i + 1, currentDay);
        }
    }

    private void CreateDailyRewardUI(DailyReward reward, int dayNumber, int currentDay)
    {
        GameObject itemObj;

        if (dailyRewardItemPrefab != null)
        {
            itemObj = Instantiate(dailyRewardItemPrefab, dailyRewardContainer);
        }
        else
        {
            itemObj = new GameObject($"Day_{dayNumber}");
            itemObj.transform.SetParent(dailyRewardContainer, false);

            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 100);

            Image bg = itemObj.AddComponent<Image>();

            // Color based on state
            if (dayNumber < currentDay)
                bg.color = new Color(0.3f, 0.3f, 0.3f); // Already claimed
            else if (dayNumber == currentDay)
                bg.color = new Color(0.2f, 0.7f, 0.2f); // Current day
            else
                bg.color = new Color(0.4f, 0.4f, 0.5f); // Future day

            // Day text
            GameObject dayObj = new GameObject("Day");
            dayObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI dayText = dayObj.AddComponent<TextMeshProUGUI>();
            dayText.text = $"Day {dayNumber}";
            dayText.fontSize = 14;
            dayText.alignment = TextAlignmentOptions.Top;
            dayText.color = Color.white;

            // Reward text
            GameObject rewardObj = new GameObject("Reward");
            rewardObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI rewardText = rewardObj.AddComponent<TextMeshProUGUI>();
            rewardText.text = reward.description;
            rewardText.fontSize = 12;
            rewardText.alignment = TextAlignmentOptions.Center;
            rewardText.color = Color.yellow;
        }
    }

    private void UpdateDailyRewardUI()
    {
        if (DailyRewardManager.Instance == null) return;

        if (streakText != null)
            streakText.text = $"Streak: {DailyRewardManager.Instance.CurrentStreak} days";

        if (claimDailyRewardButton != null)
            claimDailyRewardButton.interactable = DailyRewardManager.Instance.CanClaimToday;

        if (timeUntilNextText != null)
        {
            if (DailyRewardManager.Instance.CanClaimToday)
            {
                timeUntilNextText.text = "Claim your reward!";
            }
            else
            {
                TimeSpan timeLeft = DailyRewardManager.Instance.GetTimeUntilNextReward();
                timeUntilNextText.text = $"Next reward in: {timeLeft.Hours}h {timeLeft.Minutes}m";
            }
        }
    }

    private void ClaimDailyReward()
    {
        if (DailyRewardManager.Instance.ClaimDailyReward())
        {
            AudioManager.Instance?.PlaySFX("LevelComplete");
            UpdateCurrencyDisplays();
            UpdateDailyRewardUI();
            PopulateDailyRewards();
            UpdateDailyRewardIndicator();
        }
    }

    #endregion

    #region Settings

    private void LoadSettings()
    {
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        AudioManager.Instance?.SetSFXVolume(value);
    }

    private void OnVibrationChanged(bool enabled)
    {
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
    }

    #endregion

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void Update()
    {
        // Gentle logo animation
        if (logoImage != null && isInitialized)
        {
            float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.02f;
            logoImage.transform.localScale = Vector3.one * scale;
        }

        // Update daily reward timer
        if (dailyRewardPanel != null && dailyRewardPanel.activeSelf)
        {
            UpdateDailyRewardUI();
        }
    }

    void OnDestroy()
    {
        // Cleanup listeners
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
            CurrencyManager.Instance.OnGemsChanged -= UpdateGemDisplay;
        }
    }
}
