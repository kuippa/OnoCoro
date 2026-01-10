# OnoCoro プロジェクト - 復旧と管理ガイド

## プロジェクト概要

**OnoCoro** は、Unity で実装された地理情報可視化アプリケーションです。PLATEAU SDK（日本の都市3Dデータ標準フォーマット対応）を使用して、CityGML形式の地理データを処理・表示します。

**重要な背景**: このプロジェクトは SSD故障による2年前のバックアップから段階的に復旧されたものです。そのため、データ損失防止のため GitHub でのバックアップが重要です。

---

## 現在の状態（2026年1月3日時点）

### プロジェクト構成
- **Unity Version**: 確認予定
- **プロジェクト言語**: C#
- **C# スクリプト数**: 393個（復旧済み）
- **主な依存**: 
  - PLATEAU Unity SDK
  - Cinemachine
  - glTFast
  - Unity Input System
  - その他のUnity packages

### ストレージ構成
- **Assets フォルダ**: 22.38 GB
  - C# スクリプト: 393個
  - Models（3Dモデル）
  - Textures（テクスチャ）
  - Sounds（オーディオ）
  - Editor（エディタスクリプト）
  - その他のゲーム資産

### 既知の課題

1. **リポジトリサイズの問題**
   - Assets フォルダ全体が Git でトラッキングされている
   - 総サイズ ~22.38 GB で、GitHub へのプッシュに困難
   - 解決方法検討中

2. **.gitignore の現状**
   - `/Assets/Art/**` を除外
   - `/Assets/Models/**` を除外
   - 上記以外のAssets内容はトラッキング対象

3. **Git履歴**
   - `3dd8e931`: Initial commit（設定ファイルのみ）
   - `19a84d69` (v1.0.0): 完全なプロジェクトバックアップ（復旧済み）
   - `fea242e6`: PLATEAU GIS データとPackagesをトラッキング除外
   - **リモート**: origin/main は`0bc05a24`（まだプッシュ未完了）

---

## セットアップ手順

### 前提条件
- Unity 2022.3 LTS 以上（推奨）
- Git 2.30以上
- 管理者権限でのファイルシステムアクセス

### クローンとセットアップ

#### Windows PowerShell での手順

```powershell
# リポジトリをクローン
git clone https://github.com/kuippa/OnoCoro.git
cd OnoCoro

# Git の初期設定（初回のみ）
git config user.email "your.email@example.com"
git config user.name "Your Name"

# リポジトリの状態確認
git log --oneline -5
git status
```

#### Unity での開く方法

**方法1: Unity Hub を使用（推奨）**
1. Unity Hub を開く
2. 「プロジェクトを開く」をクリック
3. クローンしたフォルダを選択

**方法2: Unity Editor から直接開く**
1. Unity Editor を起動
2. File > Open Project で選択
3. `OnoCoro` フォルダを指定

#### Git への認証設定（HTTPS の場合）

GitHub との通信時にパスワード認証が求められた場合、Personal Access Token を使用してください：

```powershell
# GitHub へのクレデンシャルを保存（Windows Credential Manager を使用）
git config --global credential.helper manager-core

# リポジトリ固有の設定の場合
git config credential.helper manager-core
```

**Personal Access Token の取得方法:**
1. GitHub にログイン
2. Settings > Developer settings > Personal access tokens
3. "Generate new token" をクリック
4. `repo` と `workflow` スコープを選択
5. トークンを生成して安全に保管
6. Git 認証時にパスワードの代わりにトークンを使用

### 初回起動時の注意

1. **Library フォルダの再生成**
   - クローン後、Unity が Library フォルダを自動生成します（数分かかる場合があります）

2. **Packages の復元**
   - Package Manager が自動的に依存パッケージを復元します

3. **PLATEAU SDK のセットアップ**
   - PLATEAU SDK がインストールされていない場合、以下の手順で導入：
     - Window > TextMesh Pro > Import TMP Essential Resources（必要に応じて）
     - Package Manager から PLATEAU SDK を検索・インストール

---

## 開発ガイドライン

### Git ワークフロー

#### ブランチ戦略
- `main`: 安定版リリース用ブランチ
- `develop`: 開発用ブランチ
- `feature/*`: 機能追加用ブランチ
- `bugfix/*`: バグ修正用ブランチ

#### コミットメッセージ規約

```
<type>(<scope>): <subject>

<body>

<footer>
```

**type の種類:**
- `feat`: 新機能
- `fix`: バグ修正
- `docs`: ドキュメント
- `style`: コード整形（意味の変更なし）
- `refactor`: コード整理
- `perf`: パフォーマンス改善
- `test`: テストコードの追加・修正
- `chore`: ビルドやツール設定の変更

**例:**
```
feat(plateau-loader): Add support for CityGML 2.0 format

- Implemented new parser for CityGML 2.0 buildings
- Added automatic coordinate transformation

Closes #123
```

