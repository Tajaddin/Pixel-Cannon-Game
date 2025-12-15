using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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
    public bool isSelectable = true; // Only front row is selectable
    public int lineIndex = 0; // Which line this cannon is in (0 = front, 1 = middle, 2 = back)

    private Vector3 originalPosition;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private CannonManager cannonManager;

    void Start()
    {
        mainCamera = Camera.main;
        cannonManager = Object.FindFirstObjectByType<CannonManager>();
        originalPosition = transform.position;
    }

    public void Initialize(GameColor color, int cannonPower)
    {
        cannonColor = color;
        power = cannonPower;

        // Apply power multiplier from shop
        if (ShopManager.Instance != null)
        {
            power = Mathf.RoundToInt(power * ShopManager.Instance.GetPowerMultiplier());
        }

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

        // Visual feedback for selectability
        UpdateSelectableVisual();
    }

    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        UpdateSelectableVisual();
    }

    private void UpdateSelectableVisual()
    {
        // Dim non-selectable cannons
        float alpha = isSelectable ? 1f : 0.5f;

        if (bodyRenderer != null)
        {
            Color c = bodyRenderer.color;
            c.a = alpha;
            bodyRenderer.color = c;
        }

        if (colorIndicator != null)
        {
            Color c = colorIndicator.color;
            c.a = alpha;
            colorIndicator.color = c;
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
        // Only select if selectable (front row) and game is active
        if (!isSelectable || isInSlot)
        {
            // Show feedback that this cannon isn't selectable
            if (!isSelectable)
            {
                StartCoroutine(ShakeAnimation());
            }
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            SelectCannon();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isSelectable || isInSlot || GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isDragging = true;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
        mousePos.z = 0;
        dragOffset = transform.position - mousePos;

        // Play pickup sound
        AudioManager.Instance?.PlaySFX("CannonSelect");
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
            StartCoroutine(ReturnToPosition());
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
        if (isInSlot || !isSelectable) return;

        bool added = GameManager.Instance.TryAddCannonToSlot(this);
        if (added)
        {
            isInSlot = true;
            cannonManager?.OnCannonSelected(this);
            AudioManager.Instance?.PlaySFX("SlotFill");
        }
        else
        {
            // Return to original position if slots are full
            StartCoroutine(ReturnToPosition());
        }
    }

    private IEnumerator ReturnToPosition()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 2f); // Ease out

            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            yield return null;
        }

        transform.position = originalPosition;
    }

    private IEnumerator ShakeAnimation()
    {
        Vector3 startPos = transform.position;
        float duration = 0.3f;
        float elapsed = 0f;
        float shakeAmount = 0.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 50f) * shakeAmount * (1f - elapsed / duration);
            transform.position = startPos + new Vector3(x, 0, 0);
            yield return null;
        }

        transform.position = startPos;
    }

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

    public void MoveToNewPosition(Vector3 newPosition, float duration = 0.3f)
    {
        originalPosition = newPosition;
        StartCoroutine(MoveToPositionAnimation(newPosition, duration));
    }

    private IEnumerator MoveToPositionAnimation(Vector3 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }

    public IEnumerator FireAnimation()
    {
        // Scale up
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        // Play fire sound
        AudioManager.Instance?.PlaySFX("CannonFire");

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

/// <summary>
/// Enhanced CannonManager with 3-line queue system
/// </summary>
public class CannonManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnArea;
    public GameObject cannonPrefab;

    [Header("Queue Settings")]
    public int cannonsPerLine = 4;  // 4 cannons per line
    public int numberOfLines = 3;   // 3 lines total
    public float lineSpacing = 1.5f; // Vertical space between lines
    public float cannonSpacing = 1.3f; // Horizontal space between cannons

    [Header("Visual")]
    public float lineScale = 0.9f; // Back lines slightly smaller

    private List<List<Cannon>> cannonLines = new List<List<Cannon>>();
    private LevelData currentLevel;

    public List<Cannon> FrontLine => cannonLines.Count > 0 ? cannonLines[0] : new List<Cannon>();

    public void SpawnInitialCannons(LevelData level)
    {
        currentLevel = level;
        ClearAllCannons();

        // Initialize lines
        for (int i = 0; i < numberOfLines; i++)
        {
            cannonLines.Add(new List<Cannon>());
        }

        // Spawn cannons for all lines
        for (int line = 0; line < numberOfLines; line++)
        {
            for (int i = 0; i < cannonsPerLine; i++)
            {
                SpawnCannonInLine(line);
            }
        }

        UpdateCannonSelectability();
        ArrangeAllCannons(false);
    }

    public void SpawnNewCannons(int count)
    {
        // This is called after firing - add cannons to the back line
        for (int i = 0; i < count; i++)
        {
            // Add to the last line
            if (cannonLines.Count > 0 && cannonLines[numberOfLines - 1].Count < cannonsPerLine)
            {
                SpawnCannonInLine(numberOfLines - 1);
            }
        }

        ArrangeAllCannons(true);
    }

    private void SpawnCannonInLine(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= cannonLines.Count) return;

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
        cannon.lineIndex = lineIndex;

        // Scale based on line (back lines smaller)
        float scale = 1f - (lineIndex * (1f - lineScale) / numberOfLines);
        cannonObj.transform.localScale = Vector3.one * scale;

        cannonLines[lineIndex].Add(cannon);
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

    private void ArrangeAllCannons(bool animate)
    {
        if (spawnArea == null) spawnArea = transform;

        for (int line = 0; line < cannonLines.Count; line++)
        {
            ArrangeLine(line, animate);
        }
    }

    private void ArrangeLine(int lineIndex, bool animate)
    {
        if (lineIndex >= cannonLines.Count) return;

        List<Cannon> line = cannonLines[lineIndex];
        int count = line.Count;

        float startX = spawnArea.position.x - (count - 1) * cannonSpacing / 2f;
        float yPos = spawnArea.position.y - lineIndex * lineSpacing;

        // Back lines are slightly faded and smaller
        float zPos = lineIndex * 0.1f; // Slight depth offset

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(startX + i * cannonSpacing, yPos, zPos);

            if (animate)
            {
                line[i].MoveToNewPosition(pos, 0.3f);
            }
            else
            {
                line[i].transform.position = pos;
            }
        }
    }

    private void UpdateCannonSelectability()
    {
        // Only front line (index 0) is selectable
        for (int line = 0; line < cannonLines.Count; line++)
        {
            bool isSelectable = (line == 0);
            foreach (Cannon cannon in cannonLines[line])
            {
                cannon.SetSelectable(isSelectable);
            }
        }
    }

    public void OnCannonSelected(Cannon cannon)
    {
        // Remove from its line
        foreach (var line in cannonLines)
        {
            if (line.Remove(cannon))
            {
                break;
            }
        }

        // Shift lines forward
        ShiftLinesForward();

        // Respawn cannon in back line
        if (cannonLines[numberOfLines - 1].Count < cannonsPerLine)
        {
            SpawnCannonInLine(numberOfLines - 1);
        }

        UpdateCannonSelectability();
        ArrangeAllCannons(true);
    }

    private void ShiftLinesForward()
    {
        // Move cannons from back lines to front to fill gaps
        for (int line = 0; line < numberOfLines - 1; line++)
        {
            while (cannonLines[line].Count < cannonsPerLine && cannonLines[line + 1].Count > 0)
            {
                // Move cannon from next line to this line
                Cannon cannon = cannonLines[line + 1][0];
                cannonLines[line + 1].RemoveAt(0);
                cannonLines[line].Add(cannon);
                cannon.lineIndex = line;

                // Update scale
                float scale = 1f - (line * (1f - lineScale) / numberOfLines);
                cannon.transform.localScale = Vector3.one * scale;
            }
        }
    }

    public void RemoveCannonFromPool(Cannon cannon)
    {
        foreach (var line in cannonLines)
        {
            line.Remove(cannon);
        }
    }

    private void ClearAllCannons()
    {
        foreach (var line in cannonLines)
        {
            foreach (Cannon cannon in line)
            {
                if (cannon != null) Destroy(cannon.gameObject);
            }
            line.Clear();
        }
        cannonLines.Clear();
    }

    public int GetRemainingCannons()
    {
        int total = 0;
        foreach (var line in cannonLines)
        {
            total += line.Count;
        }
        return total;
    }

    /// <summary>
    /// Get cannons that are coming next (for preview)
    /// </summary>
    public List<Cannon> GetUpcomingCannons(int count)
    {
        List<Cannon> upcoming = new List<Cannon>();

        // First, add all front line cannons
        foreach (Cannon c in cannonLines[0])
        {
            if (upcoming.Count >= count) break;
            upcoming.Add(c);
        }

        // Then add from subsequent lines
        for (int line = 1; line < numberOfLines && upcoming.Count < count; line++)
        {
            foreach (Cannon c in cannonLines[line])
            {
                if (upcoming.Count >= count) break;
                upcoming.Add(c);
            }
        }

        return upcoming;
    }
}
