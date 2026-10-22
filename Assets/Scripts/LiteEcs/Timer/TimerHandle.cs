namespace LiteEcs
{
    /// <summary>
    /// 计时器句柄。带版本号，计时器结束或被停后再回池复用，旧句柄的 Stop 不会误停新计时器。
    /// default(TimerHandle) 表示空句柄，Stop 无效果。
    /// </summary>
    public struct TimerHandle
    {
        private readonly LTimer _timer;
        private readonly int _version;

        public bool IsActive => _timer != null && _timer.Active && _timer.Version == _version;

        internal TimerHandle(LTimer timer)
        {
            _timer = timer;
            _version = timer.Version;
        }

        public void Stop()
        {
            if (IsActive)
            {
                _timer.Deactivate();
            }
        }
    }
}
