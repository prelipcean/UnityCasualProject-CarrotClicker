using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CarrotClicker
{
    /// <summary>
    /// Core game manager that handles carrot currency system.
    /// Manages carrot counting, frenzy mode multipliers, and save/load functionality.
    /// Implements singleton pattern for global access throughout the game.
    /// </summary>
    public class CarrotManager : MonoBehaviour
    {
        // Singleton instance for global access
        public static CarrotManager instance;

        // Using double instead of int to handle very large numbers without overflow
        // For even bigger numbers, consider using BreakInfinity library from GitHub
        [Header("Carrot Settings")]
        [SerializeField] private double totalCarrotsCount = 0;
        [SerializeField] private int frenzyModeMultiplier = 10;
        private int carrotIncrement = 1; // Current multiplier per carrot click (1 normal, higher during frenzy)

        [Header("Save System")]
        private bool isDirty = false; // Tracks if data needs saving to avoid unnecessary disk writes
        private Coroutine autoSaveCoroutine; // Background coroutine for periodic saves

        void Awake()
        {
            // Singleton pattern implementation - ensures only one CarrotManager exists
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Safety check: Ensure all required components are present before initialization
            if (!ValidateComponents())
            {
                Debug.LogError("CarrotManager: Critical components missing. Disabling CarrotManager.");
                enabled = false;
                return;
            }

            // Load saved carrot data from PlayerPrefs
            LoadData();

            // Initialize default values
            carrotIncrement = 1;
            frenzyModeMultiplier = GameConstants.DEFAULT_FRENZY_MULTIPLIER;

            // Connect to game events (carrot clicks, frenzy mode changes)
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            // Clean up event subscriptions to prevent memory leaks
            UnsubscribeFromEvents();

            // Stop background auto-save and perform final save before destruction
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
            
            // Force save regardless of dirty flag to ensure no data loss
            ForceSave();
        }

        void Start()
        {
            // Begin periodic auto-save system
            autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        }

        // Mobile-specific: Handle app going to background (prevents data loss on task switching)
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                DebugLogger.Log("CarrotManager: Application paused - forcing save and pausing game");
                ForceSave();
                GamePauseManager.OnApplicationPause(pauseStatus); // Pause the game
            }
            else
            {
                DebugLogger.Log("CarrotManager: Application resumed - resuming game");
                GamePauseManager.OnApplicationPause(pauseStatus); // Resume the game
            }
        }

        // Handle app losing focus (Alt+Tab on PC, notifications on mobile)
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                DebugLogger.Log("CarrotManager: Application lost focus - forcing save and pausing game");
                ForceSave();
                GamePauseManager.OnApplicationFocus(hasFocus); // Pause the game
            }
            else
            {
                DebugLogger.Log("CarrotManager: Application gained focus - resuming game");
                GamePauseManager.OnApplicationFocus(hasFocus); // Resume the game
            }
        }

        // Final save before application completely closes
        void OnApplicationQuit()
        {
            Debug.Log("CarrotManager: Application quitting - forcing save");
            ForceSave();
        }

        /// <summary>
        /// Public method to add carrots from external sources (achievements, purchases, etc.)
        /// Includes validation to prevent negative values and overflow protection
        /// </summary>
        public void AddCarrots(float value)
        {
            // Input validation: reject negative values
            if (value < 0)
            {
                Debug.LogWarning($"CarrotManager: Attempted to add negative carrots: {value}");
                return;
            }
            
            // Input validation: reject invalid floating point values
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                Debug.LogError($"CarrotManager: Invalid carrot value: {value}");
                return;
            }

            // Overflow protection: clamp to maximum safe value
            double newTotal = totalCarrotsCount + value;
            if (newTotal > double.MaxValue || newTotal < 0)
            {
                Debug.LogWarning("CarrotManager: Carrot count would overflow. Clamping to max value.");
                totalCarrotsCount = double.MaxValue;
            }
            else
            {
                totalCarrotsCount = newTotal;
            }
            
            // Update UI and mark data as needing save
            NotifyUIUpdate();
            isDirty = true;
        }

        /// <summary>
        /// Event callback: Handles individual carrot clicks from InputManager
        /// Applies current multiplier (1 for normal, higher during frenzy mode)
        /// </summary>
        private void CarrotClickedCallback()
        {
            totalCarrotsCount += carrotIncrement;
            NotifyUIUpdate();
            isDirty = true; // Mark for auto-save
        }

        /// <summary>
        /// Event callback: Frenzy mode activated - increase click multiplier
        /// </summary>
        private void FrenzyModeStartedCallback()
        {
            carrotIncrement = frenzyModeMultiplier;
        }

        /// <summary>
        /// Event callback: Frenzy mode ended - reset to normal click value
        /// </summary>
        private void FrenzyModeEndedCallback()
        {
            carrotIncrement = 1;
        }

        /// <summary>
        /// Notifies UI system to update displayed carrot count
        /// Decoupled design allows UI changes without modifying CarrotManager
        /// </summary>
        private void NotifyUIUpdate()
        {
            UIManager.NotifyCarrotCountChanged(totalCarrotsCount);
        }

        /// <summary>
        /// Validates that all required components are available
        /// Currently simplified since CarrotManager is decoupled from UI components
        /// </summary>
        private bool ValidateComponents()
        {
            // CarrotManager no longer depends on UI components directly
            // All validations are now handled by individual managers
            return true;
        }

        /// <summary>
        /// Subscribe to game events with error handling
        /// Events: carrot clicks, frenzy mode start/end
        /// </summary>
        private void SubscribeToEvents()
        {
            try
            {
                InputManager.onCarrotClicked += CarrotClickedCallback;
                Carrot.onFrenzyModeStarted += FrenzyModeStartedCallback;
                Carrot.onFrenzyModeEnded += FrenzyModeEndedCallback;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CarrotManager: Failed to subscribe to events: {e.Message}");
            }
        }

        /// <summary>
        /// Unsubscribe from events to prevent memory leaks and null reference exceptions
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            try
            {
                InputManager.onCarrotClicked -= CarrotClickedCallback;
                Carrot.onFrenzyModeStarted -= FrenzyModeStartedCallback;
                Carrot.onFrenzyModeEnded -= FrenzyModeEndedCallback;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CarrotManager: Failed to unsubscribe from events: {e.Message}");
            }
        }

        /// <summary>
        /// Save carrot data to PlayerPrefs and clear dirty flag
        /// Only called when data has actually changed (performance optimization)
        /// </summary>
        private void SaveData()
        {
            PlayerPrefs.SetString(GameConstants.PREF_TOTAL_CARROTS, totalCarrotsCount.ToString());
            isDirty = false;
        }

        /// <summary>
        /// Force immediate save with error handling
        /// Used during critical moments (app closing, focus loss, etc.)
        /// </summary>
        public void ForceSave()
        {
            try
            {
                SaveData();
                PlayerPrefs.Save(); // Force immediate write to disk (not just memory)
                Debug.Log($"CarrotManager: Force save completed. Carrots: {totalCarrotsCount}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CarrotManager: Error during force save: {e.Message}");
            }
        }

        /// <summary>
        /// Load carrot data from PlayerPrefs with validation
        /// Handles missing data, corrupt data, and negative values gracefully
        /// </summary>
        private void LoadData()
        {
            if (PlayerPrefs.HasKey(GameConstants.PREF_TOTAL_CARROTS))
            {
                string savedCarrots = PlayerPrefs.GetString(GameConstants.PREF_TOTAL_CARROTS);
                if (double.TryParse(savedCarrots, out double parsedValue) && parsedValue >= 0)
                {
                    totalCarrotsCount = parsedValue;
                }
                else
                {
                    // Corrupted or invalid save data - start fresh
                    totalCarrotsCount = 0;
                }
            }
            else
            {
                // First time playing - start with zero carrots
                totalCarrotsCount = 0;
            }
            NotifyUIUpdate();
        }

        /// <summary>
        /// Background coroutine that saves data periodically when changes are detected
        /// Prevents data loss without constantly writing to disk
        /// </summary>
        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(GameConstants.AUTO_SAVE_INTERVAL);
                if (isDirty) // Only save if data has changed
                {
                    SaveData();
                }
            }
        }

        /// <summary>
        /// Public getter for current click multiplier
        /// Used by UI to show frenzy mode status
        /// </summary>
        public int GetCurrentMultiplier()
        {
            return carrotIncrement;
        }
    }
}
