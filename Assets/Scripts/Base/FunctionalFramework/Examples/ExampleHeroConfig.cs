using Functional.ConfigTable;

namespace Functional.Examples
{
    /// <summary>
    /// 示例配置表行。对应 Resources/Config/Hero.json 或 Hero.csv。
    /// </summary>
    [Config("Config/Hero", ConfigFormat.Json)]
    public class ExampleHeroConfig : IConfigRow
    {
        public int Id;
        public string Name;
        public int Hp;
        public float Speed;

        int IConfigRow.Id => Id;
    }
}
