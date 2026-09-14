using UnityEngine;

namespace Gameplay.Ocean
{
    /// <summary>
    /// OceanDemo 场景启动：生成无限海面、实例化木筏并接上相机跟随。
    /// </summary>
    public class OceanDemoBootstrap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject boatPrefab;
        [SerializeField] Material oceanMaterial;
        [SerializeField] Camera mainCamera;

        [Header("Ocean")]
        [SerializeField] float oceanSizeMeters = 600f;
        [SerializeField] float seaLevelY;
        [SerializeField] int meshSegments = 64;

        [Header("Boat")]
        [SerializeField] Vector3 boatSpawnPosition = new Vector3(0f, 0.15f, 0f);
        [SerializeField] Vector3 boatSpawnEuler = Vector3.zero;

        void Awake()
        {
            ResolveReferences();

            if (boatPrefab == null)
            {
                Debug.LogError("[OceanDemo] boatPrefab 未指定（应使用 3d_prefab_plank_rc）。");
                enabled = false;
                return;
            }

            if (oceanMaterial == null)
            {
                Debug.LogError("[OceanDemo] oceanMaterial 未指定（应使用 mat_stylized_ocean）。");
                enabled = false;
                return;
            }

            ConfigureFog();

            Transform boat = SpawnBoat();
            InfiniteOceanSurface ocean = CreateOcean();
            ocean.SetFollowTarget(boat);
            ocean.SetSeaLevel(seaLevelY);

            SetupCamera(boat);
        }

        void ResolveReferences()
        {
#if UNITY_EDITOR
            // Prefab 用路径加载，避免场景里坏的 prefab 引用在 Inspector 里刷 Quaternion 报错
            if (boatPrefab == null)
            {
                boatPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Art_Replace/3D_Prefab_replace/3d_prefab_plank_rc.prefab");
            }

            if (oceanMaterial == null)
            {
                oceanMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Material/mat_stylized_ocean.mat");
            }
#endif
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        void ConfigureFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.45f, 0.72f, 0.92f, 1f);
            RenderSettings.fogStartDistance = 80f;
            RenderSettings.fogEndDistance = 260f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.75f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.55f, 0.75f);
            RenderSettings.ambientGroundColor = new Color(0.1f, 0.2f, 0.25f);
        }

        Transform SpawnBoat()
        {
            GameObject boat = Instantiate(boatPrefab, boatSpawnPosition, Quaternion.Euler(boatSpawnEuler));
            boat.name = "Boat_Plank";

            var controller = boat.GetComponent<SimpleBoatController>();
            if (controller == null)
            {
                controller = boat.AddComponent<SimpleBoatController>();
            }

            controller.Configure(boatSpawnPosition.y);
            return boat.transform;
        }

        InfiniteOceanSurface CreateOcean()
        {
            var go = new GameObject("InfiniteOcean");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = oceanMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            mf.sharedMesh = BuildOceanMesh(oceanSizeMeters, Mathf.Max(2, meshSegments));
            go.transform.position = new Vector3(0f, seaLevelY, 0f);

            var ocean = go.AddComponent<InfiniteOceanSurface>();
            return ocean;
        }

        static Mesh BuildOceanMesh(float size, int segments)
        {
            int vertsPerSide = segments + 1;
            var vertices = new Vector3[vertsPerSide * vertsPerSide];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segments * segments * 6];

            float half = size * 0.5f;
            float step = size / segments;

            for (int z = 0; z < vertsPerSide; z++)
            {
                for (int x = 0; x < vertsPerSide; x++)
                {
                    int i = z * vertsPerSide + x;
                    float px = -half + x * step;
                    float pz = -half + z * step;
                    vertices[i] = new Vector3(px, 0f, pz);
                    uvs[i] = new Vector2(x / (float)segments, z / (float)segments);
                }
            }

            int t = 0;
            for (int z = 0; z < segments; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int i = z * vertsPerSide + x;
                    triangles[t++] = i;
                    triangles[t++] = i + vertsPerSide;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + vertsPerSide;
                    triangles[t++] = i + vertsPerSide + 1;
                }
            }

            var mesh = new Mesh
            {
                name = "OceanGrid",
                indexFormat = vertices.Length > 65000
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        void SetupCamera(Transform boat)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                var camGo = new GameObject("Main Camera");
                mainCamera = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.tag = "MainCamera";
            }

            var follow = mainCamera.GetComponent<OceanCameraFollow>();
            if (follow == null)
            {
                follow = mainCamera.gameObject.AddComponent<OceanCameraFollow>();
            }

            follow.SetTarget(boat);
            mainCamera.transform.position = boat.TransformPoint(new Vector3(0f, 12f, -18f));
            mainCamera.transform.LookAt(boat.position + Vector3.up);
            mainCamera.farClipPlane = 800f;
            mainCamera.backgroundColor = new Color(0.45f, 0.72f, 0.92f);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }
    }
}
