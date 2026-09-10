using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 主界面 View：构建布局，刷新顶栏/列表/猫舍/详情。
/// </summary>
public class CatGameHudView {
    readonly CatGameMain game;
    readonly CatGameUIFactory ui;
    readonly CatGameUITheme theme;

    Text coinsText, gemsText, dayText, timeText;
    ScrollRect catListScroll;
    Transform catListContent;
    RectTransform catLayer;
    RectTransform sceneRect;
    Transform detailPanel;
    GameObject emptyHint;

    public Transform DetailPanel => detailPanel;
    public RectTransform CatLayer => catLayer;
    public RectTransform SceneRect => sceneRect;
    public GameObject EmptyHint => emptyHint;
    public Transform CatListContent => catListContent;

    public CatGameHudView(CatGameMain game, CatGameUIFactory ui, CatGameUITheme theme) {
        this.game = game;
        this.ui = ui;
        this.theme = theme;
    }

    public Canvas BuildUI(out GameObject modalOverlay, out Transform modalContent, out RectTransform notifLayer) {
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null) {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        RectTransform root = (RectTransform)canvasGo.transform;
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        RectTransform topBar = ui.CreatePanel("TopBar", root, CatGameUITheme.Accent);
        ui.SetStretchTop(topBar, 40);
        HorizontalLayoutGroup topHLG = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        topHLG.padding = new RectOffset(16, 16, 4, 4);
        topHLG.spacing = 12;
        topHLG.childAlignment = TextAnchor.MiddleCenter;
        topHLG.childControlWidth = true;
        topHLG.childControlHeight = true;

        ui.CreateTextIn(topBar, "猫舍物语", 18, Color.white, TextAnchor.MiddleLeft).fontStyle = FontStyle.Bold;
        coinsText = ui.CreateTextIn(topBar, "1000", 14, Color.white, TextAnchor.MiddleLeft);
        gemsText = ui.CreateTextIn(topBar, "10", 14, Color.white, TextAnchor.MiddleLeft);
        dayText = ui.CreateTextIn(topBar, "第1天", 14, Color.white, TextAnchor.MiddleLeft);
        timeText = ui.CreateTextIn(topBar, "08:00", 14, Color.white, TextAnchor.MiddleLeft);

        RectTransform mainArea = ui.CreatePanel("MainArea", root, CatGameUITheme.Bg);
        ui.SetStretchMid(mainArea, 40, 50);
        HorizontalLayoutGroup mainHLG = mainArea.gameObject.AddComponent<HorizontalLayoutGroup>();
        mainHLG.spacing = 0;
        mainHLG.childControlWidth = true;
        mainHLG.childControlHeight = true;

        BuildLeftPanel(mainArea);
        BuildScenePanel(mainArea);
        BuildRightPanel(mainArea);
        BuildBottomNav(root);

        modalOverlay = new GameObject("ModalOverlay");
        modalOverlay.transform.SetParent(root, false);
        RectTransform moRt = modalOverlay.AddComponent<RectTransform>();
        moRt.anchorMin = Vector2.zero;
        moRt.anchorMax = Vector2.one;
        moRt.offsetMin = Vector2.zero;
        moRt.offsetMax = Vector2.zero;
        Image moImg = modalOverlay.AddComponent<Image>();
        moImg.color = new Color(0, 0, 0, 0.5f);
        Button moBtn = modalOverlay.AddComponent<Button>();
        GameObject overlayRef = modalOverlay;
        moBtn.onClick.AddListener(() => { overlayRef.SetActive(false); });
        modalOverlay.SetActive(false);

        GameObject modalGo = new GameObject("ModalContent");
        modalGo.transform.SetParent(modalOverlay.transform, false);
        RectTransform mcRt = modalGo.AddComponent<RectTransform>();
        mcRt.anchorMin = new Vector2(0.5f, 0.5f);
        mcRt.anchorMax = new Vector2(0.5f, 0.5f);
        mcRt.sizeDelta = new Vector2(560, 500);
        mcRt.anchoredPosition = Vector2.zero;
        Image mcImg = modalGo.AddComponent<Image>();
        mcImg.color = CatGameUITheme.Card;
        VerticalLayoutGroup mcVLG = modalGo.AddComponent<VerticalLayoutGroup>();
        mcVLG.spacing = 8;
        mcVLG.padding = new RectOffset(16, 16, 16, 16);
        mcVLG.childControlWidth = true;
        mcVLG.childControlHeight = true;
        mcVLG.childForceExpandWidth = true;
        mcVLG.childForceExpandHeight = false;
        ContentSizeFitter mcCsf = modalGo.AddComponent<ContentSizeFitter>();
        mcCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        modalContent = mcRt;

        GameObject notifGo = new GameObject("NotifLayer");
        notifGo.transform.SetParent(root, false);
        notifLayer = notifGo.AddComponent<RectTransform>();
        notifLayer.anchorMin = new Vector2(1, 1);
        notifLayer.anchorMax = new Vector2(1, 1);
        notifLayer.pivot = new Vector2(1, 1);
        notifLayer.anchoredPosition = new Vector2(-10, -50);
        notifLayer.sizeDelta = new Vector2(280, 0);
        VerticalLayoutGroup notifVLG = notifGo.AddComponent<VerticalLayoutGroup>();
        notifVLG.spacing = 6;
        notifVLG.childControlWidth = true;
        notifVLG.childControlHeight = true;
        notifVLG.childForceExpandWidth = true;
        notifVLG.childForceExpandHeight = false;
        notifVLG.childAlignment = TextAnchor.UpperRight;
        ContentSizeFitter notifCsf = notifGo.AddComponent<ContentSizeFitter>();
        notifCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return canvas;
    }

