using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Added for TextMeshPro

public class Cannon : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Cannon Properties")]
    public GameColor cannonColor;
    public int power = 10; // How many pixels it can fill

    [Header("Visual")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer colorIndicator;
    public TextMeshPro powerText;

    [Header("State")]
    public bool isInSlot = false;
    public bool isDragging = false;

    private Vector3 originalPosition;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private CannonManager cannonManager;

    void Start()
    {
        mainCamera = Camera.main;
        // FIX: Updated to FindFirstObjectByType to avoid obsolete warning
        cannonManager = Object.FindFirstObjectByType<CannonManager>();
        originalPosition = transform.position;
    }

    public void Initialize(GameColor color, int cannonPower)
    {
        cannonColor = color;
        power = cannonPower;
        
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Set the color indicator
        if (colorIndicator != null)
        {
            colorIndicator.color = GetColorValue(cannonColor);
        }

        // Update power text
        if (powerText != null)
        {
            powerText.text = power.ToString();
        }

        // If no sprite renderer, try mesh renderer as fallback
        if (bodyRenderer == null)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = GetColorValue(cannonColor);
            }
        }
    }

    private Color GetColorValue(GameColor color)
    {
        switch (color)
        {
            case GameColor.Red: return Color.red;
            case GameColor.Blue: return new Color(0.2f, 0.6f, 1f);
            case GameColor.Green: return Color.green;
            case GameColor.Yellow: return Color.yellow;
            case GameColor.Purple: return new Color(0.6f, 0.2f, 0.8f);
            case GameColor.Orange: return new Color(1f, 0.5f, 0f);
            case GameColor.Pink: return new Color(1f, 0.4f, 0.7f);
            case GameColor.Cyan: return Color.cyan;
            case GameColor.White: return Color.white;
            case GameColor.Black: return new Color(0.1f, 0.1f, 0.1f);
            default: return Color.gray;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only select if not already in slot and game is active
        if (!isInSlot && GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            SelectCannon();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInSlot || GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isDragging = true;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
        mousePos.z = 0;
        dragOffset = transform.position - mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
        mousePos.z = 0;
        transform.position = mousePos + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        // Check if dropped on slot area (simple height check)
        if (IsOverSlotArea())
        {
            SelectCannon();
        }
        else
        {
            // Return to original position
            transform.position = originalPosition;
        }
    }

    private bool IsOverSlotArea()
    {
        if (mainCamera == null) return false;
        
        // Check if cannon is in the upper portion of the screen (slot area)
        float screenHeight = mainCamera.orthographicSize * 2;
        float slotAreaY = mainCamera.transform.position.y + screenHeight * 0.2f;
        
        return transform.position.y > slotAreaY;
    }

    public void SelectCannon()
    {
        if (isInSlot) return;

        bool added = GameManager.Instance.TryAddCannonToSlot(this);
        if (added)
        {
            isInSlot = true;
            cannonManager?.RemoveCannonFromPool(this);
        }
        else
        {
            // Return to original position if slots are full
            transform.position = originalPosition;
        }
    }

    // FIX: Restored MoveToSlot method needed by GameManager
    public void MoveToSlot(Vector3 slotPosition)
    {
        StartCoroutine(MoveToSlotAnimation(slotPosition));
    }

    private IEnumerator MoveToSlotAnimation(Vector3 targetPos)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }

    // FIX: Restored FireAnimation method needed by GameManager
    public IEnumerator FireAnimation()
    {
        // Scale up
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
            transform.localScale = originalScale * scale;
            yield return null;
        }

        // Flash effect
        if (colorIndicator != null)
        {
            Color originalColor = colorIndicator.color;
            colorIndicator.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            colorIndicator.color = originalColor;
        }

        transform.localScale = originalScale;
    }
}

