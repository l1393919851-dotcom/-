using System;

namespace LiteEcs
{
    /// <summary>
    /// 单个计时器，由 LTimerManager 池化复用，外部只通过 TimerHandle 操作。
    /// 规则：先等 Delay 执行第一次；RepeatCount == 1 到此结束；
    /// 否则每隔 Interval 再执行，RepeatCount &gt; 0 时执行满次数结束，&lt;= 0 时一直重复。
    /// 一帧时间很长时会在同一帧里补执行多次。
    /// </summary>
    public sealed class LTimer
    {
        private float _delay;
        private float _interval;
        private int _repeatCount;
        private Action _callback;
        private float _elapsed;
        private int _executed;

        internal int Version { get; private set; }
        internal bool Active { get; private set; }

        internal void Init(float delay, float interval, int repeatCount, Action callback)
        {
            _delay = delay;
            _interval = interval;
            _repeatCount = repeatCount;
            _callback = callback;
            _elapsed = 0f;
            _executed = 0;
            Active = true;
        }

        internal void Deactivate()
        {
            if (!Active)
            {
                return;
            }
            Active = false;
            Version++;
        }

        internal void Reset()
        {
            _callback = null;
            _elapsed = 0f;
            _executed = 0;
        }

        /// <returns>本计时器已结束</returns>
        internal bool Tick(float dt)
        {
            _elapsed += dt;
            if (_executed == 0)
            {
                if (_elapsed < _delay)
                {
                    return false;
                }
                _elapsed -= _delay;
                _executed++;
                Execute();
                if (!Active || _repeatCount == 1 || _interval <= 0f)
                {
                    return true;
                }
            }
            while (_elapsed >= _interval)
            {
                _elapsed -= _interval;
                _executed++;
                Execute();
                if (!Active)
                {
                    return true;
                }
                if (_repeatCount > 0 && _executed >= _repeatCount)
                {
                    return true;
                }
            }
            return false;
        }

        private void Execute()
        {
            try
            {
                _callback?.Invoke();
            }
            catch (Exception ex)
            {
                EcsLog.LogException(ex);
            }
        }
    }
}
