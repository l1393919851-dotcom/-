using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 繁殖逻辑。
/// </summary>
public class CatBreedingController {
    readonly CatGameMain game;

    public CatBreedingController(CatGameMain game) {
        this.game = game;
    }

    GameSave Save => game.save;
    CatGameSaveService Store => game.SaveService;

    public void StartBreeding() {
        if (string.IsNullOrEmpty(Save.breedSel1) || string.IsNullOrEmpty(Save.breedSel2)) {
            game.Notify("请选两只猫", CatGameUITheme.Warn);
            return;
        }
        Cat p1 = Save.cats.Find(c => c.id == Save.breedSel1);
        Cat p2 = Save.cats.Find(c => c.id == Save.breedSel2);
        if (p1 == null || p2 == null) return;
        if (p1.age != "adult" || p2.age != "adult") { game.Notify("只有成年猫能繁殖", CatGameUITheme.Warn); return; }
        if (p1.breedCooldown > 0 || p2.breedCooldown > 0) { game.Notify("猫咪冷却中", CatGameUITheme.Warn); return; }
        if (Save.coins < GameConfig.BREED_COST) { game.Notify("金币不足!", CatGameUITheme.Danger); return; }
        if (Save.cats.Count >= GameConfig.MAX_CATS) { game.Notify("猫舍已满!", CatGameUITheme.Danger); return; }

        Save.coins -= GameConfig.BREED_COST;
        Save.breeding = new BreedingState {
            parent1 = p1.id, parent2 = p2.id, breed1 = p1.breedId, breed2 = p2.breedId,
            remaining = GameConfig.BREED_TIME,
        };
        p1.breedCooldown = GameConfig.BREED_COOLDOWN;
        p2.breedCooldown = GameConfig.BREED_COOLDOWN;
        game.Notify(p1.name + " 和 " + p2.name + " 开始繁殖...", CatGameUITheme.Info);
        game.CloseModal();
        Store.Persist();
        game.RefreshAll();
    }

    public void CompleteBreeding() {
        if (Save.breeding == null) return;
        string kittenBreed;
        if (UnityEngine.Random.value < 0.1f) {
            string[] upgrade = { "rare", "epic", "legendary", "legendary" };
            string b1Rar = BreedData.Get(Save.breeding.breed1).rarity.ToString();
            int idx = Array.IndexOf(new string[] { "Common", "Rare", "Epic", "Legendary" }, b1Rar);
            string targetRar = upgrade[Mathf.Clamp(idx, 0, 3)];
            List<BreedData> candidates = BreedData.All.FindAll(b => b.rarity.ToString() == targetRar);
            kittenBreed = candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)].id : Save.breeding.breed1;
            game.Notify("变异! 诞生了稀有品种!", CatGameUITheme.Success);
        } else {
            kittenBreed = UnityEngine.Random.value < 0.5f ? Save.breeding.breed1 : Save.breeding.breed2;
        }
        Cat kitten = Store.CreateCat(kittenBreed);
        kitten.hunger = 60;
        kitten.cleanliness = 60;
        kitten.happiness = 80;
        kitten.health = 100;
        kitten.affection = 30;
        Save.cats.Add(kitten);
        Save.stats.totalBred++;
        game.Meta.UnlockCatdex(kittenBreed);
        game.Notify(kitten.name + " 出生了!", CatGameUITheme.Success);
        Save.breeding = null;
        game.Meta.CheckAchievements();
        Store.Persist();
        game.RefreshAll();
    }
}
