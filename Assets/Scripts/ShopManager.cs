using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string id;
    public string name;
    public string description;
    public ShopItemType itemType;
    public PurchaseType purchaseType;
    public int coinPrice;       // For coin purchases (temporary)
    public int gemPrice;        // For gem purchases (temporary premium)
    public float realMoneyPrice; // For real money (permanent)
    public int duration;        // Hours for temporary items
    public int value;           // Effect value (e.g., +1 slot)
    public Sprite icon;
}

public enum ShopItemType
{
    ExtraSlot,          // Additional cannon slot
    PowerBoost,         // Cannon power multiplier
    ColorHint,          // Shows which color to pick next
    UndoMove,           // Undo last cannon selection
    TimeFreeze,         // Pause timer in timed modes
    DoubleCoins,        // 2x coins for X hours
    RemoveAds           // Remove all ads
}

public enum PurchaseType
{
    CoinTemporary,      // Buy with coins, temporary effect
    GemTemporary,       // Buy with gems, temporary effect
    RealMoneyPermanent  // Buy with real money, permanent
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("Slot Settings")]
    [SerializeField] private int baseSlotCount = 5;
    [SerializeField] private int maxPermanentSlots = 8;
    [SerializeField] private int maxTemporarySlots = 2; // Can have up to 2 temp slots

    // Events
    public event Action<ShopItem> OnItemPurchased;
    public event Action OnSlotsChanged;

    // Active temporary effects
    private Dictionary<ShopItemType, DateTime> temporaryEffects = new Dictionary<ShopItemType, DateTime>();

    // PlayerPrefs keys
    private const string PERMANENT_SLOTS_KEY = "PermanentExtraSlots";
    private const string TEMP_SLOT_EXPIRY_KEY = "TempSlotExpiry";
    private const string TEMP_SLOT_COUNT_KEY = "TempSlotCount";

    private int permanentExtraSlots = 0;
    private int temporaryExtraSlots = 0;
    private DateTime tempSlotExpiry;

    public int CurrentMaxSlots => baseSlotCount + permanentExtraSlots + temporaryExtraSlots;
    public int PermanentSlots => permanentExtraSlots;
    public int TemporarySlots => temporaryExtraSlots;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultItems();
            LoadState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CheckTemporaryEffects();
    }

    void Update()
    {
        // Check expired effects every second
        CheckTemporaryEffects();
    }

    private void InitializeDefaultItems()
    {
        if (shopItems.Count > 0) return;

        // Temporary slot - Coins (cheap, short duration)
        shopItems.Add(new ShopItem
        {
            id = "temp_slot_1h",
            name = "+1 Slot (1 Hour)",
            description = "Add one extra cannon slot for 1 hour",
            itemType = ShopItemType.ExtraSlot,
            purchaseType = PurchaseType.CoinTemporary,
            coinPrice = 100,
            duration = 1,
            value = 1
        });

        // Temporary slot - Coins (medium)
        shopItems.Add(new ShopItem
        {
            id = "temp_slot_4h",
            name = "+1 Slot (4 Hours)",
            description = "Add one extra cannon slot for 4 hours",
            itemType = ShopItemType.ExtraSlot,
            purchaseType = PurchaseType.CoinTemporary,
            coinPrice = 300,
            duration = 4,
            value = 1
        });

        // Temporary slot - Coins (longer)
        shopItems.Add(new ShopItem
        {
            id = "temp_slot_24h",
            name = "+1 Slot (24 Hours)",
            description = "Add one extra cannon slot for 24 hours",
            itemType = ShopItemType.ExtraSlot,
            purchaseType = PurchaseType.CoinTemporary,
            coinPrice = 800,
            duration = 24,
            value = 1
        });

        // Temporary slot - Gems (premium temp)
        shopItems.Add(new ShopItem
        {
            id = "temp_slot_week",
            name = "+1 Slot (1 Week)",
            description = "Add one extra cannon slot for 1 week",
            itemType = ShopItemType.ExtraSlot,
            purchaseType = PurchaseType.GemTemporary,
            gemPrice = 20,
            duration = 168, // 7 days
            value = 1
        });

        // Permanent slot - Real money
        shopItems.Add(new ShopItem
        {
            id = "perm_slot_1",
            name = "+1 Permanent Slot",
            description = "Permanently add one extra cannon slot",
            itemType = ShopItemType.ExtraSlot,
            purchaseType = PurchaseType.RealMoneyPermanent,
            realMoneyPrice = 1.99f,
            value = 1
        });

        // Permanent 2 slots bundle - Real money
        shopItems.Add(new ShopItem
        {
            id = "perm_slot_2",
            name = "+2 Permanent Slots",
            description = "Permanently add two extra cannon slots (Save 20%!)",
            itemType = ShopItemType.ExtraSlot,
            purchaseType = PurchaseType.RealMoneyPermanent,
            realMoneyPrice = 2.99f,
            value = 2
        });

        // Double Coins - Temp
        shopItems.Add(new ShopItem
        {
            id = "double_coins_2h",
            name = "Double Coins (2 Hours)",
            description = "Earn 2x coins from levels for 2 hours",
            itemType = ShopItemType.DoubleCoins,
            purchaseType = PurchaseType.CoinTemporary,
            coinPrice = 200,
            duration = 2,
            value = 2
        });

        // Power Boost - Temp
        shopItems.Add(new ShopItem
        {
            id = "power_boost_1h",
            name = "Power Boost (1 Hour)",
            description = "All cannons have +50% power for 1 hour",
            itemType = ShopItemType.PowerBoost,
            purchaseType = PurchaseType.CoinTemporary,
            coinPrice = 150,
            duration = 1,
            value = 50 // 50% boost
        });

        // Remove Ads - Permanent
        shopItems.Add(new ShopItem
        {
            id = "remove_ads",
            name = "Remove Ads Forever",
            description = "Permanently remove all advertisements",
            itemType = ShopItemType.RemoveAds,
            purchaseType = PurchaseType.RealMoneyPermanent,
            realMoneyPrice = 3.99f,
            value = 1
        });
    }

    private void LoadState()
    {
        permanentExtraSlots = PlayerPrefs.GetInt(PERMANENT_SLOTS_KEY, 0);

        string expiryStr = PlayerPrefs.GetString(TEMP_SLOT_EXPIRY_KEY, "");
        if (!string.IsNullOrEmpty(expiryStr) && DateTime.TryParse(expiryStr, out DateTime expiry))
        {
            tempSlotExpiry = expiry;
            if (DateTime.Now < tempSlotExpiry)
            {
                temporaryExtraSlots = PlayerPrefs.GetInt(TEMP_SLOT_COUNT_KEY, 0);
            }
        }
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt(PERMANENT_SLOTS_KEY, permanentExtraSlots);
        PlayerPrefs.SetString(TEMP_SLOT_EXPIRY_KEY, tempSlotExpiry.ToString());
        PlayerPrefs.SetInt(TEMP_SLOT_COUNT_KEY, temporaryExtraSlots);
        PlayerPrefs.Save();
    }

    private void CheckTemporaryEffects()
    {
        // Check if temporary slots expired
        if (temporaryExtraSlots > 0 && DateTime.Now >= tempSlotExpiry)
        {
            temporaryExtraSlots = 0;
            SaveState();
            OnSlotsChanged?.Invoke();
            Debug.Log("Temporary slots expired!");
        }
    }

    public List<ShopItem> GetAllItems()
    {
        return new List<ShopItem>(shopItems);
    }

    public List<ShopItem> GetItemsByType(ShopItemType type)
    {
        return shopItems.FindAll(item => item.itemType == type);
    }

    public ShopItem GetItem(string itemId)
    {
        return shopItems.Find(item => item.id == itemId);
    }

    public bool CanPurchase(ShopItem item)
    {
        switch (item.purchaseType)
        {
            case PurchaseType.CoinTemporary:
                return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(item.coinPrice);

            case PurchaseType.GemTemporary:
                return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAffordGems(item.gemPrice);

            case PurchaseType.RealMoneyPermanent:
                // Check if already purchased (for one-time items like remove ads)
                if (item.itemType == ShopItemType.RemoveAds)
                {
                    return PlayerPrefs.GetInt("IsPremium", 0) != 1;
                }
                // Check if max slots reached
                if (item.itemType == ShopItemType.ExtraSlot)
                {
                    return permanentExtraSlots + item.value <= maxPermanentSlots - baseSlotCount;
                }
                return true;
        }

        return false;
    }

    public bool PurchaseItem(string itemId)
    {
        ShopItem item = GetItem(itemId);
        if (item == null)
        {
            Debug.LogError($"Item not found: {itemId}");
            return false;
        }

        return PurchaseItem(item);
    }

    public bool PurchaseItem(ShopItem item)
    {
        if (!CanPurchase(item))
        {
            Debug.Log($"Cannot purchase {item.name}");
            return false;
        }

        bool success = false;

        switch (item.purchaseType)
        {
            case PurchaseType.CoinTemporary:
                success = CurrencyManager.Instance.SpendCoins(item.coinPrice);
                if (success) ApplyTemporaryEffect(item);
                break;

            case PurchaseType.GemTemporary:
                success = CurrencyManager.Instance.SpendGems(item.gemPrice);
                if (success) ApplyTemporaryEffect(item);
                break;

            case PurchaseType.RealMoneyPermanent:
                // Trigger IAP purchase
                success = ProcessRealMoneyPurchase(item);
                break;
        }

        if (success)
        {
            OnItemPurchased?.Invoke(item);
            Debug.Log($"Purchased: {item.name}");
        }

        return success;
    }

    private void ApplyTemporaryEffect(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.ExtraSlot:
                ActivateTemporarySlot(item.duration);
                break;

            case ShopItemType.DoubleCoins:
            case ShopItemType.PowerBoost:
                // Track effect expiry
                DateTime expiry = DateTime.Now.AddHours(item.duration);
                temporaryEffects[item.itemType] = expiry;
                PlayerPrefs.SetString($"TempEffect_{item.itemType}", expiry.ToString());
                break;
        }
    }

    public void ActivateTemporarySlot(int hours)
    {
        if (temporaryExtraSlots >= maxTemporarySlots)
        {
            Debug.Log("Max temporary slots reached!");
            return;
        }

        // Extend or set new expiry
        if (temporaryExtraSlots > 0 && DateTime.Now < tempSlotExpiry)
        {
            // Extend existing time
            tempSlotExpiry = tempSlotExpiry.AddHours(hours);
        }
        else
        {
            // New temporary slot
            temporaryExtraSlots = 1;
            tempSlotExpiry = DateTime.Now.AddHours(hours);
        }

        SaveState();
        OnSlotsChanged?.Invoke();
        Debug.Log($"Temporary slot activated! Expires: {tempSlotExpiry}");
    }

    private bool ProcessRealMoneyPurchase(ShopItem item)
    {
        // This would integrate with Unity IAP
        // For now, simulate successful purchase

        switch (item.itemType)
        {
            case ShopItemType.ExtraSlot:
                permanentExtraSlots += item.value;
                permanentExtraSlots = Mathf.Min(permanentExtraSlots, maxPermanentSlots - baseSlotCount);
                SaveState();
                OnSlotsChanged?.Invoke();
                break;

            case ShopItemType.RemoveAds:
                AdManager.Instance?.RemoveAds();
                break;
        }

        // In real implementation, return true only after IAP confirms purchase
        return true;
    }

    public bool HasActiveEffect(ShopItemType effectType)
    {
        if (!temporaryEffects.ContainsKey(effectType))
        {
            // Check PlayerPrefs
            string expiryStr = PlayerPrefs.GetString($"TempEffect_{effectType}", "");
            if (!string.IsNullOrEmpty(expiryStr) && DateTime.TryParse(expiryStr, out DateTime expiry))
            {
                if (DateTime.Now < expiry)
                {
                    temporaryEffects[effectType] = expiry;
                    return true;
                }
            }
            return false;
        }

        return DateTime.Now < temporaryEffects[effectType];
    }

    public TimeSpan GetEffectRemainingTime(ShopItemType effectType)
    {
        if (!HasActiveEffect(effectType))
            return TimeSpan.Zero;

        return temporaryEffects[effectType] - DateTime.Now;
    }

    public TimeSpan GetTemporarySlotRemainingTime()
    {
        if (temporaryExtraSlots <= 0 || DateTime.Now >= tempSlotExpiry)
            return TimeSpan.Zero;

        return tempSlotExpiry - DateTime.Now;
    }

    public float GetPowerMultiplier()
    {
        if (HasActiveEffect(ShopItemType.PowerBoost))
        {
            return 1.5f; // 50% boost
        }
        return 1f;
    }

    public float GetCoinMultiplier()
    {
        if (HasActiveEffect(ShopItemType.DoubleCoins))
        {
            return 2f;
        }
        return 1f;
    }

    #region Debug

    [ContextMenu("Add Permanent Slot")]
    public void Debug_AddPermanentSlot()
    {
        permanentExtraSlots++;
        SaveState();
        OnSlotsChanged?.Invoke();
    }

    [ContextMenu("Add Temporary Slot (1h)")]
    public void Debug_AddTempSlot()
    {
        ActivateTemporarySlot(1);
    }

    [ContextMenu("Reset All Slots")]
    public void Debug_ResetSlots()
    {
        permanentExtraSlots = 0;
        temporaryExtraSlots = 0;
        SaveState();
        OnSlotsChanged?.Invoke();
    }

    #endregion
}