    void BuildLeftPanel(RectTransform mainArea) {
        RectTransform leftPanel = ui.CreatePanel("LeftPanel", mainArea, CatGameUITheme.Panel);
        leftPanel.sizeDelta = new Vector2(200, 0);
        LayoutElement leftLE = leftPanel.gameObject.AddComponent<LayoutElement>();
        leftLE.preferredWidth = 200;
        leftLE.flexibleWidth = 0;
        VerticalLayoutGroup leftVLG = leftPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        leftVLG.spacing = 4;
        leftVLG.padding = new RectOffset(6, 6, 6, 6);
        leftVLG.childControlWidth = true;
        leftVLG.childControlHeight = true;

        Text listHeader = ui.CreateTextIn(leftPanel, "我的猫咪 0/12", 14, CatGameUITheme.Text, TextAnchor.MiddleCenter);
        listHeader.fontStyle = FontStyle.Bold;
        LayoutElement hdrLE = listHeader.gameObject.AddComponent<LayoutElement>();
        hdrLE.preferredHeight = 28;
        hdrLE.flexibleHeight = 0;

        GameObject scrollGo = new GameObject("CatListScroll");
        scrollGo.transform.SetParent(leftPanel, false);
        scrollGo.AddComponent<RectTransform>();
        Image scrollBg = scrollGo.AddComponent<Image>();
        scrollBg.color = CatGameUITheme.Panel;
        scrollGo.AddComponent<Mask>().showMaskGraphic = false;
        catListScroll = scrollGo.AddComponent<ScrollRect>();
        catListScroll.horizontal = false;
        catListScroll.vertical = true;
        LayoutElement scrollLE = scrollGo.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth = 1;

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(scrollGo.transform, false);
        RectTransform contentRt = contentGo.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;
        VerticalLayoutGroup contentVLG = contentGo.AddComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 4;
        contentVLG.padding = new RectOffset(2, 2, 2, 2);
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        ContentSizeFitter csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        catListScroll.content = contentRt;
        catListContent = contentRt;
    }

