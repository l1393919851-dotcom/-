using System;
using System.Collections.Generic;

namespace LiteEcs
{
    /// <summary>
    /// 世界内的轻量计时器，随 LWorld.Tick 推进，World 销毁时全部作废。
    /// 本帧 Tick 期间新加的计时器从下一帧开始计时；停掉的计时器在 Tick 末尾才回池，回调里随便 Stop / Add 都安全。
    /// </summary>
    public sealed class LTimerManager
    {
        private readonly List<LTimer> _timers = new List<LTimer>();
        private readonly Stack<LTimer> _cachedTimers = new Stack<LTimer>();

        public TimerHandle AddDelayTimer(float delay, Action callback)
        {
            return AddTimer(delay, 0f, 1, callback);
        }

        /// <param name="delay">第一次执行前等待的秒数</param>
        /// <param name="interval">之后每次执行的间隔秒数</param>
        /// <param name="repeatCount">总执行次数；1 只执行一次，&lt;= 0 无限重复</param>
        public TimerHandle AddTimer(float delay, float interval, int repeatCount, Action callback)
        {
            var timer = _cachedTimers.Count > 0 ? _cachedTimers.Pop() : new LTimer();
            timer.Init(delay, interval, repeatCount, callback);
            _timers.Add(timer);
            return new TimerHandle(timer);
        }

        public void Tick(float dt)
        {
            int count = _timers.Count;
            for (int i = 0; i < count; i++)
            {
                var timer = _timers[i];
                if (!timer.Active)
                {
                    continue;
                }
                if (timer.Tick(dt))
                {
                    timer.Deactivate();
                }
            }
            Compact();
        }

        public void Dispose()
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                _timers[i].Deactivate();
                _timers[i].Reset();
            }
            _timers.Clear();
            _cachedTimers.Clear();
        }

        private void Compact()
        {
            int write = 0;
            for (int read = 0; read < _timers.Count; read++)
            {
                var timer = _timers[read];
                if (timer.Active)
                {
                    _timers[write++] = timer;
                    continue;
                }
                timer.Reset();
                _cachedTimers.Push(timer);
            }
            _timers.RemoveRange(write, _timers.Count - write);
        }
    }
}
