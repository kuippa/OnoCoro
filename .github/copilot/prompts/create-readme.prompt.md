---
agent: 'copilot'
description: 'OnoCoro README 自動生成'
model: 'gpt-4'
tools: []
---

# OnoCoro README 生成

You are a technical documentation writer specializing in game development projects.

## Your Role

Generate comprehensive README.md for OnoCoro that describes:
- プロジェクト概要と目的
- PLATEAU SDK 地理情報可視化
- Tower Defense ゲーム機構
- セットアップ手順
- 開発ワークフロー

## README Template

```markdown
# OnoCoro - 地理情報ゲーム

[バナー画像]

> Unity 6.3 + PLATEAU SDK で実装された、地理情報可視化タワーディフェンスゲーム

## 概要

OnoCoro は、日本の都市3Dデータ（PLATEAU SDK）を使用した地理情報可視化ゲームです。
タワーディフェンスゲームのメカニクスと地理情報を組み合わせた、ユニークな環境クリーンアップテーマのアプリケーションです。

## 主な機能

- 🗺️ **PLATEAU データ統合**: CityGML フォーマットの都市3Dデータをリアルタイム表示
- 🎮 **Tower Defense**: 地理的な位置にタワーを配置して敵を撃退
- 📊 **地理情報可視化**: 座標系変換により正確な地理情報を表現
- 🎨 **リアルタイムレンダリング**: Unity 6.3 の最新グラフィックス

## プロジェクト情報

| 項目 | 説明 |
|------|------|
| **エンジン** | Unity 6.3 |
| **言語** | C# |
| **主依存** | PLATEAU SDK, Cinemachine, glTFast |
| **プラットフォーム** | Windows（開発時点） |
| **ステータス** | Recovery & Development Phase |

## クイックスタート

### 前提条件
- Unity 6.3 LTS
- PLATEAU SDK（自動ダウンロード）
- Git 2.30+

### セットアップ

\`\`\`bash
# リポジトリクローン
git clone https://github.com/kuippa/OnoCoro.git
cd OnoCoro

# Unity Hub または Unity Editor で開く
# File > Open Project → フォルダを選択
\`\`\`

### 初回実行
1. Unity で Assets フォルダが同期されるまで待機（数分）
2. File > Build & Run でゲーム起動

## ドキュメント

- [🏗️ アーキテクチャ](docs/architecture.md) - システム設計
- [📋 コーディング基準](docs/coding-standards.md) - C# 実装規約
- [🔧 開発ガイド](.github/instructions.md) - セットアップと運用
- [🤖 AI Agent ガイド](AGENTS.md) - Copilot 統合設定
- [📖 プロジェクト概要](docs/introduction.md) - 目的と非目的

## Recovery フェーズについて

このプロジェクトは SSD 故障からの 2 年古いバックアップから復旧されています。

- ✅ 393 個の C# スクリプトを復旧
- 🔄 段階的に機能を復元中
- 📊 Data Protection が最優先

詳細は [docs/recovery-workflow.md](docs/recovery-workflow.md) を参照。

## 開発に参加する

1. [AGENTS.md](AGENTS.md) でガイドラインを確認
2. [docs/coding-standards.md](docs/coding-standards.md) に準拠
3. Feature branch で機能開発
4. Pull Request を提出

## よくある質問

**Q: PLATEAU SDK のライセンス**
A: [PLATEAU ライセンス](https://www.mlit.go.jp/plateau/)を参照

**Q: 大規模データセット対応**
A: 段階的ロード対応。詳細は [docs/architecture.md](docs/architecture.md)

## ライセンス

MIT License - [LICENSE](LICENSE) を参照

## 貢献者

[GitHubコントリビューター表示]

---

**最終更新**: 2026-01-20
```

## Context

- **Project**: OnoCoro (Unity 6.3 + PLATEAU SDK)
- **Recovery Phase**: 進行中
- **Reference**: [.github/instructions.md](../instructions.md)
