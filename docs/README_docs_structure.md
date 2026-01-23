# ドキュメント構成ガイド

## ルート docs/ フォルダ（`docs/`）

### 目的
開発規約・技術スタンダード・実装計画書

### 対象
開発者・AI エージェント

### 特性
厳密な版管理、定期更新

### 参照元
AGENTS.md, .github/instructions.md

### ドキュメント分類

#### 開発規約・標準
- `architecture.md` - システムアーキテクチャ（3層構成）
- `coding-standards.md` - C# コーディング規約
- `recovery-workflow.md` - Recovery Phase ワークフロー

#### 実装計画書（フェーズ別）
- `prototype-phase-roadmap.md` - Prototype フェーズ詳細計画（60人日）
- `prototype-phase-quickstart.md` - Prototype フェーズ クイックスタート

#### 構成改善提案
- `scripts-folder-restructure-proposal.md` - Scripts フォルダ再構成提案
- `class-naming-convention-proposal.md` - **NEW** クラス命名規則統一提案

#### その他
- `svg-image-import-guide.md` - SVG 画像読み込みガイド
- `introduction.md` - プロジェクト概要・非目標

---

## Assets docs/ フォルダ（`Assets/docs/`）

### 目的
ゲーム企画・設計・作業メモ

### 対象
ゲームデザイナー・プロデューサー

### 特性
Unity Editor から可視、柔軟な更新

### サブフォルダ（推奨）
- `game-design/` - ゲーム企画
- `platform-docs/` - 外部SDK ドキュメント
- `work-memo/` - 開発作業メモ（履歴）