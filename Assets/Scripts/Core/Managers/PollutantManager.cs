using UnityEngine;
using TMPro;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// フィールド上に散乱しているごみオブジェクト（TagType.Garbage）の数を表示するマネージャー。
    /// 
    /// 責務：
    ///   - 外部から与えられたごみ数（SetDisplayCount）を UI テキストに反映
    ///   - UI 表示（ActivateCountUI）・非表示（HideCountUI）の切り替え
    /// 
    /// NOTE: ごみ数の集計は StageGoalController が FindGameObjectsWithTag で行う。
    ///       本クラスは表示層の責務のみ持つ。
    ///       完全 static メソッド（Instance 不要）。
    /// </summary>
    internal static class PollutantManager
    {
        private const string _UI_GARBAGE_COUNT_OBJECT_NAME = "txtGarbageCount";
        private const string _UI_TEXT_FORMAT = "ごみ {0} 個";

        private static TextMeshProUGUI _garbageCountText = null;
        private static GameObject _garbageCountUIObject = null;

        /// <summary>
        /// PollutantManager を初期化する。
        /// EventLoader.Awake() で呼び出される。
        /// シーン遷移時にも呼ぶことで UI 参照をリフレッシュ。
        /// </summary>
        internal static void Initialize()
        {
            _garbageCountUIObject = GameObject.Find(_UI_GARBAGE_COUNT_OBJECT_NAME);
            if (_garbageCountUIObject == null)
            {
                Debug.LogWarning($"[PollutantManager] UI オブジェクト '{_UI_GARBAGE_COUNT_OBJECT_NAME}' が見つかりません");
                return;
            }

            _garbageCountText = _garbageCountUIObject.GetComponent<TextMeshProUGUI>();
            if (_garbageCountText == null)
            {
                Debug.LogWarning($"[PollutantManager] TextMeshProUGUI が '{_UI_GARBAGE_COUNT_OBJECT_NAME}' に存在しません");
                return;
            }
            SetDisplayCount(0); // 初期値は 0 に設定

            // ごみチェック判定があるステージでのみ表示するため、デフォルトは非アクティブ
            _garbageCountUIObject.SetActive(false);
        }

        /// <summary>
        /// ごみ数 UI を非表示状態にする。
        /// ステージ開始時にごみチェック判定がない場合に呼び出す。
        /// </summary>
        internal static void HideCountUI()
        {
            if (_garbageCountUIObject == null)
            {
                Debug.LogWarning("[PollutantManager] UI オブジェクトが未初期化です");
                return;
            }

            _garbageCountUIObject.SetActive(false);
        }

        /// <summary>
        /// ごみ数 UI を表示状態にする。
        /// StageGoalController がごみチェック判定を開始するタイミングで呼び出す。
        /// </summary>
        internal static void ActivateCountUI()
        {
            if (_garbageCountUIObject == null)
            {
                Debug.LogWarning("[PollutantManager] UI オブジェクトが未初期化です");
                return;
            }

            _garbageCountUIObject.SetActive(true);
        }

        /// <summary>
        /// ごみ数を受け取り、UI テキストを更新する。
        /// StageGoalController が FindGameObjectsWithTag で取得した件数を渡す。
        /// </summary>
        /// <param name="count">現在のフィールド上のごみ数</param>
        internal static void SetDisplayCount(int count)
        {
            if (_garbageCountText == null)
            {
                Debug.LogWarning("[PollutantManager] TextMeshProUGUI が未初期化です");
                return;
            }

            _garbageCountText.text = string.Format(_UI_TEXT_FORMAT, count);
        }
    }
}
