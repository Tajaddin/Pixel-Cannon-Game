using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DailyReward
{
    public int day;
    public RewardType type;
    public int amount;
    public string description;

    public enum RewardType
    {
        Coins,
        Gems,
        TemporarySlot,      // Extra cannon slot for X hours
        TemporaryPowerUp,   // Power boost for X levels
        FreeUnlock          // Unlock a random level
    }
}

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [Header("Daily Rewards Configuration")]
    [SerializeField] private List<DailyReward> weeklyRewards = new List<DailyReward>();

    // Events
    public event Action<DailyReward> OnRewardClaimed;
    public event Action<int> OnStreakChanged;

    // PlayerPrefs Keys
    private const string LAST_CLAIM_DATE_KEY = "LastDailyClaimDate";
    private const string CURRENT_STREAK_KEY = "DailyStreak";
    private const string TOTAL_DAYS_CLAIMED_KEY = "TotalDaysClaimed";

    private int currentStreak = 0;
    private DateTime lastClaimDate;
    private bool hasClaimedToday = false;

    public int CurrentStreak => currentStreak;
    public bool CanClaimToday => !hasClaimedToday;
    public int TotalDaysClaimed => PlayerPrefs.GetInt(TOTAL_DAYS_CLAIMED_KEY, 0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultRewards();
            LoadState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CheckDailyReset();
    }

    private void InitializeDefaultRewards()
    {
        if (weeklyRewards.Count > 0) return;

        // Day 1: Small coins
        weeklyRewards.Add(new DailyReward
        {
            day = 1,
            type = DailyReward.RewardType.Coins,
            amount = 50,
            description = "50 Coins"
        });

        // Day 2: More coins
        weeklyRewards.Add(new DailyReward
        {
            day = 2,
            type = DailyReward.RewardType.Coins,
            amount = 75,
            description = "75 Coins"
        });

        // Day 3: Temporary extra slot (4 hours)
        weeklyRewards.Add(new DailyReward
        {
            day = 3,
            type = DailyReward.RewardType.TemporarySlot,
            amount = 4, // hours
            description = "+1 Cannon Slot (4 hours)"
        });

        // Day 4: Bigger coins
        weeklyRewards.Add(new DailyReward
        {
            day = 4,
            type = DailyReward.RewardType.Coins,
            amount = 100,
            description = "100 Coins"
        });

        // Day 5: Gems!
        weeklyRewards.Add(new DailyReward
        {
            day = 5,
            type = DailyReward.RewardType.Gems,
            amount = 5,
            description = "5 Gems"
        });

        // Day 6: Temporary slot (8 hours)
        weeklyRewards.Add(new DailyReward
        {
            day = 6,
            type = DailyReward.RewardType.TemporarySlot,
            amount = 8, // hours
            description = "+1 Cannon Slot (8 hours)"
        });

        // Day 7: BIG reward!
        weeklyRewards.Add(new DailyReward
        {
            day = 7,
            type = DailyReward.RewardType.Gems,
            amount = 15,
            description = "15 Gems + Bonus!"
        });
    }

    private void LoadState()
    {
        currentStreak = PlayerPrefs.GetInt(CURRENT_STREAK_KEY, 0);

        string lastClaimStr = PlayerPrefs.GetString(LAST_CLAIM_DATE_KEY, "");
        if (!string.IsNullOrEmpty(lastClaimStr))
        {
            if (DateTime.TryParse(lastClaimStr, out DateTime parsed))
            {
                lastClaimDate = parsed;
            }
        }
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt(CURRENT_STREAK_KEY, currentStreak);
        PlayerPrefs.SetString(LAST_CLAIM_DATE_KEY, lastClaimDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }

    private void CheckDailyReset()
    {
        DateTime today = DateTime.Today;
        DateTime yesterday = today.AddDays(-1);

        // Check if already claimed today
        if (lastClaimDate.Date == today)
        {
            hasClaimedToday = true;
            return;
        }

        hasClaimedToday = false;

        // Check if streak should be reset (missed a day)
        if (lastClaimDate.Date < yesterday)
        {
            // Missed more than one day - reset streak
            currentStreak = 0;
            SaveState();
            OnStreakChanged?.Invoke(currentStreak);
            Debug.Log("Daily streak reset - missed a day!");
        }
    }

    public DailyReward GetTodaysReward()
    {
        // Get reward based on streak (1-7, then loops)
        int rewardDay = (currentStreak % 7) + 1;

        foreach (var reward in weeklyRewards)
        {
            if (reward.day == rewardDay)
                return reward;
        }

        // Fallback
        return weeklyRewards[0];
    }

    public DailyReward GetRewardForDay(int day)
    {
        int rewardDay = ((day - 1) % 7) + 1;
        foreach (var reward in weeklyRewards)
        {
            if (reward.day == rewardDay)
                return reward;
        }
        return weeklyRewards[0];
    }

    public bool ClaimDailyReward()
    {
        if (hasClaimedToday)
        {
            Debug.Log("Already claimed today's reward!");
            return false;
        }

        DailyReward reward = GetTodaysReward();

        // Give the reward
        switch (reward.type)
        {
            case DailyReward.RewardType.Coins:
                CurrencyManager.Instance?.AddCoins(reward.amount);
                break;

            case DailyReward.RewardType.Gems:
                CurrencyManager.Instance?.AddGems(reward.amount);
                // Day 7 bonus: also give coins
                if (reward.day == 7)
                {
                    CurrencyManager.Instance?.AddCoins(200);
                }
                break;

            case DailyReward.RewardType.TemporarySlot:
                ShopManager.Instance?.ActivateTemporarySlot(reward.amount);
                break;

            case DailyReward.RewardType.TemporaryPowerUp:
                // Implement power-up system
                break;

            case DailyReward.RewardType.FreeUnlock:
                // Unlock a random locked level
                UnlockRandomLevel();
                break;
        }

        // Update state
        currentStreak++;
        hasClaimedToday = true;
        lastClaimDate = DateTime.Today;

        int totalDays = PlayerPrefs.GetInt(TOTAL_DAYS_CLAIMED_KEY, 0);
        PlayerPrefs.SetInt(TOTAL_DAYS_CLAIMED_KEY, totalDays + 1);

        SaveState();

        OnRewardClaimed?.Invoke(reward);
        OnStreakChanged?.Invoke(currentStreak);

        Debug.Log($"Claimed daily reward: {reward.description} (Streak: {currentStreak})");
        return true;
    }

    private void UnlockRandomLevel()
    {
        // Find a random locked level and unlock it
        System.Random rng = new System.Random();
        Difficulty[] difficulties = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard, Difficulty.SuperHard };

        foreach (var diff in difficulties)
        {
            int unlocked = PlayerPrefs.GetInt($"Unlocked_{diff}", 0);
            if (unlocked < 249) // If there are locked levels
            {
                // Unlock the next level
                PlayerPrefs.SetInt($"Unlocked_{diff}", unlocked + 1);
                PlayerPrefs.Save();
                Debug.Log($"Unlocked {diff} level {unlocked + 1}!");
                return;
            }
        }
    }

    public TimeSpan GetTimeUntilNextReward()
    {
        if (!hasClaimedToday)
        {
            return TimeSpan.Zero; // Can claim now
        }

        DateTime tomorrow = DateTime.Today.AddDays(1);
        return tomorrow - DateTime.Now;
    }

    public List<DailyReward> GetWeeklyRewards()
    {
        return new List<DailyReward>(weeklyRewards);
    }

    public int GetCurrentDayInCycle()
    {
        return (currentStreak % 7) + 1;
    }

    #region Debug

    [ContextMenu("Reset Streak")]
    public void Debug_ResetStreak()
    {
        currentStreak = 0;
        hasClaimedToday = false;
        lastClaimDate = DateTime.Today.AddDays(-2);
        SaveState();
        Debug.Log("Streak reset for testing");
    }

    [ContextMenu("Simulate New Day")]
    public void Debug_SimulateNewDay()
    {
        hasClaimedToday = false;
        lastClaimDate = DateTime.Today.AddDays(-1);
        Debug.Log("Simulated new day - can claim again");
    }

    #endregion
}
