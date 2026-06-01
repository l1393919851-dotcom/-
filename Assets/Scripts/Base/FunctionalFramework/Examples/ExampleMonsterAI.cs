using Functional.BehaviorTree;
using UnityEngine;

namespace Functional.Examples
{
    /// <summary>
    /// 怪物 AI 行为树示例：巡逻 → 发现玩家 → 追击。
    /// </summary>
    public class ExampleMonsterAI : BehaviorTreeRunner
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _detectRange = 5f;

        protected override void Awake()
        {
            base.Awake();
            if (_player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _player = p.transform;
            }
        }

        protected override BTNode BuildTree()
        {
            var root = new SelectorNode { Name = "Root" };
            var chase = new SequenceNode { Name = "Chase" };
            chase.AddChild(new ConditionNode(ctx => HasPlayer(ctx)));
            chase.AddChild(new ActionNode(ctx =>
            {
                MoveTowardPlayer(ctx);
                return BTStatus.Running;
            }));

            var patrol = new ActionNode(ctx =>
            {
                ctx.Agent.transform.Rotate(Vector3.up, 30f * ctx.DeltaTime);
                return BTStatus.Running;
            });

            root.AddChild(chase);
            root.AddChild(patrol);
            return root;
        }

        private bool HasPlayer(BTContext ctx)
        {
            if (_player == null) return false;
            return Vector3.Distance(ctx.Agent.transform.position, _player.position) < _detectRange;
        }

        private void MoveTowardPlayer(BTContext ctx)
        {
            var dir = (_player.position - ctx.Agent.transform.position).normalized;
            ctx.Agent.transform.position += dir * 3f * ctx.DeltaTime;
        }
    }
}
