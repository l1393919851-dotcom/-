using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Effects
{
    /// <summary>
    /// 局部空间破碎 → 中心裂开 → 露出内部怪物。
    /// 层级建议：
    ///   SpawnRoot
    ///     ShatterSurface  (本组件 + LocalSpaceShatter 材质，在前)
    ///     Monster         (在后，初始隐藏；可挂 CyberAppearEffect)
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalSpaceShatterReveal : MonoBehaviour
    {
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int HoleId = Shader.PropertyToID("_Hole");

        [Header("Surface")]
        [SerializeField] Renderer shatterRenderer;
        [SerializeField] float shatterDuration = 0.55f;
        [SerializeField] float holeDuration = 0.45f;
        [SerializeField] float holdOpen = 0.2f;
        [SerializeField] bool fadeShatterAfterReveal = true;
        [SerializeField] float fadeDuration = 0.35f;
        [SerializeField] bool playOnEnable;

        [Header("Monster")]
        [SerializeField] GameObject monster;
        [SerializeField] CyberAppearEffect monsterAppear;
        [SerializeField] float monsterSpawnAt = 0.45f; // 0-1 of whole timeline before hole finishes
        [SerializeField] bool hideMonsterOnAwake = true;

        [Header("Events")]
        [SerializeField] UnityEvent onShatterStart;
        [SerializeField] UnityEvent onMonsterRevealed;
        [SerializeField] UnityEvent onFinished;

        MaterialPropertyBlock _mpb;
        Coroutine _routine;
        bool _monsterShown;

        void Reset()
        {
            shatterRenderer = GetComponent<Renderer>();
            if (monsterAppear == null && monster != null)
                monsterAppear = monster.GetComponent<CyberAppearEffect>();
        }

        void Awake()
        {
            if (shatterRenderer == null)
                shatterRenderer = GetComponent<Renderer>();

            if (monsterAppear == null && monster != null)
                monsterAppear = monster.GetComponentInChildren<CyberAppearEffect>(true);

            SetShader(0f, 0f);

            if (hideMonsterOnAwake && monster != null)
            {
                // 若用 CyberAppear，先藏外观；否则直接关物体
                if (monsterAppear != null)
                {
                    monster.SetActive(true);
                    monsterAppear.SnapHidden();
                }
                else
                {
                    monster.SetActive(false);
                }
            }
        }

        void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        void OnDisable()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        [ContextMenu("Play Reveal")]
        public void Play()
        {
            if (!isActiveAndEnabled)
                return;

            if (_routine != null)
                StopCoroutine(_routine);

            _monsterShown = false;
            _routine = StartCoroutine(PlayRoutine());
        }

        public void SnapClosed()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            SetShader(0f, 0f);
            if (hideMonsterOnAwake && monster != null)
            {
                if (monsterAppear != null)
                    monsterAppear.SnapHidden();
                else
                    monster.SetActive(false);
            }
        }

        IEnumerator PlayRoutine()
        {
            onShatterStart?.Invoke();
            SetShader(0f, 0f);

            float shatterDur = Mathf.Max(0.01f, shatterDuration);
            float holeDur = Mathf.Max(0.01f, holeDuration);
            float totalPreHold = shatterDur + holeDur;
            float spawnGate = Mathf.Clamp01(monsterSpawnAt) * totalPreHold;

            float t = 0f;
            // Phase 1: shatter builds
            while (t < shatterDur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / shatterDur);
                float intensity = Smooth(u);
                SetShader(intensity, 0f);
                TrySpawnMonster(t, spawnGate);
                yield return null;
            }

            SetShader(1f, 0f);

            // Phase 2: hole opens → see monster
            float h = 0f;
            while (h < holeDur)
            {
                h += Time.deltaTime;
                float elapsed = shatterDur + h;
                float hole = Smooth(Mathf.Clamp01(h / holeDur));
                SetShader(1f, hole);
                TrySpawnMonster(elapsed, spawnGate);
                yield return null;
            }

            SetShader(1f, 1f);
            TrySpawnMonster(totalPreHold, spawnGate);

            if (holdOpen > 0f)
                yield return new WaitForSeconds(holdOpen);

            // Phase 3: optional fade shattered shell, leave hole clear
            if (fadeShatterAfterReveal)
            {
                float f = 0f;
                float fadeDur = Mathf.Max(0.01f, fadeDuration);
                while (f < fadeDur)
                {
                    f += Time.deltaTime;
                    float k = 1f - Smooth(Mathf.Clamp01(f / fadeDur));
                    SetShader(k, 1f);
                    yield return null;
                }

                SetShader(0f, 1f);
                // 完全清掉遮罩，只留怪物
                SetShader(0f, 0f);
            }

            onFinished?.Invoke();
            _routine = null;
        }

        void TrySpawnMonster(float elapsed, float spawnGate)
        {
            if (_monsterShown || elapsed < spawnGate)
                return;

            _monsterShown = true;
            if (monster != null)
            {
                monster.SetActive(true);
                if (monsterAppear != null)
                {
                    // CyberAppear 默认 PlayOnEnable；若已激活则手动 Play
                    monsterAppear.Play();
                }
            }

            onMonsterRevealed?.Invoke();
        }

        static float Smooth(float x)
        {
            return x * x * (3f - 2f * x);
        }

        void SetShader(float intensity, float hole)
        {
            if (shatterRenderer == null)
                return;

            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();

            shatterRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(IntensityId, Mathf.Clamp01(intensity));
            _mpb.SetFloat(HoleId, Mathf.Clamp01(hole));
            shatterRenderer.SetPropertyBlock(_mpb);
        }
    }
}
