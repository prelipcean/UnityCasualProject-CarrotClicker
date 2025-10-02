using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CarrotClicker
{
    /// <summary>
    /// Main interactive carrot object that players click to earn rewards.
    /// Handles click animations, frenzy mode progression, and visual feedback.
    /// The carrot fills up over multiple clicks and triggers a special frenzy mode when full.
    /// </summary>
    public class Carrot : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private Transform carrotRendererTransform; // Transform for the visual carrot sprite (used for scale animations)
        [SerializeField] private Image carrotFillImage; // UI fill bar that shows progress toward frenzy mode

        [Header(" Settings ")]
        [SerializeField] private float fillAmount = GameConstants.CARROT_FILL_AMOUNT; // Amount to fill the carrot per click (0.0 to 1.0)
        private bool isInFrenzyMode = false; // Prevents additional filling while frenzy mode is active

        [Header(" Actions ")]
        public static Action onFrenzyModeStarted; // Event fired when frenzy mode begins (other systems can react)
        public static Action onFrenzyModeEnded; // Event fired when frenzy mode ends (cleanup/reset other systems)

        // [Header(" Debug ")]
        // [SerializeField] private bool isDebugMode = false; // Commented out debug functionality

        private void Awake()
        {
            // Subscribe to click events from the input system
            InputManager.onCarrotClicked += CarrotClickedCallback;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            InputManager.onCarrotClicked -= CarrotClickedCallback;
        }

        /// <summary>
        /// Event callback triggered when the carrot is clicked.
        /// Handles both visual animation and frenzy mode progression.
        /// Input blocking is now handled at the InputManager level for better performance.
        /// </summary>
        private void CarrotClickedCallback()
        {
            // Animate the carrot for visual feedback
            Animate();

            // Fill the carrot toward frenzy mode (only if not already in frenzy)
            if (!isInFrenzyMode)
            {
                FillCarrot();
            }
        }

        /// <summary>
        /// Plays a scale animation on the carrot sprite to provide click feedback.
        /// Uses LeanTween for smooth scaling that grows and shrinks back to normal size.
        /// </summary>
        private void Animate()
        {
            // Reset to normal scale before starting animation
            carrotRendererTransform.localScale = Vector3.one * GameConstants.CARROT_SCALE_RESET;
            
            // Cancel any ongoing animation to prevent conflicts
            LeanTween.cancel(carrotRendererTransform.gameObject);
            
            // Scale up and back down using ping-pong loop for satisfying click feedback
            LeanTween.scale(carrotRendererTransform.gameObject, Vector3.one * GameConstants.CARROT_ANIMATION_SCALE, GameConstants.CARROT_ANIMATION_DURATION)
                .setLoopPingPong(1);
        }

        /// <summary>
        /// Increases the carrot's fill amount and triggers frenzy mode when full.
        /// Each click contributes toward filling the carrot meter.
        /// </summary>
        private void FillCarrot()
        {
            // Increment fill amount by the configured value per click
            carrotFillImage.fillAmount += fillAmount;
            
            // Check if carrot is now full and ready for frenzy mode
            if (carrotFillImage.fillAmount >= 1.0f)
            {
                StartFrenzyMode();
            }
        }

        /// <summary>
        /// Initiates frenzy mode - a special state with enhanced rewards and visual effects.
        /// During frenzy mode, the fill bar drains over time and additional filling is disabled.
        /// Notifies other systems via events and UI manager.
        /// </summary>
        private void StartFrenzyMode()
        {
            // Prevent additional filling during frenzy
            isInFrenzyMode = true;
            
            // Notify other systems that frenzy mode has started
            onFrenzyModeStarted?.Invoke();
            
            // Update UI to show frenzy mode visuals
            if (UIManager.instance != null)
            {
                UIManager.instance.ShowFrenzyMode();
            }
            
            // Animate the fill bar draining over the frenzy duration
            LeanTween.value(1, 0, GameConstants.FRENZY_DURATION)
                .setOnUpdate((float val) =>
                {
                    // Update fill amount as it drains
                    carrotFillImage.fillAmount = val;
                })
                .setOnComplete(() =>
                {
                    // Frenzy mode complete - reset state
                    isInFrenzyMode = false;
                    
                    // Notify other systems that frenzy mode has ended
                    onFrenzyModeEnded?.Invoke();
                    
                    // Update UI to hide frenzy mode visuals
                    if (UIManager.instance != null)
                    {
                        UIManager.instance.HideFrenzyMode();
                    }
                });      
        }

        /// <summary>
        /// Check if the carrot can currently be clicked (not blocked by UI interactions)
        /// </summary>
        /// <returns>True if carrot clicks are allowed</returns>
        public bool CanBeClicked()
        {
            return UIInteractionManager.CanInteract("CarrotClick");
        }

        /// <summary>
        /// Get the current interaction block status for debugging
        /// </summary>
        /// <returns>Debug information about interaction state</returns>
        public string GetInteractionDebugInfo()
        {
            return $"CanClick: {CanBeClicked()}, InFrenzy: {isInFrenzyMode}, " + UIInteractionManager.GetDebugInfo();
        }
    }
}