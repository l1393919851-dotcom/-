using UnityEngine;

namespace SBPK.Gameplay.Buildings
{
    /// <summary>
    /// 霓虹大楼卷帘门升起动画控制 (序列帧版)。
    /// 玩家走近按 E 键 -> 触发 Open 动画 -> 序列帧播放 -> 动画结束 gameObject 禁用。
    ///
    /// 依赖:
    ///   - Animator + AnimatorController (rollingdoor_controller)
    ///   - SpriteRenderer (卷帘门序列帧 Sprite)
    ///   - 可选: AudioSource (用于播放 SFX)
    ///
    /// 触发条件:
    ///   - 玩家 Tag == "Player" 进入 trigger 范围
    ///   - 按下 interactKey (默认 E) -> SetTrigger("Open")
    ///   - 动画播完 (m_StopTime = 0.65s) 后 gameObject.SetActive(false)
    ///   - 动画第 0 帧会触发 OnRollingDoorStart (SFX 占位)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class RollingDoorController : MonoBehaviour
    {
        [Header("Player Detection")]
        [Tooltip("玩家 Tag, 默认 Player")]
        public string playerTag = "Player";

        [Header("Input")]
        [Tooltip("触发卷帘门升起的按键")]
        public KeyCode interactKey = KeyCode.E;

        [Header("Animation")]
        [Tooltip("Animator Trigger 参数名")]
        public string openTriggerName = "Open";

        [Tooltip("动画时长(秒), 与 .anim 的 m_StopTime 一致 (0.65s, ease-in 8帧)")]
        public float animDuration = 0.65f;

        [Header("SFX (占位)")]
        [Tooltip("卷帘门开始升起时播放的音效. 留空则不播放")]
        public AudioClip doorOpenSfx;

        [Tooltip("SFX 音量 (0-1)")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;

        [Tooltip("是否循环播放 SFX (例如持续哐当声)")]
        public bool sfxLoop = false;

        [Header("Events")]
        [Tooltip("动画播放完毕回调 (人物进楼时切换场景用)")]
        public UnityEngine.Events.UnityEvent onDoorOpened;

        [Tooltip("SFX 触发回调 (可挂自定义逻辑, 比如震动/粒子)")]
        public UnityEngine.Events.UnityEvent onDoorStart;

        private Animator _animator;
        private AudioSource _audioSource;
        private bool _playerInRange = false;
        private bool _isOpening = false;
        private float _openStartTime = -1f;

        // 静态引用, 让外部(如Player脚本)可以检查门是否已开
        public static bool IsAnyRollingDoorOpen { get; private set; } = false;
        public bool IsOpened { get; private set; } = false;

        void Awake()
        {
            _animator = GetComponent<Animator>();

            // 自动添加或获取 AudioSource
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.loop = sfxLoop;
            }
        }

        void Update()
        {
            if (_isOpening) return;

            if (_playerInRange && !IsOpened)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    OpenDoor();
                }
            }

            // 兜底: 防止 Animator 事件没接好导致门不消失
            if (IsOpened && _openStartTime > 0)
            {
                if (Time.time - _openStartTime > animDuration + 0.05f)
                {
                    CompleteOpen();
                }
            }
        }

        /// <summary>
        /// 玩家进入触发器
        /// </summary>
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = true;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = false;
            }
        }

        /// <summary>
        /// 外部代码(如关卡管理器)也可以直接调用触发
        /// </summary>
        public void OpenDoor()
        {
            if (_isOpening || IsOpened) return;
            _isOpening = true;
            IsAnyRollingDoorOpen = true;
            _animator.SetTrigger(openTriggerName);
            _openStartTime = Time.time;
            // 也直接触发 SFX (万一 AnimationEvent 没配置好)
            OnRollingDoorStart();
        }

        /// <summary>
        /// 动画第 0 帧自动调用 (在 .anim 的 Events 里配置).
        /// 播放卷帘门 SFX 占位.
        /// </summary>
        public void OnRollingDoorStart()
        {
            if (doorOpenSfx != null && _audioSource != null)
            {
                _audioSource.clip = doorOpenSfx;
                _audioSource.loop = sfxLoop;
                _audioSource.volume = sfxVolume;
                _audioSource.Play();
            }
            onDoorStart?.Invoke();
        }

        /// <summary>
        /// 动画结束调用 (建议在 Animator 的 Open 状态尾挂一个 AnimationEvent 调用此方法)
        /// </summary>
        public void CompleteOpen()
        {
            IsOpened = true;
            _isOpening = false;

            // 停止 SFX
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            gameObject.SetActive(false);
            onDoorOpened?.Invoke();
        }
    }
}