### .gitignore の運用ルール

#### トラッキング対象
- C# スクリプト（`/Assets/**/*.cs`）
- シーン設定（`/Assets/**/*.unity`）
- プレハブ（`/Assets/**/*.prefab`）
- 基本的な設定ファイル

#### 除外対象（大容量や自動生成ファイル）
- Unity 自動生成: `/Library/`, `/Temp/`, `/Obj/`, `/Build/`
- IDE設定: `.vs/`, `.idea/`
- 大容量アセット: 
  - `/Assets/Art/**` （アートアセット）
  - `/Assets/Models/**` （3Dモデル）
  - 上記リスト参照

#### 新規ファイル追加時のチェックリスト
- [ ] ファイルサイズが 100 MB 以上ではないか確認
- [ ] バイナリ形式（.blend, .fbx, .psd等）は除外すべきか検討
- [ ] `.gitignore` に自動生成ファイルパターンが含まれているか確認

---

## トラブルシューティング

### Q: クローン後、スクリプトがコンパイルエラーになる

**A:** 以下を順に試してください：

```powershell
# Unity Editor を閉じた状態で実行

# Library フォルダを削除
Remove-Item -Path ".\Library" -Recurse -Force

# 必要に応じて Temp も削除
Remove-Item -Path ".\Temp" -Recurse -Force

# Obj フォルダも削除
Remove-Item -Path ".\obj" -Recurse -Force
```

その後、Unity Editor を再度開きます（Library が自動再生成され、コンパイルが開始されます）。

### Q: Git でプッシュできない（HTTP 500 エラー）

**A:** リポジトリサイズが大きい可能性があります。以下の手順を試してください：

```powershell
# ガベージコレクション実行（最適化）
git gc --aggressive --prune=now

# トラッキング中のファイル数を確認
(git ls-files | Measure-Object).Count

# 大容量ファイルをリストアップ
git ls-files -s | Sort-Object { [long]($_.Split()[3]) } -Descending | Select-Object -First 20

# リポジトリサイズを確認
git count-objects -v
```

**対応方法:**
1. 大容量ファイルが `.gitignore` から除外されているか確認
2. `.gitignore` に新しいルールを追加した場合は、以下を実行：
   ```powershell
   git rm -r --cached <file_or_folder> --force
   git commit -m "chore: Remove large files from tracking"
   ```
3. もう一度 `git gc --aggressive --prune=now` を実行してからプッシュ

### Q: PLATEAU SDK が見つからない

**A:** 以下の手順で確認・インストールしてください：

1. **Unity Editor でのセットアップ**
   - Window > TextMesh Pro > Import TMP Essential Resources（必要に応じて）
   - Window > Package Manager を開く

2. **パッケージの確認**
   - Package Manager の左上で「In Project」を選択
   - `com.synesthesias.plateau-unity-sdk` を検索

3. **インストール方法（1つ選択）**

   **方法A: GitHub URL から追加（推奨）**
   - Package Manager の「+」ボタン > Add package from git URL
   - 以下を入力：
     ```
     https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity.git
     ```
   - インストール完了まで待機

   **方法B: Package Manager UI から検索**
   - Package Manager の検索欄で「PLATEAU」を検索
   - Official パッケージを見つけてインストール

4. **PowerShell からのコマンドラインセットアップ**
   ```powershell
   # Packages フォルダの manifest.json を確認
   Get-Content -Path ".\Packages\manifest.json" -Raw | ConvertFrom-Json

   # 必要に応じて直接編集（テキストエディタで開く）
   notepad .\Packages\manifest.json
   ```

   `manifest.json` に以下を追加：
   ```json
   "com.synesthesias.plateau-unity-sdk": "https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity.git"
   ```

5. **セットアップ完了確認**
   ```powershell
   # Assets フォルダに PLATEAU サンプルが表示されているか確認
   Get-ChildItem -Path ".\Assets" -Recurse -Filter "*PLATEAU*" -Directory
   ```

---

## パフォーマンス最適化ガイド

### 推奨ハードウェア
- **CPU**: Intel i7/AMD Ryzen 7 以上
- **RAM**: 16 GB 以上（PLATEAU データ処理時）
- **GPU**: NVIDIA GeForce RTX 2070 以上（推奨）
- **ストレージ**: SSD 100 GB 以上（高速アクセス必須）

### システム情報の確認（Windows PowerShell）

