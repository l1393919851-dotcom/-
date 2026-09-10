using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI 控件工厂：面板、文本、按钮、属性条、头像、商店行等。
/// </summary>
public class CatGameUIFactory {
    readonly CatGameUITheme theme;

    public CatGameUIFactory(CatGameUITheme theme) {
        this.theme = theme;
    }

    public RectTransform CreatePanel(string name, Transform parent, Color color) {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = theme.WhiteSprite;
        img.raycastTarget = color.a > 0;
        return rt;
    }

    public void SetStretchTop(RectTransform rt, float height) {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(0, height);
    }

    public void SetStretchBottom(RectTransform rt, float height) {
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(0, height);
    }

    public void SetStretchMid(RectTransform rt, float top, float bottom) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(0, bottom);
        rt.offsetMax = new Vector2(0, -top);
    }

    public void SetStretchFull(RectTransform rt) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public Text CreateTextIn(Transform parent, string text, int fontSize, Color color, TextAnchor alignment) {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Text t = go.AddComponent<Text>();
        t.font = theme.Font;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = alignment;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.text = text ?? "";
        return t;
    }

    public void ClearChildren(Transform t) {
        for (int i = t.childCount - 1; i >= 0; i--)
            Object.Destroy(t.GetChild(i).gameObject);
    }

    public void CreateNavBtn(Transform parent, string label, UnityAction onClick) {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = CatGameUITheme.Card;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        Text t = CreateTextIn(go.transform, label, 13, CatGameUITheme.Text, TextAnchor.MiddleCenter);
        t.fontStyle = FontStyle.Bold;
    }

    public void CreateActionBtn(Transform parent, string label, Color bg, Color text, UnityAction onClick, bool disabled = false) {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = disabled ? new Color(0.8f, 0.8f, 0.8f, 0.6f) : bg;
        img.sprite = theme.WhiteSprite;
        Button btn = go.AddComponent<Button>();
        if (!disabled && onClick != null) btn.onClick.AddListener(onClick);
        btn.interactable = !disabled;
        Text t = go.AddComponent<Text>();
        t.font = theme.Font;
        t.fontSize = 12;
        t.color = disabled ? Color.gray : text;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = true;
        t.text = label ?? "";
    }

    public void CreateStatBar(Transform parent, string label, float value, Color barColor) {
        GameObject go = new GameObject("StatBar_" + label);
        go.transform.SetParent(parent, false);
        VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 1;

        GameObject labelRow = new GameObject("LabelRow");
        labelRow.transform.SetParent(go.transform, false);
        HorizontalLayoutGroup lrHLG = labelRow.AddComponent<HorizontalLayoutGroup>();
        lrHLG.childControlWidth = true;
        lrHLG.childControlHeight = true;
        lrHLG.childForceExpandWidth = true;
        CreateTextIn(labelRow.transform, label, 11, CatGameUITheme.Text, TextAnchor.MiddleLeft);
        CreateTextIn(labelRow.transform, Mathf.FloorToInt(value).ToString(), 11, CatGameUITheme.Text, TextAnchor.MiddleRight);

        GameObject barGo = new GameObject("Bar");
        barGo.transform.SetParent(go.transform, false);
        barGo.AddComponent<RectTransform>();
        Image barBg = barGo.AddComponent<Image>();
        barBg.color = new Color(0.88f, 0.88f, 0.88f);

        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barGo.transform, false);
        RectTransform fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(value / 100f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fillGo.AddComponent<Image>();
        fillImg.color = barColor;

        LayoutElement barLE = barGo.AddComponent<LayoutElement>();
        barLE.preferredHeight = 10;
        barLE.flexibleHeight = 0;
    }

    public GameObject CreateCatAvatar(Transform parent, BreedData breed, int size) {
        Color color = breed != null ? breed.color : Color.gray;
        string label = breed != null && !string.IsNullOrEmpty(breed.shortName) ? breed.shortName : "?";

        GameObject go = new GameObject("Avatar");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);
        Image img = go.AddComponent<Image>();
        img.sprite = theme.RoundSprite;
        img.color = color;

        // Text 放子节点，避免与 Image 同物体冲突；且先设 font 再设 text
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        Text t = labelGo.AddComponent<Text>();
        t.font = theme.Font;
        t.fontSize = Mathf.Max(8, size / 2);
        t.color = color.grayscale > 0.5f ? Color.black : Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        t.text = label;
        return go;
    }

    public void CreateShopItem(Transform parent, string iconText, string desc, string price, Color iconColor, bool canBuy, UnityAction onClick) {
        GameObject go = new GameObject("ShopItem");
        go.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 6, 6);
        hlg.spacing = 8;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = false;
        Image bg = go.AddComponent<Image>();
        bg.color = CatGameUITheme.Panel;
        bg.sprite = theme.WhiteSprite;

        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(go.transform, false);
        RectTransform iconRt = icon.AddComponent<RectTransform>();
        iconRt.sizeDelta = new Vector2(36, 36);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.sprite = theme.RoundSprite;
        iconImg.color = iconColor;
        Text iconT = icon.AddComponent<Text>();
        iconT.font = theme.Font;
        iconT.fontSize = 18;
        iconT.color = iconColor.grayscale > 0.5f ? Color.black : Color.white;
        iconT.alignment = TextAnchor.MiddleCenter;
        iconT.raycastTarget = false;
        iconT.text = iconText ?? "";
        LayoutElement iconLE = icon.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 40;
        iconLE.preferredHeight = 40;
        iconLE.flexibleWidth = 0;

        GameObject descGo = new GameObject("Desc");
        descGo.transform.SetParent(go.transform, false);
        VerticalLayoutGroup descVLG = descGo.AddComponent<VerticalLayoutGroup>();
        descVLG.childControlWidth = true;
        descVLG.childControlHeight = true;
        descVLG.childForceExpandWidth = true;
        CreateTextIn(descGo.transform, desc, 12, CatGameUITheme.Text, TextAnchor.MiddleLeft);
        Text priceT = CreateTextIn(descGo.transform, price, 14, CatGameUITheme.AccentDk, TextAnchor.MiddleLeft);
        priceT.fontStyle = FontStyle.Bold;

        GameObject btnGo = new GameObject("BuyBtn");
        btnGo.transform.SetParent(go.transform, false);
        RectTransform btnRt = btnGo.AddComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(70, 36);
        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = canBuy ? CatGameUITheme.Accent : new Color(0.7f, 0.7f, 0.7f);
        btnImg.sprite = theme.WhiteSprite;
        Button btn = btnGo.AddComponent<Button>();
        btn.interactable = canBuy;
        if (canBuy && onClick != null) btn.onClick.AddListener(onClick);
        Text btnT = btnGo.AddComponent<Text>();
        btnT.font = theme.Font;
        btnT.fontSize = 13;
        btnT.color = Color.white;
        btnT.alignment = TextAnchor.MiddleCenter;
        btnT.fontStyle = FontStyle.Bold;
        btnT.text = onClick == null ? "已拥有" : "购买";
        LayoutElement btnLE = btnGo.AddComponent<LayoutElement>();
        btnLE.preferredWidth = 70;
        btnLE.flexibleWidth = 0;

        LayoutElement itemLE = go.AddComponent<LayoutElement>();
        itemLE.preferredHeight = 56;
        itemLE.flexibleHeight = 0;
    }
}
