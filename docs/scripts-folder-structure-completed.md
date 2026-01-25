# Scripts フォルダ構成（実装完了版）

**作成日**: 2026-01-23  
**更新日**: 2026-01-26（リカバリーフェーズ完了に伴う統合）  
**対象**: OnoCoro v0.1.0-alpha (Prototype Phase)  
**ステータス**: ✅ **リカバリーフェーズ完了 - フォルダ構成確定**

---

## 📊 概要

OnoCoro プロジェクトは 2 年前の SSD バックアップから復旧されました。リカバリーフェーズでは、以下を実施：

- ✅ **フォルダ構成の整理**: 20 フォルダを 4 層 + オーソゴナル構造に再編成
- ✅ **クラス名のリネーム**: Ctrl 系（GameSpeedCtrl など）を Manager/Controller/System に統一
- ✅ **Namespace 統一**: すべてを `CommonsUtility` に統一
- ✅ **責務分離**: Presentation / Game / Data / Core の 4 層に明確化

本ドキュメントは、現在実装されているフォルダ構成、その設計意図、リネーム実施内容、および今後の方針をまとめたものです。

---

## 🎯 フォルダ構成の設計原則

### 層構造（4 層）

OnoCoro は以下の 4 層で責務を明確に分離：

| 層 | 責務 | 外部依存性 | 例 |
|----|------|---------|-----|
| **Presentation** | UI・入力・表示 | Game, View | CameraController, InputController, HUD |
| **Game** | ゲームロジック | Data, Units | Systems, Events, GameManager |
| **Data** | データ定義・アクセス | Core/Utilities | Models, Repositories, PLATEAU |
| **Core** | インフラ共通機能 | (何にも依存しない) | Managers, Utilities, Handlers, Constants |

### 命名規則（統一済み）

```
【ファイル命名】
- ManagerClass:   GameSpeedManager.cs       (リソース・状態管理)
- SystemClass:    WeatherController.cs      (ゲームシステム)
- ControllerClass: PanelController.cs       (UI 制御)
- UtilityClass:   FileUtility.cs           (static メソッド集)
- HandlerClass:   ExceptionHandler.cs      (イベント処理)

【Namespace】
namespace CommonsUtility (統一 - 全層共通)

【フォルダ名】
複数形統一: Utilities, Systems, Units, Constants, Managers, Handlers
```

---

## 📁 現在のフォルダ構成（2026-01-26 確定版）

