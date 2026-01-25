using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を管理するユーティリティクラス
/// - 遅延付きシーン読み込み
/// - 即座のシーン読み込み
/// </summary>
public static class SceneLoaderManager
{
    /// <summary>
    /// 指定した遅延後にシーンを読み込むコルーチン
    /// </summary>
    /// <param name="sceneName">読み込むシーン名</param>
    /// <param name="delaySeconds">遅延秒数</param>
    /// <returns>コルーチン</returns>
    public static IEnumerator LoadSceneWithDelay(string sceneName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 即座にシーンを読み込む
    /// </summary>
    /// <param name="sceneName">読み込むシーン名</param>
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 指定したロードモードでシーンを読み込む
    /// </summary>
    /// <param name="sceneName">読み込むシーン名</param>
    /// <param name="loadMode">ロードモード</param>
    public static void LoadScene(string sceneName, LoadSceneMode loadMode)
    {
        SceneManager.LoadScene(sceneName, loadMode);
    }
}
