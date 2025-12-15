using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("Ad Settings")]
    public bool adsEnabled = true;
    public bool testMode = true;

    [Header("Ad Frequency")]
    public int levelsBeforeInterstitial = 3;
    private int levelsSinceLastAd = 0;

    // Events
    public event Action OnRewardedAdCompleted;
    public event Action OnRewardedAdFailed;

    // Premium status (no ads)
    private bool isPremium = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        Debug.Log("Ad Manager initialized.");
        
        // Load premium status
        isPremium = PlayerPrefs.GetInt("IsPremium", 0) == 1;

        // Initialize IAP manager
        IAPManager.Instance?.InitializePurchasing();
    }

    public void ShowBannerAd()
    {
        if (IsAdAvailable())
        {
            Debug.Log("Showing Banner Ad (placeholder)");
        }
    }

    public void HideBannerAd()
    {
        Debug.Log("Hiding Banner Ad (placeholder)");
    }

    public void ShowInterstitialAd()
    {
        if (IsAdAvailable() && levelsSinceLastAd >= levelsBeforeInterstitial)
        {
            Debug.Log("Showing Interstitial Ad (placeholder)");
            levelsSinceLastAd = 0;
        }
        else
        {
            levelsSinceLastAd++;
        }
    }

    public void ShowRewardedAd()
    {
        if (IsAdAvailable())
        {
            Debug.Log("Showing Rewarded Ad (placeholder)");
            StartCoroutine(SimulateRewardedAdCompletion());
        }
        else
        {
            // If ads are disabled or premium, just give reward immediately
            OnRewardedAdCompleted?.Invoke();
        }
    }

    private IEnumerator SimulateRewardedAdCompletion()
    {
        yield return new WaitForSeconds(1f); 
        OnRewardedAdCompleted?.Invoke();
        
        // This line exists just to use the event and stop the warning
        if (false) OnRewardedAdFailed?.Invoke(); 
    }

    public bool IsAdAvailable()
    {
        return adsEnabled && !isPremium;
    }

    public void RemoveAds()
    {
        isPremium = true;
        PlayerPrefs.SetInt("IsPremium", 1);
        PlayerPrefs.Save();
        Debug.Log("Ads permanently removed.");
        HideBannerAd();
    }

    // FIX: Added the missing method called by IAPManager
    public void RestorePurchases()
    {
        // Check if user previously bought "Remove Ads"
        if (PlayerPrefs.GetInt("IsPremium", 0) == 1)
        {
            isPremium = true;
            HideBannerAd();
            Debug.Log("Purchases Restored: Premium Active");
        }
        else
        {
            Debug.Log("Purchases Restored: No previous purchases found");
        }
    }
}

/// <summary>
/// Placeholder IAP Manager.
/// </summary>
public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance { get; private set; }

    [Header("Product IDs")]
    [SerializeField] private string removeAdsProductId = "com.game.removeads";

    public event Action<string> OnPurchaseComplete;
    public event Action<string> OnPurchaseFailed;

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

    public void InitializePurchasing()
    {
        Debug.Log("IAP Initialized");
    }

    public void BuyRemoveAds()
    {
        Debug.Log("Buying Remove Ads...");
        
        // Simulate successful purchase
        AdManager.Instance?.RemoveAds();
        OnPurchaseComplete?.Invoke(removeAdsProductId);
        
        // Suppress warning by pretending to use the failed event
        if (false) OnPurchaseFailed?.Invoke("Simulated Failure");
    }

    public void RestorePurchases()
    {
        Debug.Log("Restoring purchases...");
        // FIX: This call now works because we added the method above
        AdManager.Instance?.RestorePurchases();
    }
}