```
Assets/Scripts/
│
├── Presentation/               【層 1: プレゼンテーション層】✅
│   ├── UI/                     UI コンポーネント
│   │   ├── Controls/           UI ボタン・ウィジェット制御
│   │   ├── Dialogs/            ダイアログ・ウィンドウ
│   │   ├── HUD/                ゲーム中の常時表示情報
│   │   └── Panels/             メニュー・ゲーム情報パネル
│   ├── View/                   ビューロジック
│   │   ├── Cameras/            カメラ制御 (CameraController など)
│   │   ├── Rendering/          レンダリング・光処理
│   │   └── Effects/            エフェクト制御
│   └── Input/                  入力管理
│       ├── InputController.cs
│       └── PlayerInputs.cs
│
├── Game/                       【層 2: ゲームロジック層】✅
│   ├── GameManager/            ゲーム進行管理
│   │   ├── GameManager.cs
│   │   ├── DemController.cs
│   │   └── StageGoalController.cs
│   ├── Systems/                ゲームシステム
│   │   ├── Stage/
│   │   │   ├── TitleStartController.cs
│   │   │   └── UnitFireDisaster.cs
│   │   ├── Spawn/
│   │   │   └── SpawnController.cs        ✅ (SpawnCtrl → リネーム)
│   │   └── Weather/
│   │       ├── WeatherController.cs      ✅ (WeatherCtrl → リネーム)
│   │       ├── WindController.cs         ✅ (WindCtrl → リネーム)
│   │       └── PuddleController.cs       ✅ (PuddleCtrl → リネーム)
│   ├── Units/                  ユニット管理
│   │   ├── Base/               基本データ構造
│   │   ├── Shared/             共有オブジェクト
│   │   ├── Towers/             タワーシステム
│   │   ├── Enemies/            敵ユニット
│   │   ├── Items/              アイテムシステム
│   │   ├── Bullets/            発射物
│   │   └── Structures/         構造物
│   └── Events/                 ゲームイベント
│       ├── Environmental/      環境災害
│       │   ├── Burning.cs
│       │   ├── Raining.cs
│       │   ├── RainAbsorbController.cs   ✅ (RainAbsorbCtrl → リネーム)
│       │   └── RainDropsController.cs    ✅ (RainDropsCtrl → リネーム)
│       └── System/
│           └── EventLoader.cs
│
├── Data/                       【層 3: データ層】✅
│   ├── Models/                 ゲームデータ定義
│   │   ├── Structs/
│   │   └── Config/
│   │       └── LanguageConstants.cs      ✅ (LangConst → リネーム)
│   ├── Repositories/           データアクセス層
│   │   ├── StageRepository.cs            ✅ (StageDataManager → リネーム)
│   │   ├── StageYamlRepository.cs        ✅ (StagingYamlCtrl → リネーム)
│   │   └── LoadStreamingAsset.cs
│   └── Plateau/                PLATEAU SDK 統合
│       ├── Integration/
│       ├── Data/
│       └── Utilities/
│
└── Core/                       【オーソゴナル層: 共通機能】✅
    ├── Managers/               マネージャー群
    │   ├── InitializationManager.cs
    │   ├── GameSpeedManager.cs            ✅ (GameSpeedCtrl → リネーム)
    │   ├── LanguageManager.cs             ✅ (LangCtrl → リネーム)
    │   ├── NavMeshManager.cs              ✅ (NavMeshCtrl → リネーム)
    │   ├── SceneLoaderManager.cs          ✅ (SceneLoaderUtility → リネーム)
    │   ├── MaterialManager.cs
    │   └── PrefabManager.cs
    ├── Utilities/              ユーティリティ関数
    │   ├── FileUtility.cs                 ✅ (FileOperationUtility → リネーム)
    │   ├── GameObjectUtility.cs           ✅ (GameObjectTreat → リネーム)
    │   ├── MathUtility.cs                 ✅ (CommonsCalcs → リネーム)
    │   ├── LogUtility.cs
    │   ├── DebugUtility.cs                ✅ (新規追加)
    │   ├── XMLUtility.cs                  ✅ (XMLparser → リネーム)
    │   └── SpriteResourceLoader.cs
    ├── Handlers/               ハンドラー
    │   └── ExceptionHandler.cs            ✅ (新規追加)
    ├── Constants/              定数定義
    │   ├── GameConstants.cs
    │   └── GameEnum.cs
    ├── Helpers/                ヘルパー関数
    │   ├── UIHelper.cs
    │   └── CoroutineManager.cs            ✅ (CoroutineRunner → リネーム)
    └── Editor/                 Editor 拡張
        ├── Tools/
        ├── Windows/
        ├── Validators/
        └── Builders/
```

---

## 📋 リネーム実装記録（2026-01-26 完了）

### マネージャー系（状態・リソース管理）

| 旧クラス名 | 新クラス名 | 位置 | 理由 |
|----------|----------|------|------|
| GameSpeedCtrl | GameSpeedManager | Core/Managers/ | リソース管理 |
| LangCtrl | LanguageManager | Core/Managers/ | リソース管理 |
| NavMeshCtrl | NavMeshManager | Core/Managers/ | リソース管理 |
| SceneLoaderUtility | SceneLoaderManager | Core/Managers/ | リソース管理 |

### コントローラー系（UI・システム制御）

