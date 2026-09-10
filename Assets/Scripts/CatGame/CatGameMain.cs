using UnityEngine;

/// <summary>
/// 猫舍物语入口：组装 MVC，转发刷新/通知/存档。
/// </summary>
public class CatGameMain : MonoBehaviour {
    public static CatGameMain Instance;

    public GameSave save;

    public CatGameSaveService SaveService { get; private set; }
    public CatCareController Care { get; private set; }
    public CatShopController Shop { get; private set; }
    public CatBreedingController Breeding { get; private set; }
    public CatShowController Show { get; private set; }
    public CatMetaController Meta { get; private set; }
    public CatGameTickController TickCtrl { get; private set; }

    public CatGameUITheme Theme { get; private set; }
    public CatGameUIFactory UI { get; private set; }
    public CatGameHudView Hud { get; private set; }
    public CatGameModalView Modal { get; private set; }
    public CatGameNotifyView Notifier { get; private set; }

    void Awake() {
        Instance = this;
    }

    void Start() {
        Theme = new CatGameUITheme();
        Theme.Init();
        UI = new CatGameUIFactory(Theme);

        SaveService = new CatGameSaveService();
        save = SaveService.LoadOrCreate();

        Care = new CatCareController(this);
        Shop = new CatShopController(this);
        Breeding = new CatBreedingController(this);
        Show = new CatShowController(this);
        Meta = new CatMetaController(this);
        TickCtrl = new CatGameTickController(this);

        Hud = new CatGameHudView(this, UI, Theme);
        Modal = new CatGameModalView(this, UI);
        Notifier = new CatGameNotifyView(this, UI, Theme);

        GameObject modalOverlay;
        Transform modalContent;
        RectTransform notifLayer;
        Hud.BuildUI(out modalOverlay, out modalContent, out notifLayer);
        Modal.Bind(modalOverlay, modalContent);
        Notifier.Bind(notifLayer);

        if (save.shopCats == null || save.shopCats.Count == 0) {
            save.shopCats = Shop.GenerateShopCats();
            SaveService.Persist();
        }
        if (save.dailyTasks == null || save.dailyTasks.Count == 0)
            Meta.GenerateDailyTasks();

        RefreshAll();
        StartCoroutine(TickCtrl.GameLoop());
    }

    public void RefreshAll() {
        if (Hud != null) Hud.RefreshAll();
    }

    public void Notify(string msg, Color color) {
        if (Notifier != null) Notifier.Notify(msg, color);
    }

    public void CloseModal() {
        if (Modal != null) Modal.CloseModal();
    }

    public void SaveGame() {
        SaveService.Persist();
    }
}
