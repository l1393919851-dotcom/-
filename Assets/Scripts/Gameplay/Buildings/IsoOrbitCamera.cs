using UnityEngine;

namespace Gameplay.Buildings
{
    /// <summary>
    /// Octopath Traveler-style isometric orbit camera.
    /// Q/E snap yaw, RMB free orbit, scroll zoom, WASD / MMB pan.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class IsoOrbitCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] Transform target;
        [SerializeField] Vector3 targetOffset = new Vector3(0f, 6f, 0f);

        [Header("Orbit")]
        [SerializeField] float yaw = 45f;
        [SerializeField] float pitch = 35f;
        [SerializeField] float distance = 28f;
        [SerializeField] float snapDegrees = 45f;
        [SerializeField] float yawSmooth = 10f;
        [SerializeField] float freeOrbitSpeed = 0.25f;

        [Header("Zoom (orthographic size)")]
        [SerializeField] float orthoSize = 12f;
        [SerializeField] float minOrthoSize = 4f;
        [SerializeField] float maxOrthoSize = 28f;
        [SerializeField] float zoomSpeed = 3f;

        [Header("Pan")]
        [SerializeField] float panSpeed = 12f;
        [SerializeField] float mousePanSpeed = 0.04f;

        [Header("Pitch Limits")]
        [SerializeField] float minPitch = 15f;
        [SerializeField] float maxPitch = 70f;

        Camera _cam;
        float _yawVel;
        float _displayYaw;
        Vector3 _focus;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                if (target != null)
                    _focus = target.position + targetOffset;
            }
        }

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _displayYaw = yaw;
            SyncFocusFromTarget();
        }

        void Start()
        {
            // Target may finish building in Start; re-sync once.
            SyncFocusFromTarget();
            ApplyTransform();
        }

        void SyncFocusFromTarget()
        {
            if (target != null)
                _focus = target.position + targetOffset;
            else if (_focus == Vector3.zero)
                _focus = targetOffset;
        }

        void LateUpdate()
        {
            HandleInput();
            ApplyTransform();
        }

        void HandleInput()
        {
            // Snap rotate like Octopath
            if (Input.GetKeyDown(KeyCode.Q))
                yaw -= snapDegrees;
            if (Input.GetKeyDown(KeyCode.E))
                yaw += snapDegrees;

            // Free orbit with right mouse
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * freeOrbitSpeed * 100f;
                pitch -= Input.GetAxis("Mouse Y") * freeOrbitSpeed * 80f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
                orthoSize = Mathf.Clamp(orthoSize - scroll * zoomSpeed * 4f, minOrthoSize, maxOrthoSize);

            // Keyboard pan (camera-relative XZ)
            Vector3 right = Quaternion.Euler(0f, _displayYaw, 0f) * Vector3.right;
            Vector3 forward = Quaternion.Euler(0f, _displayYaw, 0f) * Vector3.forward;
            Vector3 pan = Vector3.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) pan -= right;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) pan += right;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) pan += forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) pan -= forward;
            if (pan.sqrMagnitude > 0f)
                _focus += pan.normalized * panSpeed * Time.unscaledDeltaTime * (orthoSize / 12f);

            // Middle-mouse pan
            if (Input.GetMouseButton(2))
            {
                _focus -= right * Input.GetAxis("Mouse X") * mousePanSpeed * orthoSize;
                _focus -= forward * Input.GetAxis("Mouse Y") * mousePanSpeed * orthoSize;
            }

            // Follow moving target lightly if assigned and not panned away intentionally —
            // keep focus independent once user pans; only auto-sync when pressing F
            if (Input.GetKeyDown(KeyCode.F) && target != null)
                _focus = target.position + targetOffset;

            _displayYaw = Mathf.SmoothDampAngle(_displayYaw, yaw, ref _yawVel, 1f / Mathf.Max(0.01f, yawSmooth));
        }

        void ApplyTransform()
        {
            _cam.orthographic = true;
            _cam.orthographicSize = orthoSize;

            Quaternion rot = Quaternion.Euler(pitch, _displayYaw, 0f);
            Vector3 pos = _focus - rot * Vector3.forward * distance;
            transform.SetPositionAndRotation(pos, rot);
        }

        void OnGUI()
        {
            const float pad = 12f;
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                normal = { textColor = Color.white }
            };
            string help =
                "八方旅人式俯视角\n" +
                "Q / E     旋转 45°\n" +
                "右键拖拽   自由环绕\n" +
                "滚轮       缩放\n" +
                "WASD      平移\n" +
                "中键拖拽   平移\n" +
                "F         重置焦点到大楼";
            GUI.Box(new Rect(pad, pad, 200f, 140f), help, style);
        }
    }
}
