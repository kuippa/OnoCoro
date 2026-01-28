using UnityEngine;
using CommonsUtility;
using System.Collections.Generic;

namespace CommonsUtility
{
    /// <summary>
    /// マテリアルリソース管理クラス
    /// Materials フォルダ配下のマテリアルをキャッシュして提供します
    /// </summary>
    internal static class MaterialManager
    {
        private static readonly Dictionary<string, Material> _materialCache = 
            new Dictionary<string, Material>();

        /// <summary>
        /// リソースパスからマテリアルを取得（キャッシュ機構あり）
        /// </summary>
        private static Material GetMaterial(string resourcePath)
        {
            if (!_materialCache.ContainsKey(resourcePath) || _materialCache[resourcePath] == null)
            {
                Material material = Resources.Load<Material>(resourcePath);
                if (material != null)
                {
                    _materialCache[resourcePath] = material;
                }
                else
                {
                    Debug.LogWarning($"[MaterialManager] Failed to load material: {resourcePath}");
                }
            }
            return _materialCache[resourcePath];
        }

        // プロパティ定義は 1 行で完結
        internal static Material BGGreen => GetMaterial(GlobalConst.MATERIAL_BG_GREEN_PATH);
        internal static Material BGRed => GetMaterial(GlobalConst.MATERIAL_BG_RED_PATH);
        internal static Material PlateauGenericWood => GetMaterial(GlobalConst.MATERIAL_PLATEAU_GENERIC_WOOD_PATH);
    }
}
