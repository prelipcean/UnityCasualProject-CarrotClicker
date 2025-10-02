using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace CarrotClicker
{
  /// <summary>
  /// Centralized UI management system for the Carrot Clicker game.
  /// Handles UI updates, performance monitoring, and visual effects.
  /// Implements singleton pattern for global access.
  /// </summary>
  public class UIManager : MonoBehaviour
  {
    // Singleton instance - ensures only one UIManager exists in the scene
    public static UIManager instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalCarrotsText;
    [SerializeField] private TextMeshProUGUI isMobileText;
    [SerializeField] private TextMeshProUGUI fpsCounterText;
    [SerializeField] private TextMeshProUGUI inputBlockingText;

    [Header("Animation Settings")]
    [SerializeField] private bool enableAnimations = true;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showFpsCounter = true;
    [SerializeField] private bool showMemoryInfo = true;
    [SerializeField] private bool showInputBlockingInfo = true;
    [SerializeField] private float fpsUpdateInterval = 0.5f; // Update FPS every 0.5 seconds
    [SerializeField] private float memoryUpdateInterval = 1.0f; // Update memory every 1 second
    [SerializeField] private float inputBlockingUpdateInterval = 0.2f; // Update input blocking info every 0.2 seconds

    // Static event system - allows other scripts to notify UI of carrot count changes
    // Using static events ensures loose coupling between game logic and UI
    public static Action<double> onCarrotCountChanged;
    
    // FPS Counter variables - tracks frame rate performance
    private float deltaTime = 0.0f;      // Smoothed frame time
    private float fps = 0.0f;            // Current frames per second
    private float fpsTimer = 0.0f;       // Timer for FPS calculation intervals
    private int frameCount = 0;          // Frame counter for averaging
    
    // Memory monitoring variables - tracks application memory usage
    private float memoryTimer = 0.0f;    // Timer for memory update intervals
    private float totalMemoryMB = 0.0f;  // Total system memory available
    private float usedMemoryMB = 0.0f;   // Currently used memory by application
    private float unityMemoryMB = 0.0f;  // Memory used specifically by Unity engine
    
    // Input blocking monitoring variables - tracks UI interaction state
    private float inputBlockingTimer = 0.0f; // Timer for input blocking update intervals

    /// <summary>
    /// Initialize singleton and validate components before Start()
    /// </summary>
    void Awake()
    {
      // Implement singleton pattern - destroy duplicates to maintain single instance
      if (instance == null)
      {
        instance = this;
      }
      else
      {
        Destroy(gameObject);
        return;
      }

      // Validate required components before proceeding
      if (!ValidateComponents())
      {
        Debug.LogError("UIManager: Critical components missing. Disabling UIManager.");
        enabled = false;
        return;
      }

      // Subscribe to events after validation
      SubscribeToEvents();
    }

    /// <summary>
    /// Initialize UI display values after all objects are created
    /// </summary>
    void Start()
    {
      // Initialize FPS counter display with initial values
      UpdateFpsDisplay();
      
      // Update mobile detection display if text component is assigned
      if (isMobileText != null)
      {
        isMobileText.text = GameConstants.IsMobile() ? "Mobile Device" : "Desktop/Editor";
      }
    }

    /// <summary>
    /// Update performance monitoring systems each frame
    /// </summary>
    void Update()
    {
      // Update FPS counter if enabled (performance impact is minimal)
      if (showFpsCounter)
      {
        UpdateFpsCounter();
      }
      
      // Update memory info if enabled (less frequent updates to reduce overhead)
      if (showMemoryInfo)
      {
        UpdateMemoryInfo();
      }
      
      // Update input blocking info if enabled (frequent updates for real-time feedback)
      if (showInputBlockingInfo)
      {
        UpdateInputBlockingInfo();
      }
    }

    /// <summary>
    /// Clean up event subscriptions when object is destroyed
    /// </summary>
    void OnDestroy()
    {
      UnsubscribeFromEvents();
    }

    /// <summary>
    /// Validates that all critical UI components are assigned in the inspector
    /// </summary>
    /// <returns>True if all required components are present</returns>
    private bool ValidateComponents()
    {
      bool isValid = true;

      // Check for essential UI text components
      if (totalCarrotsText == null)
      {
        Debug.LogError("UIManager: totalCarrotsText is not assigned!");
        isValid = false;
      }

      return isValid;
    }

    /// <summary>
    /// Subscribe to game events for UI updates
    /// </summary>
    private void SubscribeToEvents()
    {
      try
      {
        // Subscribe to carrot count changes from game logic
        onCarrotCountChanged += UpdateCarrotDisplay;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Failed to subscribe to events: {e.Message}");
      }
    }

    /// <summary>
    /// Unsubscribe from events to prevent memory leaks
    /// </summary>
    private void UnsubscribeFromEvents()
    {
      try
      {
        onCarrotCountChanged -= UpdateCarrotDisplay;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Failed to unsubscribe from events: {e.Message}");
      }
    }

    /// <summary>
    /// Updates the carrot count display with formatting and animations
    /// </summary>
    /// <param name="carrotCount">New carrot count to display</param>
    public void UpdateCarrotDisplay(double carrotCount)
    {
      if (totalCarrotsText == null)
      {
        Debug.LogWarning("UIManager: totalCarrotsText is null. Cannot update display.");
        return;
      }

      try
      {
        // Format large numbers with K, M, B, T suffixes for readability
        string formattedCount = FormatLargeNumber(carrotCount);
        totalCarrotsText.text = $"{formattedCount}\n Carrots!";

        // Add visual feedback through animation if enabled
        if (enableAnimations)
        {
          AnimateCarrotText();
        }
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Error updating carrot display: {e.Message}");
      }
    }

    /// <summary>
    /// Animates the carrot text with a scale pulse effect using LeanTween
    /// </summary>
    private void AnimateCarrotText()
    {
      if (totalCarrotsText == null || totalCarrotsText.gameObject == null)
        return;

      try
      {
        // Reset scale to normal before starting animation
        totalCarrotsText.transform.localScale = Vector3.one;

        // Cancel any existing animations to prevent conflicts
        LeanTween.cancel(totalCarrotsText.gameObject);
        
        // Scale up and back down with ping-pong loop (scales once up, once down)
        LeanTween.scale(totalCarrotsText.gameObject,
            Vector3.one * GameConstants.UI_SCALE_MULTIPLIER,
            GameConstants.UI_ANIMATION_DURATION)
            .setLoopPingPong(1);
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Error animating carrot text: {e.Message}");
      }
    }

    /// <summary>
    /// Formats large numbers with appropriate suffixes (K, M, B, T) for better readability
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <returns>Formatted string with appropriate suffix</returns>
    private string FormatLargeNumber(double number)
    {
      // Show exact numbers under 1 million for precise tracking
      if (number < 1000000)
        return number.ToString("0");

      // if (number < 1000)
      //   return number.ToString("0");
      // if (number < 1000000)
      //   return (number / 1000).ToString("0.#") + "K";

      // Use millions suffix (M)
      if (number < 1000000000)
        return (number / 1000000).ToString("0.#") + "M";

      // Use billions suffix (B)
      if (number < 1000000000000)
        return (number / 1000000000).ToString("0.#") + "B";

      // Use trillions suffix (T)
      if (number < 1000000000000000)
        return (number / 1000000000000).ToString("0.#") + "T";

      // For extremely large numbers, use scientific notation
      return number.ToString("E2");
    }

    // Public methods for game state UI changes
    public void ShowFrenzyMode()
    {
      // TODO: Add frenzy mode visual effects (screen effects, color changes, etc.)
      Debug.Log("UIManager: Frenzy mode started!");
    }

    public void HideFrenzyMode()
    {
      // TODO: Remove frenzy mode visual effects
      Debug.Log("UIManager: Frenzy mode ended!");
    }

    /// <summary>
    /// Toggle animation system on/off for performance or accessibility
    /// </summary>
    public void SetAnimationsEnabled(bool enabled)
    {
      enableAnimations = enabled;
    }

    /// <summary>
    /// Static method for external systems to trigger UI updates
    /// This maintains loose coupling between game logic and UI
    /// </summary>
    /// <param name="newCount">New carrot count to display</param>
    public static void NotifyCarrotCountChanged(double newCount)
    {
      onCarrotCountChanged?.Invoke(newCount);
    }

    #region FPS Counter Methods
    // Performance monitoring system for debugging and optimization

    /// <summary>
    /// Updates FPS calculation using frame averaging for smooth readings
    /// </summary>
    private void UpdateFpsCounter()
    {
      // Smooth delta time calculation using exponential moving average
      deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
      
      // Accumulate frames and time for averaging
      frameCount++;
      fpsTimer += Time.unscaledDeltaTime;
      
      // Calculate and display FPS at specified intervals (reduces UI update overhead)
      if (fpsTimer >= fpsUpdateInterval)
      {
        fps = frameCount / fpsTimer;  // Average FPS over the interval
        frameCount = 0;               // Reset counters
        fpsTimer = 0.0f;
        UpdateFpsDisplay();
      }
    }

    /// <summary>
    /// Updates the FPS display text with color-coded performance information
    /// </summary>
    private void UpdateFpsDisplay()
    {
      if (fpsCounterText == null) return;

      try
      {
        // Build comprehensive performance info string with color coding
        string performanceInfo = BuildPerformanceInfoString();
        fpsCounterText.text = performanceInfo;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Error updating FPS display: {e.Message}");
      }
    }

    /// <summary>
    /// Builds formatted performance information string with platform-specific details
    /// </summary>
    /// <returns>Rich text formatted performance string</returns>
    private string BuildPerformanceInfoString()
    {
      // Get color coding based on performance thresholds
      string fpsColor = GetFpsColor(fps);
      string memoryColor = GetMemoryColor(usedMemoryMB);
      
      // Start with FPS (always shown)
      string info = string.Format("<color={0}>FPS: {1:0}</color>", fpsColor, fps);
      
      // Add frame time on mobile for more detailed performance tracking
      if (GameConstants.IsMobile())
      {
        info += string.Format("\n<color={0}>MS: {1:0.0}</color>", fpsColor, deltaTime * 1000.0f);
      }
      
      // Add memory information if monitoring is enabled
      if (showMemoryInfo)
      {
        info += string.Format("\n<color={0}>RAM: {1:0}MB</color>", memoryColor, usedMemoryMB);
        
        // Add Unity-specific memory usage on mobile for debugging
        if (GameConstants.IsMobile())
        {
          info += string.Format("\n<color={0}>Unity: {1:0}MB</color>", memoryColor, unityMemoryMB);
        }
      }
      
      return info;
    }

    /// <summary>
    /// Returns color code based on FPS performance thresholds
    /// </summary>
    /// <param name="currentFps">Current FPS value</param>
    /// <returns>Hex color code for UI display</returns>
    private string GetFpsColor(float currentFps)
    {
      // Color coding based on common performance expectations
      if (currentFps >= 55f) return "#00FF00"; // Green - Excellent (near 60fps)
      if (currentFps >= 45f) return "#FFFF00"; // Yellow - Good (acceptable for most games)
      if (currentFps >= 30f) return "#FFA500"; // Orange - Acceptable (minimum for smooth gameplay)
      return "#FF0000";                        // Red - Poor (below acceptable threshold)
    }

    // Public API methods for FPS counter control
    public void ToggleFpsCounter()
    {
      showFpsCounter = !showFpsCounter;
      
      if (fpsCounterText != null)
      {
        fpsCounterText.gameObject.SetActive(showFpsCounter);
      }
      
      Debug.Log($"UIManager: FPS Counter {(showFpsCounter ? "enabled" : "disabled")}");
    }

    public void SetFpsCounterVisible(bool visible)
    {
      showFpsCounter = visible;
      
      if (fpsCounterText != null)
      {
        fpsCounterText.gameObject.SetActive(visible);
      }
    }

    public float GetCurrentFps() => fps;
    public bool IsFpsCounterEnabled() => showFpsCounter;

    #endregion

    #region Memory Monitoring Methods
    // Memory usage tracking for performance optimization and debugging

    /// <summary>
    /// Updates memory usage information at specified intervals
    /// </summary>
    private void UpdateMemoryInfo()
    {
      memoryTimer += Time.unscaledDeltaTime;
      
      // Update memory info less frequently than FPS to reduce profiling overhead
      if (memoryTimer >= memoryUpdateInterval)
      {
        CollectMemoryInfo();
        memoryTimer = 0.0f;
        
        // Update display if FPS counter is also active
        if (showFpsCounter)
        {
          UpdateFpsDisplay();
        }
      }
    }

    /// <summary>
    /// Collects memory usage data from Unity's Profiler API with fallback options
    /// </summary>
    private void CollectMemoryInfo()
    {
      try
      {
        // Use Unity's newer Profiler API for accurate memory tracking
        unityMemoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        
        // Get total reserved memory (includes native Unity allocations)
        long totalReserved = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
        usedMemoryMB = totalReserved / (1024f * 1024f);
        
        // Get total system memory available
        totalMemoryMB = SystemInfo.systemMemorySize;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Error collecting memory info: {e.Message}");
        
        // Fallback to .NET garbage collector memory if Unity Profiler fails
        try
        {
          usedMemoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
          unityMemoryMB = usedMemoryMB; // Use same value as fallback
        }
        catch
        {
          // If all methods fail, reset to zero
          usedMemoryMB = 0f;
          unityMemoryMB = 0f;
        }
      }
    }

    /// <summary>
    /// Returns color code based on memory usage thresholds from GameConstants
    /// </summary>
    /// <param name="memoryMB">Memory usage in megabytes</param>
    /// <returns>Hex color code for UI display</returns>
    private string GetMemoryColor(float memoryMB)
    {
      // Color coding based on predefined memory usage thresholds
      if (memoryMB < GameConstants.MEMORY_LOW_THRESHOLD) return "#00FF00";       // Green - Low usage
      if (memoryMB < GameConstants.MEMORY_MODERATE_THRESHOLD) return "#FFFF00";  // Yellow - Moderate usage
      if (memoryMB < GameConstants.MEMORY_HIGH_THRESHOLD) return "#FFA500";      // Orange - High usage
      return "#FF0000";                                                          // Red - Critical usage
    }

    // Public API methods for memory monitoring control
    public void ToggleMemoryInfo()
    {
      showMemoryInfo = !showMemoryInfo;
      Debug.Log($"UIManager: Memory Info {(showMemoryInfo ? "enabled" : "disabled")}");
      
      // Update display immediately to reflect change
      if (showFpsCounter)
      {
        UpdateFpsDisplay();
      }
    }

    public void SetMemoryInfoVisible(bool visible)
    {
      showMemoryInfo = visible;
      
      // Update display immediately to reflect change
      if (showFpsCounter)
      {
        UpdateFpsDisplay();
      }
    }

    // Public getters for external systems that need memory information
    public float GetCurrentMemoryUsage() => usedMemoryMB;
    public float GetUnityMemoryUsage() => unityMemoryMB;
    public float GetTotalSystemMemory() => totalMemoryMB;
    public bool IsMemoryInfoEnabled() => showMemoryInfo;

    #endregion

    #region Input Blocking Monitoring Methods
    // Input blocking status tracking for debugging UI interaction issues

    /// <summary>
    /// Updates input blocking information at specified intervals
    /// </summary>
    private void UpdateInputBlockingInfo()
    {
      inputBlockingTimer += Time.unscaledDeltaTime;
      
      // Update input blocking info more frequently for real-time feedback
      if (inputBlockingTimer >= inputBlockingUpdateInterval)
      {
        UpdateInputBlockingDisplay();
        inputBlockingTimer = 0.0f;
      }
    }

    /// <summary>
    /// Updates the input blocking display text with current blocking status
    /// </summary>
    private void UpdateInputBlockingDisplay()
    {
      if (inputBlockingText == null) return;

      try
      {
        // Build input blocking status string
        string blockingInfo = BuildInputBlockingInfoString();
        inputBlockingText.text = blockingInfo;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"UIManager: Error updating input blocking display: {e.Message}");
      }
    }

    /// <summary>
    /// Builds formatted input blocking information string
    /// </summary>
    /// <returns>Rich text formatted input blocking status string</returns>
    private string BuildInputBlockingInfoString()
    {
      bool isBlocked = UIInteractionManager.IsInteractionBlocked;
      
      // Get color coding based on blocking status
      string statusColor = isBlocked ? "#FF6B6B" : "#4ECDC4"; // Red if blocked, teal if free
      
      // Build status string
      string status = isBlocked ? "BLOCKED" : "FREE";
      string info = string.Format("<color={0}>INPUT: {1}</color>", statusColor, status);
      
      // Add detailed info if there is an active block
      if (isBlocked)
      {
        // Show who is blocking input
        string activeRequester = UIInteractionManager.GetActiveRequester();
        if (!string.IsNullOrEmpty(activeRequester))
        {
          // Simplify requester name for display (remove prefixes like "Animation-" or "MenuTransition-")
          string simplified = activeRequester;
          if (activeRequester.StartsWith("Animation-")) simplified = activeRequester.Substring(10);
          else if (activeRequester.StartsWith("MenuTransition-")) simplified = activeRequester.Substring(15);
          
          // Truncate if too long for mobile display
          if (simplified.Length > 20 && GameConstants.IsMobile())
          {
            simplified = simplified.Substring(0, 17) + "...";
          }
          
          info += string.Format("\n<color=#FFFFFF>BY: {0}</color>", simplified);
        }
      }
      
      return info;
    }

    // Public API methods for input blocking display control
    public void ToggleInputBlockingInfo()
    {
      showInputBlockingInfo = !showInputBlockingInfo;
      
      if (inputBlockingText != null)
      {
        inputBlockingText.gameObject.SetActive(showInputBlockingInfo);
      }
      
      Debug.Log($"UIManager: Input Blocking Info {(showInputBlockingInfo ? "enabled" : "disabled")}");
    }

    public void SetInputBlockingInfoVisible(bool visible)
    {
      showInputBlockingInfo = visible;
      
      if (inputBlockingText != null)
      {
        inputBlockingText.gameObject.SetActive(visible);
      }
    }

    public bool IsInputBlockingInfoEnabled() => showInputBlockingInfo;

    #endregion
  }
}
