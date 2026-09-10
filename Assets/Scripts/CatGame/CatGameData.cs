using System;
using System.Collections.Generic;
using UnityEngine;

// ===== 枚举 =====
public enum Rarity { Common, Rare, Epic, Legendary }
public enum CatAge { Kitten, Adult, Senior }
public enum Personality { Playful, Lazy, Aloof, Clingy }

// ===== 品种数据 =====
[Serializable]
public class BreedData {
    public string id;
    public string name;
    public string shortName; // 用于UI显示的单字
    public string emoji;
    public Color color;
    public Rarity rarity;
    public int price;
    public Personality personality;

    public static List<BreedData> All = new List<BreedData> {
        new BreedData { id="orange",   name="橘猫",       shortName="橘", color=new Color(1f,0.55f,0f),     rarity=Rarity.Common,    price=100,   personality=Personality.Playful },
        new BreedData { id="black",    name="黑猫",       shortName="黑", color=new Color(0.2f,0.2f,0.2f),   rarity=Rarity.Common,    price=100,   personality=Personality.Aloof },
        new BreedData { id="white",    name="白猫",       shortName="白", color=new Color(0.96f,0.96f,0.96f),rarity=Rarity.Common,    price=100,   personality=Personality.Clingy },
        new BreedData { id="tabby",    name="狸花猫",     shortName="狸", color=new Color(0.55f,0.45f,0.33f),rarity=Rarity.Common,    price=150,   personality=Personality.Playful },
        new BreedData { id="siamese",  name="暹罗猫",     shortName="暹", color=new Color(0.72f,0.53f,0.04f),rarity=Rarity.Rare,      price=500,   personality=Personality.Aloof },
        new BreedData { id="ragdoll",  name="布偶猫",     shortName="布", color=new Color(0.68f,0.85f,0.9f), rarity=Rarity.Rare,      price=500,   personality=Personality.Clingy },
        new BreedData { id="british",  name="英短蓝猫",   shortName="英", color=new Color(0.39f,0.58f,0.93f),rarity=Rarity.Rare,      price=600,   personality=Personality.Lazy },
        new BreedData { id="amshort",  name="美短",       shortName="美", color=new Color(0.75f,0.75f,0.75f),rarity=Rarity.Rare,      price=550,   personality=Personality.Playful },
        new BreedData { id="fold",     name="苏格兰折耳", shortName="折", color=new Color(0.85f,0.65f,0.13f),rarity=Rarity.Epic,      price=1500,  personality=Personality.Lazy },
        new BreedData { id="bengal",   name="孟加拉豹猫", shortName="豹", color=new Color(1f,0.65f,0f),      rarity=Rarity.Epic,      price=1800,  personality=Personality.Playful },
        new BreedData { id="maine",    name="缅因猫",     shortName="缅", color=new Color(0.82f,0.41f,0.12f),rarity=Rarity.Epic,      price=2000,  personality=Personality.Aloof },
        new BreedData { id="legend",   name="传说猫仙",   shortName="仙", color=new Color(1f,0.84f,0f),      rarity=Rarity.Legendary, price=10000, personality=Personality.Aloof },
    };

    public static BreedData Get(string id) {
        return All.Find(b => b.id == id);
    }
}

// ===== 食物数据 =====
[Serializable]
public class FoodData {
    public string id, name, desc;
    public int price, hunger, happiness, health, affection;

    public static List<FoodData> All = new List<FoodData> {
        new FoodData { id="kibble",    name="猫粮",     desc="基础猫粮，填饱肚子",     price=20,  hunger=20, happiness=0,  health=0,  affection=0 },
        new FoodData { id="can",       name="猫罐头",   desc="美味罐头，猫猫最爱",     price=50,  hunger=40, happiness=5,  health=0,  affection=0 },
        new FoodData { id="fish",      name="小鱼干",   desc="提升好感度",             price=30,  hunger=30, happiness=0,  health=0,  affection=10 },
        new FoodData { id="premium",   name="高级猫粮", desc="营养均衡，恢复健康",     price=100, hunger=50, happiness=0,  health=5,  affection=0 },
        new FoodData { id="nutrition", name="营养餐",   desc="全面恢复各项属性",       price=150, hunger=40, happiness=10, health=10, affection=5 },
        new FoodData { id="catnip",    name="猫薄荷",   desc="让猫猫兴奋不已",         price=80,  hunger=0,  happiness=30, health=0,  affection=5 },
    };

    public static FoodData Get(string id) { return All.Find(f => f.id == id); }
}

// ===== 玩具数据 =====
[Serializable]
public class ToyData {
    public string id, name, desc;
    public int price, affection, exp, happiness, hungerCost;

