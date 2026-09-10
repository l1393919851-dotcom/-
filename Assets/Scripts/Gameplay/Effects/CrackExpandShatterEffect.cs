using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Effects
{
    /// <summary>
    /// 玻璃破碎感：在总时长内分 N 次阶跃加深（默认 5 秒 4 次），
    /// 每次裂纹突然多一截，中间静止，不是连续动画。
    /// 材质：URP/Effects/LocalSpaceShatter
    /// </summary>
    [DisallowMultipleComponent]
    public class CrackExpandShatterEffect : MonoBehaviour
    {
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int ExpandId = Shader.PropertyToID("_Expand");
        static readonly int HoleId = Shader.PropertyToID("_Hole");
        static readonly int StageId = Shader.PropertyToID("_Stage");

        [Header("Surface")]
        [SerializeField] Renderer shatterRenderer;
        [SerializeField] float totalDuration = 5f;
        [SerializeField] int crackSteps = 4;
        [SerializeField] float startDelay = 0.15f;
        [Tooltip("每次碎裂瞬间的短冲击（秒），之后保持静止直到下一次")]
        [SerializeField] float stepFlash = 0.08f;
        [SerializeField] bool playOnEnable = true;
        [SerializeField] bool holdAtEnd = true;

        [Header("Stage Look (0→1 per step)")]
        [SerializeField] float[] stepExpand =
        {
            0.28f, // 1: 中心细裂纹
            0.52f, // 2: 裂纹外扩
            0.78f, // 3: 大块裂开
            1.00f  // 4: 整面碎裂
        };
        [SerializeField] float[] stepIntensity =
        {
            0.55f,
            0.75f,
            0.9f,
            1.0f
        };

        [Header("After All Steps (Optional)")]
        [SerializeField] bool openHoleAfterExpand;
        [SerializeField] float holeDuration = 0.5f;
        [SerializeField] bool fadeShellAfterHole;
        [SerializeField] float fadeDuration = 0.35f;

        [Header("Monster (Optional)")]
        [SerializeField] GameObject monster;
        [SerializeField] CyberAppearEffect monsterAppear;
        [SerializeField] int spawnMonsterAtStep = 4; // 第几次破碎后刷怪（1-based）
        [SerializeField] bool hideMonsterOnAwake = true;

        [Header("Events")]
        [SerializeField] UnityEvent onStarted;
        [SerializeField] UnityEvent onStepCracked; // 每次阶跃
        [SerializeField] UnityEvent onExpandFinished;
        [SerializeField] UnityEvent onMonsterRevealed;
        [SerializeField] UnityEvent onFinished;

        MaterialPropertyBlock _mpb;
        Coroutine _routine;
        bool _monsterShown;

        void Reset()
        {
            shatterRenderer = GetComponent<Renderer>();
        }

        void Awake()
        {
            if (shatterRenderer == null)
                shatterRenderer = GetComponent<Renderer>();
            if (monsterAppear == null && monster != null)
                monsterAppear = monster.GetComponentInChildren<CyberAppearEffect>(true);

            SetProgress(0f, 0f, 0f, 0f);

            if (hideMonsterOnAwake && monster != null)
            {
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
            if (playOnEnable && Application.isPlaying)
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

        [ContextMenu("Play Glass Crack (4 steps / 5s)")]
        public void Play()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[CrackExpandShatter] 请先进入 Play 模式。", this);
                return;
            }

            if (shatterRenderer == null)
                shatterRenderer = GetComponent<Renderer>();
            if (shatterRenderer == null)
            {
                Debug.LogError("[CrackExpandShatter] 找不到 Renderer。", this);
                return;
            }

            var mat = shatterRenderer.sharedMaterial;
            if (mat == null || mat.shader == null || !mat.shader.name.Contains("LocalSpaceShatter"))
            {
                Debug.LogError(
                    "[CrackExpandShatter] 材质需为 URP/Effects/LocalSpaceShatter，当前: " +
                    (mat != null && mat.shader != null ? mat.shader.name : "null"),
                    this);
                return;
            }

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

            SetProgress(0f, 0f, 0f, 0f);
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
            onStarted?.Invoke();
            SetProgress(0f, 0f, 0f, 0f);

            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);

            int steps = Mathf.Max(1, crackSteps);
            EnsureStepArrays(steps);

            float total = Mathf.Max(0.2f, totalDuration);
            // 每段：静止等待 + 瞬间碎裂；均分在总时长里
            float interval = total / steps;

            for (int i = 0; i < steps; i++)
            {
                // 等到这一拍（前 steps-1 段等 interval，最后一段也占一份时长）
                float wait = Mathf.Max(0f, interval - stepFlash);
                if (wait > 0f)
                    yield return new WaitForSeconds(wait);

                float expand = stepExpand[Mathf.Min(i, stepExpand.Length - 1)];
                float intensity = stepIntensity[Mathf.Min(i, stepIntensity.Length - 1)];
                float stage = i + 1; // 1..N

                // 瞬间切到新裂纹状态（玻璃碎裂感）
                SetProgress(intensity, expand, 0f, stage);
                onStepCracked?.Invoke();

                if (i + 1 >= spawnMonsterAtStep)
                    TrySpawnMonster();

                if (stepFlash > 0f)
                    yield return new WaitForSeconds(stepFlash);
            }

            SetProgress(
                stepIntensity[Mathf.Min(steps - 1, stepIntensity.Length - 1)],
                stepExpand[Mathf.Min(steps - 1, stepExpand.Length - 1)],
                0f,
                steps);
            onExpandFinished?.Invoke();
            TrySpawnMonster();

            if (openHoleAfterExpand)
            {
                float h = 0f;
                float holeDur = Mathf.Max(0.01f, holeDuration);
                while (h < holeDur)
                {
                    h += Time.deltaTime;
                    float hole = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(h / holeDur));
                    SetProgress(1f, 1f, hole, steps);
                    yield return null;
                }

                SetProgress(1f, 1f, 1f, steps);
            }

            if (fadeShellAfterHole)
            {
                float f = 0f;
                float fadeDur = Mathf.Max(0.01f, fadeDuration);
                while (f < fadeDur)
                {
                    f += Time.deltaTime;
                    float k = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(f / fadeDur));
                    SetProgress(k, k, openHoleAfterExpand ? 1f : 0f, steps);
                    yield return null;
                }

                SetProgress(0f, 0f, 0f, 0f);
            }
            else if (holdAtEnd)
            {
                SetProgress(1f, 1f, openHoleAfterExpand ? 1f : 0f, steps);
            }

            onFinished?.Invoke();
            _routine = null;
        }

        void EnsureStepArrays(int steps)
        {
            if (stepExpand == null || stepExpand.Length < steps)
            {
                var arr = new float[steps];
                for (int i = 0; i < steps; i++)
                    arr[i] = (i + 1f) / steps;
                stepExpand = arr;
            }

            if (stepIntensity == null || stepIntensity.Length < steps)
            {
                var arr = new float[steps];
                for (int i = 0; i < steps; i++)
                    arr[i] = Mathf.Lerp(0.5f, 1f, (i + 1f) / steps);
                stepIntensity = arr;
            }
        }

        void TrySpawnMonster()
        {
            if (_monsterShown || monster == null)
                return;

            _monsterShown = true;
            monster.SetActive(true);
            if (monsterAppear != null)
                monsterAppear.Play();
            onMonsterRevealed?.Invoke();
        }

        void SetProgress(float intensity, float expand, float hole, float stage)
        {
            if (shatterRenderer == null)
                return;
            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();

            shatterRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(IntensityId, Mathf.Clamp01(intensity));
            _mpb.SetFloat(ExpandId, Mathf.Clamp01(expand));
            _mpb.SetFloat(HoleId, Mathf.Clamp01(hole));
            _mpb.SetFloat(StageId, stage);
            shatterRenderer.SetPropertyBlock(_mpb);
        }
    }
}