    void BuildScenePanel(RectTransform mainArea) {
        RectTransform midPanel = ui.CreatePanel("ScenePanel", mainArea, CatGameUITheme.Wall);
        LayoutElement midLE = midPanel.gameObject.AddComponent<LayoutElement>();
        midLE.flexibleWidth = 1;

        RectTransform floor = ui.CreatePanel("Floor", midPanel, CatGameUITheme.Floor);
        floor.anchorMin = new Vector2(0, 0);
        floor.anchorMax = new Vector2(1, 0.4f);
        floor.offsetMin = Vector2.zero;
        floor.offsetMax = Vector2.zero;

        RectTransform furnLayer = ui.CreatePanel("FurnLayer", midPanel, Color.clear);
        ui.SetStretchFull(furnLayer);
        furnLayer.GetComponent<Image>().raycastTarget = false;

        catLayer = ui.CreatePanel("CatLayer", midPanel, Color.clear);
        ui.SetStretchFull(catLayer);
        catLayer.GetComponent<Image>().raycastTarget = false;
        sceneRect = midPanel;

        emptyHint = new GameObject("EmptyHint");
        emptyHint.transform.SetParent(midPanel, false);
        RectTransform ehRt = emptyHint.AddComponent<RectTransform>();
        ehRt.anchorMin = new Vector2(0.5f, 0.5f);
        ehRt.anchorMax = new Vector2(0.5f, 0.5f);
        ehRt.anchoredPosition = Vector2.zero;
        Text ehText = emptyHint.AddComponent<Text>();
        ehText.font = theme.Font;
        ehText.fontSize = 18;
        ehText.color = CatGameUITheme.TextLt;
        ehText.alignment = TextAnchor.MiddleCenter;
        ehText.raycastTarget = false;
        ehText.text = "猫舍空空如也\n去商店领养猫咪吧！";
    }

    void BuildRightPanel(RectTransform mainArea) {
        RectTransform rightPanel = ui.CreatePanel("RightPanel", mainArea, CatGameUITheme.Panel);
        LayoutElement rightLE = rightPanel.gameObject.AddComponent<LayoutElement>();
        rightLE.preferredWidth = 280;
        rightLE.flexibleWidth = 0;
        rightPanel.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        ScrollRect detailScroll = rightPanel.gameObject.AddComponent<ScrollRect>();
        detailScroll.horizontal = false;
        detailScroll.vertical = true;

        GameObject detailGo = new GameObject("DetailContent");
        detailGo.transform.SetParent(rightPanel, false);
        RectTransform detailRt = detailGo.AddComponent<RectTransform>();
        detailRt.anchorMin = new Vector2(0, 1);
        detailRt.anchorMax = new Vector2(1, 1);
        detailRt.pivot = new Vector2(0.5f, 1);
        detailRt.offsetMin = Vector2.zero;
        detailRt.offsetMax = Vector2.zero;
        VerticalLayoutGroup detailVLG = detailGo.AddComponent<VerticalLayoutGroup>();
        detailVLG.spacing = 6;
        detailVLG.padding = new RectOffset(8, 8, 8, 8);
        detailVLG.childControlWidth = true;
        detailVLG.childControlHeight = true;
        detailVLG.childForceExpandWidth = true;
        detailVLG.childForceExpandHeight = false;
        ContentSizeFitter detailCsf = detailGo.AddComponent<ContentSizeFitter>();
        detailCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        detailScroll.content = detailRt;
        detailPanel = detailRt;
    }