    public static List<ToyData> All = new List<ToyData> {
        new ToyData { id="wand",   name="逗猫棒",   desc="经典互动玩具",   price=100, affection=15, exp=10, happiness=10, hungerCost=5 },
        new ToyData { id="yarn",   name="毛线球",   desc="猫猫自娱自乐",   price=50,  affection=10, exp=8,  happiness=5,  hungerCost=3 },
        new ToyData { id="laser",  name="激光笔",   desc="疯狂追逐光点",   price=200, affection=20, exp=15, happiness=15, hungerCost=8 },
        new ToyData { id="tunnel", name="猫隧道",   desc="钻来钻去超开心", price=300, affection=5,  exp=5,  happiness=20, hungerCost=4 },
        new ToyData { id="mouse",  name="电动老鼠", desc="高科技互动玩具", price=250, affection=15, exp=20, happiness=15, hungerCost=6 },
    };

    public static ToyData Get(string id) { return All.Find(t => t.id == id); }
}

// ===== 家具数据 =====
[Serializable]
public class FurnitureData {
    public string id, name, desc;
    public int price;
    public float hungerDecayMult = 1f, cleanDecayMult = 1f, happinessDecayMult = 1f, globalDecayMult = 1f, playBonus = 1f, happinessRecovery = 1f, healthRecovery = 0f;

    public static List<FurnitureData> All = new List<FurnitureData> {
        new FurnitureData { id="bed_basic",  name="基础猫窝",   desc="心情衰减-20%",    price=200,  happinessDecayMult=0.8f },
        new FurnitureData { id="bed_lux",    name="豪华猫窝",   desc="心情衰减-40%",    price=800,  happinessDecayMult=0.6f },
        new FurnitureData { id="bowl",       name="食盆",       desc="饱食衰减-15%",    price=150,  hungerDecayMult=0.85f },
        new FurnitureData { id="feeder",     name="自动喂食器", desc="饱食衰减-30%",    price=600,  hungerDecayMult=0.7f },
        new FurnitureData { id="litter",     name="猫砂盆",     desc="清洁衰减-20%",    price=200,  cleanDecayMult=0.8f },
        new FurnitureData { id="litter_lux", name="豪华猫砂盆", desc="清洁衰减-40%",    price=500,  cleanDecayMult=0.6f },
        new FurnitureData { id="tree",       name="猫爬架",     desc="玩耍效果+30%",    price=400,  playBonus=1.3f },
        new FurnitureData { id="scratcher",  name="猫抓板",     desc="心情恢复+10%",    price=100,  happinessRecovery=1.1f },
        new FurnitureData { id="ac",         name="空调",       desc="全属性衰减-10%",  price=1000, globalDecayMult=0.9f },
        new FurnitureData { id="garden",     name="猫花园",     desc="心情衰减-30%+健康恢复", price=1200, happinessDecayMult=0.7f, healthRecovery=0.5f },
    };

    public static FurnitureData Get(string id) { return All.Find(f => f.id == id); }
}

// ===== 成就数据 =====
[Serializable]
public class AchievementData {
    public string id, name, desc, icon;
    public int reward;

    public static List<AchievementData> All = new List<AchievementData> {
        new AchievementData { id="first_adopt",    name="初次领养",   icon="🏠", desc="领养第一只猫",         reward=100 },
        new AchievementData { id="cat_collector",  name="猫咪收藏家", icon="📚", desc="拥有5只猫",             reward=500 },
        new AchievementData { id="full_house",     name="猫舍满员",   icon="🏡", desc="拥有12只猫",            reward=1000 },
        new AchievementData { id="breed_collector",name="品种收集者", icon="🎨", desc="收集5种品种",           reward=500 },
        new AchievementData { id="dex_master",     name="图鉴大师",   icon="📖", desc="收集所有品种",          reward=5000 },
        new AchievementData { id="show_champ",     name="猫展冠军",   icon="🥇", desc="赢得猫展第一名",        reward=1000 },
        new AchievementData { id="show_master",    name="猫展大师",   icon="🏆", desc="赢得5次猫展",           reward=2000 },
        new AchievementData { id="breed_newbie",   name="繁殖新手",   icon="💕", desc="成功繁殖一次",          reward=300 },
        new AchievementData { id="care_expert",    name="照顾专家",   icon="🍖", desc="喂食50次",              reward=500 },
        new AchievementData { id="clean_master",   name="清洁达人",   icon="🧼", desc="清洁30次",              reward=300 },
        new AchievementData { id="cat_friend",     name="猫咪之友",   icon="❤", desc="总好感度达到500",       reward=1000 },
        new AchievementData { id="rich",           name="富甲一方",   icon="💰", desc="拥有10000金币",          reward=500 },
        new AchievementData { id="legend",         name="传奇猫舍",   icon="⭐", desc="坚持7天",               reward=2000 },
    };

    public static AchievementData Get(string id) { return All.Find(a => a.id == id); }
}

// ===== 每日任务 =====
[Serializable]
public class DailyTaskData {
    public string id, icon, name, type;
    public int target, reward;

    public static List<DailyTaskData> Templates = new List<DailyTaskData> {
        new DailyTaskData { id="feed3",    icon="🍖", name="喂食3次",   type="feed",    target=3, reward=50 },
        new DailyTaskData { id="clean2",   icon="🧼", name="清洁2次",   type="clean",   target=2, reward=50 },
        new DailyTaskData { id="play2",    icon="🎮", name="玩耍2次",   type="play",    target=2, reward=80 },
        new DailyTaskData { id="interact5",icon="✋", name="互动5次",   type="interact",target=5, reward=100 },
    };
}

