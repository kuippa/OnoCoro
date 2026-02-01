# Prototype Phase 開発ロードマップ

**目標**: v0.1.0-alpha リリース（テストユーザー向け）  
**期間**: 2026-01 ～ 2026-03  
**現在**: Phase 1 実装中（2026-02-02 時点）

---

## 全体スケジュール

```
✅ Phase 0: 構成命名規則統一 (完了: 2026-01-27)
   └─ Recovery フェーズ、ビルド検証、PlayMode 検証

✅ Phase 1: コア機能整備 (進行中: 2026-02-02)
   ├─ ✅ ログシステム統一化（完了）
   ├─ ✅ YAML バリデーション（完了）
   ├─ ✅ Resources.Load 統一化（完了）
   ├─ ✅ UI 初期化順序管理（完了）
   ├─ ✅ ゲーム速度制御（完了）
   ├─ ✅ UICanvasManager + Canvas Scaler（完了）
   └─ 📋 ドキュメント整備（進行中）

📋 Phase 2: ステージ設計・ゲーム性調整 (予定: 2026-03-初旬)
   ├─ 5 ステージレベルデザイン
   ├─ ゲームバランス調整
   └─ カメラ制御最適化

🎮 Phase 3: QA・リリース準備 (予定: 2026-03-中旬～末)
   ├─ 進行不能バグ排除
   ├─ パフォーマンス確保（60 FPS 維持）
   ├─ ドキュメント最終化
   └─ v0.1.0-alpha リリース
```

---

## ✅ Phase 0: 構成命名規則統一（完了）

**期間**: 2026-01-20 ～ 2026-01-27  
**状態**: [OK] 完了

| タスク | 詳細 | 完了日 |
|--------|------|--------|
| フォルダ再構成 | 47 フォルダ整理・4 層アーキテクチャ確立 | 2026-01-26 |
| クラス名リネーム | 18 ファイル（Ctrl → Manager/System/Controller） | 2026-01-26 |
| Namespace 統一 | すべて `CommonsUtility` に統一 | 2026-01-26 |
| ビルド検証 | Assembly-CSharp エラー 0 | 2026-01-26 |
| PlayMode 検証 | ゲーム基本動作確認（ステージ選択→プレイ→クリア） | 2026-01-27 |