    void BuildBottomNav(RectTransform root) {
        RectTransform bottomNav = ui.CreatePanel("BottomNav", root, CatGameUITheme.Panel);
        ui.SetStretchBottom(bottomNav, 50);
        HorizontalLayoutGroup navHLG = bottomNav.gameObject.AddComponent<HorizontalLayoutGroup>();
        navHLG.spacing = 4;
        navHLG.padding = new RectOffset(8, 8, 4, 4);
        navHLG.childControlWidth = true;
        navHLG.childControlHeight = true;
        navHLG.childForceExpandWidth = true;

        ui.CreateNavBtn(navHLG.transform, "商店", () => game.Modal.OpenShop("cats"));
        ui.CreateNavBtn(navHLG.transform, "繁殖", () => game.Modal.OpenBreeding());
        ui.CreateNavBtn(navHLG.transform, "猫展", () => game.Modal.OpenCatShow());
        ui.CreateNavBtn(navHLG.transform, "图鉴", () => game.Modal.OpenCatdex());
        ui.CreateNavBtn(navHLG.transform, "成就", () => game.Modal.OpenAchievements());
        ui.CreateNavBtn(navHLG.transform, "任务", () => game.Modal.OpenTasks());
    }

    public void RefreshAll() {
        UpdateTopBar();
        UpdateCatList();
        UpdateCattery();
        UpdateDetail();
    }

    void UpdateTopBar() {
        GameSave save = game.save;
        coinsText.text = save.coins.ToString();
        gemsText.text = save.gems.ToString();
        dayText.text = "第" + save.day + "天";
        timeText.text = save.hour.ToString("D2") + ":00";
    }

    void UpdateCatList() {
        GameSave save = game.save;
        ui.ClearChildren(catListContent);
        foreach (Cat cat in save.cats) {
            if (cat == null) continue;
            BreedData breed = BreedData.Get(cat.breedId);
            if (breed == null) continue;
            GameObject card = new GameObject("CatCard_" + cat.id);
            card.transform.SetParent(catListContent, false);
            card.AddComponent<RectTransform>();
            Image img = card.AddComponent<Image>();
            img.color = save.selectedCatId == cat.id ? new Color(1f, 0.94f, 0.88f) : CatGameUITheme.Card;
            img.sprite = theme.WhiteSprite;
            Button btn = card.AddComponent<Button>();
            string catId = cat.id;
            btn.onClick.AddListener(() => { save.selectedCatId = catId; RefreshAll(); });
            HorizontalLayoutGroup hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(6, 6, 4, 4);
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            GameObject avatar = ui.CreateCatAvatar(card.transform, breed, 28);
            LayoutElement avLE = avatar.AddComponent<LayoutElement>();
            avLE.preferredWidth = 32;
            avLE.preferredHeight = 32;
            avLE.flexibleWidth = 0;

            GameObject info = new GameObject("Info");
            info.transform.SetParent(card.transform, false);
            VerticalLayoutGroup infoVLG = info.AddComponent<VerticalLayoutGroup>();
            infoVLG.spacing = 1;
            infoVLG.childControlWidth = true;
            infoVLG.childControlHeight = true;
            infoVLG.childForceExpandWidth = true;

            Text nameText = ui.CreateTextIn(info.transform, cat.name + " Lv." + cat.level, 13, CatGameUITheme.Text, TextAnchor.MiddleLeft);
            nameText.fontStyle = FontStyle.Bold;
            string status = "";
            if (cat.isSick) status += "病 ";
            if (cat.hunger < 30) status += "饿 ";
            if (cat.cleanliness < 30) status += "脏 ";
            if (cat.happiness < 30) status += "闷 ";
            if (string.IsNullOrEmpty(status)) status = "OK";
            ui.CreateTextIn(info.transform, status, 10, CatGameUITheme.TextLt, TextAnchor.MiddleLeft);

            LayoutElement cardLE = card.AddComponent<LayoutElement>();
            cardLE.preferredHeight = 42;
            cardLE.flexibleHeight = 0;
        }
    }

