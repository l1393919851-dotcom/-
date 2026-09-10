using System.Collections;
using UnityEngine;

namespace Gameplay.Effects
{
    /// <summary>
    /// Drives Sprite-Cyber-Appear: ghost transparent + momentary defocus → solid reveal.
    /// Uses MaterialPropertyBlock so shared materials are not instanced.
    /// </summary>
    [DisallowMultipleComponent]
    public class CyberAppearEffect : MonoBehaviour
    {
        static readonly int AppearId = Shader.PropertyToID("_Appear");

        [SerializeField] Renderer targetRenderer;
        [SerializeField] float duration = 1.4f;
        // 前半段走得慢，让失焦/错位多停一会儿，后段再收成实体
        [SerializeField] AnimationCurve appearCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 1.2f),
            new Keyframe(0.45f, 0.35f, 0.4f, 0.4f),
            new Keyframe(0.75f, 0.7f, 0.8f, 0.8f),
            new Keyframe(1f, 1f, 1.2f, 0f));
        [SerializeField] bool playOnEnable = true;
        [SerializeField] bool startHidden = true;

        MaterialPropertyBlock _mpb;
        Coroutine _routine;

        public float Appear
        {
            get
            {
                EnsureBlock();
                targetRenderer.GetPropertyBlock(_mpb);
                return _mpb.GetFloat(AppearId);
            }
            set => SetAppear(value);
        }

        void Reset()
        {
            targetRenderer = GetComponent<Renderer>();
        }

        void Awake()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
            EnsureBlock();
            if (startHidden)
                SetAppear(0f);
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

        [ContextMenu("Play Appear")]
        public void Play()
        {
            if (!isActiveAndEnabled)
            {
                SetAppear(1f);
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(PlayRoutine());
        }

        public void Play(float customDuration)
        {
            duration = Mathf.Max(0.01f, customDuration);
            Play();
        }

        public void SnapSolid() => SetAppear(1f);

        public void SnapHidden() => SetAppear(0f);

        IEnumerator PlayRoutine()
        {
            float t = 0f;
            float dur = Mathf.Max(0.01f, duration);
            SetAppear(0f);

            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / dur);
                SetAppear(appearCurve.Evaluate(u));
                yield return null;
            }

            SetAppear(1f);
            _routine = null;
        }

        void SetAppear(float value)
        {
            if (targetRenderer == null)
                return;

            EnsureBlock();
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(AppearId, Mathf.Clamp01(value));
            targetRenderer.SetPropertyBlock(_mpb);
        }

        void EnsureBlock()
        {
            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();
        }
    }
}