// ===== 运行时猫咪实例 =====
[Serializable]
public class Cat {
    public string id;
    public string name;
    public string breedId;
    public string rarity;
    public int level = 1;
    public int exp = 0;
    public string age = "kitten";
    public int birthTick = 0;
    public float hunger = 75f;
    public float cleanliness = 75f;
    public float happiness = 75f;
    public float health = 100f;
    public float affection = 20f;
    public string personality;
    public bool isSick = false;
    public string sickness = null;
    public float sceneX = 50f;
    public float sceneY = 65f;
    public int breedCooldown = 0;
    public float floatOffset = 0f;
}

// ===== 存档辅助结构 =====
[Serializable]
public class StringIntPair {
    public string key;
    public int value;
    public StringIntPair(string k, int v) { key = k; value = v; }
    public StringIntPair() {}
}

[Serializable]
public class ShopCatEntry {
    public string breedId;
    public int price;
    public ShopCatEntry(string b, int p) { breedId = b; price = p; }
    public ShopCatEntry() {}
}

[Serializable]
public class TaskProgress {
    public string id, icon, name, type;
    public int target, reward, progress;
    public bool claimed;
}

[Serializable]
public class BreedingState {
    public string parent1, parent2, breed1, breed2;
    public int remaining;
}

[Serializable]
public class GameStats {
    public int totalAdopted = 0;
    public int totalBred = 0;
    public int showWins = 0;
    public int totalPlay = 0;
    public int totalFeed = 0;
    public int totalClean = 0;
}

// ===== 完整存档 =====
[Serializable]
public class GameSave {
    public int coins = 1000;
    public int gems = 10;
    public int day = 1;
    public int hour = 8;
    public int totalTicks = 0;
    public List<Cat> cats = new List<Cat>();
    public List<StringIntPair> foodInv = new List<StringIntPair>();
    public List<StringIntPair> toyInv = new List<StringIntPair>();
    public List<string> placedFurniture = new List<string>();
    public List<string> achievements = new List<string>();
    public List<string> catdex = new List<string>();
    public int lastLoginDay = 0;
    public int catShowLastDay = 0;
    public bool catShowUsed = false;
    public List<TaskProgress> dailyTasks = new List<TaskProgress>();
    public int dailyTaskDay = 0;
    public BreedingState breeding = null;
    public GameStats stats = new GameStats();
    public List<ShopCatEntry> shopCats = new List<ShopCatEntry>();
    public int shopRefreshDay = 0;
    public string selectedCatId = null;
    // 繁殖选择临时状态（不存档）
    [NonSerialized] public string breedSel1 = null;
    [NonSerialized] public string breedSel2 = null;
    [NonSerialized] public string currentShopTab = "cats";
}

// ===== 配置常量 =====
public static class GameConfig {
    public const float TICK_INTERVAL = 3f;       // 3秒=1游戏小时
    public const int HOURS_PER_DAY = 24;
    public const int START_COINS = 1000;
    public const int START_GEMS = 10;
    public const int MAX_CATS = 12;
    public static readonly float[] DECAY = { 1.5f, 1.2f, 0.8f }; // hunger, clean, happy
    public const int KITTEN_HOURS = 48;
    public const int SENIOR_HOURS = 168;
    public const int BREED_COST = 500;
    public const int BREED_TIME = 6;
    public const int BREED_COOLDOWN = 24;
    public static readonly int[] SHOW_REWARDS = { 500, 300, 100 };
    public const float SICK_THRESHOLD = 30f;
    public const float SICK_CHANCE = 0.15f;
    public const int HEAL_COST = 200;

    public static readonly Dictionary<Rarity, float> RARITY_MULT = new Dictionary<Rarity, float> {
        { Rarity.Common, 1f }, { Rarity.Rare, 1.2f }, { Rarity.Epic, 1.5f }, { Rarity.Legendary, 2f }
    };

    public static readonly Dictionary<Rarity, string> RARITY_NAMES = new Dictionary<Rarity, string> {
        { Rarity.Common, "普通" }, { Rarity.Rare, "稀有" }, { Rarity.Epic, "史诗" }, { Rarity.Legendary, "传说" }
    };

    public static readonly Dictionary<Personality, string> PERSONALITY_NAMES = new Dictionary<Personality, string> {
        { Personality.Playful, "活泼" }, { Personality.Lazy, "慵懒" }, { Personality.Aloof, "高冷" }, { Personality.Clingy, "粘人" }
    };

    public static readonly string[] CAT_NAMES = {
        "橘子","小黑","雪球","花花","咪咪","团子","奶昔","布丁","可可","芒果",
        "汤圆","麻薯","元宝","招财","星星","月亮","太阳","糯米","芝士","咖啡",
        "抹茶","草莓","蓝莓","桃子","西瓜","栗子","红薯","玉米","花生","豆豆"
    };
}
