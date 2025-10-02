using UnityEngine;

namespace CarrotClicker
{
    /// <summary>
    /// Central repository for all game constants and configuration values.
    /// Provides compile-time constants for consistent behavior across the game
    /// and runtime utilities for platform detection.
    /// </summary>
    public static class GameConstants
    {
        // ===== VISUAL EFFECTS =====
        
        /// <summary>
        /// Carrot Animation Settings - Controls the visual feedback when player clicks the carrot
        /// </summary>
        public const float CARROT_ANIMATION_SCALE = 0.7f;      // Scale multiplier during click animation (shrink effect)
        public const float CARROT_ANIMATION_DURATION = 0.15f;  // How long the click animation lasts in seconds
        public const float CARROT_SCALE_RESET = 0.8f;          // Default scale of the carrot when not animating

        /// <summary>
        /// Frenzy Mode Settings - Special temporary boost mode for enhanced gameplay
        /// </summary>
        public const float FRENZY_DURATION = 5.0f;             // How long frenzy mode lasts in seconds
        public const int DEFAULT_FRENZY_MULTIPLIER = 10;       // Multiplier applied to carrot generation during frenzy
        public const float CARROT_FILL_AMOUNT = 0.1f;          // How much the frenzy meter fills per click (0-1 range)

        // ===== AUTO-CLICK SYSTEM =====
        
        /// <summary>
        /// Auto Click Settings - Controls the automatic carrot generation system
        /// </summary>
        public const float ROTATOR_SPEED = -10.0f;             // Rotation speed of bunny orbit (negative = clockwise)
        public const float ROTATOR_RADIUS = 2.5f;              // Distance bunnies orbit from center
        public const float CARROTS_PER_SECOND_MULTIPLIER = 0.1f; // Base rate multiplier for auto-generation
        public const int MAX_BUNNIES = 36;                     // Maximum number of auto-clicking bunnies
        public const float BUNNY_ANGLE_STEP = 10f;             // Degrees between each bunny position (360/36 = 10)
        public const float BUNNY_JUMP_HEIGHT = -0.5f;          // Y offset when bunny "jumps" to click
        public const float BUNNY_JUMP_DURATION = 0.25f;        // Duration of bunny jump animation

        // ===== USER INTERFACE =====
        
        /// <summary>
        /// UI Animation Settings - Controls button and UI element feedback
        /// </summary>
        public const float UI_SCALE_MULTIPLIER = 1.2f;         // Scale increase for button press feedback
        public const float UI_ANIMATION_DURATION = 0.1f;       // Duration of UI button animations
        
        /// <summary>
        /// FPS Counter Settings - Performance monitoring display configuration
        /// </summary>
        public const float FPS_UPDATE_INTERVAL = 0.5f;         // How often FPS display updates (seconds)
        public const float FPS_EXCELLENT_THRESHOLD = 55f;      // FPS above this = green color
        public const float FPS_GOOD_THRESHOLD = 45f;           // FPS above this = yellow color
        public const float FPS_ACCEPTABLE_THRESHOLD = 30f;     // FPS above this = orange color (below = red)
        
        /// <summary>
        /// Memory Monitor Settings - Memory usage tracking and display
        /// </summary>
        public const float MEMORY_UPDATE_INTERVAL = 1.0f;      // How often memory display updates (seconds)
        public const float MEMORY_LOW_THRESHOLD = 100f;        // MB - below this = green color
        public const float MEMORY_MODERATE_THRESHOLD = 200f;   // MB - below this = yellow color
        public const float MEMORY_HIGH_THRESHOLD = 300f;       // MB - above this = red color

        // ===== GAME PAUSE SYSTEM =====
        
        /// <summary>
        /// Game Pause Settings - Controls when and how the game pauses
        /// </summary>
        public const float NORMAL_TIME_SCALE = 1.0f;           // Default game speed when not paused
        public const float PAUSED_TIME_SCALE = 0.0f;           // Game speed when paused
        public const bool PAUSE_ON_MENU_OPEN = true;           // Should menus pause the game by default
        public const bool PAUSE_ON_MOBILE_BACKGROUND = true;   // Should game pause when app goes to background

        // ===== DATA PERSISTENCE =====
        
        /// <summary>
        /// Save Settings - Controls automatic game state saving
        /// </summary>
        public const float AUTO_SAVE_INTERVAL = 5.0f;          // How often game auto-saves (seconds)
        public const float MOBILE_SAVE_DELAY = 0.1f;           // Delay to ensure save completion on mobile

        /// <summary>
        /// PlayerPrefs Keys - String constants for consistent save/load operations
        /// Using constants prevents typos and makes refactoring easier
        /// </summary>
        public const string PREF_TOTAL_CARROTS = "TotalCarrots";
        public const string PREF_AUTO_CLICK_LEVEL = "AutoClickLevel";

        // ===== PERFORMANCE OPTIMIZATION =====
        
        /// <summary>
        /// Object Pool Settings - Controls reusable object pools for performance
        /// </summary>
        public const int PARTICLE_POOL_DEFAULT_CAPACITY = 10;  // Initial pool size
        public const int PARTICLE_POOL_MAX_SIZE = 20;          // Maximum pool size before cleanup
        public const float PARTICLE_LIFETIME = 1.0f;           // How long particles stay active
        
        // ===== NUMBER FORMATTING =====
        
        /// <summary>
        /// Number formatting thresholds - For displaying large numbers in readable format
        /// Used to convert raw numbers into "1.2K", "3.4M", etc.
        /// </summary>
        public const double THOUSAND = 1000d;
        public const double MILLION = 1000000d;
        public const double BILLION = 1000000000d;
        public const double TRILLION = 1000000000000d;
        public const double QUADRILLION = 1000000000000000d;
        
        // ===== PLATFORM UTILITIES =====
        
        /// <summary>
        /// Determines if the game is running on a mobile platform.
        /// Used to enable/disable mobile-specific features like touch controls.
        /// </summary>
        /// <returns>True if running on Android or iOS</returns>
        public static bool IsMobile()
        {
            return Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }
        
        /// <summary>
        /// Determines if the game is running in the Unity Editor.
        /// Useful for enabling debug features or editor-only functionality.
        /// </summary>
        /// <returns>True if running in any Unity Editor</returns>
        public static bool IsEditor()
        {
            return Application.platform == RuntimePlatform.WindowsEditor ||
                   Application.platform == RuntimePlatform.OSXEditor ||
                   Application.platform == RuntimePlatform.LinuxEditor;
        }
    }
}