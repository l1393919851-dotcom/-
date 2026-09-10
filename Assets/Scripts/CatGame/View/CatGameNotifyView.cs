using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 右上角通知条。
/// </summary>
public class CatGameNotifyView {
    readonly CatGameMain game;
    readonly CatGameUIFactory ui;
    readonly CatGameUITheme theme;
    RectTransform notifLayer;

    public CatGameNotifyView(CatGameMain game, CatGameUIFactory ui, CatGameUITheme theme) {
        this.game = game;
        this.ui = ui;
        this.theme = theme;
    }

    public void Bind(RectTransform layer) {
        notifLayer = layer;
    }

    public void Notify(string msg, Color color) {
        GameObject go = new GameObject("Notif");
        go.transform.SetParent(notifLayer, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = theme.WhiteSprite;
        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 8, 8);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Text t = ui.CreateTextIn(go.transform, msg, 13, Color.white, TextAnchor.MiddleCenter);
        t.fontStyle = FontStyle.Bold;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;

        game.StartCoroutine(NotifFade(go, 3f));
    }

    IEnumerator NotifFade(GameObject go, float duration) {
        yield return new WaitForSeconds(duration);
        if (go != null) Object.Destroy(go);
    }
}
