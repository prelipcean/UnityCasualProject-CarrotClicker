using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarrotClicker
{
    /// <summary>
    /// Dedicated UI manager for shop-related functionality.
    /// Handles shop panel animations, state management, and shop-specific UI interactions.
    /// Separated from UIManager following Single Responsibility Principle.
    /// </summary>
    public class UIShopManager : MonoBehaviour
    {
        [Header("Shop UI Elements")]
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private GameObject shopButton; // Button that opens the shop

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutSine;

        [Header("Game State Settings")]
        [SerializeField] private bool pauseGameWhenOpen = true; // Should the game pause when shop is open
        
        [Header("Animation Settings")]
        [Tooltip("All shop animations use unscaled time to work properly when game is paused")]
        [SerializeField] private bool useUnscaledTime = true; // Use unscaled time for animations

        // Position states for shop panel animation
        private Vector2 openedPosition;
        private Vector2 closedPosition;
        
        // Current shop state
        private bool isShopOpen = false;

        /// <summary>
        /// Initialize shop panel positions and validate components
        /// </summary>
        void Start()
        {
            if (!ValidateComponents())
            {
                DebugLogger.LogError("UIShopManager: Critical components missing. Disabling UIShopManager.");
                enabled = false;
                return;
            }

            InitializeShopPositions();
            DebugLogger.Log("UIShopManager: Successfully initialized");
        }

        /// <summary>
        /// Validates that all required components are properly assigned
        /// </summary>
        private bool ValidateComponents()
        {
            bool isValid = true;

            if (shopPanel == null)
            {
                DebugLogger.LogError("UIShopManager: shopPanel is not assigned!");
                isValid = false;
            }

            if (shopButton == null)
            {
                DebugLogger.LogWarning("UIShopManager: shopButton is not assigned - button visibility features will be disabled!");
                // Don't mark as invalid since this is optional
            }

            return isValid;
        }

        /// <summary>
        /// Initialize the opened and closed positions for shop animation
        /// </summary>
        private void InitializeShopPositions()
        {
            openedPosition = Vector2.zero;
            closedPosition = new Vector2(shopPanel.rect.width, 0);

            // Start with shop closed
            shopPanel.anchoredPosition = closedPosition;
            isShopOpen = false;
        }

        /// <summary>
        /// Opens the shop with smooth animation, hides the shop button, and optionally pauses the game
        /// </summary>
        public void OpenShop()
        {
            if (isShopOpen)
            {
                return; // Already open
            }

            DebugLogger.Log("UIShopManager: Opening shop");
            
            // Hide the shop button immediately when opening
            SetShopButtonVisible(false);
            
            // Block all game interactions during shop opening animation
            UIInteractionManager.BlockForMenuTransition("Shop");
            
            LeanTween.cancel(shopPanel);
            // For more effects, see: https://easings.net/
            // Configure animation to work properly with pause system
            ConfigureShopAnimation(LeanTween.move(shopPanel, openedPosition, animationDuration))
                .setOnComplete(() => {
                    isShopOpen = true;
                    
                    // Pause the game AFTER the animation completes
                    if (pauseGameWhenOpen)
                    {
                        GamePauseManager.RequestPause("UIShopManager");
                    }
                    
                    // Unblock interactions once shop is fully open
                    UIInteractionManager.UnblockForMenuTransition("Shop");
                    
                    DebugLogger.Log("UIShopManager: Shop opened successfully");
                });
        }

        /// <summary>
        /// Closes the shop with smooth animation, shows the shop button, and resumes the game
        /// </summary>
        public void CloseShop()
        {
            if (!isShopOpen)
            {
                return; // Already closed
            }
            
            DebugLogger.Log("UIShopManager: Closing shop");
            
            // Block interactions during shop closing animation
            UIInteractionManager.BlockForMenuTransition("Shop");
            
            // Resume the game BEFORE starting the close animation
            // This allows the animation to play smoothly
            if (pauseGameWhenOpen)
            {
                GamePauseManager.ReleasePause("UIShopManager");
            }
            
            LeanTween.cancel(shopPanel);
            // For more effects, see: https://easings.net/
            // Configure animation to work properly with pause system
            ConfigureShopAnimation(LeanTween.move(shopPanel, closedPosition, animationDuration))
                .setOnComplete(() => {
                    isShopOpen = false;
                    // Show the shop button again when shop is fully closed
                    SetShopButtonVisible(true);
                    
                    // Unblock interactions once shop is fully closed
                    UIInteractionManager.UnblockForMenuTransition("Shop");
                    
                    DebugLogger.Log("UIShopManager: Shop closed successfully");
                });
        }

        /// <summary>
        /// Toggles shop state between open and closed with smooth animation
        /// </summary>
        public void ToggleShop()
        {
            if (isShopOpen)
            {
                CloseShop();
            }
            else
            {
                OpenShop();
            }
        }

        /// <summary>
        /// Controls the visibility of the shop button
        /// </summary>
        /// <param name="visible">True to show the button, false to hide it</param>
        private void SetShopButtonVisible(bool visible)
        {
            if (shopButton != null)
            {
                shopButton.SetActive(visible);
                DebugLogger.Log($"UIShopManager: Shop button {(visible ? "shown" : "hidden")}");
            }
            else
            {
                DebugLogger.LogWarning("UIShopManager: Cannot control shop button visibility - shopButton is not assigned!");
            }
        }

        /// <summary>
        /// Configures a LeanTween animation with proper settings for pause-compatible UI
        /// </summary>
        /// <param name="tweenDescr">The LeanTween animation to configure</param>
        /// <returns>Configured LeanTween for method chaining</returns>
        private LTDescr ConfigureShopAnimation(LTDescr tweenDescr)
        {
            tweenDescr.setEase(easeType);
            
            // Use unscaled time so animations work even when game is paused
            if (useUnscaledTime)
            {
                tweenDescr.setIgnoreTimeScale(true);
            }
            
            return tweenDescr;
        }

        /// <summary>
        /// Public method to control shop button visibility externally
        /// </summary>
        /// <param name="visible">True to show the button, false to hide it</param>
        public void SetShopButtonVisibility(bool visible)
        {
            SetShopButtonVisible(visible);
        }

        /// <summary>
        /// Cleanup when the shop manager is destroyed to prevent pause leaks and interaction blocks
        /// </summary>
        void OnDestroy()
        {
            // If shop was open and paused the game, make sure to release the pause
            if (isShopOpen && pauseGameWhenOpen)
            {
                GamePauseManager.ReleasePause("UIShopManager");
                DebugLogger.Log("UIShopManager: Released pause on destroy");
            }
            
            // Cancel any ongoing animations and unblock interactions
            if (shopPanel != null)
            {
                LeanTween.cancel(shopPanel);
            }
            
            // Ensure we don't leak interaction blocks
            UIInteractionManager.UnblockForMenuTransition("Shop");
        }

        /// <summary>
        /// Public getter for current shop state
        /// </summary>
        public bool IsShopOpen => isShopOpen;
    }
}
