using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// ファイル操作に関する汎用ユーティリティクラス
/// - プラットフォーム別エディタ起動
/// - 画像ファイル読み込み
/// </summary>
public static class FileOperationUtility
{
    // Editor Commands
    private const string EDITOR_WINDOWS = "notepad.exe";
    private const string EDITOR_MAC = "open";
    private const string EDITOR_MAC_ARGS = "-a TextEdit ";
    private const string EDITOR_LINUX = "xdg-open";
    
    // Error Messages
    private const string MSG_FILE_OPEN_ERROR = "ファイルを開けませんでした: ";
    private const string MSG_FILE_NOT_EXIST = "ファイルが存在しません: ";
    private const string MSG_UNSUPPORTED_PLATFORM = "Unsupported platform";
    
    /// <summary>
    /// プラットフォームに応じたテキストエディタでファイルを開く
    /// </summary>
    /// <param name="filepath">開くファイルのパス</param>
    public static void OpenFileInEditor(string filepath)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Process.Start(EDITOR_WINDOWS, filepath);
        }
        else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            Process.Start(EDITOR_MAC, EDITOR_MAC_ARGS + filepath);
        }
        else if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            Process.Start(EDITOR_LINUX, filepath);
        }
        else
        {
            Debug.LogError(MSG_UNSUPPORTED_PLATFORM);
        }
    }
    
    /// <summary>
    /// デフォルトのプログラムでファイルを開く
    /// </summary>
    /// <param name="filepath">開くファイルのパス</param>
    public static void OpenFileInDefaultProgram(string filepath)
    {
        if (File.Exists(filepath))
        {
            try
            {
                Process.Start(new ProcessStartInfo(filepath)
                {
                    UseShellExecute = true
                });
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError(MSG_FILE_OPEN_ERROR + filepath + "\nエラー: " + ex.Message);
                return;
            }
        }
        Debug.LogWarning(MSG_FILE_NOT_EXIST + filepath);
    }
    
    /// <summary>
    /// StreamingAssetsから画像ファイルを読み込んでSpriteを作成
    /// </summary>
    /// <param name="imagePath">画像ファイルのパス（相対パスまたは絶対パス）</param>
    /// <returns>読み込んだSprite、失敗時はnull</returns>
    public static Sprite LoadSpriteFromStreamingAssets(string imagePath)
    {
        // 拡張子がすでに含まれているか確認
        string fullPath = imagePath;
        if (Path.HasExtension(imagePath))
        {
            // 拡張子付きの場合はそのまま使用
            fullPath = LoadStreamingAsset.StageFilePath(imagePath);
        }
        
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"画像ファイルが見つかりません: {fullPath}");
            return null;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                return sprite;
            }
            else
            {
                Debug.LogWarning($"画像の読み込みに失敗しました: {fullPath}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"画像読み込みエラー: {fullPath}\n{ex.Message}");
            return null;
        }
    }
}
