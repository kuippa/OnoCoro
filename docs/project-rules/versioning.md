# OnoCoro バージョン管理規則

## 概要

OnoCoro はビルド時に **自動的にバージョン番号を発番** する仕組みを採用しています。正式なバージョン番号は `Assets/Resources/BuildDate.txt` に記録されます。

---

## バージョン番号の形式

### ゲーム内での表示形式

```
Version: [MAJOR].[MINOR].[BUILD].[YYYY.MM.DD.HH.mm].[BuildTarget]
```

**例**:
```
Version: 0.0.20.2026.03.09.01.34.StandaloneWindows64
```

**詳細**:
- 表示内容は `GetAppBuildDate()` メソッドが返す
- 内部的には `Utility.GetAppVersion()` が `BuildDate.txt` の5行を "." で結合
- BuildTarget (最後の値) は `PostProcessBuild.cs` の実行時にビルドターゲットを記録

### ファイル形式（BuildDate.txt）

```
0
0
20
2026.03.09.01.34
StandaloneWindows64
```

| 行 | 内容 | 説明 |
|----|------|------|
| 1 | `0` | MAJOR バージョン |
| 2 | `0` | MINOR バージョン |
| 3 | `20` | BUILD バージョン（ビルド回数） |
| 4 | `2026.03.09.01.34` | ビルド実行日時（YYYY.MM.DD.HH.mm） |
| 5 | `StandaloneWindows64` | ビルドターゲット |

---

## バージョン番号の自動発番

### 仕組み

**PostProcessBuild.cs** が Unity ビルド完了後に自動実行され：

1. 現在の `BuildDate.txt` から MAJOR / MINOR / BUILD を読み込む
2. **BUILD を +1 インクリメント**
3. ビルド実行日時を記録（`DateTime.Now.ToString("yyyy.MM.dd.HH.mm")`）
4. 更新した内容を `BuildDate.txt` に書き込み

```csharp
// PostProcessBuild.cs の処理流れ
[PostProcessBuild(1)]
public static void OnPostProcessBuild(BuildTarget target, string path)
{
    // BuildDate.txt から読み込む
    version_major = sr.ReadLine();  // 0
    version_minor = sr.ReadLine();  // 0
    version_build = sr.ReadLine();  // 20
    
    // BUILD をインクリメント
    version_build = (build + 1).ToString(); // 21
    
    // 新しい BUILD 日時を記録
    writeStr = DateTime.Now.ToString("yyyy.MM.dd.HH.mm");
    
    // BuildDate.txt に書き込み
    sw.WriteLine(version_major);     // 0
    sw.WriteLine(version_minor);     // 0
    sw.WriteLine(version_build);     // 21
    sw.WriteLine(writeStr);          // 2026.03.10.15.30
    sw.WriteLine(target);            // StandaloneWindows64
}
```

---

## リリース版バージョンの決定方法

### 開発フェーズ別バージョン

| フェーズ | MAJOR | MINOR | BUILD 範囲 | 説明 |
|---------|-------|-------|-----------|------|
| **Phase 0** | 0 | 0 | 1 - 10 | Recovery・初期構築 |
| **Phase 1** | 0 | 0 | 11 - 50 | コア機能整備 |
| **Phase 2** | 0 | 0 | 51 - 100 | ステージ設計・ゲーム性調整 |
| **Phase 3** | 0 | 0 | 101 - 150 | QA・最適化・リリース準備 |
| **Alpha 1** | 0 | 1 | 151 - 200 | テストユーザー向けリリース |
| **Beta 1** | 0 | 2 | 201 - 300 | 広範囲テスト向けリリース |
| **v1.0** | 1 | 0 | 301 + | 正式リリース |

### リリース前の戻し方（手動調整）

万が一ビルドを間違えた場合、`BuildDate.txt` を手動編集して BUILD を減らすことができます：

```diff
- 0
- 0
- 21
+ 0
+ 0
+ 20
  2026.03.09.01.34
  StandaloneWindows64
```

**注意**: 次のビルド時に新しい日時で上書きされるため、すぐに正しいバージョンでビルドしてください。

---

## ゲーム内での実装（UI 表示）

### DebugInfoCtrl.cs での表示

ゲーム内デバッグ画面に以下の形式で表示されます：

```
BuildDate: Version: 0.0.20.2026.03.09.01.34.StandaloneWindows64
Application: [Unity Application.version]
```

**取得メソッド**:
- `GameObjectTreat.GetAppBuildDate()` → BuildDate.txt の内容（"Version: ..." 形式）
- `GameObjectTreat.GetAppVersion()` → Unity の `Application.version`
- `Utility.GetAppVersion()` → BuildDate.txt を読んで "Version: " + 5行を "." で結合した文字列