    void UpdateCattery() {
        GameSave save = game.save;
        ui.ClearChildren(catLayer);
        emptyHint.SetActive(save.cats.Count == 0);

        Transform furnLayer = sceneRect.Find("FurnLayer");
        if (furnLayer != null) {
            ui.ClearChildren(furnLayer);
            Vector2[] furnPos = {
                new Vector2(0.1f, 0.55f), new Vector2(0.3f, 0.65f), new Vector2(0.5f, 0.55f),
                new Vector2(0.7f, 0.65f), new Vector2(0.85f, 0.55f), new Vector2(0.2f, 0.75f),
                new Vector2(0.6f, 0.75f), new Vector2(0.8f, 0.75f), new Vector2(0.4f, 0.75f),
                new Vector2(0.05f, 0.65f),
            };
            for (int i = 0; i < save.placedFurniture.Count; i++) {
                FurnitureData f = FurnitureData.Get(save.placedFurniture[i]);
                if (f == null) continue;
                Vector2 pos = i < furnPos.Length ? furnPos[i] : new Vector2(0.5f, 0.6f);
                GameObject fGo = new GameObject("Furn_" + f.id);
                fGo.transform.SetParent(furnLayer, false);
                RectTransform fRt = fGo.AddComponent<RectTransform>();
                fRt.anchorMin = pos;
                fRt.anchorMax = pos;
                fRt.sizeDelta = new Vector2(36, 36);
                fRt.anchoredPosition = Vector2.zero;
                Text fT = fGo.AddComponent<Text>();
                fT.font = theme.Font;
                fT.fontSize = 28;
                fT.color = new Color(0.5f, 0.35f, 0.2f);
                fT.alignment = TextAnchor.MiddleCenter;
                fT.raycastTarget = false;
                fT.text = f.name[0].ToString();
            }
        }

        foreach (Cat cat in save.cats) {
            BreedData breed = BreedData.Get(cat.breedId);
            if (breed == null) continue;
            GameObject catGo = new GameObject("SceneCat_" + cat.id);
            catGo.transform.SetParent(catLayer, false);
            RectTransform catRt = catGo.AddComponent<RectTransform>();
            catRt.anchorMin = new Vector2(cat.sceneX / 100f, cat.sceneY / 100f);
            catRt.anchorMax = catRt.anchorMin;
            catRt.sizeDelta = new Vector2(60, 60);
            catRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = catGo.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            GameObject avatar = ui.CreateCatAvatar(catGo.transform, breed, 40);
            if (save.selectedCatId == cat.id) {
                avatar.GetComponent<Image>().color = new Color(breed.color.r, breed.color.g, breed.color.b, 1f);
                Outline outline = avatar.AddComponent<Outline>();
                outline.effectColor = CatGameUITheme.Accent;
                outline.effectDistance = new Vector2(3, 3);
            }

            GameObject nameGo = new GameObject("Name");
            nameGo.transform.SetParent(catGo.transform, false);
            RectTransform nameRt = nameGo.AddComponent<RectTransform>();
            nameRt.sizeDelta = new Vector2(60, 14);
            Text nameT = nameGo.AddComponent<Text>();
            nameT.font = theme.Font;
            nameT.fontSize = 10;
            nameT.color = Color.white;
            nameT.alignment = TextAnchor.MiddleCenter;
            nameT.raycastTarget = false;
            nameT.text = cat.name;
            Image nameBg = nameGo.AddComponent<Image>();
            nameBg.color = new Color(0, 0, 0, 0.5f);
            nameBg.raycastTarget = false;
            nameT.transform.SetAsLastSibling();

            string mood = GetMoodEmoji(cat);
            if (!string.IsNullOrEmpty(mood)) {
                GameObject moodGo = new GameObject("Mood");
                moodGo.transform.SetParent(catGo.transform, false);
                RectTransform moodRt = moodGo.AddComponent<RectTransform>();
                moodRt.sizeDelta = new Vector2(20, 14);
                Text moodT = moodGo.AddComponent<Text>();
                moodT.font = theme.Font;
                moodT.fontSize = 12;
                moodT.color = Color.white;
                moodT.alignment = TextAnchor.MiddleCenter;
                moodT.raycastTarget = false;
                moodT.text = mood;
            }

            Button catBtn = catGo.AddComponent<Button>();
            catBtn.targetGraphic = avatar.GetComponent<Image>();
            string cid = cat.id;
            catBtn.onClick.AddListener(() => { save.selectedCatId = cid; RefreshAll(); });
            game.StartCoroutine(FloatAnim(catRt, cat.floatOffset));
        }

        if (save.breeding != null) {
            Cat p1 = save.cats.Find(c => c.id == save.breeding.parent1);
            Cat p2 = save.cats.Find(c => c.id == save.breeding.parent2);
            if (p1 != null && p2 != null) {
                GameObject brGo = new GameObject("BreedProgress");
                brGo.transform.SetParent(catLayer, false);
                RectTransform brRt = brGo.AddComponent<RectTransform>();
                brRt.anchorMin = new Vector2(0.5f, 1f);
                brRt.anchorMax = new Vector2(0.5f, 1f);
                brRt.pivot = new Vector2(0.5f, 1f);
                brRt.sizeDelta = new Vector2(250, 24);
                brRt.anchoredPosition = new Vector2(0, -6);
                Image brBg = brGo.AddComponent<Image>();
                brBg.color = new Color(1, 1, 1, 0.9f);
                Text brT = brGo.AddComponent<Text>();
                brT.font = theme.Font;
                brT.fontSize = 12;
                brT.color = CatGameUITheme.Text;
                brT.alignment = TextAnchor.MiddleCenter;
                brT.raycastTarget = false;
                brT.text = "繁殖中: " + p1.name + " x " + p2.name + " (" + save.breeding.remaining + "h)";
            }
        }
    }

