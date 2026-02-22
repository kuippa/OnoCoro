using UnityEngine;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// TreeSakura 動作テスト用クラス
    /// 
    /// 責務：
    /// - OnTargetEnter で TreeSakura のマテリアルをトグル
    /// - 透明 ← → オリジナルのマテリアルを交互に切り替え
    /// 
    /// 使用方法：
    /// - TreeSakura コンポーネントと同じ GameObject にアタッチ
    /// - OnTargetEnter() メソッドを外部から呼び出す（例：DeleteUnitProcess）
    /// </summary>
    public class TreeSakuraTests : MonoBehaviour
    {
        [SerializeField]
        private TreeSakura _treeSakura = null;
        
        private bool _isInvisible = false;

        private void Start()
        {
            Debug.Log("[TreeSakuraTests] Start");
            if (_treeSakura == null)
            {
                Debug.LogWarning("[TreeSakuraTests] TreeSakura component not assigned");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// マテリアル状態をトグル
        /// 透明 ← → オリジナル
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("[TreeSakuraTests] OnTriggerEnter called");
            if (_treeSakura == null)
            {
                return;
            }

            if (_isInvisible)
            {
                _treeSakura.SetMatToOriginal();
                _isInvisible = false;
            }
            else
            {
                _treeSakura.SetMatToInvisible();
                _isInvisible = true;
            }
        }
    }
}