| 旧クラス名 | 新クラス名 | 位置 | 理由 |
|----------|----------|------|------|
| SpawnCtrl | SpawnController | Game/Systems/Spawn/ | UI・システム制御 |
| WeatherCtrl | WeatherController | Game/Systems/Weather/ | UI・システム制御 |
| PuddleCtrl | PuddleController | Game/Systems/Weather/ | UI・システム制御 |
| RainAbsorbCtrl | RainAbsorbController | Game/Events/Environmental/ | UI・システム制御 |
| RainDropsCtrl | RainDropsController | Game/Events/Environmental/ | UI・システム制御 |
| WindCtrl | WindController | Game/Systems/Weather/ | UI・システム制御 |

### ユーティリティ系（静的関数）

| 旧クラス名 | 新クラス名 | 位置 | 理由 |
|----------|----------|------|------|
| FileOperationUtility | FileUtility | Core/Utilities/ | 簡潔化 |
| GameObjectTreat | GameObjectUtility | Core/Utilities/ | 統一化 |
| CommonsCalcs | MathUtility | Core/Utilities/ | 統一化 |
| XMLparser | XMLUtility | Core/Utilities/ | 統一化 |

### リポジトリ系（データアクセス）

| 旧クラス名 | 新クラス名 | 位置 | 理由 |
|----------|----------|------|------|
| StageDataManager | StageRepository | Data/Repositories/ | Repository パターン |
| StagingYamlCtrl | StageYamlRepository | Data/Repositories/ | Repository パターン |

### 定数系（データ定義）

| 旧クラス名 | 新クラス名 | 位置 | 理由 |
|----------|----------|------|------|
| LangConst | LanguageConstants | Data/Models/Config/ | 定数命名統一 |

### ヘルパー系（インフラ）

| 旧クラス名 | 新クラス名 | 位置 | 理由 |
|----------|----------|------|------|
| CoroutineRunner | CoroutineManager | Core/Helpers/ | リソース管理 |

### 新規追加（Recovery フェーズ）

| クラス名 | 位置 | 目的 |
|---------|------|------|
| DebugUtility | Core/Utilities/ | Debug ログ統一管理 |
| ExceptionHandler | Core/Handlers/ | 例外処理統一 |

**合計**: 18 ファイルリネーム + 2 ファイル新規追加

---

## 🔄 フォルダ移動マッピング（実施済み）

### Phase 1: フォルダ再構成 ✅ 完了

| 旧フォルダ | 新フォルダ | ファイル数 | ステータス |
|-----------|----------|---------|---------|
| Utility | Core/Utilities + Core/Managers + Data/Repositories | 25 | ✅ 完了 |
| Utilities | Core/Utilities | 1 | ✅ 完了 |
| APP | Game + Core/Managers | 6 | ✅ 完了 |
| Models | Game/Units + Data/Models | 15 | ✅ 完了 |
| Tower | Game/Units/Towers | 15 | ✅ 完了 |
| Enemy | Game/Units/Enemies | ? | ✅ 完了 |
| Item | Game/Units/Items | ? | ✅ 完了 |
| GameEvents | Game/Events | 10 | ✅ 完了 |
| StageOrnaments | Presentation/View/Effects | ? | ✅ 完了 |
| View | Presentation/View | ? | ✅ 完了 |
| Player + Input | Presentation/Input | ? | ✅ 完了 |
| Bullet | Game/Units/Bullets | ? | ✅ 完了 |
| Stages | Game/Systems/Stage | ? | ✅ 完了 |
| Plateau | Data/Plateau | 6 | ✅ 完了 |

### Phase 2: Namespace 統一 ✅ 完了

- ✅ **CommonsUtility**: すべてのプロジェクトコードに統一
- ✅ **外部資産**: StarterAssets, PostProcessBuild の Namespace は保持

---

## 🎯 設計意図（なぜこの構成か）

### 1. **層構造による責務分離**

```
Presentation層 → Game層 → Data層 → Core層
     ↓            ↓         ↓
   表示          ロジック   保存       共通基盤
```