**ドキュメント**:
- [AGENTS.md - Class Naming Convention](../../AGENTS.md#クラス命名規則)
- [docs/scripts-folder-structure-completed.md](../scripts-folder-structure-completed.md)

---

## ✅ Phase 1: コア機能整備（進行中）

**期間**: 2026-01-27 ～ 2026-02-末  
**状態**: [WARN] 進行中・ドキュメント整備フェーズ

### 完了済みタスク

#### 1.1 ログシステム統一化（完了）

| 要素 | 詳細 |
|------|------|
| **DebugLevel enum** | 5 段階（Editor/Log/Warning/Error/None）実装 |
| **LogUtility** | ファイル出力・ログレベル制御 |
| **GameConfig** | 実行時ログレベル設定 |
| **テスト** | LogUtilityTest.cs で包括的テスト |

#### 1.2 YAML バリデーション（完了）

| ファイル | 検証内容 | 状態 |
|---------|---------|------|
| **YamlValidator.cs** | YAML 形式検証ロジック実装 | ✅ |
| **pathmakers.yaml** | マーカー一意性・座標形式 | ✅ |
| **itemlists.yaml** | item_name 定義・バランス | ✅ |
| **stages.yaml** | stage_id 一意性・参照確認 | ✅ |
| **goals.yaml** | goal_type 妥当性確認 | ✅ |
| **boards.yaml** | board_size・spawn_point 有効性 | ✅ |

#### 1.3 Resources.Load 統一化（完了）

| クラス | 用途 | 状態 |
|--------|------|------|
| **TextureResourceLoader** | Texture2D キャッシング | ✅ |
| **TextAssetLoader** | TextAsset キャッシング | ✅ |
| **MaterialManager** | Material キャッシング | ✅ |
| **PrefabManager** | Prefab キャッシング | ✅ |
| **CursorManager** | カーソル管理 | ✅ |
| **GlobalConst** | リソースパス定数化 | ✅ |

#### 1.5 UI 初期化順序管理（完了）

| コンポーネント | 状態 |
|-------------|------|
| **IInitializable interface** | ✅ |
| **UIControllerBase base class** | ✅ |
| **Panels（5 個）** | ✅ |
| **Dialogs（4 個）** | ✅ |
| **InitializationManager 統合** | ✅ |

#### 1.6 ゲーム速度制御（完了）

| 機能 | 詳細 |
|------|------|
| **GameSpeedManager** | Time.timeScale 連携 |
| **SetGameSpeed()** | コールバック機能 |
| **デバッグパネル** | 自動更新 |

#### 1.4 UICanvasManager + Canvas Scaler（完了）

| 機能 | 詳細 |
|------|------|
| **UICanvasManager** | Canvas Scaler 一元設定 |
| **マルチ解像度対応** | 1920×1080 基準 |
| **WorldSpace Canvas 保持** | 3D UI 形式を変更しない |
| **InitializationManager Phase 3** | 自動初期化 |

### 進行中タスク

#### ドキュメント整備（進行中）

| ドキュメント | 完了度 | 状態 |
|-------------|--------|------|
| [docs/project-rules/](../project-rules/) | [OK] 100% | ✅ 完成 |
| [docs/architecture/](../architecture/) | [OK] 100% | ✅ 完成 |
| [docs/vision/](../vision/) | [OK] 100% | ✅ 完成 |
| [docs/reference/](../reference/) | [PENDING] 0% | 📋 作成予定 |
| [docs/archive/](../archive/) | [PENDING] 0% | 📋 作成予定 |

---

## 📋 Phase 2: ステージ設計・ゲーム性調整（予定）

**期間**: 2026-03-01 ～ 2026-03-15  
**目安**: 10 日間

### 2.1 5 ステージレベルデザイン

| ステージ | 難易度 | 概要 | 推定工数 |
|---------|--------|------|---------|
| **Stage 1: 導入** | Easy | 基本システム習得 | 1 日 |
| **Stage 2: 初級** | Easy | 複合防衛戦略 | 1.5 日 |
| **Stage 3: 中級** | Normal | リソース管理重視 | 1.5 日 |
| **Stage 4: 上級** | Hard | 複雑なマップ設計 | 2 日 |
| **Stage 5: チャレンジ** | Very Hard | 最高難易度 | 2 日 |

### 2.2 ゲームバランス調整

- 敵スポーン間隔・パターン最適化
- タワー効果・コスト・範囲バランス
- リソース獲得量・消費量の調整
- ウェーブ難易度曲線の設計

### 2.3 カメラ制御最適化

- Cinemachine による自動追従
- ズーム・パン操作の最適化
- UI 非表示領域での視認性確保

---

## 🎮 Phase 3: QA・リリース準備（予定）

**期間**: 2026-03-16 ～ 2026-03-31  
**目安**: 15 日間

### 3.1 進行不能バグ排除

| チェック項目 | 完了 |
|-------------|------|
| ゲーム起動 エラーなし | [ ] |
| ステージ選択・ロード | [ ] |
| プレイフロー全工程 | [ ] |
| クリア・ゲームオーバー判定 | [ ] |
| リスタート・メニュー戻り | [ ] |
| セーブ・ロード機能 | [ ] |

### 3.2 パフォーマンス確保

| 指標 | 目標 | 測定 |
|------|------|------|
| **FPS** | 60 以上（PC） | [ ] |
| **メモリ** | < 1GB | [ ] |
| **起動時間** | < 30 秒 | [ ] |
| **GC pause** | < 50ms | [ ] |

### 3.3 ドキュメント最終化

- [docs/](../) 全ドキュメント整備
- README.md 充実化
- インストール・実行ガイド作成
- API リファレンス完備

### 3.4 v0.1.0-alpha リリース

```
v0.1.0-alpha
├─ Git tag: v0.1.0-alpha
├─ Release notes: 機能一覧・既知バグ
├─ ビルド配布: Unity build output
└─ テストユーザー向け公開
```

---

## 進捗管理

### 現在の進捗

```
Phase 0: ████████████████████░░░░░░░░░░ 100% (完了)
Phase 1: ██████████████░░░░░░░░░░░░░░░░ 70% (進行中)
Phase 2: ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0%
Phase 3: ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0%

全体: ██████████░░░░░░░░░░░░░░░░░░░░░░ 28%
```

### マイルストーン

| マイルストーン | 目標日 | 状態 | 備考 |
|-------------|--------|------|------|
| Phase 0 完了 | 2026-01-27 | ✅ | 予定通り |
| Phase 1 完了（ドキュメント）| 2026-02-15 | [WARN] 進行中 | ドキュメント整備が主 |
| Phase 2 開始 | 2026-03-01 | 📋 予定 | 後続フェーズ開始予定 |
| Phase 3 開始 | 2026-03-16 | 📋 予定 | リリース準備 |
| v0.1.0-alpha リリース | 2026-03-31 | 📋 予定 | 最終目標 |

---

## チェックリスト

### Phase 1 完了確認

- [x] ログシステム統一化
- [x] YAML バリデーション
- [x] Resources.Load 統一化
- [x] UI 初期化順序管理
- [x] ゲーム速度制御
- [x] UICanvasManager + Canvas Scaler
- [x] project-rules/ ドキュメント整備
- [x] architecture/ ドキュメント整備
- [x] vision/ ドキュメント整備

### Phase 2 準備

- [ ] ステージレベルデザイン仕様確定
- [ ] ゲームバランス目標値設定
- [ ] カメラ制御要件定義

### Phase 3 準備

- [ ] テストケース定義
- [ ] パフォーマンス測定方法確立
- [ ] リリースチェックリスト作成

---

## 関連資料

- [introduction.md](introduction.md) - プロジェクト概要
- [../project-rules/](../project-rules/) - 実装ルール
- [../architecture/](../architecture/) - システム設計
- [AGENTS.md](../../AGENTS.md) - プロジェクト全体ルール
