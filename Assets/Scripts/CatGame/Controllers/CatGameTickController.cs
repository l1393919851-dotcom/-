using System.Collections;
using UnityEngine;

/// <summary>
/// 游戏时间推进 Tick。
/// </summary>
public class CatGameTickController {
    readonly CatGameMain game;

    public CatGameTickController(CatGameMain game) {
        this.game = game;
    }

    GameSave Save => game.save;
    CatGameSaveService Store => game.SaveService;

    public IEnumerator GameLoop() {
        while (true) {
            yield return new WaitForSeconds(GameConfig.TICK_INTERVAL);
            Tick();
        }
    }

    public void Tick() {
        Save.totalTicks++;
        Save.hour++;
        var fx = game.Care.GetFurnEffects();

        foreach (Cat cat in Save.cats) {
            game.Care.UpdateGrowth(cat);
            float ageMult = cat.age == "senior" ? 1.3f : cat.age == "kitten" ? 0.8f : 1f;

            cat.hunger = Mathf.Clamp(cat.hunger - GameConfig.DECAY[0] * fx.hunger * fx.global * ageMult, 0, 100);
            cat.cleanliness = Mathf.Clamp(cat.cleanliness - GameConfig.DECAY[1] * fx.clean * fx.global * ageMult, 0, 100);

            float happyChange = -GameConfig.DECAY[2] * fx.happy * fx.global * ageMult;
            if (cat.hunger < 30) happyChange -= 0.5f;
            if (cat.cleanliness < 30) happyChange -= 0.5f;
            if (cat.hunger > 70 && cat.cleanliness > 70) happyChange += 0.3f * fx.happyRecovery;
            cat.happiness = Mathf.Clamp(cat.happiness + happyChange, 0, 100);

            if (fx.healthRecovery > 0 && !cat.isSick && cat.health < 100)
                cat.health = Mathf.Clamp(cat.health + fx.healthRecovery, 0, 100);
            if (cat.hunger < 15) cat.health = Mathf.Clamp(cat.health - 0.5f, 0, 100);
            if (cat.cleanliness < 15) cat.health = Mathf.Clamp(cat.health - 0.3f, 0, 100);
            if (cat.happiness < 15) cat.health = Mathf.Clamp(cat.health - 0.2f, 0, 100);

            if (cat.breedCooldown > 0) cat.breedCooldown--;
            cat.sceneX = Mathf.Clamp(cat.sceneX + Random.Range(-2f, 2f), 10f, 90f);
            cat.sceneY = Mathf.Clamp(cat.sceneY + Random.Range(-1.5f, 1.5f), 45f, 85f);
        }

        if (Save.hour == 8) game.Care.CheckSickness();
        if (Save.breeding != null) {
            Save.breeding.remaining--;
            if (Save.breeding.remaining <= 0) game.Breeding.CompleteBreeding();
        }

        if (Save.hour >= GameConfig.HOURS_PER_DAY) {
            Save.hour = 0;
            Save.day++;
            Save.shopCats = game.Shop.GenerateShopCats();
            Save.shopRefreshDay = Save.day;
            game.Meta.GenerateDailyTasks();
            Save.catShowUsed = false;
            game.Meta.CheckDailyCheckin();
            game.Meta.CheckAchievements();
        }

        Store.Persist();
        game.RefreshAll();
    }
}
