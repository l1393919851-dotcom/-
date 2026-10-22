namespace LiteEcs
{
    /// <summary>
    /// 可进对象池的对象。回池前调用，负责把自己清回刚 new 出来的状态。
    /// </summary>
    public interface IPoolable
    {
        void OnRecycle();
    }
}
