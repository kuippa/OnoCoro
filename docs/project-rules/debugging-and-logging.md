---
title: デバッグとログ確認ガイド
description: Unity Editor のログ取得方法、エラー確認、トラブルシューティング手法
---

# デバッグとログ確認ガイド

OnoCoro 開発時にエラーが発生したときの、ログ確認方法と診断手順を説明します。

---

## Unity Editor ログ取得方法

### ログファイルの場所

Windows では以下の場所に保存されます：

```
%LOCALAPPDATA%\Unity\Editor\Editor.log
```

PowerShell での パス例：
```powershell
$env:LOCALAPPDATA\Unity\Editor\Editor.log
```

### ログファイル情報確認

ファイルサイズ、最終更新時刻を確認：

```powershell
$logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"
if (Test-Path $logPath) {
    $file = Get-Item $logPath
    "ファイル: $($file.Name)"
    "サイズ: $($file.Length) bytes ($([math]::Round($file.Length / 1MB, 2)) MB)"
    "最終更新: $($file.LastWriteTime)"
} else {
    "ログファイルが見つかりません"
}
```

### ログの最新行を取得

PowerShell でログの最新 100-200 行を確認：

```powershell
# 最新 100 行を表示
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 100

# 最新 200 行をテキストファイルに保存
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 200 | 
    Out-File -Encoding UTF8 "C:\temp\unity_log_latest.txt"
```

---

## Unity Editor コンソールから確認

### GUI での確認方法

1. **Unity Editor を開く**
2. **Window > General > Console** を選択
3. コンソール内でエラー、警告、ログを確認
4. **Open Editor Log** ボタンからログファイルを直接開く

### コンソール出力レベル

| レベル | 表示 | 説明 |
|--------|------|------|
| **Log** | 白 | 通常ログ（`Debug.Log()` など） |
| **Warning** | 黄 | 警告（`Debug.LogWarning()` など） |
| **Error** | 赤 | エラー（`Debug.LogError()` など） |
| **Exception** | 赤 | 例外スタックトレース |

---

## よくあるエラーと確認方法

### [OK] コンパイルエラー

**症状**: C# コード実装後にコンパイルエラー

**確認手順**:
1. VS Code の **Problems パネル** を開く (`Ctrl + Shift + M`)
2. Unity Editor の **Console** を確認
3. ログファイルから `error CS` を検索

```powershell
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 300 | 
    Select-String "error CS"
```

### [OK] Null Reference Exception

**症状**: `NullReferenceException` が発生

**確認手順**:
1. コンソールのスタックトレースを確認
2. 発生行のコード前後をチェック
3. `GetComponent()`, `Find()` 結果が null か確認

```csharp
// [NG] null チェックなし
Transform child = transform.Find("ChildObject");
child.localScale = Vector3.one;  // null なら例外

// [OK] null チェック必須
Transform child = transform.Find("ChildObject");
if (child == null) {
    Debug.LogWarning("ChildObject not found");
    return;
}
child.localScale = Vector3.one;
```

### [OK] アセットロード失敗

**症状**: Prefab, Texture, Script Asset が見つからない

**確認手順**:
1. `Assets/` パスが正しいか確認
2. ファイル名・拡張子をチェック
3. ログで "not found" を検索

```powershell
Get-Content "$env:LOCALAPPDATA\Unity\Editor\Editor.log" -Tail 200 | 
    Select-String "not found", "cannot find"
```

---

## ログファイルのクリア

デバッグ後、ログファイルをリセット：

```powershell
# ログファイルをクリア（内容を削除）
$logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"
Clear-Content $logPath

# または削除して新規作成
Remove-Item $logPath -Force
# 次回 Unity 起動時に自動作成される
```

---

## ログ取得の自動化

### PowerShell スクリプト例

ログの最新行を自動インポート＆表示：

```powershell
# 関数: Unity ログの最新 N 行を表示
function Get-UnityLog {
    param(
        [int]$Lines = 100,
        [string]$FilterKeyword = ""
    )
    
    $logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"
    
    if (-not (Test-Path $logPath)) {
        Write-Host "ログファイルが見つかりません: $logPath"
        return
    }
    
    $content = Get-Content $logPath -Tail $Lines
    
    if ($FilterKeyword) {
        $content = $content | Select-String $FilterKeyword
    }
    
    return $content
}

# 使用例
Get-UnityLog -Lines 150                    # 最新 150 行表示
Get-UnityLog -Lines 200 -FilterKeyword "error"  # エラーのみ表示
```

---

## トラブルシューティングチェックリスト

エラー発生時の確認順序：

- [OK] Unity Editor のコンソール確認
- [OK] ログファイルの最新行を取得
- [OK] エラーメッセージの行番号・ファイル名をメモ
- [OK] コード実装の null チェック・ブレース確認
- [OK] AGENTS.md・coding-csharp.md のルール確認
- [OK] Visual Studio / VS Code よりコードを確認
- [OK] 関連ファイルの更新がコンパイルに反映されているか確認
- [OK] Unity Editor のリフレッシュ/再起動（Cache リセット）

---

## 関連ドキュメント

- [AGENTS.md](../../AGENTS.md) - Null チェック・コード基準
- [coding-csharp.md](coding-csharp.md) - C# 実装ルール
- [unity-design-patterns.md](unity-design-patterns.md) - MonoBehaviour パターン

---

**最終更新**: 2026-02-27

