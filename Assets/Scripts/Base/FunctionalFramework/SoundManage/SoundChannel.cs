namespace Functional.SoundManage
{
    /// <summary>
    /// 音效通道类型。
    /// </summary>
    public enum SoundChannel
    {
        /// <summary>背景音乐，同时只播放一首。</summary>
        BGM,

        /// <summary>音效，可同时播放多个。</summary>
        SFX,

        /// <summary>语音/旁白。</summary>
        Voice
    }
}
