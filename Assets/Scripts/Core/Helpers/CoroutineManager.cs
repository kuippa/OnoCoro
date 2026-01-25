using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// グローバルなコルーチン実行マネージャー
    /// Recovery フェーズにおいて、すべてのコルーチン実行を一元管理するためのシングルトンクラス
    /// 
    /// 使用例:
    ///   CoroutineManager.Instance.StartCoroutine(MyCoroutine());
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        private const string _INSTANCE_NAME = "CoroutineManager";
        private static CoroutineManager _instance;
        public static CoroutineManager Instance
        {
            get
            {
                return GetOrCreateInstance();
            }
        }

        // ========================
        // Methods
        // ========================
        /// <summary>
        /// インスタンスを取得、または作成します
        /// Recovery フェーズ: null チェックを明示的に実行
        /// </summary>
        private static CoroutineManager GetOrCreateInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            // インスタンスが null の場合、新規作成
            GameObject managerGameObject = new GameObject(_INSTANCE_NAME);
            if (managerGameObject == null)
            {
                return null;
            }

            _instance = managerGameObject.AddComponent<CoroutineManager>();
            if (_instance == null)
            {
                return null;
            }
            Object.DontDestroyOnLoad(managerGameObject);
            return _instance;
        }
    }
}
