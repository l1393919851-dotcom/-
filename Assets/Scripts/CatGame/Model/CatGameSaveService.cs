using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档读写、猫咪创建、背包操作。
/// </summary>
public class CatGameSaveService {
    const string SaveKey = "CatGameSave";

    public GameSave Save { get; private set; }

    public GameSave LoadOrCreate() {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (!string.IsNullOrEmpty(json)) {
            try {
                Save = JsonUtility.FromJson<GameSave>(json);
                if (Save == null || Save.cats == null) Save = NewSave();
            } catch {
                Save = NewSave();
            }
        } else {
            Save = NewSave();
            Save.cats.Add(CreateCat("orange"));
            Save.cats.Add(CreateCat("white"));
            Save.catdex.Add("orange");
            Save.catdex.Add("white");
            SetInv("food", "kibble", 3);
            SetInv("food", "can", 2);
            SetInv("toy", "yarn", 1);
            Persist();
        }

        Save.breedSel1 = null;
        Save.breedSel2 = null;
        return Save;
    }

    public void Persist() {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Save));
        PlayerPrefs.Save();
    }

    public GameSave NewSave() {
        return new GameSave {
            coins = GameConfig.START_COINS,
            gems = GameConfig.START_GEMS,
            day = 1,
            hour = 8,
            cats = new List<Cat>(),
            foodInv = new List<StringIntPair>(),
            toyInv = new List<StringIntPair>(),
            placedFurniture = new List<string>(),
            achievements = new List<string>(),
            catdex = new List<string>(),
            dailyTasks = new List<TaskProgress>(),
            stats = new GameStats(),
            shopCats = new List<ShopCatEntry>(),
        };
    }

    public Cat CreateCat(string breedId) {
        BreedData breed = BreedData.Get(breedId);
        if (breed == null) return null;
        return new Cat {
            id = "cat_" + System.DateTime.Now.Ticks + "_" + Random.Range(0, 99999),
            name = GameConfig.CAT_NAMES[Random.Range(0, GameConfig.CAT_NAMES.Length)],
            breedId = breedId,
            rarity = breed.rarity.ToString(),
            age = "kitten",
            birthTick = Save != null ? Save.totalTicks : 0,
            personality = breed.personality.ToString(),
            sceneX = Random.Range(20f, 80f),
            sceneY = Random.Range(55f, 80f),
            floatOffset = Random.Range(0f, 3f),
        };
    }

    public int GetInv(string type, string id) {
        var list = type == "food" ? Save.foodInv : Save.toyInv;
        var pair = list.Find(p => p.key == id);
        return pair != null ? pair.value : 0;
    }

    public void SetInv(string type, string id, int count) {
        var list = type == "food" ? Save.foodInv : Save.toyInv;
        var pair = list.Find(p => p.key == id);
        if (pair != null) pair.value = count;
        else list.Add(new StringIntPair(id, count));
    }

    public void AddInv(string type, string id, int delta) {
        var list = type == "food" ? Save.foodInv : Save.toyInv;
        var pair = list.Find(p => p.key == id);
        if (pair != null) pair.value += delta;
        else if (delta > 0) list.Add(new StringIntPair(id, delta));
    }
}