**メリット**:
- 各層が独立（テストしやすい）
- 新機能追加時の配置場所が明確
- レイヤー間の依存関係が一方向

### 2. **命名規則の統一**

```csharp
// Manager: リソース・状態管理
public class GameSpeedManager { }
public class LanguageManager { }

// Controller: UI・システム制御
public class SpawnController { }
public class WeatherController { }

// Utility: 静的関数集
public static class FileUtility { }
public static class MathUtility { }

// Repository: データアクセス
public class StageRepository { }
```

**メリット**:
- C# 標準命名規則に準拠
- 新規開発者が役割を理解しやすい
- IDE のインテリセンスが効果的

### 3. **Core 層の完全独立**

Core 層は他のすべての層から依存されるが、Core 層は何にも依存しない（オーソゴナル設計）

```
Game層 ─┐
Data層 ─┼→ Core層（何にも依存しない）
        └─Presentation層
```

**メリット**:
- Core の変更が他層に影響しない
- 共通機能が安定している

---

## 🚀 今後の方針（Prototype Phase）

### 短期（Week 1-4: 2026-02-末まで）

✅ **リカバリーフェーズ完了**
- フォルダ構成確定
- クラス名リネーム完了
- Namespace 統一完了

⏳ **プロトタイプ版完成に向けて**
- [ ] コンパイル検証（Phase 4）
- [ ] 最終検証（Phase 5）
- [ ] ゲーム性調整・QA
- [ ] v0.1.0-alpha リリース

### 中期（2026-03-以降: Alpha Phase）

- [ ] **細分化の検討**: Units/Towers, Units/Enemies などのさらなる細分化
- [ ] **新しいシステムの追加**: Physics, Audio, Animation など
- [ ] **テスト層の拡充**: Unit テスト・Integration テストの充実

### 長期（Beta/Release Phase）

- [ ] **機能拡張**: ゲーム性の拡張に応じたフォルダ追加
- [ ] **パフォーマンス最適化**: 大規模プロジェクト対応
- [ ] **ドキュメント整備**: API ドキュメント・開発ガイドの充実

---

## 📌 重要な原則（守ること）

### DO ✅

- ✅ 新規ファイルは指定されたフォルダに配置
- ✅ クラス名は役割に応じた suffix を使用 (Manager/Controller/Utility/Repository)
- ✅ Namespace は CommonsUtility に統一
- ✅ フォルダ名は複数形を使用 (Managers, Utilities, Systems)
- ✅ レイヤー間の依存は一方向（下層に依存するのは OK、上層に依存するのは NG）

### DON'T ❌

- ❌ 旧フォルダ構成に戻す（Utility/, Utilities/ 混在など）
- ❌ 旧クラス名を使用（GameSpeedCtrl など Ctrl 系）
- ❌ 旧 Namespace を使用（OnoCoro.Core.Managers など）
- ❌ Core 層に Game/Data 層の依存を追加
- ❌ フォルダ構成の大規模変更（Phase の承認なし）

---

## 📚 関連ドキュメント

- [architecture.md](architecture.md) - システムアーキテクチャ（詳細）
- [coding-standards.md](coding-standards.md) - C# コーディング基準
- [AGENTS.md](../AGENTS.md) - プロジェクト全体ルール
- [refactoring-completion-2026-01-26.md](refactoring-completion-2026-01-26.md) - リカバリーフェーズ完了報告書

---

## 📝 変更履歴

| 日付 | 変更内容 | 担当 |
|-----|--------|------|
| 2026-01-23 | 提案書作成 | AI |
| 2026-01-24 | フォルダ移行監査完了 | AI |
| 2026-01-26 | **リカバリーフェーズ完了・ファイルリネーム完了** | AI |
| 2026-01-26 | ドキュメント統合（proposal → completed） | AI |

---

**ステータス**: ✅ **リカバリーフェーズ完了 - Prototype Phase へ移行**

このドキュメントは現在の確定フォルダ構成を記録しています。大規模な構成変更は実施しない予定です。
