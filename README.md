# OnoCoro - つくるオノコロ

> 国土交通省の PLATEAU を使った、まちづくりをテーマにしたタワーディフェンスゲーム

![version](https://img.shields.io/badge/version-0.1.0--alpha-blue)
![unity](https://img.shields.io/badge/Unity-6.3.10f1-green)
![license](https://img.shields.io/badge/license-MIT-blue)

## 概要

OnoCoro は、日本の都市 3D データ（[PLATEAU](https://www.mlit.go.jp/plateau/)）を使ったインタラクティブゲームです。

**ストーリー**: 都市のゴミ問題を解決するため、清掃ロボットを配置して汚染物質を撃退し、建物を修復するタワーディフェンスゲーム。

**開発背景**: 2024 年の SSD 障害から復旧し、2026 年元日に再始動。オープンソース化。

---

## リリース

### 📦 [v0.1.0-alpha (Prototype)](https://github.com/kuippa/OnoCoro/releases) - 2026-03-08

**プロトタイプビルド公開開始します！**

実装済み機能：
- ✅ マルチシーン環境（タイトル + 2 ステージ）
- ✅ ユニット作成・配置システム
- ✅ Enemy Litter スポーン・移動
- ✅ カメラ制御（FPS / TPS / LongShot / BirdView）
- ✅ YAML ベース PLATEAU 統合
- ✅ Cinemachine 3.x 対応

[詳細な変更履歴を見る →](CHANGELOG.md)

---

## 動作環境

| 要件 | 仕様 |
|------|------|
| **OS** | Windows 10 / 11 |
| **RAM** | 8GB 以上推奨 |
| **GPU** | VRAM 2GB 以上（Intel/NVIDIA/AMD 対応） |
| **Unity** | 6.3.10f1（開発環境）|
| **Build** | Standalone PC |

詳細な開発環境・パッケージ仕様は [docs/BUILD_ENVIRONMENT.md](docs/BUILD_ENVIRONMENT.md) を参照してください。

---

## インストール

### ダウンロード・実行

1. [リリースページ](https://github.com/kuippa/OnoCoro/releases) から `OnoCoro_v0.1.0-alpha.zip` をダウンロード
2. 解凍
3. `OnoCoro.exe` をダブルクリック

### ビルドから実行（開発者向け）

```bash
# リポジトリクローン
git clone https://github.com/kuippa/OnoCoro.git
cd OnoCoro

# Unity 6.3.10f1 で開く
unity -projectPath . -openFile

# Editor で Build Settings を確認 → File > Build and Run
```

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

### ゲーム目標

- **ステージ 1（石川県金沢市兼六園）**: すべての建物を修復する（チュートリアル）
- **ステージ 2（三鷹井の頭）**: Wave をクリアし、5 分間生き残る

---

## 開発者向け情報

### ドキュメント

- [`docs/`](docs/) - アーキテクチャ・設計ドキュメント
- [`AGENTS.md`](AGENTS.md) - **コーディング基準（必読）**
- [`docs/BUILD_ENVIRONMENT.md`](docs/BUILD_ENVIRONMENT.md) - **開発環境・パッケージ仕様**
- [`TODO.md`](TODO.md) - 実装ロードマップ
- [`CHANGELOG.md`](CHANGELOG.md) - 変更履歴

### コード品質

```
使用言語: C# (AGENTS.md に準拠)
プロジェクト構造: 4-Layer Architecture
  - Presentation (UI / Input)
  - Game (Logic / Systems)
  - Data (Models / Repositories)
  - Core (Managers / Utilities)
Namespace: CommonsUtility
```

### フォルダ構造

```
Assets/Scripts/
├── Presentation/     UI・カメラ・入力制御
├── Game/             ゲームロジック・ユニット
├── Data/             YAML・PLATEAU・ステージデータ
└── Core/             マネージャー・ユーティリティ
```

詳細: [docs/project-rules/folder-structure.md](docs/project-rules/folder-structure.md)

### 開発・ビルド手順

1. **開発**: Unity Editor で Assets/Scenes/TitleScene.unity を開く
2. **テスト**: Play Mode で実行
3. **ビルド**: File > Build Settings > Build and Run
4. **git**: コミット前に [Pre-Commit Checklist](AGENTS.md#pre-commit-checklist) を確認

---

## 既知の問題

詳細は [KNOWN_ISSUES.md](KNOWN_ISSUES.md) を参照。

- [ ] マップ端から落ちる可能性（ステージ設計改善予定）
- [ ] Fire 延焼表示が未実装
- [ ] セーブ機能未実装

**テストユーザーへ**: バグ報告は [Issues](https://github.com/kuippa/OnoCoro/issues) にお願いします。

---

## 動画・ライブ配信

- 📺 [つくるオノコロ - YouTube 再生リスト](https://www.youtube.com/playlist?list=PLxWlv9T7cA6bZvEUqwVlGvnpeO2LvK5qa)
- 🔴 [開発ライブ - YouTube 再生リスト](https://www.youtube.com/playlist?list=PLxWlv9T7cA6YhDW4aLlfn6BCZQFYPrOJ3)

---

## ライセンス

[MIT License](LICENSE) - 自由に使用・改変・配布可能です。

---

## 貢献

プルリクエスト・Issue 報告を歓迎します！

1. Fork する
2. Feature branch を作成 (`git checkout -b feature/amazing-feature`)
3. コミット (`git commit -m 'Add amazing feature'`)
4. Push (`git push origin feature/amazing-feature`)
5. Pull Request を作成

**コーディング規約**: [AGENTS.md](AGENTS.md) を参照

---

## お問い合わせ

- GitHub: [@kuippa](https://github.com/kuippa)
- Issues: [GitHub Issues](https://github.com/kuippa/OnoCoro/issues)

