using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CarrotClicker
{
    /// <summary>
    /// Manages automatic carrot generation through animated bunny workers.
    /// Each level increases both the number of bunnies and carrots per second production.
    /// </summary>
    public class AutoClickManager : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private Transform rotator; // Parent transform that rotates all bunnies in a circle
        [SerializeField] private GameObject bunnyPrefab; // Prefab for individual bunny workers

        [Header(" Settings ")]
        [SerializeField] private float rotatorSpeed = GameConstants.ROTATOR_SPEED; // Speed of circular rotation (degrees/second)
        [SerializeField] private float rotatorRadius = GameConstants.ROTATOR_RADIUS; // Distance from center for bunny placement
        private int currentBunnyIndex = 0; // Index for sequential bunny animation cycle

        [Header(" Data ")]
        [SerializeField] private int level; // Current auto-clicker level (determines bunny count and production)
        [SerializeField] private float carrotsPerSecond; // Current carrot production rate

        private void Awake()
        {
            // Load saved progression data before any initialization
            LoadData();
        }

        // Start is called before the first frame update
        void Start()
        {
        if (!ValidateComponents())
        {
            DebugLogger.LogError("AutoClickManager: Critical components missing. Disabling AutoClickManager.");
            enabled = false;
            return;
        }            // Calculate production rate based on current level
            carrotsPerSecond = level * GameConstants.CARROTS_PER_SECOND_MULTIPLIER;
            DebugLogger.Log($"AutoClickManager initialized: Level {level}, Production rate: {carrotsPerSecond} carrots/sec");

            // Start automatic carrot generation every second
            InvokeRepeating(nameof(AddCarrots), 1.0f, 1.0f);

            // Initialize visual bunny workers
            SpawnBunnies();
            StartAnimatingBunnies();
        }

        // Update is called once per frame
        void Update()
        {
            // Continuously rotate the bunny circle for visual appeal
            if (rotator != null)
            {
                rotator.Rotate(Vector3.forward, rotatorSpeed * Time.deltaTime);
            }
        }

        #region Mobile Lifecycle Management
        // These methods ensure data persistence when the app is minimized or closed on mobile devices
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                DebugLogger.Log("AutoClickManager: Application paused - forcing save");
                ForceSave();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                DebugLogger.Log("AutoClickManager: Application lost focus - forcing save");
                ForceSave();
            }
        }

        void OnApplicationQuit()
        {
            DebugLogger.Log("AutoClickManager: Application quitting - forcing save");
            ForceSave();
        }
        #endregion

        /// <summary>
        /// Validates that all required components are properly assigned in the inspector
        /// </summary>
        private bool ValidateComponents()
        {
            bool isValid = true;
            
            if (rotator == null)
            {
                DebugLogger.LogError("AutoClickManager: rotator is not assigned!");
                isValid = false;
            }
            
            if (bunnyPrefab == null)
            {
                DebugLogger.LogError("AutoClickManager: bunnyPrefab is not assigned!");
                isValid = false;
            }
            
            return isValid;
        }

        /// <summary>
        /// Creates and positions bunny workers in a circle around the rotator.
        /// Number of bunnies is based on current level (capped at MAX_BUNNIES).
        /// </summary>
        private void SpawnBunnies()
        {
            if (rotator == null || bunnyPrefab == null)
            {
                Debug.LogError("AutoClickManager: Cannot spawn bunnies - missing required components");
                return;
            }

            // Clean up any existing bunnies before spawning new ones
            try
            {
                while (rotator.childCount > 0)
                {
                    Transform child = rotator.GetChild(0);
                    if (child != null)
                    {
                        child.SetParent(null);
                        if (child.gameObject != null)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AutoClickManager: Error clearing existing bunnies: {e.Message}");
                return;
            }

            // Limit bunny count to prevent overcrowding and performance issues
            int bunnyCount = Mathf.Min(level, GameConstants.MAX_BUNNIES);

            try
            {
                // Place bunnies evenly around the circle
                for (int i = 0; i < bunnyCount; i++)
                {
                    // Calculate position on circle using trigonometry
                    float angle = i * GameConstants.BUNNY_ANGLE_STEP;

                    Vector2 position = new Vector2();
                    position.x = rotatorRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    position.y = rotatorRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

                    GameObject bunny = Instantiate(bunnyPrefab, position, Quaternion.identity, rotator);
                    
                    if (bunny != null)
                    {
                        // Orient bunny to face outward from center
                        bunny.transform.up = position.normalized;
                    }
                    else
                    {
                        Debug.LogWarning($"AutoClickManager: Failed to instantiate bunny at index {i}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AutoClickManager: Error spawning bunnies: {e.Message}");
            }
        }

        /// <summary>
        /// Called every second to generate carrots automatically.
        /// Production rate is based on current level.
        /// </summary>
        private void AddCarrots()
        {
            // Validate CarrotManager instance
            if (CarrotManager.instance == null)
            {
                DebugLogger.LogError("AutoClickManager: CarrotManager instance is null. Cannot add carrots.");
                return;
            }

            if (carrotsPerSecond < 0)
            {
                DebugLogger.LogWarning($"AutoClickManager: Invalid carrotsPerSecond value: {carrotsPerSecond}");
                return;
            }

            CarrotManager.instance.AddCarrots(carrotsPerSecond);
            DebugLogger.Log($"AutoClickManager: Added {carrotsPerSecond} carrots. Total level: {level}");
        }

        /// <summary>
        /// Increases the auto-clicker level, spawns additional bunnies, and updates production rate.
        /// Called when player purchases an upgrade.
        /// </summary>
        public void UpgradeAutoClicker()
        {
            int previousLevel = level;
            level++;
            carrotsPerSecond = level * GameConstants.CARROTS_PER_SECOND_MULTIPLIER;
            
            DebugLogger.Log($"AutoClickManager: Upgraded from level {previousLevel} to {level}. New production: {carrotsPerSecond} carrots/sec");

            SaveData();

            // Only spawn new bunnies if we haven't reached the visual limit
            if (level > GameConstants.MAX_BUNNIES)
            {
                DebugLogger.LogWarning($"AutoClickManager: Reached visual bunny limit ({GameConstants.MAX_BUNNIES}). Level {level} will not spawn new bunnies.");
                return; // Continue increasing production but cap visual bunnies
            }
                
            SpawnBunnies();
            StartAnimatingBunnies();
        }

        /// <summary>
        /// Initiates the sequential bunny jumping animation cycle.
        /// Each bunny jumps in turn to show they're "working" to produce carrots.
        /// </summary>
        private void StartAnimatingBunnies()
        {
            if (rotator == null || rotator.childCount <= 0)
            {
                Debug.LogWarning("AutoClickManager: Cannot start animating bunnies - no rotator or no children");
                return;
            }

            try
            {
                // Cancel any existing animations to prevent conflicts
                LeanTween.cancel(gameObject);

                for (int i = 0; i < rotator.childCount; i++)
                {
                    Transform child = rotator.GetChild(i);
                    if (child != null && child.gameObject != null)
                    {
                        LeanTween.cancel(child.gameObject);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AutoClickManager: Error preparing bunny animations: {e.Message}");
                return;
            }

            try
            {
                // Animate the current bunny in the sequence
                if (currentBunnyIndex < rotator.childCount)
                {
                    Transform bunny = rotator.GetChild(currentBunnyIndex);
                    if (bunny != null && bunny.childCount > 0)
                    {
                        // Animate the child sprite, not the parent positioning transform
                        Transform bunnyChild = bunny.GetChild(0);
                        if (bunnyChild != null && bunnyChild.gameObject != null)
                        {
                            // Create a jump animation (up and down)
                            LeanTween.moveLocalY(bunnyChild.gameObject, GameConstants.BUNNY_JUMP_HEIGHT, GameConstants.BUNNY_JUMP_DURATION)
                                .setLoopPingPong(1)
                                .setOnComplete(() =>
                                {
                                    AnimateNextBunny();
                                });
                        }
                        else
                        {
                            Debug.LogWarning($"AutoClickManager: Bunny at index {currentBunnyIndex} has no valid child for animation");
                            AnimateNextBunny();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"AutoClickManager: Invalid bunny at index {currentBunnyIndex}");
                        AnimateNextBunny();
                    }
                }
                else
                {
                    Debug.LogWarning($"AutoClickManager: Bunny index {currentBunnyIndex} out of range");
                    ResetBunniesAnimation();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AutoClickManager: Error starting bunny animation: {e.Message}");
                AnimateNextBunny();
            }
        }

        /// <summary>
        /// Moves to the next bunny in the animation sequence.
        /// If all bunnies have been animated, restart the cycle with a delay.
        /// </summary>
        private void AnimateNextBunny()
        {
            currentBunnyIndex++;

            if (currentBunnyIndex >= rotator.childCount)
            {
                // All bunnies animated, restart the cycle
                ResetBunniesAnimation();
            }
            else
            {
                // Continue with next bunny
                StartAnimatingBunnies();
            }
        }

        /// <summary>
        /// Resets the animation cycle back to the first bunny.
        /// Includes a delay that decreases as more bunnies are active (faster work pace).
        /// </summary>
        private void ResetBunniesAnimation()
        {
            currentBunnyIndex = 0;

            // More bunnies = faster animation cycling (they work more efficiently together)
            float delay = Mathf.Max(10 - rotator.childCount, 0);

            LeanTween.delayedCall(gameObject, delay, StartAnimatingBunnies);
        }

        #region Data Persistence
        /// <summary>
        /// Loads the auto-clicker level from PlayerPrefs on game start
        /// </summary>
        private void LoadData()
        {
            int previousLevel = level;
            level = PlayerPrefs.GetInt(GameConstants.PREF_AUTO_CLICK_LEVEL, 0);
            DebugLogger.Log($"AutoClickManager: Loaded data - Level: {level} (was {previousLevel})");
        }

        /// <summary>
        /// Saves the current auto-clicker level to PlayerPrefs
        /// </summary>
        private void SaveData()
        {
            PlayerPrefs.SetInt(GameConstants.PREF_AUTO_CLICK_LEVEL, level);
        }

        /// <summary>
        /// Forces an immediate save to disk. Used during app lifecycle events
        /// to prevent data loss on mobile devices.
        /// </summary>
        public void ForceSave()
        {
            try
            {
                SaveData();
                PlayerPrefs.Save(); // Force immediate write to disk
                Debug.Log($"AutoClickManager: Force save completed. Level: {level}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AutoClickManager: Error during force save: {e.Message}");
            }
        }
        #endregion
    }
}