```powershell
# CPU 情報
Get-WmiObject win32_processor | Select-Object Name, Cores, Threads

# RAM 情報
$ram = Get-WmiObject Win32_ComputerSystem
Write-Host "Total RAM: $($ram.TotalPhysicalMemory / 1GB) GB"

# GPU 情報
Get-WmiObject win32_videocontroller | Select-Object Name, AdapterRAM

# ストレージ情報
Get-Volume | Select-Object DriveLetter, FileSystemLabel, Size, SizeRemaining | Where-Object { $_.Size -gt 0 }

# Unity プロジェクトサイズ
$assetSize = (Get-ChildItem "Assets" -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB
Write-Host "Assets folder size: $([Math]::Round($assetSize, 2)) GB"

# Library フォルダサイズ（デバッグ用）
$libSize = (Get-ChildItem "Library" -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1GB
Write-Host "Library folder size: $([Math]::Round($libSize, 2)) GB"
```

### よくある最適化

1. **シーンの簡略化**
   - 不要なゲームオブジェクトを非表示化
   - LOD（Level of Detail）設定の活用

2. **メモリ管理**
   - Assets を遅延ロード化
   - Assetbundle の活用

3. **レンダリング最適化**
   - Batching の有効化
   - Shader 最適化

### プロジェクトのキャッシュをクリア（Windows）

```powershell
# Unity キャッシュの削除
Remove-Item -Path "$env:APPDATA\..\LocalLow\Unity\Editor-5.x\Cache" -Recurse -Force -ErrorAction SilentlyContinue

# ビルドキャッシュの削除
Remove-Item -Path ".\Library\ScriptAssemblies" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".\Library\Artifacts" -Recurse -Force -ErrorAction SilentlyContinue

# 再度 Unity Editor を起動してキャッシュを再構築
Write-Host "キャッシュをクリアしました。Unity Editor を再起動してください。"
```

### メモリ使用量の監視（Windows）

```powershell
# リアルタイムでメモリ使用量を監視
while ($true) {
    $proc = Get-Process "Unity" -ErrorAction SilentlyContinue
    if ($proc) {
        $memMB = $proc.WorkingSet64 / 1MB
        Write-Host "Unity Memory: $([Math]::Round($memMB, 2)) MB" -ForegroundColor Cyan
    }
    Start-Sleep -Seconds 5
}
```

---

## リリースプロセス

### バージョニング規約
Semantic Versioning を採用: `MAJOR.MINOR.PATCH`

- **MAJOR**: 互換性を破る変更
- **MINOR**: 互換性のある新機能
- **PATCH**: バグ修正

### リリース手順

1. `develop` ブランチで開発を進める
2. リリースの際に `release/vX.Y.Z` ブランチを作成
3. テストとバージョン番号の更新
4. `main` にマージして、タグを付与
   ```bash
   git tag -a vX.Y.Z -m "Release version X.Y.Z"
   git push origin main --tags
   ```

---

## Windows PowerShell での注意事項

### コマンドの互換性

このプロジェクトは **Windows 環境** を想定しています。Linux/macOS コマンドは利用できません。

**使えないコマンド（Linux/macOS）:**
```
ls, grep, wc, du, sed, awk, cat, head, tail など
```

**Windows PowerShell での代替コマンド:**

| Linux コマンド | Windows PowerShell | 説明 |
|---|---|---|
| `ls` | `Get-ChildItem` | ファイル/フォルダ一覧 |
| `ls -la` | `Get-ChildItem -Force` | 隠しファイル含む一覧 |
| `grep pattern` | `Select-String "pattern"` | テキスト検索 |
| `wc -l` | `Measure-Object -Line` | 行数カウント |
| `du -sh` | `(Get-ChildItem -Recurse \| Measure-Object -Property Length -Sum).Sum / 1GB` | フォルダサイズ |
| `cat file` | `Get-Content file` | ファイル内容表示 |
| `head -n 10` | `Get-Content file \| Select-Object -First 10` | 最初の10行 |
| `tail -n 10` | `Get-Content file \| Select-Object -Last 10` | 最後の10行 |
| `find . -name "*.cs"` | `Get-ChildItem -Recurse -Filter "*.cs"` | ファイル検索 |
| `rm -rf folder` | `Remove-Item -Path folder -Recurse -Force` | フォルダ削除 |
| `cp file dest` | `Copy-Item -Path file -Destination dest` | ファイルコピー |
| `mv file dest` | `Move-Item -Path file -Destination dest` | ファイル移動 |

### PowerShell のパイプ処理

PowerShell ではパイプ（`\|`）で複数コマンドをつなげることができます：

```powershell
# 例1: .cs ファイルをすべて検索して、ファイル数をカウント
Get-ChildItem -Path ".\Assets" -Recurse -Filter "*.cs" | Measure-Object | Select-Object -ExpandProperty Count

# 例2: 大きなファイルを上位10個表示
Get-ChildItem -Path ".\Assets" -Recurse -File | Sort-Object -Property Length -Descending | Select-Object -First 10

# 例3: テキスト内で特定パターンを検索
Get-Content ".\file.txt" | Select-String "searchPattern"
```

---

## Git 操作チートシート（Windows PowerShell）

### ブランチ管理

