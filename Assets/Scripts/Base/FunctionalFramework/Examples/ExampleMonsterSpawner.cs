using Functional.ECS;
using Functional.ECS.Components;
using UnityEngine;

namespace Functional.Examples
{
    /// <summary>
    /// ECS 怪物批量生成示例。生成大量实体并由 MovementSystem 驱动。
    /// 需场景中有 ECSWorldRunner。
    /// </summary>
    public class ExampleMonsterSpawner : MonoBehaviour
    {
        [SerializeField] private ECSWorldRunner _runner;
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private int _spawnCount = 100;
        [SerializeField] private Vector3 _areaSize = new Vector3(20, 0, 20);

        private void Start()
        {
            if (_runner == null) _runner = FindObjectOfType<ECSWorldRunner>();
            if (_runner == null)
            {
                Debug.LogWarning("[ExampleMonsterSpawner] 场景缺少 ECSWorldRunner");
                return;
            }

            var world = _runner.World;
            var views = _runner.ViewRegistry;
            var random = new System.Random(42);

            for (int i = 0; i < _spawnCount; i++)
            {
                var entity = world.CreateEntity();
                var pos = new Vector3(
                    (float)(random.NextDouble() - 0.5) * _areaSize.x,
                    0,
                    (float)(random.NextDouble() - 0.5) * _areaSize.z);

                GameObject go = null;
                if (_monsterPrefab != null)
                {
                    go = Instantiate(_monsterPrefab, pos, Quaternion.identity);
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.position = pos;
                    go.transform.localScale = Vector3.one * 0.5f;
                }

                int viewId = views.Register(go);
                world.AddComponent(entity, new PositionComponent { Value = pos });
                world.AddComponent(entity, new VelocityComponent
                {
                    Value = new Vector3((float)(random.NextDouble() - 0.5) * 2f, 0, (float)(random.NextDouble() - 0.5) * 2f)
                });
                world.AddComponent(entity, new HealthComponent { Current = 100, Max = 100 });
                world.AddComponent(entity, new ViewLinkComponent { ViewId = viewId });
            }

            Debug.Log($"[ExampleMonsterSpawner] 已生成 {_spawnCount} 个 ECS 实体");
        }
    }
}
