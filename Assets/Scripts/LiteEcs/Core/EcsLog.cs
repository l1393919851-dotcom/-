using System;

namespace LiteEcs
{
    /// <summary>
    /// 框架日志出口。默认写到 Console；接入引擎时把三个处理器换掉，例如 Unity：
    /// EcsLog.s_OnError = UnityEngine.Debug.LogError;
    /// </summary>
    public static class EcsLog
    {
        public static Action<string> s_OnWarn = msg => Console.WriteLine("[LiteEcs][Warn] " + msg);
        public static Action<string> s_OnError = msg => Console.Error.WriteLine("[LiteEcs][Error] " + msg);
        public static Action<Exception> s_OnException = ex => Console.Error.WriteLine("[LiteEcs][Exception] " + ex);

        public static void Warn(string msg)
        {
            s_OnWarn?.Invoke(msg);
        }

        public static void Error(string msg)
        {
            s_OnError?.Invoke(msg);
        }

        public static void LogException(Exception ex)
        {
            s_OnException?.Invoke(ex);
        }
    }
}