    static string GetMoodEmoji(Cat cat) {
        if (cat.isSick) return "病";
        if (cat.hunger < 20) return "饿";
        if (cat.happiness > 70) return "乐";
        if (cat.happiness < 30) return "闷";
        return "";
    }

    IEnumerator FloatAnim(RectTransform rt, float offset) {
        Vector2 basePos = rt.anchoredPosition;
        while (rt != null && rt.gameObject.activeSelf) {
            float y = Mathf.Sin((Time.time + offset) * 1.5f) * 6f;
            rt.anchoredPosition = basePos + new Vector2(0, y);
            yield return null;
        }
    }

    void UpdateDetail() {
        GameSave save = game.save;
        ui.ClearChildren(detailPanel);

        if (string.IsNullOrEmpty(save.selectedCatId)) {
            Text hint = ui.CreateTextIn(detailPanel, "点击猫咪\n查看详情和操作", 14, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);
            LayoutElement le = hint.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 200;
            return;
        }

        Cat cat = save.cats.Find(c => c.id == save.selectedCatId);
        if (cat == null) { save.selectedCatId = null; UpdateDetail(); return; }
        BreedData breed = BreedData.Get(cat.breedId);
        Rarity rar = (Rarity)System.Enum.Parse(typeof(Rarity), cat.rarity);

        GameObject hdrGo = new GameObject("Header");
        hdrGo.transform.SetParent(detailPanel, false);
        VerticalLayoutGroup hdrVLG = hdrGo.AddComponent<VerticalLayoutGroup>();
        hdrVLG.childControlWidth = true;
        hdrVLG.childControlHeight = true;
        hdrVLG.childForceExpandWidth = true;
        hdrVLG.childAlignment = TextAnchor.MiddleCenter;
        hdrVLG.spacing = 2;

        GameObject avatarBig = ui.CreateCatAvatar(hdrGo.transform, breed, 56);
        LayoutElement avLE = avatarBig.AddComponent<LayoutElement>();
        avLE.preferredHeight = 60;
        avLE.flexibleHeight = 0;

        ui.CreateTextIn(hdrGo.transform, cat.name, 18, CatGameUITheme.Text, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;
        string ageName = cat.age == "kitten" ? "幼猫" : cat.age == "adult" ? "成年" : "老年";
        Personality pers = (Personality)System.Enum.Parse(typeof(Personality), cat.personality);
        ui.CreateTextIn(hdrGo.transform, breed.name + " · " + GameConfig.PERSONALITY_NAMES[pers] + " · " + ageName, 12, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);
        ui.CreateTextIn(hdrGo.transform, GameConfig.RARITY_NAMES[rar] + " · Lv." + cat.level, 12, CatGameUITheme.AccentDk, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;

        if (cat.isSick) {
            Text sickT = ui.CreateTextIn(hdrGo.transform, "生病了: " + cat.sickness, 12, CatGameUITheme.Danger, TextAnchor.MiddleCenter);
            sickT.fontStyle = FontStyle.Bold;
        }

        ui.CreateStatBar(detailPanel, "饱食", cat.hunger, new Color(1f, 0.44f, 0.27f));
        ui.CreateStatBar(detailPanel, "清洁", cat.cleanliness, new Color(0.26f, 0.65f, 0.96f));
        ui.CreateStatBar(detailPanel, "心情", cat.happiness, new Color(1f, 0.79f, 0.16f));
        ui.CreateStatBar(detailPanel, "健康", cat.health, new Color(0.4f, 0.73f, 0.31f));
        ui.CreateStatBar(detailPanel, "好感", cat.affection, new Color(0.93f, 0.25f, 0.38f));
        ui.CreateTextIn(detailPanel, "EXP: " + cat.exp + "/" + (cat.level * 50), 11, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);

        GameObject btnGrid = new GameObject("BtnGrid");
        btnGrid.transform.SetParent(detailPanel, false);
        GridLayoutGroup glg = btnGrid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(120, 44);
        glg.spacing = new Vector2(6, 6);
        glg.childAlignment = TextAnchor.MiddleCenter;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;

        ui.CreateActionBtn(btnGrid.transform, "喂食", new Color(1f, 0.8f, 0.5f), CatGameUITheme.AccentDk, () => game.Modal.OpenFeedMenu(cat.id));
        ui.CreateActionBtn(btnGrid.transform, "清洁", new Color(0.56f, 0.79f, 0.98f), new Color(0.08f, 0.4f, 0.75f), () => game.Care.CleanCat(cat.id));
        ui.CreateActionBtn(btnGrid.transform, "玩耍", new Color(1f, 0.84f, 0.31f), new Color(0.96f, 0.5f, 0.09f), () => game.Modal.OpenPlayMenu(cat.id));
        bool canHeal = cat.isSick || cat.health < 80;
        ui.CreateActionBtn(btnGrid.transform, "治疗 " + GameConfig.HEAL_COST + "G", new Color(0.65f, 0.84f, 0.65f), new Color(0.18f, 0.49f, 0.2f), () => game.Care.HealCat(cat.id), !canHeal);
        bool canShow = !(save.catShowUsed && save.catShowLastDay == save.day) && !cat.isSick;
        ui.CreateActionBtn(btnGrid.transform, "猫展", new Color(1f, 0.8f, 0.5f), CatGameUITheme.AccentDk, () => game.Show.EnterCatShow(cat.id), !canShow);
        ui.CreateActionBtn(btnGrid.transform, "送养", new Color(0.81f, 0.58f, 0.85f), new Color(0.42f, 0.11f, 0.6f), () => game.Shop.ConfirmSell(cat.id));

        int foodCount = 0, toyCount = 0;
        foreach (var p in save.foodInv) foodCount += p.value;
        foreach (var p in save.toyInv) toyCount += p.value;
        ui.CreateTextIn(detailPanel, "食物: " + foodCount + "件 | 玩具: " + toyCount + "件", 11, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);
    }
}
