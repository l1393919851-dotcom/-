using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 弹窗 View：商店、喂食、玩耍、繁殖、猫展、图鉴、成就、任务。
/// </summary>
public class CatGameModalView {
    readonly CatGameMain game;
    readonly CatGameUIFactory ui;

    GameObject modalOverlay;
    Transform modalContent;
    string currentShopTab = "cats";

    public CatGameModalView(CatGameMain game, CatGameUIFactory ui) {
        this.game = game;
        this.ui = ui;
    }

    public void Bind(GameObject overlay, Transform content) {
        modalOverlay = overlay;
        modalContent = content;
    }

    public void CloseModal() {
        if (modalOverlay != null) modalOverlay.SetActive(false);
    }

    public void ShowModal(string title) {
        ui.ClearChildren(modalContent);
        ui.CreateTextIn(modalContent, title, 20, CatGameUITheme.AccentDk, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;
        modalOverlay.SetActive(true);
    }

    public void OpenShop(string tab) {
        GameSave save = game.save;
        currentShopTab = tab;
        ShowModal("商店");
        ui.CreateTextIn(modalContent, "金币: " + save.coins, 14, CatGameUITheme.AccentDk, TextAnchor.MiddleCenter);

        GameObject tabRow = new GameObject("TabRow");
        tabRow.transform.SetParent(modalContent, false);
        HorizontalLayoutGroup tabHLG = tabRow.AddComponent<HorizontalLayoutGroup>();
        tabHLG.childControlWidth = true;
        tabHLG.childControlHeight = true;
        tabHLG.childForceExpandWidth = true;
        string[] tabs = { "cats", "food", "toy", "furniture" };
        string[] tabLabels = { "猫咪", "食物", "玩具", "家具" };
        for (int i = 0; i < tabs.Length; i++) {
            string t = tabs[i];
            ui.CreateActionBtn(tabRow.transform, tabLabels[i], t == tab ? CatGameUITheme.Accent : CatGameUITheme.Card,
                t == tab ? Color.white : CatGameUITheme.Text, () => OpenShop(t));
        }

        GameObject scrollGo = new GameObject("ShopScroll");
        scrollGo.transform.SetParent(modalContent, false);
        ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
        sr.horizontal = false;
        Image srBg = scrollGo.AddComponent<Image>();
        srBg.color = CatGameUITheme.Bg;
        scrollGo.AddComponent<Mask>().showMaskGraphic = false;
        LayoutElement srLE = scrollGo.AddComponent<LayoutElement>();
        srLE.preferredHeight = 340;
        srLE.flexibleHeight = 0;

        GameObject sContent = new GameObject("ShopContent");
        sContent.transform.SetParent(scrollGo.transform, false);
        RectTransform sRt = sContent.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 1);
        sRt.anchorMax = new Vector2(1, 1);
        sRt.pivot = new Vector2(0.5f, 1);
        sRt.offsetMin = Vector2.zero;
        sRt.offsetMax = Vector2.zero;
        VerticalLayoutGroup sVLG = sContent.AddComponent<VerticalLayoutGroup>();
        sVLG.spacing = 6;
        sVLG.padding = new RectOffset(4, 4, 4, 4);
        sVLG.childControlWidth = true;
        sVLG.childControlHeight = true;
        sVLG.childForceExpandWidth = true;
        sVLG.childForceExpandHeight = false;
        ContentSizeFitter sCsf = sContent.AddComponent<ContentSizeFitter>();
        sCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = sRt;

        if (tab == "cats") {
            for (int i = 0; i < save.shopCats.Count; i++) {
                ShopCatEntry sc = save.shopCats[i];
                BreedData breed = BreedData.Get(sc.breedId);
                int idx = i;
                ui.CreateShopItem(sRt, breed.shortName, breed.name + " " + GameConfig.RARITY_NAMES[breed.rarity], sc.price + "G", breed.color,
                    save.coins >= sc.price && save.cats.Count < GameConfig.MAX_CATS, () => game.Shop.AdoptCat(idx));
            }
            ui.CreateActionBtn(sRt, "刷新 (50钻)", CatGameUITheme.Info, Color.white, () => game.Shop.RefreshShopWithGems());
        } else if (tab == "food") {
            foreach (FoodData f in FoodData.All) {
                ui.CreateShopItem(sRt, f.name[0].ToString(), f.name + " " + f.desc + " (库存:" + game.SaveService.GetInv("food", f.id) + ")", f.price + "G", CatGameUITheme.Card,
                    save.coins >= f.price, () => game.Shop.BuyFood(f.id));
            }
        } else if (tab == "toy") {
            foreach (ToyData t in ToyData.All) {
                ui.CreateShopItem(sRt, t.name[0].ToString(), t.name + " " + t.desc + " (库存:" + game.SaveService.GetInv("toy", t.id) + ")", t.price + "G", CatGameUITheme.Card,
                    save.coins >= t.price, () => game.Shop.BuyToy(t.id));
            }
        } else if (tab == "furniture") {
            foreach (FurnitureData f in FurnitureData.All) {
                bool owned = save.placedFurniture.Contains(f.id);
                ui.CreateShopItem(sRt, f.name[0].ToString(), f.name + " " + f.desc, owned ? "已拥有" : f.price + "G", CatGameUITheme.Card,
                    !owned && save.coins >= f.price, owned ? null : () => game.Shop.BuyFurniture(f.id));
            }
        }

        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void OpenFeedMenu(string catId) {
        Cat cat = game.save.cats.Find(c => c.id == catId);
        if (cat == null) return;
        ShowModal("喂食 " + cat.name);
        ui.CreateTextIn(modalContent, "饱食度: " + Mathf.FloorToInt(cat.hunger) + "/100", 13, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);

        foreach (FoodData f in FoodData.All) {
            int count = game.SaveService.GetInv("food", f.id);
            string fid = f.id;
            ui.CreateShopItem(modalContent, f.name[0].ToString(), f.name + " " + f.desc + " (库存:" + count + ")", "喂食",
                CatGameUITheme.Card, count > 0, () => game.Care.FeedCat(catId, fid));
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void OpenPlayMenu(string catId) {
        Cat cat = game.save.cats.Find(c => c.id == catId);
        if (cat == null) return;
        ShowModal("玩耍 " + cat.name);
        ui.CreateTextIn(modalContent, "心情: " + Mathf.FloorToInt(cat.happiness) + " | 好感: " + Mathf.FloorToInt(cat.affection), 13, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);

        foreach (ToyData t in ToyData.All) {
            int count = game.SaveService.GetInv("toy", t.id);
            string tid = t.id;
            ui.CreateShopItem(modalContent, t.name[0].ToString(), t.name + " " + t.desc + " (库存:" + count + ")", "使用",
                CatGameUITheme.Card, count > 0, () => game.Care.PlayWithCat(catId, tid));
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void OpenBreeding(bool resetSelection = true) {
        GameSave save = game.save;
        if (resetSelection) {
            save.breedSel1 = null;
            save.breedSel2 = null;
        }
        ShowModal("猫咪繁殖");
        ui.CreateTextIn(modalContent, "费用: " + GameConfig.BREED_COST + "G | 等待: " + GameConfig.BREED_TIME + "h | 10%变异概率", 12, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);

        if (save.breeding != null) {
            Cat p1 = save.cats.Find(c => c.id == save.breeding.parent1);
            Cat p2 = save.cats.Find(c => c.id == save.breeding.parent2);
            if (p1 != null && p2 != null)
                ui.CreateTextIn(modalContent, p1.name + " x " + p2.name + " 繁殖中... " + save.breeding.remaining + "h", 14, CatGameUITheme.Warn, TextAnchor.MiddleCenter);
            ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
            return;
        }

        List<Cat> adults = save.cats.FindAll(c => c.age == "adult");
        ui.CreateTextIn(modalContent, "可用成年猫: " + adults.Count + "只", 13, CatGameUITheme.Text, TextAnchor.MiddleCenter);

        for (int slot = 1; slot <= 2; slot++) {
            string selId = slot == 1 ? save.breedSel1 : save.breedSel2;
            Cat selCat = !string.IsNullOrEmpty(selId) ? save.cats.Find(c => c.id == selId) : null;
            string slotLabel = "选择第" + slot + "只: " + (selCat != null ? selCat.name : "点击选择");
            ui.CreateActionBtn(modalContent, slotLabel, CatGameUITheme.Panel, CatGameUITheme.Text, null, true);
        }

        bool canBreed = !string.IsNullOrEmpty(save.breedSel1) && !string.IsNullOrEmpty(save.breedSel2) &&
                        save.coins >= GameConfig.BREED_COST && save.cats.Count < GameConfig.MAX_CATS;
        ui.CreateActionBtn(modalContent, "开始繁殖", canBreed ? CatGameUITheme.Accent : new Color(0.7f, 0.7f, 0.7f), Color.white,
            () => game.Breeding.StartBreeding(), !canBreed);

        foreach (Cat cat in adults) {
            bool canSel = cat.breedCooldown == 0;
            string info = cat.name + " Lv." + cat.level + (canSel ? "" : " (冷却中)");
            string cid = cat.id;
            ui.CreateActionBtn(modalContent, info, canSel ? CatGameUITheme.Panel : new Color(0.85f, 0.85f, 0.85f),
                canSel ? CatGameUITheme.Text : Color.gray,
                canSel ? () => {
                    if (save.breedSel1 == cid) save.breedSel1 = null;
                    else if (save.breedSel2 == cid) save.breedSel2 = null;
                    else {
                        if (string.IsNullOrEmpty(save.breedSel1)) save.breedSel1 = cid;
                        else save.breedSel2 = cid;
                    }
                    OpenBreeding(false);
                } : null);
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void OpenCatShow() {
        GameSave save = game.save;
        ShowModal("猫展比赛");
        ui.CreateTextIn(modalContent, "每天可参加一次，根据猫咪属性评分排名", 12, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);

        if (save.catShowUsed && save.catShowLastDay == save.day) {
            ui.CreateTextIn(modalContent, "今天已参加过，明天再来吧!", 14, CatGameUITheme.Warn, TextAnchor.MiddleCenter);
        } else {
            ui.CreateTextIn(modalContent, "选择参赛猫咪:", 14, CatGameUITheme.Text, TextAnchor.MiddleCenter);
            foreach (Cat cat in save.cats) {
                if (cat.isSick) continue;
                BreedData breed = BreedData.Get(cat.breedId);
                int score = game.Show.EstimateScore(cat);
                string cid = cat.id;
                ui.CreateShopItem(modalContent, breed.shortName, cat.name + " Lv." + cat.level + " 预估得分:" + score, "参赛", breed.color, true,
                    () => game.Show.EnterCatShow(cid));
            }
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void ShowCatShowResult(CatShowResult result) {
        ui.ClearChildren(modalContent);
        Cat cat = result.cat;
        int rank = result.rank;
        string rankStr = rank == 1 ? "第1名!" : rank == 2 ? "第2名" : rank == 3 ? "第3名" : "第" + rank + "名";
        ui.CreateTextIn(modalContent, "猫展结果", 20, CatGameUITheme.AccentDk, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;
        BreedData breed = BreedData.Get(cat.breedId);
        GameObject av = ui.CreateCatAvatar(modalContent, breed, 48);
        LayoutElement le = av.AddComponent<LayoutElement>();
        le.preferredHeight = 52;
        le.flexibleHeight = 0;
        ui.CreateTextIn(modalContent, cat.name, 16, CatGameUITheme.Text, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;
        ui.CreateTextIn(modalContent, "得分: " + result.score, 14, CatGameUITheme.Text, TextAnchor.MiddleCenter);
        ui.CreateTextIn(modalContent, "对手: " + result.opponents[0] + " / " + result.opponents[1] + " / " + result.opponents[2], 12, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);
        ui.CreateTextIn(modalContent, rankStr, 28, rank <= 3 ? CatGameUITheme.Success : CatGameUITheme.TextLt, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;
        if (result.reward > 0)
            ui.CreateTextIn(modalContent, "奖金: " + result.reward + " 金币", 18, CatGameUITheme.AccentDk, TextAnchor.MiddleCenter).fontStyle = FontStyle.Bold;
        else
            ui.CreateTextIn(modalContent, "未获奖，再接再厉!", 14, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);
        ui.CreateActionBtn(modalContent, "确定", CatGameUITheme.Accent, Color.white, CloseModal);
        modalOverlay.SetActive(true);
    }

    public void OpenCatdex() {
        GameSave save = game.save;
        ShowModal("猫咪图鉴 " + save.catdex.Count + "/" + BreedData.All.Count);
        foreach (BreedData breed in BreedData.All) {
            bool unlocked = save.catdex.Contains(breed.id);
            string info = unlocked ? breed.name + " " + GameConfig.RARITY_NAMES[breed.rarity] : "??? 未解锁";
            ui.CreateShopItem(modalContent, breed.shortName, info, unlocked ? GameConfig.RARITY_NAMES[breed.rarity] : "???",
                unlocked ? breed.color : new Color(0.6f, 0.6f, 0.6f), false, null);
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void OpenAchievements() {
        GameSave save = game.save;
        ShowModal("成就 " + save.achievements.Count + "/" + AchievementData.All.Count);
        foreach (AchievementData ach in AchievementData.All) {
            bool done = save.achievements.Contains(ach.id);
            string info = ach.icon + " " + ach.name + " - " + ach.desc + " (+" + ach.reward + "G)";
            ui.CreateTextIn(modalContent, (done ? "[v] " : "[ ] ") + info, 13, done ? CatGameUITheme.Success : CatGameUITheme.TextLt, TextAnchor.MiddleLeft);
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }

    public void OpenTasks() {
        GameSave save = game.save;
        ShowModal("每日任务");
        ui.CreateTextIn(modalContent, "第" + save.day + "天任务 (每日刷新)", 12, CatGameUITheme.TextLt, TextAnchor.MiddleCenter);
        foreach (TaskProgress t in save.dailyTasks) {
            bool done = t.progress >= t.target;
            string info = t.icon + " " + t.name + " (" + t.progress + "/" + t.target + ") 奖励:" + t.reward + "G";
            if (t.claimed) {
                ui.CreateTextIn(modalContent, "[已领取] " + info, 13, CatGameUITheme.TextLt, TextAnchor.MiddleLeft);
            } else if (done) {
                string tid = t.id;
                ui.CreateActionBtn(modalContent, "[领取] " + info, CatGameUITheme.Success, Color.white, () => game.Meta.ClaimTask(tid));
            } else {
                ui.CreateTextIn(modalContent, "[进行中] " + info, 13, CatGameUITheme.Text, TextAnchor.MiddleLeft);
            }
        }
        ui.CreateActionBtn(modalContent, "关闭", CatGameUITheme.Accent, Color.white, CloseModal);
    }
}
