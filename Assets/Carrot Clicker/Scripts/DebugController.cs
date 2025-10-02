using UnityEngine;

namespace CarrotClicker
{
    /// <summary>
    /// Handles debug functionality like FPS counter, memory info, and input blocking status display.
    /// Supports desktop keyboard shortcuts and automatically enables debug features on mobile devices.
    /// </summary>
    public class DebugController : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private KeyCode toggleFpsKey = KeyCode.F;
        [SerializeField] private KeyCode toggleMemoryKey = KeyCode.M;
        [SerializeField] private KeyCode toggleInputBlockingKey = KeyCode.I;
        [SerializeField] private bool enableDebugControls = true;
        
        [Header("Mobile Debug")]
        [SerializeField] private bool autoEnableOnMobile = true; // Automatically enable debug features on mobile
        [SerializeField] private bool showInputBlockingInfo = true; // Show input blocking status on screen

        private bool mobileDebugInitialized = false;

        void Update()
        {
            if (!enableDebugControls) return;

            // Desktop/Editor: Use keyboard shortcuts for debug toggles
            if (Input.GetKeyDown(toggleFpsKey))
            {
                ToggleFpsCounter();
            }
            
            if (Input.GetKeyDown(toggleMemoryKey))
            {
                ToggleMemoryInfo();
            }
            
            if (Input.GetKeyDown(toggleInputBlockingKey))
            {
                ToggleInputBlockingInfo();
            }

            // Mobile: Automatically enable debug features permanently
            if (GameConstants.IsMobile() && autoEnableOnMobile && !mobileDebugInitialized)
            {
                EnableMobileDebugFeatures();
                mobileDebugInitialized = true;
            }
        }

        /// <summary>
        /// Automatically enables debug features for mobile devices.
        /// Called once when the controller initializes on mobile platforms.
        /// </summary>
        private void EnableMobileDebugFeatures()
        {
            if (UIManager.instance != null)
            {
                // Enable FPS counter permanently on mobile
                if (!UIManager.instance.IsFpsCounterEnabled())
                {
                    UIManager.instance.ToggleFpsCounter();
                    Debug.Log("DebugController: FPS Counter automatically enabled on mobile");
                }

                // Enable memory info permanently on mobile
                if (!UIManager.instance.IsMemoryInfoEnabled())
                {
                    UIManager.instance.ToggleMemoryInfo();
                    Debug.Log("DebugController: Memory Info automatically enabled on mobile");
                }

                // Enable input blocking info if configured to show
                if (showInputBlockingInfo && !UIManager.instance.IsInputBlockingInfoEnabled())
                {
                    UIManager.instance.ToggleInputBlockingInfo();
                    Debug.Log("DebugController: Input Blocking Info automatically enabled on mobile");
                }
            }
            else
            {
                Debug.LogWarning("DebugController: UIManager instance not found! Cannot enable mobile debug features.");
            }
        }

        /// <summary>
        /// Toggles the FPS counter display through UIManager.
        /// Provides console feedback and optional mobile UI feedback.
        /// </summary>
        private void ToggleFpsCounter()
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.ToggleFpsCounter();
                
                // Log current state for debugging
                string status = UIManager.instance.IsFpsCounterEnabled() ? "ON" : "OFF";
                Debug.Log($"DebugController: FPS Counter {status}");
                
                // TODO: Add temporary UI message for mobile feedback
                if (GameConstants.IsMobile())
                {
                    // You could add a temporary UI message here
                }
            }
            else
            {
                Debug.LogWarning("DebugController: UIManager instance not found!");
            }
        }

        /// <summary>
        /// Enables or disables all debug controls.
        /// Useful for release builds or when debug features should be temporarily disabled.
        /// </summary>
        public void EnableDebugControls(bool enabled)
        {
            enableDebugControls = enabled;
        }

        /// <summary>
        /// Changes the keyboard shortcut for toggling FPS counter.
        /// Allows runtime customization of debug controls.
        /// </summary>
        public void SetFpsToggleKey(KeyCode newKey)
        {
            toggleFpsKey = newKey;
        }

        /// <summary>
        /// Enables or disables automatic debug feature activation on mobile.
        /// </summary>
        public void SetAutoEnableOnMobile(bool enabled)
        {
            autoEnableOnMobile = enabled;
            if (enabled && GameConstants.IsMobile() && !mobileDebugInitialized)
            {
                EnableMobileDebugFeatures();
                mobileDebugInitialized = true;
            }
        }

        /// <summary>
        /// Toggles memory information display through UIManager.
        /// Similar to FPS counter but for memory usage statistics.
        /// </summary>
        private void ToggleMemoryInfo()
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.ToggleMemoryInfo();
                
                // Log current state for debugging
                string status = UIManager.instance.IsMemoryInfoEnabled() ? "ON" : "OFF";
                Debug.Log($"DebugController: Memory Info {status}");
            }
            else
            {
                Debug.LogWarning("DebugController: UIManager instance not found!");
            }
        }

        /// <summary>
        /// Toggles input blocking information display through UIManager.
        /// Shows current UI interaction blocking status and which systems are blocking input.
        /// </summary>
        private void ToggleInputBlockingInfo()
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.ToggleInputBlockingInfo();
                
                // Log current state for debugging
                string status = UIManager.instance.IsInputBlockingInfoEnabled() ? "ON" : "OFF";
                Debug.Log($"DebugController: Input Blocking Info {status}");
            }
            else
            {
                Debug.LogWarning("DebugController: UIManager instance not found!");
            }
        }

        // Public methods for UI button integration

        /// <summary>
        /// Public wrapper for FPS toggle - can be called from UI buttons or external scripts.
        /// </summary>
        public void ManualToggleFps()
        {
            ToggleFpsCounter();
        }

        /// <summary>
        /// Public wrapper for memory info toggle - can be called from UI buttons or external scripts.
        /// </summary>
        public void ManualToggleMemory()
        {
            ToggleMemoryInfo();
        }

        /// <summary>
        /// Public wrapper for input blocking info toggle - can be called from UI buttons or external scripts.
        /// </summary>
        public void ManualToggleInputBlocking()
        {
            ToggleInputBlockingInfo();
        }
    }
}