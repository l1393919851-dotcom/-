namespace Functional.ConfigTable
{
    /// <summary>
    /// 配置表行接口。所有配置行类型需实现此接口以便统一加载。
    /// </summary>
    public interface IConfigRow
    {
        /// <summary>配置主键 ID（用于字典索引）。</summary>
        int Id { get; }
    }
}
