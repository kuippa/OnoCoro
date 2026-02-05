using System.Collections.Generic;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 立て看板マネージャー
    /// 
    /// YAML の boards セクションで座標が指定されている立て看板を
    /// 動的に生成し、SignboardCtrl コンポーネントを設定します。
    /// </summary>
    internal static class SignboardManager
    {
        private const string PARENT_CONTAINER_NAME = "gameeventobj";
        
        /// <summary>
        /// 座標付き立て看板を生成します
        /// YAML の boards セクションで pos が指定されているもののみを処理
        /// </summary>
        internal static void CreateSignboards(Dictionary<string, (string text, Vector3 pos)> signboardData)
        {
            if (signboardData == null || signboardData.Count == 0)
            {
                return;
            }
            
            Transform parentTransform = GetOrCreateParentContainer();
            if (parentTransform == null)
            {
                Debug.LogWarning("[SignboardManager.CreateSignboards] Parent container creation failed");
                return;
            }
            
            GameObject signboardPrefab = PrefabManager.SignboardPrefab;
            if (signboardPrefab == null)
            {
                Debug.LogWarning("[SignboardManager.CreateSignboards] Signboard prefab not found");
                return;
            }
            
            foreach (KeyValuePair<string, (string text, Vector3 pos)> entry in signboardData)
            {
                CreateSignboardInstance(signboardPrefab, entry.Key, entry.Value.text, 
                                       entry.Value.pos, parentTransform);
            }
        }
        
        /// <summary>
        /// 立て看板インスタンスを生成します
        /// </summary>
        private static void CreateSignboardInstance(GameObject prefab, string code, string text, 
                                                   Vector3 position, Transform parentTransform)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[SignboardManager.CreateSignboardInstance] Prefab is null");
                return;
            }
            
            // SetActive(false) でインスタンス化（Awake() 実行を遅延）
            GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity, parentTransform);
            if (instance == null)
            {
                Debug.LogWarning($"[SignboardManager.CreateSignboardInstance] Instantiate failed for code: {code}");
                return;
            }
            
            instance.SetActive(false);
            instance.name = $"Signboard_{code}";
            
            SignboardCtrl signboardCtrl = instance.GetComponent<SignboardCtrl>();
            if (signboardCtrl == null)
            {
                Debug.LogWarning($"[SignboardManager.CreateSignboardInstance] SignboardCtrl not found for code: {code}");
                Object.Destroy(instance);
                return;
            }
            
            // Awake 実行前にセットアップ
            signboardCtrl.SetupSignboard(code, text);
            
            // アクティブ化（Awake / Start 実行）
            instance.SetActive(true);
        }
        
        /// <summary>
        /// 立て看板の親コンテナを取得または作成します
        /// </summary>
        private static Transform GetOrCreateParentContainer()
        {
            GameObject container = GameObject.Find(PARENT_CONTAINER_NAME);
            
            if (container == null)
            {
                container = new GameObject(PARENT_CONTAINER_NAME);
                if (container == null)
                {
                    return null;
                }
            }
            
            return container.transform;
        }
    }
}
