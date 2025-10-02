using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CarrotClicker
{
    /// <summary>
    /// Visual particle effect that displays bonus carrot values as floating text.
    /// Used to provide player feedback when bonus carrots are earned from clicks or other events.
    /// This component is typically instantiated temporarily and destroyed after animation completes.
    /// </summary>
    public class BonusParticle : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private TextMeshPro bonusText; // Text component that displays the bonus amount (e.g., "+5", "+10")

        /// <summary>
        /// Initializes the particle with the bonus carrot amount to display.
        /// Call this immediately after instantiation to set up the visual feedback.
        /// </summary>
        /// <param name="carrotMultiplier">The number of bonus carrots earned (will be displayed as "+{value}")</param>
        public void Configure(int carrotMultiplier)
        {
            // Format the bonus amount with a plus sign for positive reinforcement
            bonusText.text = $"+{carrotMultiplier}";
        }

        // TODO: Consider adding animation methods here such as:
        // - Fade out animation using LeanTween or DOTween
        // - Upward movement animation to simulate floating effect
        // - Scale animation for pop-in effect when spawned
        // - Self-destruction after animation completes to prevent memory leaks
    }
}
