# OnoCoro プロジェクト - 復旧と管理ガイド

> **注意**: AI Agent 向けの最上位ルールは [AGENTS.md](AGENTS.md) を参照してください。
> このファイルは人間の開発者向けのプロジェクト管理・運用ガイドです。

> **関連ドキュメント**:
> - [introduction.md](../docs/introduction.md) - プロジェクトの概要・目的・非目的
> - [architecture.md](../docs/architecture.md) - システム全体のアーキテクチャ
> - [coding-standards.md](../docs/coding-standards.md) - C#実装・Unity設計の詳細規約
> - [AGENTS.md](../AGENTS.md) - AI Agent 最上位ルール
> - [copilot/README.md](copilot/README.md) - GitHub Copilot カスタマイズガイド
> - [copilot/skills/README.md](copilot/skills/README.md) - Agent Skills（microsoft-docs, microsoft-code-reference, make-skill-template）

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

**Conventional Commits** を採用。詳細は以下を参照：
- [conventional-commit プロンプト](copilot/prompts/conventional-commit.prompt.md)
- [AGENTS.md](../../AGENTS.md#git-workflow)

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

## Windows PowerShell での基本操作

**このプロジェクトは Windows 環境（PowerShell）を前提としています。**

```powershell
# ファイル検索
Get-ChildItem -Recurse -Filter "*.cs"

# テキスト検索
Select-String "pattern" file.txt

# フォルダサイズ
(Get-ChildItem -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB
```

詳細は [PowerShell 公式ドキュメント](https://learn.microsoft.com/powershell/) を参照。

---

## Git コマンドクイックリファレンス

```powershell
# ブランチ・コミット
git checkout -b feature/name
git add .; git commit -m "feat: description"

# 確認
git status; git log --oneline -5

# プッシュ
git push origin main

# 差分確認
git diff; git diff --cached

# リモート更新
git fetch origin; git pull origin main
```

詳細な Git ワークフローは [GitHub フロー](https://guides.github.com/introduction/flow/) を参照してください。

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

## コーディング規約について

OnoCoro プロジェクトのコーディング規約は以下を参照してください：

- **基本ルール**: [AGENTS.md](../AGENTS.md#coding-standards) の「Coding Standards」セクション
- **詳細な実装ガイド**: [docs/coding-standards.md](../docs/coding-standards.md)
- **Recovery フェーズ特有ガイド**: [.github/instructions/unity-csharp-recovery.instructions.md](.github/instructions/unity-csharp-recovery.instructions.md)

特に以下のポイントに注意：Null チェック（guard clause）・マジックナンバー禁止・メソッド 40 行以内・制御文に波括弧・3 項演算子禁止

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
