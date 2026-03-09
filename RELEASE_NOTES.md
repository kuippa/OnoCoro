# OnoCoro v0.0.21-prototype - Prototype Build

**Release Date**: 2026-03-10  
**Tag**: `v0.0.21-prototype`  
**Version**: v0.0.21-prototype
**Build Number**: 21

---

## 概要

OnoCoro プロトタイプビルド。前回リリース（v0.0.6, 2026-01-26）からの開発成果をまとめました。

本ビルドでは **パフォーマンス診断インフラ実装** と **複数シーン間のユニットメニュー更新問題の根本解決** を実施。ゲームシステムの安定性が向上しました。

### ゲーム概要

> 国土交通省の PLATEAU を使った、まちづくりをテーマにしたタワーディフェンスゲーム

**ストーリー**: 都市のゴミ問題を解決するため、清掃ロボットを配置して汚染物質を撃退し、建物を修復するタワーディフェンス。

---

## ダウンロード

| 形式 | リンク |
|------|--------|
| **Standalone PC (EXE)** | [OnoCoro_v0.0.21-prototype.zip](https://github.com/kuippa/OnoCoro/releases/download/v0.0.21-prototype/OnoCoro_v0.0.21-prototype.zip) |
| **ソースコード (ZIP)** | GitHub リポジトリ から Clone |
| **ソースコード (TAR)** | GitHub リポジトリ から Clone |

---

## インストール・実行

### 方法 1: EXE ファイル（推奨）

1. `OnoCoro_v0.0.21-prototype.zip` をダウンロード
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

## プレイ動画

### プロトタイプビルド デモンストレーション

以下の動画でゲームプレイと基本操作をご確認いただけます：

**[OnoCoro v0.0.21-prototype - Gameplay Demo](https://youtu.be/VTvzlBCW4Jg)**

▶️ 動画内容：
- タイトル画面～ステージ選択
- 兼六園ステージでのチュートリアル
- ユニット配置・操作方法
- ゲームプレイの流れ
- カメラ操作・視点切り替え

*注: 低スペック環境での起動遅延が含まれています（診断中）*

---

### 起動パフォーマンス診断インフラ

- **LogWithTimestamp()** - 初期化各フェーズでタイムスタンプ記録
- **LogWithMilliseconds()** - 各フェーズの実行時間を計測
- **InitializationManager 計測** - Phase 2-3 の ResourceLoaders / Managers / UIComponents ごとにタイミング記録
- **パフォーマンスログ出力** - Application.persistentDataPath に YYYYMMDD_onoco.log として自動出力

### ドキュメント充実化

- **versioning.md** - バージョン管理・タグ付ルール、BuildDate.txt フォーマット、GitHub リリースタグ規則を体系化
- **debugging-and-logging.md** - デバッグログ・パフォーマンス診断ガイドを追加
- **内部ドキュメント更新** - AGENTS.md / aboutthisgame.txt をプロトタイプ版表記に統一

---

## バグ修正 [FIX]

### Build 21 での修正

- **(2026-03-10) Debug.cs コンパイルエラー** - `using UnityEngine;` を追加
  - Time.realtimeSinceStartup へのアクセス時に UnityEngine の参照が必要

### 既存修正（v0.0.20 からの継続）

- **(2026-03-08) シーン遷移時 itemlist が更新されない** - `StageYamlRepository._ItemList.Clear()` を追加
- **(2026-03-06) GameTimer GameObject.Find 統合** - Naraku 配置ロジック改善
- **(2026-02-27) GarbageCount 表示されない** - TextMeshProUGUI 参照をシーン遷移時にリフレッシュ
- **(2026-02-11) Stage Goal 判定エラー** - static フラグリセット漏れ修正
- **(2026-02-07) Raycast QueryTriggerInteraction** - IgnoreTriggers を全 Physics.Raycast に統一

---

## リファクタリング [REFACTOR]

### パフォーマンス診断インフラ

- **Debug.cs 拡張** - LogWithTimestamp / LogWithMilliseconds メソッド追加
- **GameConfig ログ設定** - LogFilePath / LogFileName を Application.persistentDataPath に統一
- **InitializationManager 計測** - Phase 2-3 で詳細なタイミング情報を記録

### ドキュメント整備

- **AGENTS.md / CHANGELOG.md** - プロトタイプ版表記に統一
- **versioning.md 新規作成** - GitHub タグ・バージョン管理規則を体系化
- **aboutthisgame.txt 更新** - Unity 6.3.10f1、プロトタイプ版表記

---

## ドキュメント [DOCS]

### 新規作成

- `docs/project-rules/versioning.md` - バージョン管理・タグ付ルール完全ガイド
- パフォーマンス診断ガイド - ログファイル位置・フォーマット説明

### 更新

- `AGENTS.md` - プロトタイプ版表記、セッション情報要件統一
- `CHANGELOG.md` - v0.0.20-prototype / v0.0.21-prototype の詳細変更履歴
- `aboutthisgame.txt` - Unity 6.3.10f1、プロトタイプ版表記に統一
- `README.md` - ビルド環境・ログ位置情報を詳細化

---

## パフォーマンス [PERF]

- **起動パフォーマンス診断インフラ完成** - Phase 2-3 の詳細な実行時間計測機能を実装
- **ログ記録の標準化** - LogWithTimestamp / LogWithMilliseconds で統一フォーマット確立

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
| **低スペック環境での起動遅延** | ⏳ 診断中 | ログ分析により原因特定・最適化予定 |
| マップの端から落ちる可能性 | ⏳ | Phase 3 で修正予定 |
| Fire イベント延焼範囲の表示 | ❌ 未実装 | Phase 3 機能追加 |
| ユニットアップグレード機能 | ❌ 未実装 | Phase 3 機能追加 |
| ゲーム効果音・BGM | ❌ 未実装 | Phase 3 機能追加 |
| セーブ機能 | ❌ 未実装 | Phase 3 以降検討 |

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
- ビルド番号: v0.0.21-prototype

**Reproduction**:
1. 手順1
2. 手順2
3. 現象発生

**Screenshots**:
(スクリーンショット添付)

**Logs**:
ログファイルをご確認の上、添付ください。

場所: `C:\Users\[username]\AppData\LocalLow\Hagurachaya\Onokoro\`

ファイル:
- `Player.log` - Unity 標準ログ（エラー・警告など）
- `[日付]_onoco.log` - パフォーマンス計測ログ（起動タイミング情報）

例: 
```
C:\Users\[ユーザー名]\AppData\LocalLow\Hagurachaya\Onokoro\
├── Player.log
└── 20260310_onoco.log
```
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
- **Version**: v0.0.21-prototype (Build 21)

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
- � [プレイ動画](https://youtu.be/VTvzlBCW4Jg) - v0.0.21-prototype デモンストレーション
- �📺 [開発ライブ配信](https://www.youtube.com/playlist?list=PLxWlv9T7cA6YhDW4aLlfn6BCZQFYPrOJ3)

---

**感謝**: 本プロジェクトの復旧とリリースを可能にしていただいたすべてのコントリビューターとテストユーザーの皆様に感謝申し上げます。

**作成日**: 2026-03-10  
**バージョン**: v0.0.21-prototype  
**ビルド番号**: 21  
**Tag**: `v0.0.21-prototype`