```powershell
# ブランチ一覧表示
git branch -a

# 新しいブランチを作成
git checkout -b feature/my-feature

# ブランチを削除
git branch -d feature/my-feature

# リモートブランチを削除
git push origin --delete feature/my-feature
```

### コミット・プッシュ

```powershell
# ステージング（すべてのファイル）
git add .

# ステージング（特定ファイルのみ）
git add .\Assets\Scripts\MyScript.cs

# コミット
git commit -m "feat: Add new feature description"

# 直前のコミットを修正
git commit --amend -m "feat: Corrected message"

# プッシュ
git push origin main

# 強制プッシュ（慎重に使用）
git push origin main --force

# タグの作成とプッシュ
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### 差分確認

```powershell
# ワーキングディレクトリの差分を表示
git diff

# ステージされた差分を表示
git diff --cached

# コミット間の差分
git diff <commit1> <commit2>

# ファイル一覧のみを表示
git diff --name-only

# 統計情報を表示
git diff --stat
```

### 履歴確認

```powershell
# コミット履歴（1行表示）
git log --oneline -10

# グラフ表示で分岐を可視化
git log --oneline --graph --all

# 特定ファイルの履歴
git log -p .\Assets\Scripts\MyScript.cs

# 日時範囲で検索
git log --since="2025-01-01" --until="2026-01-03"

# ファイルの削除履歴も含める
git log --full-history -- .\Assets\MyFile.cs
```

### 変更の戻し

```powershell
# ワーキングディレクトリの変更を取り消し
git checkout -- .\Assets\Scripts\MyScript.cs

# ステージを取り消し
git reset HEAD .\Assets\Scripts\MyScript.cs

# 直前のコミットを取り消し（変更は保持）
git reset --soft HEAD~1

# 直前のコミットを完全に取り消し
git reset --hard HEAD~1

# コミット履歴から特定ファイルを削除（履歴を上書き）
git rm -r --cached .\Assets\LargeData\
git commit -m "chore: Remove large data from tracking"
```

### リモート操作

```powershell
# リモート一覧表示
git remote -v

# リモート追加
git remote add origin https://github.com/kuippa/OnoCoro.git

# リモート削除
git remote remove origin

# リモートの最新情報を取得（フェッチ）
git fetch origin

# リモートの最新をローカルにマージ
git pull origin main

# フェッチ＋マージを一度に実行
git pull --rebase origin main
```

### マージ・リベース

```powershell
# 他のブランチをマージ
git merge feature/my-feature

# マージの中止
git merge --abort

# リベース（慎重に）
git rebase main

# リベースの中止
git rebase --abort

# マージ競合の解決後（コンフリクト）
git add .
git commit -m "Resolve merge conflict"
```

### クリーンアップ

```powershell
# トラッキングされていないファイルを表示
git clean -n -d

# トラッキングされていないファイルを削除
git clean -f -d

# 追跡されなくなったリモートブランチの参照を削除
git fetch --prune

# ローカルの削除されたリモートブランチを削除
git branch -vv | Select-String "gone" | ForEach-Object { 
    $branch = $_.Line.Split()[0]
    git branch -d $branch
}
```

### 診断・確認

```powershell
# リポジトリの状態確認
git status

# トラッキング中のファイル数
(git ls-files | Measure-Object).Count

# 最大のトラッキング中のファイルを表示
git ls-files -s | Sort-Object { [long]($_.Split()[3]) } -Descending | Select-Object -First 10

# リポジトリサイズの詳細
git count-objects -v

# リモートのURL確認
git config --get remote.origin.url

# ローカル設定の確認
git config --local --list
```

このプロジェクトへの貢献を歓迎します。以下のガイドラインをお守りください：

1. **Issue の作成**
   - バグ報告や機能リクエスト前に、既存の Issue を確認

2. **Pull Request**
   - 明確な説明と関連 Issue の記述
   - コードレビュー前にローカルでテスト実施

3. **ドキュメント**
   - 新機能にはドキュメント追加が必須
   - 既存ドキュメントの更新も忘れずに

---

## 関連リソース

- [PLATEAU SDK 公式ドキュメント](https://www.mlit.go.jp/plateau/)
- [Unity Manual](https://docs.unity3d.com/)
- [CityGML 仕様](https://www.ogc.org/standards/citygml)
- [このプロジェクトの GitHub Issues](https://github.com/kuippa/OnoCoro/issues)

---

## コーディング規約

このプロジェクトでは、コードの一貫性と保守性を確保するため、以下のコーディング規約を遵守してください。

### 1. 名前空間の衝突回避

**Debug クラスの明示的なエイリアス**

System.Diagnostics.Debug と UnityEngine.Debug の衝突を避けるため、必ず using エイリアスを使用してください。

```csharp
using System.Diagnostics;
using Debug = UnityEngine.Debug;

