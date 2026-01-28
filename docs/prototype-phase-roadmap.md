# Prototype Phase ロードマップ

テストユーザーへのリリース（v0.1.0-alpha）を目標とした実装計画書

**目標リリース**: 2026-03-末  
**現在段階**: Phase 0 完了 → Phase 1 開始  
**最終更新**: 2026-01-27

## ✅ 完了済みフェーズ

| フェーズ | タスク | 完了日 | 詳細 |
|--------|--------|--------|------|
| **Recovery フェーズ** | 構成命名規則統一 | 2026-01-26 | フォルダ47個整理、クラス18ファイルリネーム、Namespace統一 |
| **Phase 0 ビルド検証** | コンパイルエラー排除 | 2026-01-26 | Assembly-CSharp エラー0、エディタ起動確認 |
| **Phase 0 PlayMode 検証** | ゲーム基本動作確認 | 2026-01-27 | ステージロード→プレイ→クリア判定動作確認 |
| **Phase 1.1 ログシステム** | LogUtility実装・統一 | 2026-01-27 | DebugLevel enum、ファイル出力、テストスクリプト完備 |
| **Phase 1.2 YAML バリデーション** | YamlValidator実装・検証 | 2026-01-29 | 5ファイル形式検証、Editor統合、既存YAML修復 |
| **Phase 1.3 Resources.Load統一化** | TextureLoader/TextAssetLoader/MaterialManager | 2026-01-29 | TextureResourceLoader、TextAssetLoader、CursorManager、MaterialManager統合 |
| **Phase 1.5 UI初期化順序管理** | UIControllerBase実装、Panels/Dialogs統合 | 2026-01-29 | IInitializable、IsInitializationRequired、9/22コンポーネント完成 |

---

## 🎯 Prototype Phase 全体スケジュール

```
✅ Phase 0: 構成命名規則統一 (完了)
   - Recovery フェーズ継続：フォルダ・命名規則統一
   - ビルド検証：コンパイルエラー排除
   - PlayMode 検証：ゲーム基本動作確認

✅ Phase 1: コア機能整備 (2026-01-27 ～ 2026-01-29 完了)
   - ✅ ログシステム統一化（完了）
   - ✅ YAML バリデーション実装（完了）
   - ✅ Resources.Load 統一化（完了）
   - ✅ UI 初期化順序管理（完了）
   - UI 改善（1920×1080標準化） - 計画中

📋 Phase 2: ステージ設計 & ゲーム性調整 (2026-03-初旬 ～ 2026-03-中旬)
   - 5ステージレベルデザイン
   - ゲームバランス調整
   - カメラ制御最適化

🎮 Phase 3: QA & リリース準備 (2026-03-中旬 ～ 2026-03-末)
   - 進行不能バグ排除
   - パフォーマンス確保
   - ドキュメント整備
   - v0.1.0-alpha リリース
```

---

## ✅ Phase 0: 構成命名規則統一 [完了]

**実装内容**:
- ✅ フォルダ再構成（47フォルダ整理）
- ✅ クラス名リネーム（18ファイル）
- ✅ Namespace 統一（CommonsUtility）
- ✅ クラス命名規則ドキュメント整備
- ✅ ビルド検証（コンパイルエラー 0）
- ✅ PlayMode 検証（ゲーム基本動作確認）

