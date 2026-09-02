using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

// コンパイルエラーになるのでビルド時にはEditor下フォルダに配置する
namespace PostProcessBuild
{
    /// <summary>
    /// ビルド完了後の記録（Editor 拡張）
    ///
    /// [注意] バージョンの採番はここではなく BuildVersionStamper（ビルド前）が行う。
    /// BuildDate.txt は Assets/Resources 配下にあり、ビルド時にプレイヤーへ焼き込まれる。
    /// ビルド後に書き換えると配布物の中身が 1 つ前のビルド番号のままになるため、
    /// 採番をビルド前へ移した。ここで書き戻すと二重採番になるので追加しないこと。
    /// </summary>
    public class ReleaseDateWhenBuild
    {
        [PostProcessBuild(1)]   // (N)は実行順序
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            Debug.Log($"[PostProcessBuild] ビルド完了 {target} : {path}"
                + $" / Version {PlayerSettings.bundleVersion}");
        }
    }
}
