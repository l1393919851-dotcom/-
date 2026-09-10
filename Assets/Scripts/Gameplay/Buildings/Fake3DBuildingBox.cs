using UnityEngine;

namespace Gameplay.Buildings
{
    /// <summary>
    /// Builds a fake-3D building box from a front sprite + procedurally generated sides,
    /// so an isometric / Octopath-style camera can walk around and still see volume.
    /// </summary>
    public class Fake3DBuildingBox : MonoBehaviour
    {
        [Header("Art")]
        [SerializeField] Sprite frontSprite;
        [SerializeField] Sprite neonSprite;
        [SerializeField] Material frontMaterial;
        [SerializeField] Material neonMaterial;

        [Header("Box Size")]
        [SerializeField] float depth = 8f;
        [SerializeField] [Range(0.5f, 1.2f)] float widthScale = 1f;
        [SerializeField] float sideDarken = 0.55f;
        [SerializeField] Color roofColor = new Color(0.08f, 0.06f, 0.14f, 1f);
        [SerializeField] Color groundColor = new Color(0.12f, 0.1f, 0.18f, 1f);
        [SerializeField] float groundSize = 60f;

        [Header("Side Look")]
        [SerializeField] Color sideBase = new Color(0.12f, 0.08f, 0.2f, 1f);
        [SerializeField] Color sideWindow = new Color(0.35f, 0.55f, 0.85f, 0.85f);
        [SerializeField] Color sideNeonA = new Color(1f, 0.2f, 0.75f, 1f);
        [SerializeField] Color sideNeonB = new Color(0.2f, 0.95f, 1f, 1f);

        Transform _facesRoot;
        Sprite _sideSprite;
        Texture2D _sideTex;
        Texture2D _solidTex;

        public Transform FocusPoint => transform;
        public float BuildingHeight { get; private set; }

        void Start()
        {
            // Procedural side sprites don't serialize — always rebuild in play mode.
            Rebuild();
        }

        public void Build(Sprite front, Sprite neon = null, Material frontMat = null, Material neonMat = null)
        {
            frontSprite = front;
            neonSprite = neon;
            if (frontMat != null) frontMaterial = frontMat;
            if (neonMat != null) neonMaterial = neonMat;
            Rebuild();
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            ClearChildren();
            if (frontSprite == null)
            {
                Debug.LogWarning("[Fake3DBuildingBox] frontSprite is missing.");
                return;
            }

            GetVisualSize(frontSprite, out float width, out float height, out float centerY);
            width *= widthScale;
            BuildingHeight = height;

            if (depth < 0.5f)
                depth = Mathf.Max(4f, width * 0.45f);
            float halfD = depth * 0.5f;
            float halfW = width * 0.5f;

            _facesRoot = new GameObject("Faces").transform;
            _facesRoot.SetParent(transform, false);

            // Place so the visual bottom sits near y=0.
            float bottomY = centerY - height * 0.5f;
            _facesRoot.localPosition = new Vector3(0f, -bottomY, 0f);

            CreateFace("Front", frontSprite, frontMaterial, new Vector3(0f, 0f, halfD), Quaternion.identity, Color.white, 0);
            if (neonSprite != null)
                CreateFace("Neon", neonSprite, neonMaterial, new Vector3(0f, 0f, halfD - 0.02f), Quaternion.identity, Color.white, 2);

            CreateFace("Back", frontSprite, frontMaterial, new Vector3(0f, 0f, -halfD), Quaternion.Euler(0f, 180f, 0f), new Color(sideDarken, sideDarken, sideDarken, 1f), 0);

            int sideH = Mathf.Max(64, Mathf.RoundToInt(height * frontSprite.pixelsPerUnit));
            int sideW = Mathf.Max(32, Mathf.RoundToInt(depth * frontSprite.pixelsPerUnit));
            _sideSprite = CreateSideSprite(sideW, sideH);
            CreateFace("Left", _sideSprite, null, new Vector3(-halfW, centerY, 0f), Quaternion.Euler(0f, -90f, 0f), Color.white, 0);
            CreateFace("Right", _sideSprite, null, new Vector3(halfW, centerY, 0f), Quaternion.Euler(0f, 90f, 0f), Color.white, 0);

            CreateSolidQuad("Roof", width, depth, new Vector3(0f, centerY + height * 0.5f, 0f), Quaternion.Euler(90f, 0f, 0f), roofColor);
            CreateSolidQuad("Ground", groundSize, groundSize, new Vector3(0f, bottomY + 0.02f, 0f), Quaternion.Euler(90f, 0f, 0f), groundColor);
        }

