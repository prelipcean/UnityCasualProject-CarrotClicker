using UnityEngine;

namespace CarrotClicker
{
    /// <summary>
    /// Centralized manager for controlling UI interaction states.
    /// Prevents carrot clicks and other game interactions during menu animations and transitions.
    /// Uses a simple boolean flag system.
    /// </summary>
    public static class UIInteractionManager
    {
        // Simple boolean interaction blocking system
        private static bool isInteractionBlocked = false;
        private static string currentRequester = "";
        
        /// <summary>
        /// True if any UI interactions are currently blocked (e.g., during animations)
        /// </summary>
        public static bool IsInteractionBlocked => isInteractionBlocked;

        /// <summary>
        /// Block all game interactions (carrot clicks, buttons, etc.)
        /// </summary>
        /// <param name="requester">Name of the system requesting the block (for debugging)</param>
        public static void BlockInteractions(string requester)
        {
            if (!isInteractionBlocked)
            {
                isInteractionBlocked = true;
                currentRequester = requester;
                DebugLogger.Log($"UIInteractionManager: Interactions blocked by {requester}");
            }
            else
            {
                DebugLogger.Log($"UIInteractionManager: Interactions already blocked by {currentRequester}, ignoring request from {requester}");
            }
        }

        /// <summary>
        /// Unblock game interactions.
        /// </summary>
        /// <param name="requester">Name of the system releasing the block (for debugging)</param>
        public static void UnblockInteractions(string requester)
        {
            if (!isInteractionBlocked)
            {
                DebugLogger.LogWarning($"UIInteractionManager: {requester} tried to unblock interactions, but no blocks are active!");
                return;
            }

            if (currentRequester == requester)
            {
                isInteractionBlocked = false;
                currentRequester = "";
                DebugLogger.Log($"UIInteractionManager: Interactions unblocked by {requester}");
            }
            else
            {
                DebugLogger.LogWarning($"UIInteractionManager: {requester} tried to unblock interactions, but they were blocked by {currentRequester}!");
            }
        }

        /// <summary>
        /// Force clear all interaction blocks. Use with caution - only for emergency situations.
        /// </summary>
        public static void ForceUnblockAll()
        {
            if (isInteractionBlocked)
            {
                DebugLogger.LogWarning($"UIInteractionManager: Force unblocking all interactions! Was blocked by: {currentRequester}");
                isInteractionBlocked = false;
                currentRequester = "";
            }
        }

        /// <summary>
        /// Check if a specific type of interaction should be allowed
        /// </summary>
        /// <param name="interactionType">Type of interaction to check (for debugging)</param>
        /// <returns>True if interaction is allowed</returns>
        public static bool CanInteract(string interactionType = "Generic")
        {
            bool canInteract = !IsInteractionBlocked;
            
            if (!canInteract)
            {
                DebugLogger.Log($"UIInteractionManager: {interactionType} interaction blocked by {currentRequester}");
            }
            
            return canInteract;
        }

        /// <summary>
        /// Get debug information about current interaction state
        /// </summary>
        /// <returns>Formatted string with current state</returns>
        public static string GetDebugInfo()
        {
            if (isInteractionBlocked)
            {
                return $"Blocked: {IsInteractionBlocked}, By: {currentRequester}";
            }
            else
            {
                return $"Blocked: {IsInteractionBlocked}";
            }
        }

        /// <summary>
        /// Get the current active requester (if any)
        /// </summary>
        /// <returns>Name of system currently blocking interactions, or empty string if not blocked</returns>
        public static string GetActiveRequester()
        {
            return currentRequester;
        }

        #region Convenience Methods for Common UI Elements

        /// <summary>
        /// Block interactions during a UI animation. Use in animation start callbacks.
        /// </summary>
        /// <param name="uiElementName">Name of the UI element being animated</param>
        public static void BlockForAnimation(string uiElementName)
        {
            BlockInteractions($"Animation-{uiElementName}");
        }

        /// <summary>
        /// Unblock interactions after a UI animation completes. Use in animation complete callbacks.
        /// </summary>
        /// <param name="uiElementName">Name of the UI element that finished animating</param>
        public static void UnblockForAnimation(string uiElementName)
        {
            UnblockInteractions($"Animation-{uiElementName}");
        }

        /// <summary>
        /// Block interactions while a menu is transitioning (opening/closing)
        /// </summary>
        /// <param name="menuName">Name of the menu being transitioned</param>
        public static void BlockForMenuTransition(string menuName)
        {
            BlockInteractions($"MenuTransition-{menuName}");
        }

        /// <summary>
        /// Unblock interactions after a menu transition completes
        /// </summary>
        /// <param name="menuName">Name of the menu that finished transitioning</param>
        public static void UnblockForMenuTransition(string menuName)
        {
            UnblockInteractions($"MenuTransition-{menuName}");
        }

        #endregion
    }
}