// これにより Debug.Log() は UnityEngine.Debug を参照
Debug.Log("Unity のデバッグログ");

// System.Diagnostics.Process は完全修飾名で使用
Process.Start("notepad.exe");
```

### 2. マジックナンバー・マジックストリングの禁止

**コード内に直接値を書かないこと**

数値や文字列リテラルは、必ず名前付き定数として定義してください。

**例外規定:**
以下の場合は、可読性向上のため直値の使用を許可します：
- **一度しか使われない値**で、その後の処理が成立しないような必須のオブジェクト名
- 再利用や変更の予定がない値
- 定数化することで逆にコードが読みにくくなる場合

例: 初期化メソッド内でのみ使用されるGameObject名など
```csharp
// ✅ 一度しか使わない場合は直値でOK
private void Initialize()
{
    _loading = UIHelper.FindOrInstantiatePrefab("nowloading", path, missingObjects);
    UIHelper.RegisterButton("btnStart", OnClickStart, missingObjects);
}
```

❌ **悪い例:**
```csharp
if (count > 10)
{
    Debug.Log("Too many items");
}
GameObject obj = GameObject.Find("PlayerCanvas");
float speed = 9.8f;
```

✅ **良い例:**
```csharp
private const int MAX_ITEM_COUNT = 10;
private const string MSG_TOO_MANY_ITEMS = "Too many items";
private const string OBJ_PLAYER_CANVAS = "PlayerCanvas";
private const float GRAVITY_ACCELERATION = 9.8f;

if (count > MAX_ITEM_COUNT)
{
    Debug.Log(MSG_TOO_MANY_ITEMS);
}
GameObject obj = GameObject.Find(OBJ_PLAYER_CANVAS);
float speed = GRAVITY_ACCELERATION;
```

**定数の命名規則:**
- プライベート定数: `_CONSTANT_NAME_FORMAT` (アンダースコア + 大文字 + スネークケース)
- パブリック定数: `CONSTANT_NAME_FORMAT` (大文字 + スネークケース)
- 意味を明確に示す名前を使用

**定数のグループ化:**
```csharp
// File Constants
private const string _STAGE_LIST_FILE_NAME = "stagelist.csv";
private const string _YAML_FILE_EXTENSION = ".yaml";

// GameObject Names
private const string _OBJ_LOADING = "nowloading";
private const string _OBJ_BTN_START = "btnStart";

// Numeric Constants
private const float _SCENE_LOAD_DELAY = 0.1f;
private const int _MAX_RETRY_COUNT = 3;
```

### 3. 制御文の中括弧 {} を必須とする

**すべての if, else, for, while, foreach に {} を使用すること**

単一行の文でも、必ず中括弧で囲んでください。

❌ **悪い例:**
```csharp
if (condition)
    DoSomething();

if (x > 0)
    x = 0;
else
    x = -1;

for (int i = 0; i < 10; i++)
    Debug.Log(i);
```

✅ **良い例:**
```csharp
if (condition)
{
    DoSomething();
}

if (x > 0)
{
    x = 0;
}
else
{
    x = -1;
}

for (int i = 0; i < 10; i++)
{
    Debug.Log(i);
}
```

### 4. 三項演算子とnull条件演算子の使用制限

**三項演算子 `? :` は使用禁止**

条件式は if-else で明示的に記述してください。

❌ **悪い例:**
```csharp
int result = (x > 0) ? 10 : -10;
string message = isValid ? "Valid" : "Invalid";
GameObject obj = transform != null ? transform.gameObject : null;
```

✅ **良い例:**
```csharp
int result;
if (x > 0)
{
    result = 10;
}
else
{
    result = -10;
}

string message;
if (isValid)
{
    message = "Valid";
}
else
{
    message = "Invalid";
}

GameObject foundObject = null;
if (transform != null)
{
    foundObject = transform.gameObject;
}
```

**null条件演算子 `?.` も使用禁止**

null チェックは明示的に行ってください。

❌ **悪い例:**
```csharp
component?.DoSomething();
int? count = list?.Count;
```

✅ **良い例:**
```csharp
if (component != null)
{
    component.DoSomething();
}

int count = 0;
if (list != null)
{
    count = list.Count;
}
```

### 5. 関数の行数制限

**1つの関数は40行以内に収めること**

40行を超える関数は、機能ごとに分割してください。

❌ **悪い例:**
```csharp
void Awake()
{
    // 100行以上の初期化コード
    // GameObject の検索、設定、イベント登録などが混在
}
```

✅ **良い例:**
```csharp
void Awake()
{
    List<string> missingObjects = new List<string>();
    
    InitializeLoadingCanvas(missingObjects);
    InitializePanels(missingObjects);
    RegisterButtonListeners(missingObjects);
    InitializeStageContents(missingObjects);
    InitializeVersionInfo(missingObjects);
    
    CheckMissingObjects(missingObjects);
}

