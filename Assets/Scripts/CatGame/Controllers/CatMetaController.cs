using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 图鉴、成就、每日任务、签到。
/// </summary>
public class CatMetaController {
    readonly CatGameMain game;

    public CatMetaController(CatGameMain game) {
        this.game = game;
    }

    GameSave Save => game.save;
    CatGameSaveService Store => game.SaveService;

    public void UnlockCatdex(string breedId) {
        if (!Save.catdex.Contains(breedId)) {
            Save.catdex.Add(breedId);
            BreedData b = BreedData.Get(breedId);
            game.Notify("图鉴解锁: " + b.name, CatGameUITheme.Info);
        }
    }

    public void CheckAchievements() {
        foreach (AchievementData ach in AchievementData.All) {
            if (Save.achievements.Contains(ach.id)) continue;
            bool ok = false;
            switch (ach.id) {
                case "first_adopt":     ok = Save.stats.totalAdopted >= 1; break;
                case "cat_collector":   ok = Save.cats.Count >= 5; break;
                case "full_house":      ok = Save.cats.Count >= 12; break;
                case "breed_collector": ok = Save.catdex.Count >= 5; break;
                case "dex_master":      ok = Save.catdex.Count >= BreedData.All.Count; break;
                case "show_champ":      ok = Save.stats.showWins >= 1; break;
                case "show_master":     ok = Save.stats.showWins >= 5; break;
                case "breed_newbie":    ok = Save.stats.totalBred >= 1; break;
                case "care_expert":     ok = Save.stats.totalFeed >= 50; break;
                case "clean_master":    ok = Save.stats.totalClean >= 30; break;
                case "cat_friend":      ok = Save.cats.Sum(c => c.affection) >= 500; break;
                case "rich":            ok = Save.coins >= 10000; break;
                case "legend":          ok = Save.day >= 7; break;
            }
            if (ok) {
                Save.achievements.Add(ach.id);
                Save.coins += ach.reward;
                game.Notify("成就解锁: " + ach.name + " (+" + ach.reward + "G)", CatGameUITheme.Success);
            }
        }
    }

    public void GenerateDailyTasks() {
        Save.dailyTasks.Clear();
        Save.dailyTaskDay = Save.day;
        List<DailyTaskData> shuffled = DailyTaskData.Templates.OrderBy(x => Random.value).Take(3).ToList();
        foreach (var t in shuffled) {
            Save.dailyTasks.Add(new TaskProgress {
                id = t.id, icon = t.icon, name = t.name, type = t.type,
                target = t.target, reward = t.reward, progress = 0, claimed = false,
            });
        }
    }

    public void UpdateDailyTask(string type) {
        foreach (TaskProgress t in Save.dailyTasks) {
            if (t.type == type && !t.claimed) {
                t.progress = Mathf.Min(t.progress + 1, t.target);
                if (t.progress >= t.target) game.Notify("任务完成: " + t.name, CatGameUITheme.Success);
            }
        }
    }

    public void ClaimTask(string taskId) {
        TaskProgress t = Save.dailyTasks.Find(x => x.id == taskId);
        if (t == null || t.claimed || t.progress < t.target) return;
        t.claimed = true;
        Save.coins += t.reward;
        game.Notify("领取奖励: " + t.reward + " 金币", CatGameUITheme.Success);
        Store.Persist();
        game.RefreshAll();
        game.Modal.OpenTasks();
    }

    public void CheckDailyCheckin() {
        if (Save.lastLoginDay != Save.day) {
            Save.lastLoginDay = Save.day;
            int reward = 50 + Save.day * 10;
            Save.coins += reward;
            game.Notify("每日签到: " + reward + " 金币", CatGameUITheme.Success);
        }
    }
}
