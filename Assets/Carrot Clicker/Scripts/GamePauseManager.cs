using UnityEngine;

namespace CarrotClicker
{
    /// <summary>
    /// Centralized game pause management system.
    /// Handles pausing/unpausing game time and tracking which systems requested the pause.
    /// This allows multiple UI systems (shop, settings, achievements) to pause the game independently.
    /// 
    /// IMPORTANT: When creating UI animations that should work while the game is paused,
    /// use .setIgnoreTimeScale(true) on your LeanTween animations, otherwise they will freeze
    /// when Time.timeScale = 0.
    /// 
    /// Example:
    /// LeanTween.move(panel, targetPos, duration)
    ///     .setIgnoreTimeScale(true)  // Critical for pause-compatible animations
    ///     .setOnComplete(callback);
    /// </summary>
    public static class GamePauseManager
    {
        // Stack-based pause system - allows multiple systems to request pause
        private static int pauseRequestCount = 0;
        private static float previousTimeScale = 1.0f;

        /// <summary>
        /// Current pause state of the game
        /// </summary>
        public static bool IsPaused => pauseRequestCount > 0;

        /// <summary>
        /// Current number of active pause requests
        /// </summary>
        public static int ActivePauseRequests => pauseRequestCount;

        /// <summary>
        /// Request to pause the game. Multiple systems can request pause simultaneously.
        /// Game will remain paused until all systems release their pause requests.
        /// </summary>
        /// <param name="requester">Name of the system requesting pause (for debugging)</param>
        public static void RequestPause(string requester)
        {
            if (pauseRequestCount == 0)
            {
                // First pause request - actually pause the game
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                DebugLogger.Log($"GamePauseManager: Game paused by {requester}");
            }
            else
            {
                DebugLogger.Log($"GamePauseManager: Additional pause request from {requester} (total: {pauseRequestCount + 1})");
            }

            pauseRequestCount++;
        }

        /// <summary>
        /// Release a pause request. Game will resume only when all pause requests are released.
        /// </summary>
        /// <param name="requester">Name of the system releasing pause (for debugging)</param>
        public static void ReleasePause(string requester)
        {
            if (pauseRequestCount <= 0)
            {
                DebugLogger.LogWarning($"GamePauseManager: {requester} tried to release pause, but no pause requests are active!");
                return;
            }

            pauseRequestCount--;

            if (pauseRequestCount == 0)
            {
                // Last pause request released - resume the game
                Time.timeScale = previousTimeScale;
                DebugLogger.Log($"GamePauseManager: Game resumed by {requester}");
            }
            else
            {
                DebugLogger.Log($"GamePauseManager: Pause request released by {requester} (remaining: {pauseRequestCount})");
            }
        }

        /// <summary>
        /// Force resume the game by clearing all pause requests.
        /// Use with caution - only for emergency situations or game state resets.
        /// </summary>
        public static void ForceResume()
        {
            if (pauseRequestCount > 0)
            {
                DebugLogger.LogWarning($"GamePauseManager: Force resuming game! Had {pauseRequestCount} active pause requests.");
                pauseRequestCount = 0;
                Time.timeScale = previousTimeScale;
            }
        }

        /// <summary>
        /// Set a custom time scale when the game is not paused.
        /// Useful for slow-motion effects or game speed modifications.
        /// </summary>
        /// <param name="newTimeScale">New time scale to use when game is not paused</param>
        public static void SetNormalTimeScale(float newTimeScale)
        {
            previousTimeScale = newTimeScale;
            
            // Apply immediately if game is not currently paused
            if (pauseRequestCount == 0)
            {
                Time.timeScale = newTimeScale;
            }
            
            DebugLogger.Log($"GamePauseManager: Normal time scale set to {newTimeScale}");
        }

        /// <summary>
        /// Get debug information about current pause state
        /// </summary>
        /// <returns>Formatted string with pause state information</returns>
        public static string GetDebugInfo()
        {
            return $"Paused: {IsPaused}, Requests: {pauseRequestCount}, TimeScale: {Time.timeScale}, Normal: {previousTimeScale}";
        }

        #region Unity Lifecycle Support
        
        /// <summary>
        /// Call this when the application loses focus to handle mobile lifecycle
        /// </summary>
        public static void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                RequestPause("ApplicationPause");
            }
            else
            {
                ReleasePause("ApplicationPause");
            }
        }

        /// <summary>
        /// Call this when the application loses focus to handle desktop lifecycle
        /// </summary>
        public static void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                RequestPause("ApplicationFocus");
            }
            else
            {
                ReleasePause("ApplicationFocus");
            }
        }

        #endregion
    }
}