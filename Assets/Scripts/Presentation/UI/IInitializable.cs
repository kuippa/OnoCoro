using UnityEngine;

/// <summary>
/// 初期化管理インターフェース
/// InitializationManager で動的に検出されるための標準インターフェース
/// 
/// 実装することで、GamePrefabs にアタッチされたコンポーネントの初期化を
/// 自動的に追跡・管理できるようになります。
/// 
/// 使用方法:
/// public class MyCtrl : UIControllerBase // UIControllerBase が実装
/// {
///     // IsInitialized と GetComponentName() が自動継承される
/// }
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// 初期化が完了したかどうか
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// コンポーネント名を取得（ログ出力用）
    /// デバッグ時に何のコンポーネントが初期化されたか確認できます。
    /// </summary>
    string GetComponentName();
}
