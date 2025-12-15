using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int gems = 0; // Premium currency

    // Events
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnGemsChanged;

    // Keys for PlayerPrefs
    private const string COINS_KEY = "PlayerCoins";
    private const string GEMS_KEY = "PlayerGems";
    private const string TOTAL_COINS_EARNED_KEY = "TotalCoinsEarned";

    public int Coins => coins;
    public int Gems => gems;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCurrency();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadCurrency()
    {
        coins = PlayerPrefs.GetInt(COINS_KEY, 100); // Start with 100 coins
        gems = PlayerPrefs.GetInt(GEMS_KEY, 0);
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.SetInt(GEMS_KEY, gems);
        PlayerPrefs.Save();
    }

    #region Coins

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        coins += amount;

        // Track total coins earned for achievements
        int totalEarned = PlayerPrefs.GetInt(TOTAL_COINS_EARNED_KEY, 0);
        PlayerPrefs.SetInt(TOTAL_COINS_EARNED_KEY, totalEarned + amount);

        SaveCurrency();
        OnCoinsChanged?.Invoke(coins);

        Debug.Log($"Added {amount} coins. Total: {coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (coins < amount)
        {
            Debug.Log($"Not enough coins! Have: {coins}, Need: {amount}");
            return false;
        }

        coins -= amount;
        SaveCurrency();
        OnCoinsChanged?.Invoke(coins);

        Debug.Log($"Spent {amount} coins. Remaining: {coins}");
        return true;
    }

    public bool CanAfford(int amount)
    {
        return coins >= amount;
    }

    #endregion

    #region Gems (Premium Currency)

    public void AddGems(int amount)
    {
        if (amount <= 0) return;

        gems += amount;
        SaveCurrency();
        OnGemsChanged?.Invoke(gems);

        Debug.Log($"Added {amount} gems. Total: {gems}");
    }

    public bool SpendGems(int amount)
    {
        if (amount <= 0) return false;
        if (gems < amount)
        {
            Debug.Log($"Not enough gems! Have: {gems}, Need: {amount}");
            return false;
        }

        gems -= amount;
        SaveCurrency();
        OnGemsChanged?.Invoke(gems);

        Debug.Log($"Spent {amount} gems. Remaining: {gems}");
        return true;
    }

    public bool CanAffordGems(int amount)
    {
        return gems >= amount;
    }

    #endregion

    #region Level Rewards

    /// <summary>
    /// Calculate coin reward based on level completion
    /// </summary>
    public int CalculateLevelReward(Difficulty difficulty, int stars, int movesUsed, int optimalMoves)
    {
        int baseReward = 0;

        // Base reward by difficulty
        switch (difficulty)
        {
            case Difficulty.Easy:
                baseReward = 10;
                break;
            case Difficulty.Medium:
                baseReward = 20;
                break;
            case Difficulty.Hard:
                baseReward = 35;
                break;
            case Difficulty.SuperHard:
                baseReward = 50;
                break;
        }

        // Star bonus
        float starMultiplier = 1f + (stars * 0.25f); // 1.25x for 1 star, 1.75x for 3 stars

        // Efficiency bonus (completing under optimal moves)
        float efficiencyBonus = 1f;
        if (movesUsed <= optimalMoves)
        {
            efficiencyBonus = 1.5f; // 50% bonus for perfect completion
        }

        int totalReward = Mathf.RoundToInt(baseReward * starMultiplier * efficiencyBonus);

        return totalReward;
    }

    /// <summary>
    /// Award coins for completing a level
    /// </summary>
    public void AwardLevelCompletion(Difficulty difficulty, int stars, int movesUsed, int optimalMoves, bool isFirstCompletion)
    {
        int reward = CalculateLevelReward(difficulty, stars, movesUsed, optimalMoves);

        // First completion bonus
        if (isFirstCompletion)
        {
            reward = Mathf.RoundToInt(reward * 1.5f);
        }

        AddCoins(reward);
    }

    #endregion

    #region Debug/Testing

    [ContextMenu("Add 1000 Coins")]
    public void Debug_AddCoins()
    {
        AddCoins(1000);
    }

    [ContextMenu("Add 100 Gems")]
    public void Debug_AddGems()
    {
        AddGems(100);
    }

    [ContextMenu("Reset Currency")]
    public void Debug_ResetCurrency()
    {
        coins = 100;
        gems = 0;
        SaveCurrency();
        OnCoinsChanged?.Invoke(coins);
        OnGemsChanged?.Invoke(gems);
    }

    #endregion
}
