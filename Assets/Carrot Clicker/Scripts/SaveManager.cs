using UnityEngine;
using System.Collections;

namespace CarrotClicker
{
    /// <summary>
    /// Centralized save system that handles automatic saving during application lifecycle events.
    /// Ensures player progress is preserved when the app is minimized, loses focus, or quits.
    /// Particularly important for mobile platforms where apps can be killed by the OS.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager instance;

        [Header("Mobile Save Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        
        void Awake()
        {
            // Implement singleton pattern to ensure only one SaveManager exists
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes to maintain save functionality
            }
            else
            {
                // Destroy duplicate instances to maintain singleton integrity
                Destroy(gameObject);
                return;
            }
        }

        // === APPLICATION LIFECYCLE EVENT HANDLERS ===
        // These methods are automatically called by Unity when the application state changes
        
        /// <summary>
        /// Called when application is paused/resumed (primarily mobile platforms).
        /// On mobile, this triggers when user switches apps or opens notification panel.
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App is being paused - save immediately as the app might be killed
                if (enableDebugLogs)
                    Debug.Log("SaveManager: Application paused - triggering global save");
                SaveAll();
            }
            else
            {
                // App is resuming - no action needed, just log for debugging
                if (enableDebugLogs)
                    Debug.Log("SaveManager: Application resumed");
            }
        }

        /// <summary>
        /// Called when application gains/loses focus (desktop and mobile).
        /// Triggers when user clicks away from the game window or returns to it.
        /// </summary>
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // App lost focus - save data in case user doesn't return
                if (enableDebugLogs)
                    Debug.Log("SaveManager: Application lost focus - triggering global save");
                SaveAll();
            }
            else
            {
                // App gained focus - player is back, no save needed
                if (enableDebugLogs)
                    Debug.Log("SaveManager: Application gained focus");
            }
        }

        /// <summary>
        /// Called when application is about to quit (desktop platforms mainly).
        /// Final opportunity to save before the process terminates.
        /// </summary>
        void OnApplicationQuit()
        {
            if (enableDebugLogs)
                Debug.Log("SaveManager: Application quitting - triggering global save");
            SaveAll();
        }

        /// <summary>
        /// Triggers save for all managers in the game.
        /// Coordinates saving across multiple systems to ensure data consistency.
        /// Uses try-catch to prevent save failures from crashing the game.
        /// </summary>
        public void SaveAll()
        {
            try
            {
                // Save CarrotManager data (total carrots, current state, etc.)
                if (CarrotManager.instance != null)
                {
                    CarrotManager.instance.ForceSave();
                }

                // Save AutoClickManager data (upgrade levels, bunny count, etc.)
                // Note: Using FindObjectOfType since AutoClickManager might not have a singleton pattern
                AutoClickManager autoClickManager = FindObjectOfType<AutoClickManager>();
                if (autoClickManager != null)
                {
                    autoClickManager.ForceSave();
                }

                // Force PlayerPrefs to write immediately to disk
                // This ensures data is physically saved and not just cached in memory
                PlayerPrefs.Save();

                if (enableDebugLogs)
                    Debug.Log("SaveManager: Global save completed successfully");
            }
            catch (System.Exception e)
            {
                // Log errors but don't crash the game - player can try saving again
                Debug.LogError($"SaveManager: Error during global save: {e.Message}");
            }
        }

        /// <summary>
        /// Manual save trigger - can be called from UI buttons or external scripts.
        /// Provides players with a way to save their progress on demand.
        /// </summary>
        public void ManualSave()
        {
            Debug.Log("SaveManager: Manual save triggered");
            SaveAll();
        }

        /// <summary>
        /// Enable/disable debug logging for save operations.
        /// Useful for reducing console spam in release builds while maintaining debug capability.
        /// </summary>
        public void SetDebugLogging(bool enabled)
        {
            enableDebugLogs = enabled;
        }
    }
}