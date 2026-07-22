using UnityEngine;

namespace Gameplay.Buildings
{
    /// <summary>
    /// Controls the NeonTower's neon light effects entirely in code.
    /// Animator is disabled — this script takes full control of SpriteRenderer color.
    /// Animations don't work with additive blending (alpha is ignored),
    /// so we use sin/step/noise for breathing/pulse/glitch effects directly.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class NeonTowerController : MonoBehaviour
    {
        [Header("Neon Settings")]
        [SerializeField]
        [Tooltip("Master toggle. ON = dynamic effects, OFF = 80% constant.")]
        private bool neonEnabled = true;

        [SerializeField]
        [Tooltip("Global intensity multiplier 0-1. Scales ALL neon brightness.")]
        [Range(0f, 1f)]
        private float staticIntensity = 0.8f;

        [SerializeField]
        [Tooltip("Which neon animation mode to run.")]
        private NeonMode mode = NeonMode.Breathing;

        [SerializeField]
        [Tooltip("Reference to the Neon overlay SpriteRenderer. Auto-assigned if left null.")]
        private SpriteRenderer neonSpriteRenderer;

        public enum NeonMode { Breathing, Pulse, Glitch, Constant }

        private Animator animator;
        private float elapsed;

        // --- Hashing for Animator state name matching (we keep the animator disabled but read its state for future use) ---
        private static readonly int BreathingHash = Animator.StringToHash("Breathing");
        private static readonly int PulseHash = Animator.StringToHash("Pulse");
        private static readonly int GlitchHash = Animator.StringToHash("Glitch");
        private static readonly int DisabledHash = Animator.StringToHash("Disabled");

        private void Awake()
        {
            // Disable the Animator — we handle color in code
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            if (neonSpriteRenderer == null)
            {
                var renderers = GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in renderers)
                {
                    // Neon layer is now sortingOrder == 2 (Interior is 1, Base is 0)
                    if (sr.sortingOrder == 2)
                    {
                        neonSpriteRenderer = sr;
                        break;
                    }
                }
            }
        }

        private void OnEnable()
        {
            elapsed = 0f;
        }

        private void LateUpdate()
        {
            if (neonSpriteRenderer == null) return;

            elapsed += Time.deltaTime;
            float intensity;

            if (!neonEnabled)
            {
                // Disabled: constant 80% brightness, scaled by staticIntensity
                intensity = 0.8f * staticIntensity;
            }
            else
            {
                // Dynamic effect, scaled by staticIntensity
                float mod = ComputeModulation(mode, elapsed);
                intensity = mod * staticIntensity;
            }

            intensity = Mathf.Clamp01(intensity);
            neonSpriteRenderer.color = new Color(intensity, intensity, intensity, 1f);
        }

        /// <summary>
        /// Returns a modulation value 0-1 for the current neon mode at time t.
        /// </summary>
        private static float ComputeModulation(NeonMode mode, float t)
        {
            switch (mode)
            {
                case NeonMode.Breathing:
                    // Slow, smooth sine: oscillates between 0.5 and 1.0
                    // Period ≈ 3 seconds
                    return 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 0.33f);

                case NeonMode.Pulse:
                    // Quick bright flash, then slow decay, repeat every ~1.5s
                    float phase = t % 1.5f;
                    if (phase < 0.1f)
                        return 1.0f;                              // Flash ON
                    else if (phase < 0.4f)
                        return 1.0f - (phase - 0.1f) / 0.3f * 0.4f; // Quick drop
                    else
                        return 0.6f + 0.1f * Mathf.Sin(phase * 4f); // Gentle flicker

                case NeonMode.Glitch:
                    // Random spikes ~3-6 times per second
                    float noise = Mathf.PerlinNoise(t * 3.7f, 0.5f);
                    float spike = Mathf.PerlinNoise(t * 8.1f, 0.3f);
                    if (spike > 0.85f)
                        return 0.0f;          // Total blackout
                    else if (spike > 0.7f)
                        return 0.3f;           // Dim flicker
                    else if (noise > 0.8f)
                        return 1.2f;           // Bright spike (clamped to 1 later)
                    else
                        return 0.7f + 0.2f * Mathf.Sin(t * 2.3f); // Base flicker

                case NeonMode.Constant:
                default:
                    return 1.0f;
            }
        }

        // ===== Public API =====

        public void SetNeonEnabled(bool enabled)
        {
            neonEnabled = enabled;
        }

        public bool ToggleNeon()
        {
            neonEnabled = !neonEnabled;
            return neonEnabled;
        }

        public bool IsNeonEnabled => neonEnabled;

        public void SetMode(NeonMode newMode)
        {
            mode = newMode;
        }

        public void SetIntensity(float intensity)
        {
            staticIntensity = Mathf.Clamp01(intensity);
        }

        public float GetIntensity() => staticIntensity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && neonSpriteRenderer != null)
            {
                // Reflect changes immediately in editor play mode
                float testIntensity = neonEnabled
                    ? 0.8f * staticIntensity   // use a mid-range value for editor preview
                    : 0.8f * staticIntensity;
                neonSpriteRenderer.color = new Color(testIntensity, testIntensity, testIntensity, 1f);
            }
        }

        private void Reset()
        {
            neonSpriteRenderer = null;
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                // Neon layer is now sortingOrder == 2
                if (sr.sortingOrder == 2)
                {
                    neonSpriteRenderer = sr;
                    break;
                }
            }
        }
#endif
    }
}
