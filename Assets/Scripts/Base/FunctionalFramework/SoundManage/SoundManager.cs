using System.Collections.Generic;
using UnityEngine;
using Base.Common;
using Base.ResourceFramework;

namespace Functional.SoundManage
{
    /// <summary>
    /// 音效管理总入口。支持 BGM / SFX / Voice 分通道、音量控制、资源加载与对象池。
    /// 
    /// 典型用法：
    ///   SoundManager.Instance.PlayBGM("Audio/BGM_Main");
    ///   SoundManager.Instance.PlaySFX("Audio/SFX_Click");
    ///   SoundManager.Instance.SetVolume(SoundChannel.BGM, 0.5f);
    /// </summary>
    public class SoundManager : MonoSingleton<SoundManager>
    {
        private AudioSource _bgmSource;
        private AudioSource _voiceSource;
        private AudioSourcePool _sfxPool;
        private readonly Dictionary<SoundChannel, float> _volumes = new Dictionary<SoundChannel, float>
        {
            { SoundChannel.BGM, 1f },
            { SoundChannel.SFX, 1f },
            { SoundChannel.Voice, 1f }
        };

        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
        private bool _muted;

        /// <summary>全局静音开关。</summary>
        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                ApplyMute();
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            var bgmGo = new GameObject("BGM");
            bgmGo.transform.SetParent(transform);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;

            var voiceGo = new GameObject("Voice");
            voiceGo.transform.SetParent(transform);
            _voiceSource = voiceGo.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;

            var sfxRoot = new GameObject("SFXPool").transform;
            sfxRoot.SetParent(transform);
            _sfxPool = new AudioSourcePool(sfxRoot);
        }

        private void Update()
        {
            _sfxPool?.RecycleFinished();
        }

        /// <summary>
        /// 播放背景音乐（会替换当前 BGM）。
        /// </summary>
        public void PlayBGM(string clipPath, bool loop = true, float fadeIn = 0f)
        {
            var clip = LoadClip(clipPath);
            if (clip == null) return;
            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = GetVolume(SoundChannel.BGM);
            _bgmSource.Play();
        }

        /// <summary>
        /// 停止背景音乐。
        /// </summary>
        public void StopBGM() => _bgmSource?.Stop();

        /// <summary>
        /// 播放音效（支持同时多个）。
        /// </summary>
        public void PlaySFX(string clipPath, float volumeScale = 1f)
        {
            var clip = LoadClip(clipPath);
            if (clip == null) return;
            var src = _sfxPool.Get();
            src.clip = clip;
            src.loop = false;
            src.volume = GetVolume(SoundChannel.SFX) * volumeScale;
            src.Play();
        }

        /// <summary>
        /// 播放语音。
        /// </summary>
        public void PlayVoice(string clipPath)
        {
            var clip = LoadClip(clipPath);
            if (clip == null) return;
            _voiceSource.clip = clip;
            _voiceSource.volume = GetVolume(SoundChannel.Voice);
            _voiceSource.Play();
        }

        /// <summary>
        /// 设置通道音量 0~1。
        /// </summary>
        public void SetVolume(SoundChannel channel, float volume)
        {
            _volumes[channel] = Mathf.Clamp01(volume);
            ApplyVolume(channel);
        }

        /// <summary>
        /// 获取通道音量。
        /// </summary>
        public float GetVolume(SoundChannel channel)
        {
            return _muted ? 0f : _volumes.TryGetValue(channel, out var v) ? v : 1f;
        }

        /// <summary>
        /// 停止所有音效。
        /// </summary>
        public void StopAllSFX() => _sfxPool?.StopAll();

        /// <summary>
        /// 卸载音效缓存。
        /// </summary>
        public void ClearCache()
        {
            _clipCache.Clear();
        }

        private AudioClip LoadClip(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_clipCache.TryGetValue(path, out var cached)) return cached;

            AudioClip clip = null;
            if (ResourceManager.HasInstance)
            {
                clip = ResourceManager.Instance.Load<AudioClip>(path);
            }
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>(path);
            }
            if (clip != null) _clipCache[path] = clip;
            else Debug.LogWarning($"[SoundManager] 找不到音频：{path}");
            return clip;
        }

        private void ApplyVolume(SoundChannel channel)
        {
            var vol = GetVolume(channel);
            switch (channel)
            {
                case SoundChannel.BGM:
                    if (_bgmSource != null) _bgmSource.volume = vol;
                    break;
                case SoundChannel.Voice:
                    if (_voiceSource != null) _voiceSource.volume = vol;
                    break;
            }
        }

        private void ApplyMute()
        {
            ApplyVolume(SoundChannel.BGM);
            ApplyVolume(SoundChannel.Voice);
        }
    }
}
