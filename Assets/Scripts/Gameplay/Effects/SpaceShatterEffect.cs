using System.Collections;
using UnityEngine;

namespace Gameplay.Effects
{
    /// <summary>
    /// 空间碎裂：Voronoi 碎片错位 + 裂隙虚空。
    /// 挂到主相机上会自动生成全屏覆盖 Quad（采样 Opaque Texture）。
    /// </summary>
    [DisallowMultipleComponent]
    public class SpaceShatterEffect : MonoBehaviour
    {
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int EpicenterId = Shader.PropertyToID("_Epicenter");

        [SerializeField] Material shatterMaterial;
        [SerializeField] Camera targetCamera;
        [SerializeField] float duration = 1.1f;
        [SerializeField] float hold = 0.15f;
        [SerializeField] AnimationCurve intensityCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 1f),
            new Keyframe(0.7f, 1f),
            new Keyframe(1f, 0f));
        [SerializeField] bool playOnEnable;
        [SerializeField] Vector2 epicenter = new Vector2(0.5f, 0.5f);

        Transform _overlay;
        MeshRenderer _renderer;
        MaterialPropertyBlock _mpb;
        Coroutine _routine;
        Material _runtimeMat;

        public float Intensity { get; private set; }

        void Reset()
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
                if (targetCamera == null)
                    targetCamera = Camera.main;
            }

            EnsureOverlay();
            SetIntensity(0f);
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
            SetIntensity(0f);
        }

        void OnDestroy()
        {
            if (_overlay != null)
                Destroy(_overlay.gameObject);
            if (_runtimeMat != null)
                Destroy(_runtimeMat);
        }

        [ContextMenu("Play Shatter")]
        public void Play()
        {
            PlayAt(epicenter);
        }

        public void PlayAtScreenUV(Vector2 screenUV01)
        {
            PlayAt(screenUV01);
        }

        public void PlayAtWorld(Vector3 worldPos)
        {
            if (targetCamera == null)
            {
                Play();
                return;
            }

            Vector3 sp = targetCamera.WorldToViewportPoint(worldPos);
            PlayAt(new Vector2(sp.x, sp.y));
        }

        void PlayAt(Vector2 uv)
        {
            epicenter = uv;
            EnsureOverlay();
            if (!isActiveAndEnabled)
            {
                SetIntensity(1f);
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(PlayRoutine());
        }

        public void SnapOff() => SetIntensity(0f);

        IEnumerator PlayRoutine()
        {
            float t = 0f;
            float dur = Mathf.Max(0.01f, duration);
            float holdTime = Mathf.Max(0f, hold);

            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / dur);
                // 前段曲线，中段可额外 hold
                float sampleU = u;
                if (holdTime > 0f && u > 0.25f && u < 0.25f + holdTime / dur)
                    sampleU = 0.35f;
                SetIntensity(intensityCurve.Evaluate(sampleU));
                yield return null;
            }

            SetIntensity(0f);
            _routine = null;
        }

        void EnsureOverlay()
        {
            if (targetCamera == null || shatterMaterial == null)
                return;

            if (_overlay != null)
                return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "SpaceShatterOverlay";
            Destroy(go.GetComponent<Collider>());
            _overlay = go.transform;
            _overlay.SetParent(targetCamera.transform, false);

            // 贴在近裁剪面前方，盖满视野
            float z = targetCamera.nearClipPlane + 0.05f;
            _overlay.localPosition = new Vector3(0f, 0f, z);
            _overlay.localRotation = Quaternion.identity;

            float h = 2f * z * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float w = h * targetCamera.aspect;
            if (!targetCamera.orthographic)
                _overlay.localScale = new Vector3(w, h, 1f);
            else
            {
                float oh = targetCamera.orthographicSize * 2f;
                _overlay.localScale = new Vector3(oh * targetCamera.aspect, oh, 1f);
            }

            _runtimeMat = new Material(shatterMaterial);
            _renderer = go.GetComponent<MeshRenderer>();
            _renderer.sharedMaterial = _runtimeMat;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;

            var rf = go.AddComponent<SpaceShatterOverlayFix>();
            rf.Init(targetCamera, _overlay);
        }

        void SetIntensity(float value)
        {
            Intensity = Mathf.Clamp01(value);
            if (_runtimeMat == null && shatterMaterial != null)
                EnsureOverlay();
            if (_runtimeMat == null)
                return;

            EnsureBlock();
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(IntensityId, Intensity);
            _mpb.SetVector(EpicenterId, new Vector4(epicenter.x, epicenter.y, 0f, 0f));
            _renderer.SetPropertyBlock(_mpb);

            if (_overlay != null)
                _overlay.gameObject.SetActive(Intensity > 0.001f);
        }

        void EnsureBlock()
        {
            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();
        }
    }

    /// <summary>保持 overlay 贴合相机 FOV/Aspect 变化。</summary>
    [DisallowMultipleComponent]
    sealed class SpaceShatterOverlayFix : MonoBehaviour
    {
        Camera _cam;
        Transform _overlay;

        public void Init(Camera cam, Transform overlay)
        {
            _cam = cam;
            _overlay = overlay;
        }

        void LateUpdate()
        {
            if (_cam == null || _overlay == null)
                return;

            float z = _cam.nearClipPlane + 0.05f;
            _overlay.localPosition = new Vector3(0f, 0f, z);
            if (!_cam.orthographic)
            {
                float h = 2f * z * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                _overlay.localScale = new Vector3(h * _cam.aspect, h, 1f);
            }
            else
            {
                float oh = _cam.orthographicSize * 2f;
                _overlay.localScale = new Vector3(oh * _cam.aspect, oh, 1f);
            }
        }
    }
}
