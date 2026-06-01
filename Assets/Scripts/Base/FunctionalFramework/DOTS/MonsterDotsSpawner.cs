using System.Collections;
using Functional.DOTS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Functional.DOTS
{
    /// <summary>
    /// DOTS + Entities Graphics 怪物生成器。
    /// 不创建 GameObject，使用 RenderMeshUtility 批量实例化实体并由 GPU 绘制。
    /// 
    /// 前置条件见：DOTS/README_DOTS设置指南.md
    /// </summary>
    public class MonsterDotsSpawner : MonoBehaviour
    {
        [Header("渲染资源（材质须为 URP Lit）")]
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        [Header("生成")]
        [SerializeField] private int _spawnCount = 10000;
        [SerializeField] private Vector3 _areaSize = new Vector3(200f, 0f, 200f);
        [SerializeField] private float _scale = 0.5f;
        [SerializeField] private int _spawnPerFrame = 2000;

        [Header("运动")]
        [SerializeField] private float _speed = 2f;

        private Entity _prototype;

        private void Start()
        {
            if (_mesh == null)
            {
                var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Destroy(temp);
            }
            if (_material == null)
            {
                Debug.LogError("[MonsterDotsSpawner] 请指定 URP Lit 材质");
                return;
            }

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                Debug.LogError("[MonsterDotsSpawner] Entities 默认 World 未创建，请确认已安装 com.unity.entities");
                return;
            }

            _prototype = CreateRenderPrototype(world.EntityManager);
            StartCoroutine(SpawnBatched(world.EntityManager));
        }

        private Entity CreateRenderPrototype(EntityManager em)
        {
            var entity = em.CreateEntity();

            var desc = new RenderMeshDescription(
                ShadowCastingMode.On,
                receiveShadows: true);

            var renderMeshArray = new RenderMeshArray(new[] { _material }, new[] { _mesh });

            RenderMeshUtility.AddComponents(
                entity,
                em,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            em.AddComponentData(entity, LocalTransform.FromPositionRotationScale(
                float3.zero, quaternion.identity, _scale));
            em.AddComponentData(entity, new MonsterVelocity { Value = float3.zero });
            em.AddComponent<Prefab>(entity);

            return entity;
        }

        private IEnumerator SpawnBatched(EntityManager em)
        {
            var random = new Unity.Mathematics.Random((uint)System.Environment.TickCount);
            int spawned = 0;

            while (spawned < _spawnCount)
            {
                int batch = math.min(_spawnPerFrame, _spawnCount - spawned);
                for (int i = 0; i < batch; i++)
                {
                    var e = em.Instantiate(_prototype);
                    var pos = new float3(
                        random.NextFloat(-_areaSize.x * 0.5f, _areaSize.x * 0.5f),
                        0f,
                        random.NextFloat(-_areaSize.z * 0.5f, _areaSize.z * 0.5f));

                    em.SetComponentData(e, LocalTransform.FromPositionRotationScale(
                        pos, quaternion.identity, _scale));

                    var dir = math.normalize(new float3(
                        random.NextFloat(-1f, 1f), 0f, random.NextFloat(-1f, 1f)));
                    if (math.lengthsq(dir) < 0.01f) dir = new float3(1, 0, 0);

                    em.SetComponentData(e, new MonsterVelocity { Value = dir * _speed });
                }

                spawned += batch;
                yield return null;
            }

            Debug.Log($"[MonsterDotsSpawner] DOTS 实体生成完成：{spawned}（无 GameObject，由 Entities Graphics 渲染）");
        }

        private void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated || _prototype == Entity.Null) return;
            if (world.EntityManager.Exists(_prototype))
            {
                world.EntityManager.DestroyEntity(_prototype);
            }
        }
    }
}