private void InitializeLoadingCanvas(List<string> missingObjects)
{
    // 具体的な初期化処理（10-20行程度）
}

private void InitializePanels(List<string> missingObjects)
{
    // 具体的な初期化処理（10-20行程度）
}
```

**関数分割の指針:**
- 1つの関数は1つの責任を持つ（単一責任の原則）
- 関数名で処理内容が明確にわかるようにする
- 引数と戻り値で依存関係を明確にする

### 6. コメント規約

**コメントは最小限に、コードで意図を表現すること**

```csharp
// ❌ 悪い例: コメントで説明が必要
// ユーザーの年齢が18歳以上かチェック
if (user.age >= 18)
{
    // ...
}

// ✅ 良い例: 定数名で意図が明確
private const int ADULT_AGE_THRESHOLD = 18;

if (user.age >= ADULT_AGE_THRESHOLD)
{
    // ...
}
```

### 7. 変数名の命名規則

**意味のある名前を使用し、一時的な名前を避ける**

変数名は、その変数が何を表すのかを明確に示すものにしてください。

❌ **悪い例:**
```csharp
GameObject obj = GameObject.Find("Canvas");
Transform t = parent.transform.Find("Content");
int temp = CalculateValue();
string str = GetMessage();
Button btn = GetComponent<Button>();
```

✅ **良い例:**
```csharp
GameObject canvasObject = GameObject.Find("Canvas");
Transform contentTransform = parent.transform.Find("Content");
int calculatedValue = CalculateValue();
string errorMessage = GetMessage();
Button startButton = GetComponent<Button>();
```

**例外的に許可される短い変数名:**
- ループカウンタ: `i`, `j`, `k`（ただしネストが深い場合は `index`, `row`, `col` など明確な名前を推奨）
- 座標: `x`, `y`, `z`
- イベント引数: `e` （ただし `eventArgs` の方が望ましい）

**変数名の接頭辞・接尾辞:**
```csharp
// GameObjectには適切な接尾辞を
GameObject loadingCanvas;        // "Canvas"がついているとUIだとわかる
GameObject playerCharacter;      // "Character"で実体がわかる

// Transformには"Transform"を
Transform contentTransform;
Transform scrollbarTransform;

// Componentには型を示す名前を
Button closeButton;
Text statusText;
Image backgroundImage;
```

### 8. ユーティリティクラスの設計原則

**関連する機能は適切なユーティリティクラスに集約すること**

コードの重複を避け、保守性を高めるため、共通処理は静的ユーティリティクラスに集約してください。

#### ユーティリティクラスの分類

**LoadStreamingAsset** - StreamingAssetsフォルダ関連
- ファイルの読み込み
- ファイルパスの生成
- ファイル存在チェック
- StreamingAssets固有の操作

```csharp
// ✅ 良い例: StreamingAssets関連はすべてLoadStreamingAssetに集約
public static class LoadStreamingAsset
{
    public const string YAML_FILE_EXTENSION = ".yaml";
    public const string STAGE_LIST_FILE_NAME = "stagelist.csv";
    
    // YAML関連の操作を集約
    public static string GetYamlFileName(string sceneName)
    {
        return Path.GetFileName(sceneName + YAML_FILE_EXTENSION);
    }
    
    public static bool YamlFileExists(string sceneName)
    {
        string yamlFileName = GetYamlFileName(sceneName);
        string yamlFilePath = StageFilePath(yamlFileName);
        return File.Exists(yamlFilePath);
    }
    
    internal static string StageFilePath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, _STAGING_SUB_FOLDER, fileName);
    }
}

// 使用例
string fileName = LoadStreamingAsset.GetYamlFileName(sceneName);
bool exists = LoadStreamingAsset.YamlFileExists(sceneName);
```

**UIHelper** - UI操作関連
- GameObject検索
- ボタン登録
- スクロールバー制御
- パネルのセットアップ

```csharp
public static class UIHelper
{
    // Prefabパス定数
    public const string PREFAB_PATH_LOADING = "Prefabs/UI/nowloading";
    
    // GameObject検索（エラーハンドリング付き）
    public static GameObject FindGameObject(string path, List<string> missingObjects, 
                                           string objectType = "", GameObject parent = null);
    
    // ボタン登録
    public static void RegisterButton(string buttonName, UnityEngine.Events.UnityAction action, 
                                     List<string> missingObjects, GameObject parent = null);
    
    public static void RegisterChildButton(GameObject parent, string childButtonName, 
                                          System.Action<Button> action);
    
    // スクロールバー制御
    public static void ResetScrollbarInPanel(GameObject parentPanel);
    
