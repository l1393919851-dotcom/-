using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店与领养/送养。
/// </summary>
public class CatShopController {
    readonly CatGameMain game;

    public CatShopController(CatGameMain game) {
        this.game = game;
    }

    GameSave Save => game.save;
    CatGameSaveService Store => game.SaveService;

    public void AdoptCat(int index) {
        if (index < 0 || index >= Save.shopCats.Count) return;
        ShopCatEntry sc = Save.shopCats[index];
        if (Save.cats.Count >= GameConfig.MAX_CATS) { game.Notify("猫舍已满!", CatGameUITheme.Warn); return; }
        if (Save.coins < sc.price) { game.Notify("金币不足!", CatGameUITheme.Danger); return; }
        Save.coins -= sc.price;
        Cat cat = Store.CreateCat(sc.breedId);
        Save.cats.Add(cat);
        Save.stats.totalAdopted++;
        game.Meta.UnlockCatdex(sc.breedId);
        Save.shopCats.RemoveAt(index);
        game.Notify("领养成功! " + cat.name + " 加入猫舍", CatGameUITheme.Success);
        game.Meta.CheckAchievements();
        Store.Persist();
        game.RefreshAll();
        game.CloseModal();
    }

    public void ConfirmSell(string catId) {
        Cat cat = Save.cats.Find(c => c.id == catId);
        if (cat == null) return;
        BreedData breed = BreedData.Get(cat.breedId);
        int price = Mathf.FloorToInt(breed.price * 0.5f * (1 + cat.level * 0.05f));
        Save.coins += price;
        Save.cats.Remove(cat);
        if (Save.selectedCatId == catId) Save.selectedCatId = null;
        game.Notify("送养 " + cat.name + ", 获得 " + price + " 金币", CatGameUITheme.Info);
        game.Meta.CheckAchievements();
        Store.Persist();
        game.RefreshAll();
        game.CloseModal();
    }

    public void BuyFurniture(string id) {
        FurnitureData f = FurnitureData.Get(id);
        if (f == null) return;
        if (Save.coins < f.price) { game.Notify("金币不足!", CatGameUITheme.Danger); return; }
        if (Save.placedFurniture.Contains(id)) { game.Notify("已拥有", CatGameUITheme.Warn); return; }
        Save.coins -= f.price;
        Save.placedFurniture.Add(id);
        game.Notify("购买 " + f.name + "! " + f.desc, CatGameUITheme.Success);
        Store.Persist();
        game.RefreshAll();
        game.Modal.OpenShop("furniture");
    }

    public void BuyFood(string id) {
        FoodData f = FoodData.Get(id);
        if (f == null) return;
        if (Save.coins < f.price) { game.Notify("金币不足!", CatGameUITheme.Danger); return; }
        Save.coins -= f.price;
        Store.AddInv("food", id, 1);
        game.Notify("购买 " + f.name, CatGameUITheme.Success);
        Store.Persist();
        game.RefreshAll();
        game.Modal.OpenShop("food");
    }

    public void BuyToy(string id) {
        ToyData t = ToyData.Get(id);
        if (t == null) return;
        if (Save.coins < t.price) { game.Notify("金币不足!", CatGameUITheme.Danger); return; }
        Save.coins -= t.price;
        Store.AddInv("toy", id, 1);
        game.Notify("购买 " + t.name, CatGameUITheme.Success);
        Store.Persist();
        game.RefreshAll();
        game.Modal.OpenShop("toy");
    }

    public List<ShopCatEntry> GenerateShopCats() {
        int count = Random.Range(3, 6);
        List<ShopCatEntry> cats = new List<ShopCatEntry>();
        for (int i = 0; i < count; i++) {
            float r = Random.value;
            string targetRar;
            if (Save.day < 3) targetRar = r < 0.7f ? "Common" : r < 0.95f ? "Rare" : "Epic";
            else if (Save.day < 7) targetRar = r < 0.5f ? "Common" : r < 0.85f ? "Rare" : r < 0.98f ? "Epic" : "Legendary";
            else targetRar = r < 0.35f ? "Common" : r < 0.7f ? "Rare" : r < 0.95f ? "Epic" : "Legendary";
            List<BreedData> candidates = BreedData.All.FindAll(b => b.rarity.ToString() == targetRar);
            BreedData breed = candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : BreedData.All[0];
            cats.Add(new ShopCatEntry(breed.id, breed.price));
        }
        return cats;
    }

    public void RefreshShopWithGems() {
        if (Save.gems < 50) { game.Notify("钻石不足!", CatGameUITheme.Danger); return; }
        Save.gems -= 50;
        Save.shopCats = GenerateShopCats();
        Store.Persist();
        game.RefreshAll();
        game.Modal.OpenShop("cats");
    }
}
