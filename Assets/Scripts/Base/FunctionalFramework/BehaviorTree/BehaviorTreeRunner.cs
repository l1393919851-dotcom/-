using UnityEngine;

namespace Functional.BehaviorTree
{
    /// <summary>
    /// 行为树运行器。挂在 AI GameObject 上，每帧 Tick 行为树。
    /// 子类实现 BuildTree 返回根节点。
    /// </summary>
    public abstract class BehaviorTreeRunner : MonoBehaviour
    {
        protected BehaviorTreeAsset Tree { get; private set; }
        protected BTContext Context { get; private set; }

        [SerializeField] private float _tickInterval = 0f;
        private float _timer;

        protected virtual void Awake()
        {
            Context = new BTContext { Agent = gameObject };
            Tree = new BehaviorTreeAsset
            {
                Name = GetType().Name,
                Root = BuildTree()
            };
        }

        protected virtual void Update()
        {
            Context.DeltaTime = Time.deltaTime;

            if (_tickInterval <= 0f)
            {
                Tree.Tick(Context);
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= _tickInterval)
            {
                _timer = 0f;
                Tree.Tick(Context);
            }
        }

        /// <summary>
        /// 构建行为树根节点。
        /// </summary>
        protected abstract BTNode BuildTree();

        /// <summary>
        /// 重启行为树。
        /// </summary>
        public void Restart()
        {
            Tree?.Reset();
        }
    }
}