    // パネルセットアップ
    public static GameObject FindAndSetupPanel(string panelName, List<string> missingObjects, 
                                              bool setActiveFalse = false);
}
```

**FileOperationUtility** - ファイル操作関連
- エディタでファイルを開く
- デフォルトアプリでファイルを開く
- 画像読み込み

**StageDataManager** - ステージデータ管理
- シーンリストの取得
- CSVからのデータ読み込み
- シーン情報の統合

**LogUtility** - ログ管理
- レベル別ログ出力
- ファイル出力
- 条件付きコンパイル

#### 設計ルール

1. **単一責任の原則**: 1つのユーティリティクラスは1つの責務を持つ
2. **静的クラス**: ユーティリティは`static class`として定義
3. **定数の集約**: 関連する定数は該当ユーティリティクラスに配置
4. **名前空間**: 特定の名前空間は使わず、グローバルに配置（UnityのScriptAssembliesに準拠）

❌ **悪い例: コントローラに直接実装**
```csharp
public class TitleStartCtrl : MonoBehaviour
{
    private void OnClickStageEditor(Button btnStageEditor)
    {
        // 直接Path.GetFileNameを使用
        string fileName = Path.GetFileName(sceneName + ".yaml");
        
        // 直接File.Existsを使用
        if (File.Exists(LoadStreamingAsset.StageFilePath(fileName)))
        {
            // ...
        }
    }
}
```

✅ **良い例: ユーティリティクラスに集約**
```csharp
public class TitleStartCtrl : MonoBehaviour
{
    private void OnClickStageEditor(Button btnStageEditor)
    {
        // LoadStreamingAssetに集約された操作を使用
        string fileName = LoadStreamingAsset.GetYamlFileName(sceneName);
        
        if (LoadStreamingAsset.YamlFileExists(sceneName))
        {
            // ...
        }
    }
}
```

### 9. UIスクロールビューの取り扱い

**ScrollRectの位置制御とタイミング**

スクロールビューの位置を制御する際は、以下の原則に従ってください。

#### 基本原則

1. **ScrollRect.normalizedPositionを使用する**
   - `Scrollbar.value`の直接操作は避ける
   - `normalizedPosition = new Vector2(0, 1)`で最上部へ

2. **コンテンツ更新後にリセットする**
   - テキストやアイテムの設定後にスクロール位置を更新
   - コンテンツサイズが確定してから実行

3. **アクティブ状態をチェックする**
   - 非アクティブなパネルのScrollbarは操作しない
   - ユーティリティ関数内でチェックを行う

#### 実装パターン

```csharp
// UIHelper.cs
public static void ResetScrollbarInPanel(GameObject parentPanel)
{
    // アクティブチェックを内包
    if (parentPanel == null || !parentPanel.activeSelf)
    {
        return;
    }
    
    // ScrollRectを使用（Scrollbar直接操作は不可）
    ScrollRect scrollRect = parentPanel.GetComponentInChildren<ScrollRect>(true);
    if (scrollRect != null)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1); // x=0(左), y=1(上)
    }
}
```

#### 呼び出しタイミング

```csharp
// ❌ 悪い例: コンテンツ設定前にリセット
private void OnClickAboutGame()
{
    _pnlAboutThisGame.SetActive(value: true);
    UIHelper.ResetScrollbarInPanel(_pnlAboutThisGame); // 早すぎる
    
    textComponent.text = LoadStreamingAsset.AllTextStream(...);
}

// ✅ 良い例: コンテンツ設定後にリセット
private void OnClickAboutGame()
{
    _pnlAboutThisGame.SetActive(value: true);
    
    string text = LoadStreamingAsset.AllTextStream(...);
    textComponent.text = text;
    
    UIHelper.ResetScrollbarInPanel(_pnlAboutThisGame); // コンテンツ確定後
}
```

#### パネル切り替え時

```csharp
// ✅ 良い例: SetActive後にリセット、activeチェックは関数内で実施
private void OnClickStageSelect()
{
    _pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
    UIHelper.ResetScrollbarInPanel(_pnlStageSelector); // 内部でactiveチェック
}
```

#### 変数格納の不要化

従来のように`_StageScrollbar`などの変数にScrollbarを格納する必要はありません。

❌ **悪い例: 変数に格納して管理**
```csharp
private GameObject _StageScrollbar;
private const string _CHILD_PATH_SCROLLVIEW_SCROLLBAR = "Scroll View/Scrollbar Vertical";

void Awake()
{
    _StageScrollbar = UIHelper.FindGameObject(_CHILD_PATH_SCROLLVIEW_SCROLLBAR, ...);
    UIHelper.SetScrollbarTopPosition(_StageScrollbar);
}

void OnClickStageSelect()
{
    UIHelper.SetScrollbarTopPosition(_StageScrollbar);
}
```

✅ **良い例: 親パネルから自動検索**
```csharp
// フィールド変数不要、定数も不要

void Awake()
{
    // コンテンツ設定後に親パネルを渡すだけ
    UIHelper.ResetScrollbarInPanel(_pnlStageSelector);
}

