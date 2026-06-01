using System;

namespace Functional.ConfigTable
{
    /// <summary>
    /// 标记配置表类型及其资源路径。
    /// 
    /// 示例：
    ///   [Config("Config/Hero", ConfigFormat.Json)]
    ///   public class HeroConfig : IConfigRow { ... }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute : Attribute
    {
        public string Path { get; }
        public ConfigFormat Format { get; }

        public ConfigAttribute(string path, ConfigFormat format = ConfigFormat.Json)
        {
            Path = path;
            Format = format;
        }
    }

    /// <summary>配置表文件格式。</summary>
    public enum ConfigFormat
    {
        Json,
        Csv
    }
}