### TitleStartController での表示

タイトル画面にも同様の形式でバージョン情報が表示されます：

```csharp
textComponent.SetText(GameObjectTreat.GetAppBuildDate());
// 出力例: "Version: 0.0.20.2026.03.09.01.34.StandaloneWindows64"
```

---

### GitHub リリースタグ

```
v0.0.20-prototype
v0.1.0-alpha
v0.2.0-beta
v1.0.0
```

**形式**:
```
v[MAJOR].[MINOR].[BUILD][-status]
```

| 例 | 説明 | 用途 |
|----|------|------|
| `v0.0.20-prototype` | プロトタイプ・Recovery フェーズ | Phase 0-2 |
| `v0.1.0-alpha` | テストユーザー希望者向け | Phase 3 前半 |
| `v0.2.0-beta` | 広範囲テスト向け | Phase 3 後半 |
| `v1.0.0` | 正式リリース | Post-Phase 3 |

### リリースノート

```markdown
## OnoCoro v0.0.20-prototype

**Release Date**: 2026-03-09
**Build Number**: 20
**Build DateTime**: 2026.03.09.01.34
**Build Target**: StandaloneWindows64
**Status**: Prototype / Recovery Phase 2.2
**In-Game Display**: Version: 0.0.20.2026.03.09.01.34.StandaloneWindows64
**Tag**: `v0.0.20-prototype`
```

### README.md

```markdown
![version](https://img.shields.io/badge/version-0.0.20-blue)
```

---

## ビルド前チェックリスト

リリース前には以下を確認してください：

- [ ] CHANGELOG.md が最新か確認
- [ ] README.md のバージョン表記が古くないか確認
- [ ] RELEASE_NOTES.md が手動更新か確認（BuildDate.txt は自動更新）
- [ ] GitHub タグ名を決定（例: `v0.1.0-alpha`）
- [ ] ビルトターゲットが正しいか確認（StandaloneWindows64）

**ビルド実行後：**

- [ ] BuildDate.txt が更新されているか確認
- [ ] 新しいビルド番号がゲーム内で表示されているか確認（UI メニューに表示機能がある場合）

---

## 例：Prototype リリース時の流れ

### ビルド前（手動）

```
BuildDate.txt:
0
0
19          ← 前回のビルド
2026.03.08.00.01
StandaloneWindows64

RELEASE_NOTES.md:
Release Date: 2026-03-10
Tag: v0.0.20-prototype
```

### ビルド実行

```bash
# Unity Build する
# → PostProcessBuild.cs 自動実行
# → BuildDate.txt を自動更新（BUILD を +1, DateTime を更新）
```

### ビルド後（自動）

```
BuildDate.txt:
0
0
20          ← 自動インクリメント
2026.03.10.15.30 ← 自動生成（ビルド日時）
StandaloneWindows64

ゲーム内表示:
Version: 0.0.20.2026.03.10.15.30.StandaloneWindows64
```
StandaloneWindows64

ゲーム内表示:
"Version 0.0.20+2026.03.10.15.30"
```

### GitHub リリース（手動）

```markdown
## v0.0.20-phase2.2

**Official Version**: 0.0.20+2026.03.10.15.30
**Build Number**: 20
**Release Date**: 2026-03-10

...ビルド内容...
```

---

## トラブルシューティング

### Q: BuildDate.txt が見当たらない

**A**: ビルド後に自動生成されます。初回ビルド時に以下の内容で作成されます：

```
0
0
1
2026.03.10.12.00
StandaloneWindows64
```

### Q: ビルド番号が間違った

**A**: BuildDate.txt を手動編集してから、すぐに再ビルドしてください。

```
# 前のビルド
0
0
21

# 修正
0
0
20
```

### Q: PostProcessBuild.cs が実行されない

**A**: 以下を確認してください：

- [ ] ファイルが `Assets/Scripts/Editor/PostProcessBuild.cs` に配置されているか
- [ ] Unity Editor が起動していて、スクリプトコンパイルが完了しているか
- [ ] `[PostProcessBuild(1)]` 属性が正しく付与されているか

---

## 参考

- **PostProcessBuild.cs**: [Assets/Scripts/Editor/PostProcessBuild.cs](../../Assets/Scripts/Editor/PostProcessBuild.cs)
- **BuildDate.txt**: [Assets/Resources/BuildDate.txt](../../Assets/Resources/BuildDate.txt)
- **Unity PostProcessBuild Documentation**: https://docs.unity3d.com/ScriptReference/Callbacks.PostProcessBuildAttribute.html

---

**Last Updated**: 2026-03-10