void OnClickStageSelect()
{
    _pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
    UIHelper.ResetScrollbarInPanel(_pnlStageSelector); // 親から自動検索
}
```

**メリット:**
- 変数管理不要（メモリ効率向上）
- パス定数不要（保守性向上）
- コードがシンプルで読みやすい
- GetComponentInChildren(true)で非アクティブな子も検索可能

### 10. 冗長な条件分岐の回避

### Early Return パターンの徹底使用

**必須**: 条件チェックは Early Return（ガード句）パターンを使用し、ネストしたif文を避けること。

#### ❌ 絶対に避けるべきパターン
```csharp
// NG: ネストしたif文
if (transform.parent != null)
{
    if (someCondition)
    {
        if (anotherCondition)
        {
            // 正常系の処理
        }
    }
}
```

#### ✅ 必ず使用すべきパターン
```csharp
// OK: Early Return パターン
if (transform.parent == null)
{
    Debug.LogWarning("No parent found");
    return;
}

if (!someCondition)
{
    return;
}

if (!anotherCondition)
{
    return;
}

// 正常系の処理（ネストなし）
```

### 具体例: WindowCloseCtrl

#### ❌ 悪い例（現在のコード）
```csharp
private void CloseWindow()
{
    if (transform.parent != null)
    {
        if (transform.parent.gameObject.name == "titlebar")
        {
            if (transform.parent.parent != null)
            {
                transform.parent.parent.gameObject.SetActive(false);
            }
            return;
        }
        transform.parent.gameObject.SetActive(false);
    }
    else
    {
        Debug.LogWarning("No parent found");
    }
}
```

#### ✅ 良い例（Early Return使用）
```csharp
private void CloseWindow()
{
    // ガード句: 異常系を先に処理
    if (transform.parent == null)
    {
        Debug.LogWarning("No parent found");
        return;
    }

    // タイトルバーの場合は親の親を非表示
    if (transform.parent.gameObject.name == "titlebar")
    {
        if (transform.parent.parent == null)
        {
            Debug.LogWarning("Grandparent not found");
            return;
        }
        transform.parent.parent.gameObject.SetActive(false);
        return;
    }

    // 通常の場合は親を非表示
    transform.parent.gameObject.SetActive(false);
}
```

### Early Return のメリット

1. **可読性**: 正常系のコードがフラットで読みやすい
2. **保守性**: 条件追加時にネストが深くならない
3. **認知負荷軽減**: 前提条件が明確で理解しやすい
4. **バグ予防**: else節の書き忘れを防げる

### 適用ルール

- **nullチェック**: 必ず最初に行い、nullの場合は即return
- **前提条件**: ビジネスロジックの前にすべてチェック
- **ネストの深さ**: 2段階以上のネストは禁止（Early Returnで解消）

---

## コード提案前のチェックリスト

AIアシスタントがコードを提案する際は、以下のチェックリストを必ず確認してください：

### 必須チェック項目

- [ ] **定数化**: マジックナンバー・マジックストリングがないか
- [ ] **中括弧**: すべての制御文に`{}`があるか
- [ ] **三項演算子**: `? :`や`?.`を使用していないか
- [ ] **Early Return**: ネストした`if`を避け、ガード句を使用しているか
- [ ] **関数の長さ**: 40行以内に収まっているか
- [ ] **変数名**: 意味のある名前になっているか（`obj`, `temp`, `str`などを避ける）
- [ ] **ユーティリティの使用**: 共通処理を適切なユーティリティクラスに集約しているか
  - StreamingAssets操作 → `LoadStreamingAsset`
  - UI操作 → `UIHelper`
  - ファイル操作 → `FileOperationUtility`
  - ステージデータ → `StageDataManager`
  - ログ出力 → `LogUtility`
- [ ] **ScrollRect**: スクロール位置制御は`normalizedPosition`を使用しているか
- [ ] **activeチェック**: UI操作前のactiveチェックは関数内で行っているか

### コード提案の手順

1. **規約違反がないか確認**
2. **違反がある場合は修正してから提案**
3. **提案時に使用した規約を簡潔に説明**

```
例:
「LoadStreamingAssetにYAML操作を集約するため、GetYamlFileName()とYamlFileExists()を追加しました。
これによりPath.GetFileNameとFile.Existsの直接使用を避け、StreamingAssets関連の操作が一元化されます。」
```

---

## ライセンス

MIT License © 2026 kuippa

詳細は [LICENSE](../LICENSE) ファイルを参照してください。

---

## 最後に

このプロジェクトは SSD 故障からの復旧プロジェクトです。データ損失防止の観点から：

- **定期的なローカルバックアップ** を推奨
- **重要な変更は Git に頻繁にコミット** してください
- **大容量ファイルを追加する場合は、事前に相談** ください

何か質問や提案がある場合は、GitHub Issues をご利用ください。
