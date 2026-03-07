# OnoCoro v0.1.0-alpha - Prototype Build

**Release Date**: 2026-03-08  
**Tag**: `prototype2026.03.08.01.40`  
**Version**: v0.1.0-alpha

---

## 概要

OnoCoro プロトタイプビルド第 2 段階です。前回リリース（v0.0.6, 2026-01-26）から約 1.5 ヶ月にわたり、新機能実装、バグ修正、リファクタリングを進めました。

本ビルドでは **複数シーン間のユニットメニュー更新問題の根本解決** と **ItemCreateCtrl の責任分離** を実施。ゲームシステムの安定性が向上しました。

### ゲーム概要

> 国土交通省の PLATEAU を使った、まちづくりをテーマにしたタワーディフェンスゲーム

**ストーリー**: 都市のゴミ問題を解決するため、清掃ロボットを配置して汚染物質を撃退し、建物を修復するタワーディフェンス。

---

## ダウンロード

| 形式 | リンク |
|------|--------|
| **Standalone PC (EXE)** | [OnoCoro_v0.1.0-alpha.zip](https://github.com/kuippa/OnoCoro/releases/download/v0.1.0-alpha/OnoCoro_v0.1.0-alpha.zip) |
| **ソースコード (ZIP)** | GitHub リポジトリ から Clone |
| **ソースコード (TAR)** | GitHub リポジトリ から Clone |

---

## インストール・実行

### 方法 1: EXE ファイル（推奨）

1. `OnoCoro_v0.1.0-alpha.zip` をダウンロード
2. 任意のフォルダに解凍
3. `OnoCoro.exe` をダブルクリック

**動作環境:**
- OS: Windows 10 / 11
- RAM: 8GB 以上推奨
- GPU: VRAM 2GB 以上

### 方法 2: ソースコードからビルド（開発者向け）

```powershell
# リポジトリクローン
git clone https://github.com/kuippa/OnoCoro.git
cd OnoCoro

# Unity 6.3.10f1 で開く
unity -projectPath . -openFile

# Editor で File > Build and Run
```

詳細: [README.md](https://github.com/kuippa/OnoCoro#インストール) 参照

---

## 新機能 [NEW]

### ゲームシステム

- **Water System Wind Speed Control**
  - Water Surface が WindController と統合
  - 風速に応じた波の動きを実装

- **TreeSakura Ornament System**
  - `bloom_sakura` イベントで桜開花演出実装
  - 季節演出の拡張ポイント

- **PollutantManager**
  - ごみ数表示を static class で一元管理
  - UI リアルタイム更新

- **Enemy Litter**
  - ナビゲーションシステム統合
  - タイムアウト検出、DustBox破壊機能

### UI・ゲーム制御

- **UIControllerBase**
  - Panel・Dialog の初期化ベースクラス導入
  - UI 責任分離の基盤

- **Game Speed Debugging**
  - Time.timeScale 統合
  - デバッグパネル自動更新機能

- **Path Tracking**
  - `off_bloom_path_complete` イベントで敵生命周期管理実装

- **YAML Route 互換性**
  - Factory/Spawner パターンでルート名互換性実装

---

## バグ修正 [FIX]

### Critical

**2026-03-08 - シーン遷移時 itemlist が更新されない（#e31348b4）**

**問題**: Scene A (itemlist: [Sweeper, GarbageCube]) → Scene B (既存の Sweeper, GarbageCube に加えて新規アイテムが追加) - 古いデータが蓄積

**原因**: `StageYamlRepository._ItemList` が static List で、シーン遷移時に Clear() されていなかった

**解決**:
- `StageYamlRepository.LoadYamlData()` に `_ItemList.Clear()` を追加
- `EnvironmentalYamlProvider.LoadItemLists()` に `itemList.Clear()` を追加
- `ItemCreateCtrl._UIItemCreate` を `static` → `instance` に変更し MissingReferenceException も対策

**影響**: 複数シーン間でユニット作成メニューのアイテムがシーンごとに正しく更新される

### Major

- **(2026-02-27) GarbageCount 表示されない**
  - TextMeshProUGUI 参照をシーン遷移時にリフレッシュ

- **(2026-02-11) Stage Goal 判定エラー**
  - static フラグリセット漏れを修正
  - UINotice 自動閉じ機能追加

- **(2026-02-07) Raycast QueryTriggerInteraction**
  - IgnoreTriggers を全 Physics.Raycast に統一（衝突判定の安定化）

### Minor

- Enemy Litter タイムアウト検出と破壊フォールバック改善
- Naraku 落下フォールバック改善 + Y 位置プレイヤースポーン追従
- CircularIndicator Prefab パス修正

---

## リファクタリング [REFACTOR]

### Code Quality

**ItemCreateCtrl 全体リファクタリング (2026-03-08)**

責任範囲を明確化：

- `RegisterButtonListeners()` - ボタン登録のみに限定
- `RebuildItemList()` - 最新データでリスト再構築（SwitchActive 時）
- `RefreshView()` - SetActive 後に画面更新（アイコン再描画保証）

**その他**

- **TriggerHandler Refactoring** - 12個すべての trigger handler 実装完了
- **Resources.Load 統一化** - PrefabManager へ全コード統一
- **YAML コマンド処理の統一** - Validation を簡素化

### Architecture

- **Factory/Spawner パターン** - Enemy・Unit 生成の統一化
- **UI Canvas 統一** - UICanvasManager で Canvas 設定一元管理
- **Debug Logger** - Debug alias を CommonsUtility.Debug に統一

---

## ドキュメント [DOCS]

### 新規作成

- `docs/architecture/camera-exposure-settings.md` - カメラ・Depth of Field トラブルシューティング
- `docs/architecture/camera-deoccluder-implementation.md` - CinemachineDeoccluder 実装ガイド
- `docs/BUILD_ENVIRONMENT.md` - Unity 6.3.10f1 パッケージ完全仕様書

### 更新

- `AGENTS.md` - スリム化・ドキュメント構造整理
- `prototype-phase-roadmap.md` - Phase 2-3 進捗追跡
- `TODO.md` - carrera地面潜り対策 TODO 追加
- `README.md` - プロトタイプリリース用に全面更新
- `CHANGELOG.md` - v0.1.0-alpha の詳細変更履歴

---

## パフォーマンス [PERF]

- **Depth of Field 無効化** による描画最適化（HDRP 17.3.0+ 対応）
- **Sweep movement physics 統合** による Sweeper 移動滑らか化

---

## テスト対象機能

### ✓ 確認済み

- [x] マルチシーン遷移・UI初期化
- [x] itemlist シーン別更新（本ビルドで修正）
- [x] YAML イベント・パス読み込み
- [x] ユニット作成・配置
- [x] Enemy Litter スポーン・移動
- [x] カメラ制御（FPS / TPS / LongShot / BirdView）
- [x] Sweeper 移動・掃除

### ⏳ 未確認（テストユーザー向け検証必要）

- [ ] GPU 互換性（Intel / NVIDIA / AMD）
- [ ] 低仕様環境での FPS 安定性
- [ ] 長時間連続プレイ（メモリリーク検証）

---

## 既知の問題 [KNOWN_ISSUES]

| 問題 | 状態 | 予定 |
|------|------|------|
| マップの端から落ちる可能性 | ⏳ | Phase 3 で修正予定 |
| Fire イベント延焼範囲の表示 | ❌ 未実装 | Phase 3 機能追加 |
| ユニットアップグレード機能 | ❌ 未実装 | Phase 3 機能追加 |
| ゲーム効果音・BGM | ❌ 未実装 | Phase 3 機能追加 |
| セーブ機能 | ❌ 未実装 | Phase 3 以降検討 |

詳細: [KNOWN_ISSUES.md](https://github.com/kuippa/OnoCoro/blob/main/KNOWN_ISSUES.md)

---

## リリース対象シーン

| シーン | 説明 | ステージ選択 |
|--------|------|-----------|
| **TitleScene** | タイトル・ステージ選択 | メインメニュー |
| **Kanazawa Kenroku-en** | 石川県金沢市兼六園 | チュートリアルステージ |
| **Mitaka Inokashira** | 三鷹井の頭 | Wave テストステージ |

---

## 基本操作

| 操作 | 機能 |
|------|------|
| **WASD** | 移動 |
| **Shift + WASD** | 走行 |
| **マウス** | 視点操作 |
| **マウスホイール** | ズーム |
| **SPACE** | ジャンプ |
| **TAB** | ユニット作成メニュー |
| **1-5** | ユニット選択 |
| **F2** | 一時停止（デバッグ） |
| **F3-F5** | 時間倍速（デバッグ） |

---

## 技術仕様

### 開発環境

| 項目 | バージョン |
|------|----------|
| **Unity Engine** | 6.3.10f1 |
| **Cinemachine** | 3.1.6 |
| **HDRP (Render Pipeline)** | 17.3.0 |
| **PLATEAU SDK** | Latest |
| **Input System** | 1.18.0 |
| **Visual Effect Graph** | 17.3.0 |

**詳細な開発環境・全パッケージ仕様**: [docs/BUILD_ENVIRONMENT.md](https://github.com/kuippa/OnoCoro/blob/main/docs/BUILD_ENVIRONMENT.md)

### システム要件

**最小要件:**
- OS: Windows 10以上
- CPU: Intel Core i5-8400 相当
- RAM: 8GB
- GPU: VRAM 2GB以上

**推奨スペック:**
- OS: Windows 11
- CPU: Intel Core i7-12700K / AMD Ryzen 7 5800X 相当
- RAM: 16GB以上
- GPU: VRAM 4GB以上

---

## テストユーザー向け情報

### バグ報告方法

[GitHub Issues](https://github.com/kuippa/OnoCoro/issues) にて以下のテンプレートで報告をお願いします：

```
**Title**: [BUG] 現象を一行で説明

**Environment**:
- Windows バージョン:
- GPU モデル:
- ビルド番号: prototype2026.03.08.01.40

**Reproduction**:
1. 手順1
2. 手順2
3. 現象発生

**Screenshots**:
(スクリーンショット添付)

**Logs**:
(Editor.log からの関連部分)
```

### アンケート

プレイ後のアンケートも大歓迎です：
- ゲームバランス（難度、報酬のバランス）
- UI/UX（操作感、メニュー構成）
- グラフィックス（フレームレート、描画品質）
- その他ご意見・ご要望

---

## 開発者向け情報

### ソースコード

- **Repository**: https://github.com/kuippa/OnoCoro
- **Branch**: main
- **Commit**: e31348b4

### 開発ドキュメント

- [README.md](https://github.com/kuippa/OnoCoro/blob/main/README.md) - プロジェクト概要
- [AGENTS.md](https://github.com/kuippa/OnoCoro/blob/main/AGENTS.md) - コーディング基準（必読）
- [CHANGELOG.md](https://github.com/kuippa/OnoCoro/blob/main/CHANGELOG.md) - 詳細な変更履歴
- [docs/](https://github.com/kuippa/OnoCoro/tree/main/docs) - アーキテクチャ・設計ドキュメント

### プロジェクト構成

```
Assets/Scripts/
├── Presentation/     UI・カメラ・入力制御
├── Game/             ゲームロジック・ユニット
├── Data/             YAML・PLATEAU・ステージデータ
└── Core/             マネージャー・ユーティリティ
```

**4-Layer Architecture** により責任範囲を明確化。詳細: [docs/architecture.md](https://github.com/kuippa/OnoCoro/blob/main/docs/architecture.md)

---

## 次のマイルストーン

### Phase 3 (2026-04月予定)

- [ ] Fire イベント延焼表示実装
- [ ] ユニットアップグレード機能実装
- [ ] マップ端落下についての改善
- [ ] セーブ機能実装（ローカル）

**進捗**: [prototype-phase-roadmap.md](https://github.com/kuippa/OnoCoro/blob/main/docs/vision/prototype-phase-roadmap.md)

---

## ライセンス

MIT License - [LICENSE](https://github.com/kuippa/OnoCoro/blob/main/LICENSE)

---

## コミュニティ

- 🐛 [バグ報告](https://github.com/kuippa/OnoCoro/issues)
- 💬 [Discussions](https://github.com/kuippa/OnoCoro/discussions)
- 📺 [開発ライブ配信](https://www.youtube.com/playlist?list=PLxWlv9T7cA6YhDW4aLlfn6BCZQFYPrOJ3)

---

**感謝**: 本プロジェクトの復旧とリリースを可能にしていただいたすべてのコントリビューターとテストユーザーの皆様に感謝申し上げます。

**作成日**: 2026-03-08  
**バージョン**: v0.1.0-alpha  
**Tag**: `prototype2026.03.08.01.40`
