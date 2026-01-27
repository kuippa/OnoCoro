using CommonsUtility;

namespace CommonsUtility
{
    /// <summary>
    /// YAML コマンド基底インターフェース
    /// 各コマンドはこれを実装して YAML から解析される
    /// </summary>
    internal interface IYamlCommand
    {
        /// <summary>コマンドタイプを取得</summary>
        YamlSectionType SectionType { get; }
        
        /// <summary>バリデーション実行</summary>
        ValidationResult Validate();
    }
}
