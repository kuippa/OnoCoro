using UnityEngine.EventSystems;

namespace CommonsUtility
{
    /// <summary>
    /// UI 操作用ユーティリティ
    /// 
    /// 責務:
    /// - EventSystem 関連の安全な操作
    /// - UI 要素の汎用的なヘルパー機能
    /// 
    /// 設計原則:
    /// - null チェックを含めて操作を保証
    /// - EventSystem が存在しない環境でも動作
    /// - MonoBehaviour に依存しない（static）
    /// </summary>
    internal static class UIUtility
    {
        /// <summary>
        /// EventSystem の選択状態をクリア
        /// 
        /// EventSystem が存在しない場合は何もしない（エラー出力なし）
        /// テスト環境やエディタ環境で安全に使用可能
        /// </summary>
        public static void ClearEventSystemSelection()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// EventSystem に選択オブジェクトを設定
        /// 
        /// EventSystem が存在しない場合は何もしない（エラー出力なし）
        /// </summary>
        /// <param name="selectedGameObject">選択対象の GameObject（null 可）</param>
        public static void SetEventSystemSelection(UnityEngine.GameObject selectedGameObject)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(selectedGameObject);
            }
        }
    }
}
