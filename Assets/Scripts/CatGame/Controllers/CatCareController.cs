using System;
using UnityEngine;

/// <summary>
/// 照顾相关：喂食、清洁、玩耍、治疗、生病、成长、经验、家具效果。
/// </summary>
public class CatCareController {
    readonly CatGameMain game;

    public CatCareController(CatGameMain game) {
        this.game = game;
    }

    GameSave Save => game.save;
    CatGameSaveService Store => game.SaveService;

    public void FeedCat(string catId, string foodId) {
        Cat cat = Save.cats.Find(c => c.id == catId);
        FoodData food = FoodData.Get(foodId);
        if (cat == null || food == null) return;
        if (Store.GetInv("food", foodId) <= 0) { game.Notify("没有" + food.name + "了", CatGameUITheme.Warn); return; }
        Store.AddInv("food", foodId, -1);
        cat.hunger = Mathf.Clamp(cat.hunger + food.hunger, 0, 100);
        cat.happiness = Mathf.Clamp(cat.happiness + food.happiness, 0, 100);
        cat.health = Mathf.Clamp(cat.health + food.health, 0, 100);
        cat.affection = Mathf.Clamp(cat.affection + food.affection, 0, 100);
        AddExp(cat, 5);
        Save.stats.totalFeed++;
        game.Meta.UpdateDailyTask("feed");
        game.Meta.UpdateDailyTask("interact");
        game.Notify(cat.name + " 吃了 " + food.name, CatGameUITheme.Success);
        game.Meta.CheckAchievements();
        Store.Persist();
        game.RefreshAll();
        game.CloseModal();
    }

    public void CleanCat(string catId) {
        Cat cat = Save.cats.Find(c => c.id == catId);
        if (cat == null) return;
        cat.cleanliness = 100;
        cat.happiness = Mathf.Clamp(cat.happiness + 5, 0, 100);
        AddExp(cat, 3);
        Save.stats.totalClean++;
        game.Meta.UpdateDailyTask("clean");
        game.Meta.UpdateDailyTask("interact");
        game.Notify(cat.name + " 洗得干干净净!", CatGameUITheme.Success);
        game.Meta.CheckAchievements();
        Store.Persist();
        game.RefreshAll();
    }

    public void PlayWithCat(string catId, string toyId) {
        Cat cat = Save.cats.Find(c => c.id == catId);
        ToyData toy = ToyData.Get(toyId);
        if (cat == null || toy == null) return;
        if (Store.GetInv("toy", toyId) <= 0) { game.Notify("没有" + toy.name + "了", CatGameUITheme.Warn); return; }
        if (cat.hunger < 15) { game.Notify(cat.name + " 太饿了", CatGameUITheme.Warn); return; }
        Store.AddInv("toy", toyId, -1);
        float bonus = 1f;
        if (Save.placedFurniture.Contains("tree")) bonus *= FurnitureData.Get("tree").playBonus;
        cat.affection = Mathf.Clamp(cat.affection + toy.affection * bonus, 0, 100);
        cat.happiness = Mathf.Clamp(cat.happiness + toy.happiness * bonus, 0, 100);
        cat.hunger = Mathf.Clamp(cat.hunger - toy.hungerCost, 0, 100);
        AddExp(cat, toy.exp);
        Save.stats.totalPlay++;
        game.Meta.UpdateDailyTask("play");
        game.Meta.UpdateDailyTask("interact");
        game.Notify(cat.name + " 玩得超开心!", CatGameUITheme.Success);
        game.Meta.CheckAchievements();
        Store.Persist();
        game.RefreshAll();
        game.CloseModal();
    }

    public void HealCat(string catId) {
        Cat cat = Save.cats.Find(c => c.id == catId);
        if (cat == null) return;
        if (!cat.isSick && cat.health >= 80) { game.Notify(cat.name + " 很健康", CatGameUITheme.Info); return; }
        if (Save.coins < GameConfig.HEAL_COST) { game.Notify("金币不足!", CatGameUITheme.Danger); return; }
        Save.coins -= GameConfig.HEAL_COST;
        cat.isSick = false;
        cat.sickness = null;
        cat.health = 100;
        game.Notify(cat.name + " 恢复健康!", CatGameUITheme.Success);
        Store.Persist();
        game.RefreshAll();
    }

    public void CheckSickness() {
        string[] sicknesses = { "感冒", "肠胃炎", "猫癣", "结膜炎" };
        foreach (Cat cat in Save.cats) {
            if (cat.isSick) { cat.health = Mathf.Clamp(cat.health - 2, 0, 100); continue; }
            if (cat.health < GameConfig.SICK_THRESHOLD && UnityEngine.Random.value < GameConfig.SICK_CHANCE) {
                cat.isSick = true;
                cat.sickness = sicknesses[UnityEngine.Random.Range(0, sicknesses.Length)];
                game.Notify(cat.name + " 生病了: " + cat.sickness + "!", CatGameUITheme.Danger);
            }
        }
    }

    public void AddExp(Cat cat, int amount) {
        cat.exp += amount;
        int need = cat.level * 50;
        while (cat.exp >= need) {
            cat.exp -= need;
            cat.level++;
            cat.health = Mathf.Clamp(cat.health + 10, 0, 100);
            need = cat.level * 50;
            game.Notify(cat.name + " 升到 Lv." + cat.level + "!", CatGameUITheme.Success);
        }
    }

    public void UpdateGrowth(Cat cat) {
        int ageTicks = Save.totalTicks - cat.birthTick;
        if (cat.age == "kitten" && ageTicks >= GameConfig.KITTEN_HOURS) {
            cat.age = "adult";
            game.Notify(cat.name + " 长大了!", CatGameUITheme.Info);
        } else if (cat.age == "adult" && ageTicks >= GameConfig.SENIOR_HOURS) {
            cat.age = "senior";
            game.Notify(cat.name + " 步入老年", CatGameUITheme.Info);
        }
    }

    public (float hunger, float clean, float happy, float global, float playBonus, float happyRecovery, float healthRecovery) GetFurnEffects() {
        float h = 1, c = 1, hp = 1, g = 1, pb = 1, hr = 1, hcr = 0;
        foreach (string fid in Save.placedFurniture) {
            FurnitureData f = FurnitureData.Get(fid);
            if (f == null) continue;
            h *= f.hungerDecayMult;
            c *= f.cleanDecayMult;
            hp *= f.happinessDecayMult;
            g *= f.globalDecayMult;
            pb *= f.playBonus;
            hr *= f.happinessRecovery;
            hcr += f.healthRecovery;
        }
        return (h, c, hp, g, pb, hr, hcr);
    }
}
