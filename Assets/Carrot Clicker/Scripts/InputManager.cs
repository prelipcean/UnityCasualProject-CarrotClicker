#define CARROT_USE_TOUCH_INPUT // Uncomment this line to use touch input on mobile devices

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CarrotClicker
{
    /// <summary>
    /// Handles input detection for the carrot clicker game.
    /// Supports both mouse input (PC) and touch input (mobile) based on compilation directives.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Action")]
        // Static events that other scripts can subscribe to for carrot click notifications
        public static Action onCarrotClicked;
        public static Action<Vector2> onCarrotClickedPosition; // Passes world position of the click

        // Start is called before the first frame update
        void Start()
        {
            // Set target framerate to 60 FPS for consistent performance
            Application.targetFrameRate = 60;
        }

        // Update is called once per frame
        void Update()
        {
            try
            {
                // Check if interactions are blocked by UI animations or menu transitions
                if (!UIInteractionManager.CanInteract("Input"))
                {
                    // Debug: Show when input is being blocked (only log occasionally to avoid spam)
                    if (Time.frameCount % 30 == 0) // Log every 30 frames (roughly twice per second at 60fps)
                    {
                        DebugLogger.Log($"InputManager: Input blocked - {UIInteractionManager.GetDebugInfo()}");
                    }
                    return; // Skip all input processing when interactions are blocked
                }

#if CARROT_USE_TOUCH_INPUT
                // Mobile touch input: Check for any active touches
                if (Input.touchCount > 0)
                {
                    ThrowRaycast();
                }
#else
                // PC mouse input: Check for left mouse button press
                if (Input.GetMouseButtonDown(0))
                {
                    ThrowRaycast();
                }
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"InputManager: Error in Update: {e.Message}");
            }
        }

#if CARROT_USE_TOUCH_INPUT
        /// <summary>
        /// Handles touch input for mobile devices. Supports multi-touch detection.
        /// </summary>
        private void ThrowRaycast()
        {
            // Process each active touch point
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // Only process when touch begins (not during drag or release)
                if (touch.phase == TouchPhase.Began)
                {
                    // Convert touch screen position to world ray and check for 2D collider hits
                    RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(touch.position));

                    if (hit.collider != null)
                    {
                        DebugLogger.Log("InputManager: TouchHit detected - " + hit.collider.name);
                        // Notify subscribers that a carrot was clicked
                        onCarrotClicked?.Invoke();
                        // Pass the world position where the touch occurred
                        onCarrotClickedPosition?.Invoke(hit.point);
                    }
                }
            }
        }
#else
    /// <summary>
    /// Handles mouse input for PC/desktop platforms. Single click detection only.
    /// </summary>
    private void ThrowRaycast()
    {
        // Safety check to ensure main camera exists
        if (Camera.main == null)
        {
            Debug.LogError("InputManager: Main camera is null. Cannot perform raycast.");
            return;
        }

        try
        {
            // Convert mouse screen position to world ray and check for 2D collider hits
            // Input.mousePosition is in screen coordinates (pixels)
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (hit.collider != null)
            {
                DebugLogger.Log("InputManager: MouseHit detected - " + hit.collider.name);
                // Notify subscribers that a carrot was clicked
                onCarrotClicked?.Invoke();
                // Pass the world position where the click occurred (hit.point is in world coordinates)
                onCarrotClickedPosition?.Invoke(hit.point);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InputManager: Error in mouse raycast: {e.Message}");
        }
    }
#endif
    }
}
