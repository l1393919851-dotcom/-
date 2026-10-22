namespace LiteEcs
{
    /// <summary>
    /// 实体上的黑板。Set 总是覆盖写；实体回池时清空。
    /// </summary>
    public class BlackboardComponent : LComponent
    {
        private readonly Blackboard _blackboard = new Blackboard();

        public object Get(string key)
        {
            return _blackboard.Read(key);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            return _blackboard.Read(key, defaultValue);
        }

        public bool TryGet<T>(string key, out T value)
        {
            return _blackboard.TryRead(key, out value);
        }

        public void Set(string key, object value)
        {
            _blackboard.Write(key, value, true);
        }

        /// <summary>
        /// 传 key 删一项，不传清空。
        /// </summary>
        public void Clear(string key = null)
        {
            _blackboard.ClearByKey(key);
        }

        public override void OnComponentDestroy()
        {
            _blackboard.Clear();
        }

        public override void OnRecycle()
        {
            _blackboard.Clear();
            base.OnRecycle();
        }
    }
}
