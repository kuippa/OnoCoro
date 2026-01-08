// TextMesh Pro Font Asset生成後に自動的にキャッシュをクリアし、Atlas Population Modeを適切に設定するエディタ拡張
//
// 【目的】
// Font Asset CreatorでFont Assetを生成・更新した際の問題を自動解決：
// 1. 古いキャッシュが残っていると新しく追加した文字（括弧など）が正しく表示されない
// 2. Atlas Population Modeが不適切だと文字が表示されない
//
// 【動作】
// - Font Asset (.asset) がインポート/更新されると自動実行
// - Atlas Population ModeをDynamicに設定（ランタイムで文字を動的生成）
// - TMP_FontAssetのキャッシュをクリア
// - コンソールにクリア完了メッセージを表示
//
// 【Atlas Population Modeについて】
// - Static: Font Asset作成時に指定した文字のみ（軽量だが柔軟性なし）
// - Dynamic: 必要な文字をランタイムで動的生成（重いが柔軟）
// - Dynamic OS: OSのフォントから動的生成（最も柔軟だが最も重い）
//
// 【配置場所】
// Assets/Editor/ フォルダ配下（エディタ拡張専用）

#if UNITY_EDITOR
using UnityEditor;
using TMPro;
using UnityEngine;

public class FontAssetPostProcessor : AssetPostprocessor
{
    // Font Asset更新時に自動的にDynamicモードに設定するか
    private const bool AUTO_SET_DYNAMIC_MODE = true;
    
    static void OnPostprocessAllAssets(
        string[] importedAssets, 
        string[] deletedAssets, 
        string[] movedAssets, 
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".asset"))
            {
                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (fontAsset != null)
                {
                    // Atlas Population Modeを確認・設定
                    if (AUTO_SET_DYNAMIC_MODE && fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
                    {
                        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                        EditorUtility.SetDirty(fontAsset);
                        Debug.Log($"[FontAssetPostProcessor] Set Dynamic mode for: {fontAsset.name}");
                    }
                    
                    // Font Assetのキャッシュをクリア
                    fontAsset.ClearFontAssetData(true);
                    
                    // アセットを保存
                    AssetDatabase.SaveAssets();
                    
                    Debug.Log($"[FontAssetPostProcessor] Cleared cache and updated: {fontAsset.name} (Mode: {fontAsset.atlasPopulationMode})");
                }
            }
        }
    }
}
#endif