**詳細**: [AGENTS.md クラス命名規則](../AGENTS.md#クラス命名規則) 参照

**実施期間**: 2026-01-20 ～ 2026-01-27

---

## ✅ Phase 1.1: ログシステム統一化 [完了]

**実装内容**: 
- ✅ DebugLevel enum（5段階：Editor/Log/Warning/Error/None）実装
- ✅ LogUtility.cs のファイル出力機能（.log 生成）完成
- ✅ GameConfig による実行時ログレベル設定
- ✅ LogUtilityTest.cs による包括的テスト
- ✅ すべての Debug.Log を統一化
- ✅ ログファイル永続化（StreamWriter）完了

**詳細**: [Core/Utilities/Debug.cs](../../Assets/Scripts/Core/Utilities/Debug.cs) 参照

**実施期間**: 2026-01-20 ～ 2026-01-27  
**実施工数**: 9-12人日（完了）

---

## 🔄 Phase 1: コア機能整備 (2026-02-初旬 ～ 2026-02-末)

**概要**: ログシステムは完了済み。以下3つの機能を優先実装

### 1.1 YAML バリデーション システム [完了]

**目的**: ステージデータの不整合による進行不能バグを事前防止  
**優先度**:  🔴 最高  
**実施工数**: 6-8 人日  
**期間**: 2026-01-20 ～ 2026-01-29

#### 実装内容

| タスク | 詳細 | 期間 |
|--------|------|------|
| **YamlValidator クラス実装** | 検証ロジック実装 | 2-3日 |
| **Editor スクリプト作成** | 事前チェック UI | 2日 |
| **既存 YAML ファイル検証** | pathmakers, itemlists, stages, goals, boards | 2日 |

#### 検証項目

- pathmakers: マーカー名一意性、座標形式確認
- itemlists: item_name 定義、コスト/効果バランス
- stages: stage_id 一意性、参照マーカー確認
- goals: goal_type 妥当性確認
- boards: board_size、spawn_point 有効性確認

**チェックリスト**:
- [ ] YamlValidator クラス実装完了
- [ ] Editor スクリプトでの事前チェック機能
- [ ] 既存 YAML ファイルの検証修復完了

---

### 1.2 Resources.Load 統一化 [完了]

**目的**: アセット管理の一元化、メモリ効率化  
**優先度**:  最高  
**実施工数**: 6-8 人日
**期間**: 2026-01-20 ～ 2026-01-29

#### 実装内容

| タスク | 詳細 | 状態 |
|--------|------|------|
| **TextureResourceLoader** | Texture2D リソース一元化 | ✅ 完了 |
| **TextAssetLoader** | TextAsset リソース一元化 | ✅ 完了 |
| **MaterialManager リファクタリング** | Dictionary基底キャッシュ化 | ✅ 完了 |
| **CursorManager 新規作成** | カーソル管理統一 | ✅ 完了 |
| **PrefabManager 拡張** | UIプレファブ統合 | ✅ 完了 |
| **GlobalConst 拡張** | リソースパス定数化 | ✅ 完了 |

**完了事項**:
- [x] TextureResourceLoader クラス実装完了
- [x] TextAssetLoader クラス実装完了
- [x] PrefabManager へ統一完了
- [x] メモリプロファイルで改善確認

**参照**: [resources-load-refactoring.md](resources-load-refactoring.md)

---

### 1.5 UI 初期化順序管理 [完了]

**目的**: UI コンポーネント統一初期化、InitializationManager との自動連携  
**優先度**:  🔴 最高  
**実施工数**: 4-5 人日  
**期間**: 2026-01-29  
**完了日**: 2026-01-29 ✅

#### 実装内容

| タスク | 詳細 | 状態 |
|--------|------|------|
| **IInitializable インターフェース** | 初期化管理インターフェース実装 | ✅ 完了 |
| **UIControllerBase クラス** | UI コントローラー共通基底クラス実装 | ✅ 完了 |
| **IsInitializationRequired プロパティ** | 初期化戦略制御（必須 vs オンデマンド） | ✅ 完了 |
| **Panels（5個）統合** | EscMenuCtrl, TabMenuCtrl, NoticeCtrl, DebugInfoCtrl, SpawnMarkerPointerCtrl | ✅ 完了 |
| **Dialogs（4個）統合** | EventLogCtrl, GameTimerCtrl, InfoWindowCtrl, MessageBoxCtrl | ✅ 完了 |
| **ドキュメント** | ui-initialization-reference.md（リファレンス）作成 | ✅ 完了 |
| **PlayerInputs 修正** | WASD入力バグ修正（SetEscMenuOpen 追加） | ✅ 完了 |

#### 完了事項

- [x] IInitializable インターフェース実装完了
- [x] UIControllerBase 基底クラス実装完了（IsInitializationRequired デフォルト true）
- [x] Panels 5個すべてを UIControllerBase 継承に統合完了
- [x] Dialogs 4個すべてを UIControllerBase 継承に統合完了
- [x] 動的検出ロジック（GetComponentsInChildren<IInitializable>）実装完了
- [x] ui-initialization-reference.md リファレンスドキュメント作成完了
- [x] PlayerInputs.SetEscMenuOpen() メソッド追加（EscMenuCtrl との連携）

#### 実装統計

```
完成したコンポーネント: 9/22 (41%)
├─ Panels: 5/5 ✅
├─ Dialogs: 4/4 ✅
├─ HUD: 0/8 （プロトタイプ対象外）
└─ Controls: 0/5 （プロトタイプ対象外）

ドキュメント:
├─ ui-initialization-reference.md（リファレンス）
└─ prototype-phase-roadmap.md（本ドキュメント）

初期化フロー:
Awake（参照取得）→ Start（Initialize）→ IsInitialized = true（自動）
```

#### デバッグ方法

コンソール出力で初期化状況を確認:

```
[UIControllerBase] EscMenuCtrl 初期化完了
[UIControllerBase] TabMenuCtrl 初期化完了
[UIControllerBase] NoticeCtrl 初期化完了
[UIControllerBase] DebugInfoCtrl 初期化完了
[UIControllerBase] SpawnMarkerPointerCtrl 初期化完了
[UIControllerBase] EventLogCtrl 初期化完了
[UIControllerBase] GameTimerCtrl 初期化完了
[UIControllerBase] InfoWindowCtrl 初期化完了
[UIControllerBase] MessageBoxCtrl 初期化完了
```

**参照**: [docs/ui-initialization-reference.md](ui-initialization-reference.md)

---

## 📋 Phase 2: ステージ設計 & ゲーム性調整 (2026-03-初旬 ～ 2026-03-中旬)

### 2.1 ステージレベルデザイン（5段階）

**目的**: チュートリアル → 中級 → 上級への段階的進行  
**優先度**:  🔴 最高  
**想定工数**: 20-25 人日  
**期間**: 2026-03-初旬 ～ 2026-03-10

#### ステージ構成

| ステージ | 難易度 | 目的 | 工数 |
|---------|--------|------|------|
| **Stage 1: チュートリアル** | Easy | タワー配置基本操作 | 3日 |
| **Stage 2: 基本マップ** | Normal | 複数敵タイプ対応 | 4日 |
| **Stage 3: 複合マップ** | Normal | 複合タワー戦略 | 4日 |
| **Stage 4: 高難度マップ** | Hard | 全タイプ敵出現 | 5日 |
| **Stage 5: エクストラ** | Very Hard | リプレイ性強化 | 5日 |

#### ステージ設計チェックリスト

**すべてのステージで確認**:
- [ ] PLATEAU 地形データとの整合性
- [ ] スポーン点配置の妥当性
- [ ] タワー配置可能エリアの明確化
- [ ] ゴール到達不可能なバグがないか
- [ ] ウェーブ難易度のバランス
- [ ] パフォーマンス測定（FPS 60+ 安定）

---

### 2.2 ゲームバランス調整

**目的**: 難易度の適切な段階化、リプレイ性向上  
**優先度**:  🔴 最高  
**想定工数**: 10-12 人日

#### バランス調整項目

| 要素 | 調整内容 | 指標 |
|------|---------|------|
| **ユニット コスト** | タワーのコスト効果比 | 平均プレイ時間 10-15分 |
| **敵難易度** | 敵 HP速度 | クリア成功率 60-70% |
| **リソース配分** | 初期ゴールド増加率 | 中盤以降の選択肢確保 |

**テスト方法**:
```
各ステージ 10回プレイ
→ クリア率、平均時間を測定
→ データに基づいてパラメータ調整
```

**チェックリスト**:
- [ ] 全ステージ 10回プレイテスト完了
- [ ] コスト表調整完了
- [ ] 敵パラメータ調整完了

---

### 2.3 カメラ制御の最適化

**目的**: ユーザー体験向上、操作感改善  
**優先度**:  🟡 高  
**想定工数**: 4-5 人日

#### 実装内容

| タスク | 詳細 | 期間 |
|--------|------|------|
| **ズームレベル再調整** | 動作感速度の改善 | 2日 |
| **視界障害物対応** | 上下反転バグの修正 | 2日 |

**テスト項目**:
- [ ] ズーム速度が自然か
- [ ] 障害物衝突時に上下反転しないか
- [ ] 移動時のラグがないか

---

## 🎮 Phase 3: QA & リリース準備 (2026-03-中旬 ～ 2026-03-末)

### 3.1 進行不能バグの完全排除

**目的**: ゲーム進行を阻害するバグを 100% 解決  
**優先度**:  🔴 最高  
**想定工数**: 12-15 人日  
**期間**: 2026-03-中旬 ～ 2026-03-20

#### テスト項目

| バグカテゴリ | テスト内容 |
|-------------|----------|
| **クリア不可** | 敵が詰まる、タワー配置不可、ゴール到達不可 |
| **UI 非応答** | ボタン無反応、メニュー固定 |
| **データ破損** | ステージロード失敗、YAML パース失敗 |
| **クラッシュ** | NullReferenceException、OutOfMemory |

**テスト実行方法**:
```
Development Build で実行
→ すべてのステージ完全攻略プレイ
→ ログ確認（Error/Exception なし）
→ 異常終了なし
```

**チェックリスト**:
- [ ] 全ステージクリア可能
- [ ] 10時間以上連続プレイでクラッシュなし
- [ ] エラーログ出力なし

---

### 3.2 パフォーマンス最適化

**目的**: 60 FPS 安定、メモリ効率化  
**優先度**:  🔴 最高  
**想定工数**: 6-8 人日  
**期間**: 2026-03-20 ～ 2026-03-24

#### 測定項目

| 項目 | 目標値 |
|------|--------|
| **フレームレート** | 60 FPS（最小 45 FPS） |
| **メモリ使用量** | < 1GB |
| **ロード時間** | < 3秒/ステージ |

**チェックリスト**:
- [ ] Profiler で FPS 測定
- [ ] Memory Profiler で検証
- [ ] GameObject.Find() 廃止完了
- [ ] Texture 圧縮設定確認

---

### 3.3 ユーザー向けドキュメント

**目的**: テストユーザーの利便性向上  
**優先度**:  高  
**想定工数**: 3-4 人日

#### ドキュメント種別

| ドキュメント | 内容 |
|-------------|------|
| **README_Prototype.md** | ゲーム概要操作方法 |
| **GAMEPLAY_GUIDE.md** | ゲーム進行ガイド |
| **KNOWN_ISSUES.md** | 既知の問題回避方法 |
| **BUG_REPORT_TEMPLATE.md** | バグ報告テンプレート |
| **LOG_GUIDE.md** | ログファイル確認方法 |

**チェックリスト**:
- [ ] 5つのドキュメント作成完了
- [ ] GitHub Wiki に掲載

---

### 3.4 リリース前最終チェック

**目的**: 本番環境での動作確認  
**優先度**:  最高  
**想定工数**: 3-4 人日

#### チェックリスト

- [ ] **ビルド確認**
  - [ ] Release Build 成功
  - [ ] Windows 10/11 で起動確認
  
- [ ] **バージョン情報**
  - [ ] Version 0.1.0-alpha に設定
  - [ ] CHANGELOG.md 記載
  
- [ ] **バグ登録**
  - [ ] 発見バグを GitHub Issues に登録
  - [ ] Priority を設定

---

### 3.5 リリース & テストユーザー対応

**目的**: 安定した初期版配布  
**優先度**:  最高  
**想定工数**: 2-3 人日

#### リリースプロセス

```
1. Git tag: v0.1.0-alpha 作成
2. Release notes 作成
3. テストユーザー配布（GitHub Releases）
4. バグ報告チャネル設定
```

**チェックリスト**:
- [ ] v0.1.0-alpha tag 作成
- [ ] Release notes 作成完了
- [ ] テストユーザー配布完了

---

## 📊 工数配分（実績ベース）

```
✅ Phase 0: 構成命名規則統一 [完了]
  - Recovery フェーズ: 4-6人日
  - ビルド検証: 1-2人日
  - PlayMode 検証: 1-2人日
  【計: 6-10人日】

✅ Phase 1.1: ログシステム統一 [完了]
  - DebugLevel enum + LogUtility実装: 8-10人日
  - LogUtilityTest + ドキュメント: 1-2人日
  【計: 9-12人日】

✅ Phase 1.2: YAML バリデーション [完了]
  - YamlValidator実装 + Editor統合: 6-8人日

✅ Phase 1.3: Resources.Load 統一化 [完了]
  - TextureResourceLoader + TextAssetLoader + MaterialManager: 6-8人日

✅ Phase 1.5: UI 初期化順序管理 [完了]
  - UIControllerBase + IInitializable 実装: 3-4人日
  - Panels（5個）+ Dialogs（4個）統合: 1-2人日

🔄 Phase 1.4: UI 改善 (約 2-3 人日)
  - キャンバスレイアウト改善: 1-2人日
  - 1920×1080 標準化テスト: 1人日

📋 Phase 2: ステージ & ゲーム性 (約 30-37 人日)
  - ステージレベルデザイン: 20-25人日
  - ゲームバランス調整: 10-12人日

🎮 Phase 3: QA & リリース (約 24-32 人日)
  - 進行不能バグ排除: 12-15人日
  - パフォーマンス最適化: 6-8人日
  - ドキュメント整備: 3-4人日
  - リリース: 2-3人日
  - 最終チェック: 3-4人日

【進行中までの実績】約 25-36 人日
【残り工程】約 56-72 人日
【合計】約 81-108 人日（1人チームで 3-5 週間）
```

---

## 🎯 成功指標

| 指標 | 目標値 |
|------|--------|
| **ゲームプレイ時間** | 10-15分/ステージ |
| **クリア成功率** | 初見 50-60% |
| **クラッシュ率** | 0% |
| **フレームレート** | 60 FPS (45 FPS最小) |
| **メモリ使用量** | < 1 GB |
| **ステージ数** | 5（チュートリアル含む） |
| **ドキュメント** | 5ファイル以上 |

---

## 📚 関連ドキュメント

| ドキュメント | 目的 |
|------------|------|
| [AGENTS.md](../AGENTS.md) | プロジェクト開発ガイドライン |
| [docs/architecture.md](architecture.md) | システムアーキテクチャ（4層構造） |
| [docs/coding-standards.md](coding-standards.md) | C# コーディング規約 |
| [docs/scripts-folder-structure-completed.md](scripts-folder-structure-completed.md) | フォルダ構成 |
| [docs/ui-initialization-reference.md](ui-initialization-reference.md) | UI初期化リファレンス（Phase 1.5） |

---

## 📌 進捗サマリー

| 項目 | 状態 | 進捗 |
|------|------|------|
| **Phase 0 完了** | ✅ 完了 | 100% |
| **Phase 1.1 (ログシステム)** | ✅ 完了 | 100% |
| **Phase 1.2 (YAML バリデーション)** | ✅ 完了 | 100% |
| **Phase 1.3 (Resources.Load)** | ✅ 完了 | 100% |
| **Phase 1.5 (UI初期化順序管理)** | ✅ 完了 | 100% |
| **Phase 1.4 (UI改善)** | 🔄 計画中 | 0% |
| **Phase 2 (ステージ)** | 📋 計画中 | 0% |
| **Phase 3 (QA/リリース)** | 📋 計画中 | 0% |
| **総合進捗** | - | ~32-35% |

**Phase 1.5 完成内容**:
- UIControllerBase + IInitializable インターフェース実装
- Panels（5個）+ Dialogs（4個）統合完了
- 動的検出ロジック（GetComponentsInChildren）実装
- WASD入力バグ修正（PlayerInputs.SetEscMenuOpen）
- ui-initialization-reference.md ドキュメント作成

**次のマイルストーン**: 2026-02-初旬に Phase 1.4 開始（UI改善：1920×1080標準化）

**ロードマップ最終更新**: 2026-01-29