// FIX: Restored full CannonManager implementation
public class CannonManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnArea;
    public GameObject cannonPrefab;
    public int initialCannons = 6;
    public int maxCannonsOnScreen = 12;

    [Header("Spawn Grid")]
    public int columns = 4;
    public int rows = 3;
    public float cannonSpacing = 1.5f;

    private List<Cannon> cannonPool = new List<Cannon>();
    private LevelData currentLevel;

    // FIX: Restored SpawnInitialCannons
    public void SpawnInitialCannons(LevelData level)
    {
        currentLevel = level;
        ClearAllCannons();
        SpawnNewCannons(initialCannons);
    }

    // FIX: Restored SpawnNewCannons
    public void SpawnNewCannons(int count)
    {
        if (currentLevel == null) return;

        for (int i = 0; i < count && cannonPool.Count < maxCannonsOnScreen; i++)
        {
            SpawnCannon();
        }

        ArrangeCannons();
    }

    private void SpawnCannon()
    {
        GameObject cannonObj;
        
        if (cannonPrefab != null)
        {
            cannonObj = Instantiate(cannonPrefab, transform);
        }
        else
        {
            cannonObj = CreateDefaultCannon();
        }

        Cannon cannon = cannonObj.GetComponent<Cannon>();
        if (cannon == null)
        {
            cannon = cannonObj.AddComponent<Cannon>();
        }

        // Randomize cannon properties based on level
        GameColor color = GetRandomColorFromLevel();
        int power = GetRandomPower();
        
        cannon.Initialize(color, power);
        cannonPool.Add(cannon);
    }

    private GameObject CreateDefaultCannon()
    {
        GameObject cannon = new GameObject("Cannon");
        cannon.transform.parent = transform;

        // Create body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.parent = cannon.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.8f, 1f, 0.3f);
        
        Collider col = body.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Add collider for clicks
        BoxCollider2D boxCol = cannon.AddComponent<BoxCollider2D>();
        boxCol.size = new Vector2(1f, 1.2f);
        
        // Add minimal mesh renderer for visuals
        MeshRenderer mr = body.GetComponent<MeshRenderer>();
        if (mr != null) mr.material = new Material(Shader.Find("Sprites/Default"));

        return cannon;
    }

    private GameColor GetRandomColorFromLevel()
    {
        if (currentLevel != null && currentLevel.availableColors != null && currentLevel.availableColors.Length > 0)
        {
            int index = Random.Range(0, currentLevel.availableColors.Length);
            return currentLevel.availableColors[index];
        }

        // Default colors
        GameColor[] defaultColors = { GameColor.Red, GameColor.Blue, GameColor.Green, GameColor.Yellow, GameColor.Purple };
        return defaultColors[Random.Range(0, defaultColors.Length)];
    }

    private int GetRandomPower()
    {
        if (currentLevel != null)
        {
            switch (currentLevel.difficulty)
            {
                case Difficulty.Easy: return Random.Range(20, 50);
                case Difficulty.Medium: return Random.Range(15, 40);
                case Difficulty.Hard: return Random.Range(10, 30);
                case Difficulty.SuperHard: return Random.Range(5, 20);
            }
        }
        return Random.Range(10, 40);
    }

    private void ArrangeCannons()
    {
        if (spawnArea == null) spawnArea = transform;

        float startX = spawnArea.position.x - (columns - 1) * cannonSpacing / 2f;
        float startY = spawnArea.position.y;

        for (int i = 0; i < cannonPool.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;

            Vector3 pos = new Vector3(
                startX + col * cannonSpacing,
                startY - row * cannonSpacing,
                0
            );

            cannonPool[i].transform.position = pos;
        }
    }

    public void RemoveCannonFromPool(Cannon cannon)
    {
        if (cannonPool.Contains(cannon))
        {
            cannonPool.Remove(cannon);
            ArrangeCannons();
        }
    }

    private void ClearAllCannons()
    {
        foreach (Cannon cannon in cannonPool)
        {
            if (cannon != null) Destroy(cannon.gameObject);
        }
        cannonPool.Clear();
    }

    public int GetRemainingCannons()
    {
        return cannonPool.Count;
    }
}