# CHANGELOG

OnoCoro プロジェクトの変更履歴です。

---

## [0.1.0-alpha] - 2026-03-08

**前回リリース**: 0.0.6 (2026-01-26)

### [NEW] 新機能

#### ゲームシステム
- **Water System Wind Speed Control** - Water Surface が WindController と統合され、風速に応じた波の動きを実装
- **TreeSakura Ornament System** - bloom_sakura イベントで桜開花演出実装
- **PollutantManager** - ごみ数表示を static class で一元管理
- **Enemy Litter** - ナビゲーションシステム統合、タイムアウト検出、DustBox破壊機能

#### UI・ゲーム制御
- **UIControllerBase** - Panel・Dialog の初期化ベースクラス導入
- **Game Speed Debugging** - Time.timeScale 統合、デバッグパネル自動更新機能
- **Path Tracking** - off_bloom_path_complete イベントで敵生命周期管理実装
- **YAML Route 互換性** - Factory/Spawner パターンでルート名互換性実装

### [FIX] バグ修正

#### Critical
- **(2026-03-08) シーン遷移時 itemlist 更新されない** - `StageYamlRepository._ItemList.Clear()` を追加 #e31348b4
  - 影響: 複数シーン間でユニット作成メニューのアイテムがシーンごとに正しく更新される
  - ItemCreateCtrl も `static _UIItemCreate` をインスタンス変数に変更し MissingReferenceException 対策

#### Major
- **(2026-02-27) GarbageCount 表示されない** - TextMeshProUGUI 参照をシーン遷移時にリフレッシュ
- **(2026-02-11) Stage Goal 判定** - static フラグリセット漏れ & UINotice 自動閉じ機能追加
- **(2026-02-07) Raycast QueryTriggerInteraction** - IgnoreTriggersを全 Physics.Raycast に統一（衝突判定の安定化）

#### Minor
- Enemy Litter タイムアウト検出と破壊フォールバック改善
- Naraku 落下フォールバック改善 + Y位置プレイヤースポーン追従
- CircularIndicator Prefab パス修正

### [REFACTOR] リファクタリング

#### Code Quality
- **(2026-03-08) ItemCreateCtrl 全体リファクタリング** - 責任範囲を明確化
  - `RegisterButtonListeners()` - ボタン登録のみに限定
  - `RebuildItemList()` - 最新データでリスト再構築（SwitchActive 時）
  - `RefreshView()` - SetActive 後に画面更新（アイコン再描画保証）
- **TriggerHandler Refactoring** - 12個すべての trigger handler 実装完了
- **Resources.Load 統一化** - PrefabManager へ全コード統一完了
- **YAML コマンド処理の統一** - Validation を簡素化

#### Architecture
- **Factory/Spawner パターン** - Enemy・Unit 生成の統一化
- **UI Canvas 統一** - UICanvasManager で Canvas 設定一元管理
- **Debug Logger** - Debug alias を CommonsUtility.Debug に統一

### [DOCS] ドキュメント

#### 新規作成
- `docs/architecture/camera-exposure-settings.md` - カメラ・Depth of Field トラブルシューティング
- `docs/architecture/camera-deoccluder-implementation.md` - CinemachineDeoccluder 実装ガイド

#### 更新
- AGENTS.md - スリム化・ドキュメント構造整理
- prototype-phase-roadmap.md - Phase 2-3 進捗追跡
- TODO.md - carrera地面潜り対策 TODO 追加

### [PERF] パフォーマンス

- Depth of Field 無効化による描画最適化（HDRP 17.3.0+ 対応）
- Sweep movement physics 統合による Sweeper 移動滑らか化

### [KNOWN_ISSUES] 既知の問題

- [ ] マップの端から落ちる可能性がある（TODO リストにあり）
- [ ] Fire イベント延焼範囲の表示が未実装
- [ ] ユニットアップグレード機能が未実装
- [ ] ゲーム効果音・BGM が未実装
- [ ] セーブ機能が未実装

### 技術基盤

| 項目 | バージョン |
|------|----------|
| **Unity** | 6.3.10f1 |
| **Cinemachine** | 3.1.6 |
| **HDRP** | 17.3.0 |
| **PLATEAU SDK** | Latest |

詳細なパッケージ構成と開発環境設定は [docs/BUILD_ENVIRONMENT.md](docs/BUILD_ENVIRONMENT.md) を参照。

---

## [0.0.6] - 2026-01-26

Phase 0 初期ビルド版。エディター参照削除、設定更新実施。

---

## リリース対象シーン

- TitleScene
- 石川県金沢市兼六園 （チュートリアルステージ）
- 三鷹井の頭 （Wave テストステージ）

---

## テスト対象機能

### ✓ 確認済み
- [x] マルチシーン遷移・UI初期化
- [x] itemlist シーン別更新
- [x] YAML イベント・パス読み込み
- [x] ユニット作成・配置
- [x] Enemy Litter スポーン・移動
- [x] カメラ制御（FPS/TPS/LongShot/BirdView）
- [x] Sweeper 移動・掃除

### ⏳ 未確認（テストユーザー向け検証必要）
- [ ] GPU 互換性（Intel/NVIDIA/AMD）
- [ ] 低仕様環境での FPS 安定性
- [ ] 長時間連続プレイ（メモリリーク検証）

---

## テストユーザー向け報告方法

GitHub Issues で以下の形式で報告をお願いします：

```
**Title**: [BUG] 現象を一行で説明

**Environment**:
- Windows バージョン
- GPU モデル
- ビルド番号

**Reproduction**:
1. 手順1
2. 手順2

**Expected vs Actual**:
期待: xxx
実際: yyy

**Log**:
(ログファイルまたはスクリーンショット)
```

ログファイル: `OnoCoro_Data/output_log.txt`

---

## 導入手順

1. `OnoCoro.zip` を解凍
2. `OnoCoro.exe` を実行
3. シーン選択画面から「石川県金沢市兼六園」を選択
4. TAB キーでユニット作成メニュー表示
5. ユニットを配置してテスト

---

**公式リポジトリ**: https://github.com/kuippa/OnoCoro  
**ライセンス**: MIT License
