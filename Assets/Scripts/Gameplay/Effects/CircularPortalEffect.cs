using UnityEngine;

namespace Gameplay.Effects
{
    /// <summary>
    /// 2D circular portal on SpriteRenderer.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CircularPortalEffect : MonoBehaviour
    {
        static readonly int OpenId = Shader.PropertyToID("_Open");
        static readonly int SpriteUVRectId = Shader.PropertyToID("_SpriteUVRect");

        [SerializeField] SpriteRenderer portalRenderer;
        [SerializeField] float openDuration = 0.9f;
        [SerializeField] bool playOnEnable = false;
        [SerializeField] [Range(0f, 1f)] float open = 1f;
        [SerializeField] AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        MaterialPropertyBlock _mpb;
        Coroutine _routine;

        public float Open
        {
            get => open;
            set => SetOpen(value);
        }

        void Reset()
        {
            portalRenderer = GetComponent<SpriteRenderer>();
        }

        void Awake()
        {
            if (portalRenderer == null)
                portalRenderer = GetComponent<SpriteRenderer>();
            EnsureBlock();
            SetOpen(playOnEnable ? 0f : Mathf.Max(open, 1f));
        }

        void OnEnable()
        {
            if (playOnEnable)
                PlayOpen();
            else
                SetOpen(open <= 0f ? 1f : open);
        }

        void OnDisable()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        void OnValidate()
        {
            if (portalRenderer == null)
                portalRenderer = GetComponent<SpriteRenderer>();
            if (!Application.isPlaying)
                SetOpen(open);
        }

        [ContextMenu("Play Open")]
        public void PlayOpen()
        {
            if (!isActiveAndEnabled)
            {
                SetOpen(1f);
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(OpenRoutine());
        }

        public void SnapOpen() => SetOpen(1f);

        public void SnapClosed() => SetOpen(0f);

        System.Collections.IEnumerator OpenRoutine()
        {
            float t = 0f;
            float dur = Mathf.Max(0.01f, openDuration);
            SetOpen(0f);
            while (t < dur)
            {
                t += Time.deltaTime;
                SetOpen(openCurve.Evaluate(Mathf.Clamp01(t / dur)));
                yield return null;
            }

            SetOpen(1f);
            _routine = null;
        }

        void SetOpen(float value)
        {
            open = Mathf.Clamp01(value);
            if (portalRenderer == null)
                return;

            EnsureBlock();
            portalRenderer.GetPropertyBlock(_mpb);

            Sprite sprite = portalRenderer.sprite;
            if (sprite != null)
                _mpb.SetVector(SpriteUVRectId, UnityEngine.Sprites.DataUtility.GetOuterUV(sprite));
            else
                _mpb.SetVector(SpriteUVRectId, new Vector4(0f, 0f, 1f, 1f));

            _mpb.SetFloat(OpenId, open);
            portalRenderer.SetPropertyBlock(_mpb);
        }

        void EnsureBlock()
        {
            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();
        }
    }
}
