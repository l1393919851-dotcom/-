using System;
using UnityEngine;

public struct CatShowResult {
    public Cat cat;
    public int score;
    public int rank;
    public int reward;
    public int[] opponents;
}

/// <summary>
/// 猫展比赛逻辑。
/// </summary>
public class CatShowController {
    readonly CatGameMain game;

    public CatShowController(CatGameMain game) {
        this.game = game;
    }

    GameSave Save => game.save;
    CatGameSaveService Store => game.SaveService;

    public void EnterCatShow(string catId) {
        Cat cat = Save.cats.Find(c => c.id == catId);
        if (cat == null) return;
        if (cat.isSick) { game.Notify("生病的猫不能参赛!", CatGameUITheme.Warn); return; }
        if (Save.catShowUsed && Save.catShowLastDay == Save.day) { game.Notify("今天已参加过", CatGameUITheme.Warn); return; }

        Rarity rar = (Rarity)Enum.Parse(typeof(Rarity), cat.rarity);
        float baseScore = (cat.level * 10 + cat.happiness + cat.health + cat.cleanliness + cat.affection) / 5f;
        int score = Mathf.FloorToInt(baseScore * GameConfig.RARITY_MULT[rar]);
        int[] opponents = new int[3];
        for (int i = 0; i < 3; i++)
            opponents[i] = Mathf.FloorToInt(UnityEngine.Random.Range(40f, 95f) * (0.8f + Save.day * 0.03f));

        int[] allScores = new int[] { score, opponents[0], opponents[1], opponents[2] };
        Array.Sort(allScores);
        Array.Reverse(allScores);
        int rank = Array.IndexOf(allScores, score) + 1;

        int reward = rank <= 3 ? GameConfig.SHOW_REWARDS[rank - 1] : 0;
        Save.coins += reward;
        Save.catShowUsed = true;
        Save.catShowLastDay = Save.day;
        if (rank == 1) Save.stats.showWins++;
        game.Care.AddExp(cat, 20);

        game.Modal.ShowCatShowResult(new CatShowResult {
            cat = cat, score = score, rank = rank, reward = reward, opponents = opponents
        });
        game.Meta.CheckAchievements();
        Store.Persist();
    }

    public int EstimateScore(Cat cat) {
        Rarity rar = (Rarity)Enum.Parse(typeof(Rarity), cat.rarity);
        float baseScore = (cat.level * 10 + cat.happiness + cat.health + cat.cleanliness + cat.affection) / 5f;
        return Mathf.FloorToInt(baseScore * GameConfig.RARITY_MULT[rar]);
    }
}
