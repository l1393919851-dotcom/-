using System.Linq;
using UnityEngine;

/// <summary>
/// UI 主题色与基础资源（字体、白图、圆形头像）。
/// </summary>
public class CatGameUITheme {
    public Font Font { get; private set; }
    public Sprite WhiteSprite { get; private set; }
    public Sprite RoundSprite { get; private set; }

    public static readonly Color Bg = new Color(1f, 0.96f, 0.9f);
    public static readonly Color Panel = new Color(1f, 0.91f, 0.8f);
    public static readonly Color Card = Color.white;
    public static readonly Color Accent = new Color(1f, 0.55f, 0.26f);
    public static readonly Color AccentDk = new Color(0.88f, 0.44f, 0.13f);
    public static readonly Color AccentLt = new Color(1f, 0.69f, 0.48f);
    public static readonly Color Text = new Color(0.29f, 0.22f, 0.16f);
    public static readonly Color TextLt = new Color(0.55f, 0.45f, 0.33f);
    public static readonly Color Wall = new Color(1f, 0.85f, 0.71f);
    public static readonly Color Floor = new Color(0.87f, 0.72f, 0.53f);
    public static readonly Color Success = new Color(0.31f, 0.69f, 0.31f);
    public static readonly Color Danger = new Color(0.96f, 0.26f, 0.21f);
    public static readonly Color Warn = new Color(1f, 0.6f, 0f);
    public static readonly Color Info = new Color(0.26f, 0.65f, 0.96f);

    public void Init() {
        Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (Font == null) Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (Font == null) Font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Microsoft YaHei", "PingFang SC", "Helvetica" }, 16);
        if (Font == null) Debug.LogError("[CatGame] 无法加载 UI 字体");

        Texture2D tex = new Texture2D(4, 4);
        tex.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
        tex.Apply();
        WhiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
        RoundSprite = MakeRoundSprite(64);
    }

    static Sprite MakeRoundSprite(int size) {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float center = size * 0.5f;
        float radius = size * 0.45f;
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * size + x] = d <= radius ? Color.white : Color.clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
    }
}
