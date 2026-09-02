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

**BuildVersionStamper.cs** が **ビルド開始前** に自動実行され：

1. 現在の `BuildDate.txt` から MAJOR / MINOR / BUILD を読み込む
2. **BUILD を +1 インクリメント**
3. ビルド実行日時とビルドターゲットを記録
4. 更新した内容を `BuildDate.txt` に書き込み、`AssetDatabase.ImportAsset` で取り込ませる
5. **`PlayerSettings.bundleVersion` にも同じ番号を設定する**

```csharp
// BuildVersionStamper.cs（IPreprocessBuildWithReport）
public void OnPreprocessBuild(BuildReport report)
{
    VersionInfo info = ReadVersion();
    info.Build = info.Build + 1;

    WriteVersion(info, report.summary.platform);
    AssetDatabase.ImportAsset(_FILE_PATH, ImportAssetOptions.ForceSynchronousImport);

    PlayerSettings.bundleVersion = $"{info.Major}.{info.Minor}.{info.Build}";
}
```

### なぜビルド前なのか

[IMPORTANT] `BuildDate.txt` は `Assets/Resources` 配下にあり、
**ビルド時にプレイヤーへ焼き込まれる**。

以前は `PostProcessBuild.cs` がビルド完了後に採番していたが、
これでは焼き込みが終わったあとにファイルを書き換えることになり、
**配布物の中身は 1 つ前のビルド番号のまま**になる。
（例: BuildDate.txt が 25 になっていても、その exe が表示するのは 24）

このため採番をビルド前へ移した。
`PostProcessBuild.cs` はログ出力のみを行う。**ここで書き戻すと二重採番になる**。

### Unity の Application.version との関係

Unity 側の `PlayerSettings.bundleVersion`（= `Application.version`）は
以前は手動更新の運用で、実際のビルド番号と食い違っていた
（v0.0.25 時点で `0.1` のまま放置されていた）。

現在は採番時に自動で同じ値が入るため、**手で書き換える必要はない**。

| 取得方法 | 返る値 |
|---------|-------|
| `Application.version` | `0.0.25` |
| `Utility.GetAppVersion()` | `Version: 0.0.25.2026.09.03.02.11.StandaloneWindows64` |

[NOTE] ビルドが途中で失敗しても採番は進む。番号が飛ぶだけで実害は無いが、
戻したい場合は `BuildDate.txt` を手で書き換えてから再ビルドする。

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
- [ ] RELEASE_NOTES.md が手動更新か確認（BuildDate.txt と bundleVersion は自動更新）
- [ ] GitHub タグ名を決定（例: `v0.1.0-alpha`）
- [ ] ビルドターゲットが正しいか確認（StandaloneWindows64）

**ビルド実行後：**

- [ ] BuildDate.txt が更新されているか確認
- [ ] **ゲームを起動して、タイトル画面のバージョン表示が今回の番号になっているか確認**
      （BuildDate.txt の値と一致しない場合は採番のタイミングがずれている）

公開までの手順は [howto/release-build.md](../howto/release-build.md) を参照。

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
