using UnityEngine;
using System.Collections.Generic;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// ステージオーナメント（装飾オブジェクト）管理システム
    /// 
    /// 責務：
    /// - TreeSakura など舞台装飾要素を一元管理
    /// - イベントから指定されたオーナメントを制御
    /// - 同名のオブジェクト複数対応
    /// </summary>
    internal class OrnamentSystem : MonoBehaviour
    {
        /// <summary>
        /// 桜の木を咲かせる（複数対応）
        /// 
        /// 使用方法（YAML イベント）:
        /// - event: bloom_sakura
        ///   value: TreeSakura, TreeSakura_01
        /// 
        /// または単体：
        /// - event: bloom_sakura
        ///   value: TreeSakura
        /// 
        /// 同名のオブジェクトが複数ある場合、すべてに処理を適用
        /// </summary>
        internal void BloomSakura(string objectNames)
        {
            if (string.IsNullOrWhiteSpace(objectNames))
            {
                Debug.LogWarning("[OrnamentSystem.BloomSakura] オブジェクト名が空です");
                return;
            }

            string[] names = objectNames.Split(',');

            foreach (string name in names)
            {
                string trimmedName = name.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedName))
                {
                    continue;
                }

                ProcessTreeSakura(trimmedName);
            }
        }

        /// <summary>
        /// 単一の TreeSakura を処理（同名のオブジェクト複数対応）
        /// </summary>
        private void ProcessTreeSakura(string objectName)
        {
            TreeSakura[] allTreeSakuras = Object.FindObjectsByType<TreeSakura>(FindObjectsSortMode.None);
            
            List<TreeSakura> matchedTrees = new List<TreeSakura>();
            
            foreach (TreeSakura tree in allTreeSakuras)
            {
                if (tree.gameObject.name == objectName)
                {
                    matchedTrees.Add(tree);
                }
            }

            if (matchedTrees.Count == 0)
            {
                Debug.LogWarning($"[OrnamentSystem.ProcessTreeSakura] '{objectName}' という名前のオブジェクトが見つかりません");
                return;
            }

            foreach (TreeSakura tree in matchedTrees)
            {
                ApplyTreeSakura(tree);
            }
        }

        /// <summary>
        /// 単一オブジェクトに TreeSakura を適用
        /// </summary>
        private void ApplyTreeSakura(TreeSakura treeSakura)
        {
            if (treeSakura == null)
            {
                return;
            }

            treeSakura.SetMatToOriginal();
            Debug.Log($"[OrnamentSystem.ApplyTreeSakura] '{treeSakura.gameObject.name}' を咲かせました");
        }
    }
}
