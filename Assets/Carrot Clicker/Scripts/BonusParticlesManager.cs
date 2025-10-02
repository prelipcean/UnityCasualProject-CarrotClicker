#define CARROT_USE_UNITY_POOLING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CARROT_USE_UNITY_POOLING
using UnityEngine.Pool;
#endif

namespace CarrotClicker
{
    /// <summary>
    /// Manages the spawning and lifecycle of bonus particle effects that appear when carrots are clicked.
    /// Uses Unity's object pooling system for performance optimization when CARROT_USE_UNITY_POOLING is defined.
    /// Particles are automatically positioned at click locations and destroyed after a set lifetime.
    /// </summary>
    public class BonusParticlesManager : MonoBehaviour
    {
        [Header(" Elements ")]
        [SerializeField] private GameObject bonusParticlesPrefab; // Prefab for bonus particle effects (contains BonusParticle component)
        [SerializeField] private CarrotManager carrotManager; // Reference to get current carrot multiplier for display

#if CARROT_USE_UNITY_POOLING
        [Header(" Pooling ")]
        private ObjectPool<GameObject> bonusParticlePool; // Unity object pool for efficient particle reuse
#endif

        void Awake()
        {
            // Subscribe to carrot click events to spawn particles at click positions
            InputManager.onCarrotClickedPosition += CarrotClickedCallback;
        }

        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            InputManager.onCarrotClickedPosition -= CarrotClickedCallback;
        }

        // Start is called before the first frame update
        void Start()
        {
#if CARROT_USE_UNITY_POOLING
            // Initialize the object pool for bonus particles
            bonusParticlePool = new ObjectPool<GameObject>(
                // CreateFunction: Instantiate a new bonus particle prefab as child of this transform
                createFunc: () => Instantiate(bonusParticlesPrefab, transform),
                
                // ActionOnGet: Activate the particle when retrieved from pool
                actionOnGet: bonusParticle => bonusParticle.SetActive(true),
                
                // ActionOnRelease: Deactivate the particle when returned to pool
                actionOnRelease: bonusParticle => bonusParticle.SetActive(false),
                
                // ActionOnDestroy: Clean up particle when pool is destroyed or capacity exceeded
                actionOnDestroy: bonusParticle => Destroy(bonusParticle),
                
                // CollectionCheck: Disable for performance (don't check if items are already in pool)
                collectionCheck: false,
                
                // Pool capacity settings from GameConstants
                defaultCapacity: GameConstants.PARTICLE_POOL_DEFAULT_CAPACITY,
                maxSize: GameConstants.PARTICLE_POOL_MAX_SIZE
            );
#endif
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Event callback triggered when a carrot is clicked.
        /// Spawns a bonus particle at the click position showing the current multiplier value.
        /// Uses object pooling if enabled, otherwise creates and destroys particles normally.
        /// </summary>
        /// <param name="position">World position where the carrot was clicked</param>
        private void CarrotClickedCallback(Vector2 position)
        {
            // Safety check: Don't spawn particles if interactions are blocked
            // This is a backup check since InputManager should already prevent this
            if (!UIInteractionManager.CanInteract("ParticleSpawn"))
            {
                DebugLogger.Log("BonusParticlesManager: Particle spawn blocked by UI interactions");
                return;
            }
#if CARROT_USE_UNITY_POOLING
            // Get particle from pool for performance
            GameObject bonusParticleInstance = bonusParticlePool.Get();
            bonusParticleInstance.transform.position = position;

            // Configure the particle with current multiplier value
            BonusParticle bonusParticle = bonusParticleInstance.GetComponent<BonusParticle>();
            bonusParticle.Configure(carrotManager.GetCurrentMultiplier());

            // Return particle to pool after its lifetime expires
            LeanTween.delayedCall(GameConstants.PARTICLE_LIFETIME, () =>
            {
                bonusParticlePool.Release(bonusParticleInstance);
            });
#else
            // Fallback: Traditional instantiate/destroy approach (less performance-friendly)
            GameObject bonusParticleInstance = Instantiate(bonusParticlesPrefab, position, Quaternion.identity, transform);

            // Configure the particle with current multiplier value
            BonusParticle bonusParticle = bonusParticleInstance.GetComponent<BonusParticle>();
            bonusParticle.Configure(carrotManager.GetCurrentMultiplier());

            // Destroy particle after its lifetime expires
            Destroy(bonusParticleInstance, GameConstants.PARTICLE_LIFETIME);
#endif
        }
    }
}