        /// <summary>
        /// Uses tight mesh vertices when available so transparent padding doesn't inflate the box.
        /// </summary>
        static void GetVisualSize(Sprite sprite, out float width, out float height, out float centerY)
        {
            var verts = sprite.vertices;
            if (verts != null && verts.Length > 0)
            {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector2 v = verts[i];
                    if (v.x < minX) minX = v.x;
                    if (v.x > maxX) maxX = v.x;
                    if (v.y < minY) minY = v.y;
                    if (v.y > maxY) maxY = v.y;
                }
                width = Mathf.Max(0.1f, maxX - minX);
                height = Mathf.Max(0.1f, maxY - minY);
                centerY = (minY + maxY) * 0.5f;
                return;
            }

            width = sprite.bounds.size.x;
            height = sprite.bounds.size.y;
            centerY = sprite.bounds.center.y;
        }

        void CreateFace(string name, Sprite sprite, Material mat, Vector3 localPos, Quaternion localRot, Color tint, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_facesRoot, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
            if (mat != null) sr.sharedMaterial = mat;
        }

        void CreateSolidQuad(string name, float w, float d, Vector3 localPos, Quaternion localRot, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(_facesRoot, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = new Vector3(w, d, 1f);

            var col = go.GetComponent<Collider>();
            if (col != null)
                DestroyImmediate(col);

            var mr = go.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            var mat = new Material(shader);
            if (_solidTex == null)
            {
                _solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _solidTex.SetPixel(0, 0, Color.white);
                _solidTex.Apply();
            }
            mat.mainTexture = _solidTex;
            mat.color = color;
            mr.sharedMaterial = mat;
        }

        Sprite CreateSideSprite(int texW, int texH)
        {
            texW = Mathf.Clamp(texW, 32, 512);
            texH = Mathf.Clamp(texH, 64, 1024);

            _sideTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            _sideTex.filterMode = FilterMode.Point;
            _sideTex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < texH; y++)
            {
                float v = y / (float)(texH - 1);
                for (int x = 0; x < texW; x++)
                {
                    Color c = sideBase;

                    if (x % Mathf.Max(8, texW / 6) == 0)
                        c *= 0.7f;

                    int cellX = x / 10;
                    int cellY = y / 14;
                    int lx = x % 10;
                    int ly = y % 14;
                    if (lx >= 2 && lx <= 7 && ly >= 3 && ly <= 10 && cellY > 1 && cellY < texH / 14 - 1)
                    {
                        float lit = ((cellX * 17 + cellY * 31) % 7) / 7f;
                        if (lit > 0.35f)
                            c = Color.Lerp(c, sideWindow, 0.55f + lit * 0.35f);
                    }

                    if (x <= 2 || x >= texW - 3)
                        c = Color.Lerp(c, (y / 40) % 2 == 0 ? sideNeonA : sideNeonB, 0.85f);

                    if (ly == 1 && cellY % 3 == 0)
                        c = Color.Lerp(c, sideNeonB, 0.7f);

                    if (v < 0.08f || v > 0.92f)
                        c *= 0.55f;

                    c.a = 1f;
                    _sideTex.SetPixel(x, y, c);
                }
            }

            _sideTex.Apply();
            float ppu = frontSprite != null ? frontSprite.pixelsPerUnit : 32f;
            return Sprite.Create(_sideTex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), ppu);
        }

        void ClearChildren()
        {
            // DestroyImmediate so rebuild can run in the same frame (Destroy is deferred).
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            if (_sideTex != null)
            {
                DestroyImmediate(_sideTex);
                _sideTex = null;
            }

            _sideSprite = null;
        }

        void OnDestroy()
        {
            if (_sideTex != null) Destroy(_sideTex);
            if (_solidTex != null) Destroy(_solidTex);
        }
    }
}
