# Tasklist 層 - バグ・FIXME・機能要望の管理（高頻度アクセス）

**目的**: 細粒度なタスク・バグ・改善案を一元管理  
**配置**: docs/_tasklist/ （アルファベット `_` で最上位、高頻度）  
**対象**: コード内 FIXME、バグ報告、Phase 2 以降の要望  
**更新頻度**: 随時（開発中）

---

## 概要

tasklist/ フォルダは、**ドキュメント層（docs/）** とは異なる、**実装段階のタスク管理** を行います。

| フォルダ | 対象 | 更新頻度 |
|---------|------|---------|
| **ルート/** | README.md（概要）、TODO.md（現在作業中） | 毎日 |
| **tasklist/** | bugs.md、fixme.md、backlog.md | 随時 |
| **docs/** | vision、project-rules、architecture、reference | 定期 |

---

## ファイル構成

### 作業中のロードマップ（Season 3）

| ロードマップ | 対象 | 状態 | 参照 |
|-----------|----------|------|------|
| [../vision/roadmap_season3.md](../vision/roadmap_season3.md) | Season 3 全体（防災投資シミュレーション MVP） | 進行中 | 2026-06 |
| [../season3_schedure.md](../season3_schedure.md) | 週単位スケジュール（W1-W4） | 進行中 | 2026-06 |

### フェーズ別詳細実装計画（detailed/ フォルダ）

| ドキュメント | 対象 | 状態 |
|------------|----------|------|
| [season3-w1-turnbased-detailed-plan.md](./detailed/season3-w1-turnbased-detailed-plan.md) | W1 ターンベース化 | 完了（2026-06-12） |
| [season3-w2-policy-map-detailed-plan.md](./detailed/season3-w2-policy-map-detailed-plan.md) | W2 小マップ + 施策 + カメラシェイク | 完了（2026-06-13） |
| [season3-w3-result-roi-detailed-plan.md](./detailed/season3-w3-result-roi-detailed-plan.md) | W3 結果表示（被害率・ROI）+ デモフロー | 計画確定 |

**Season 2 の計画（アーカイブ）**:
- [../season2_archive/](../season2_archive/) - roadmap-phase-2/3、phase-2/3 詳細計画、bugs/fixme の Season 2 分

### 1. bugs.md - バグ報告・修正追跡

**用途**: 見つかったバグの報告と修正状況を追跡

```
| バグID | 内容 | 重大度 | 状態 | 修正予定 |
|--------|------|--------|------|---------|
| BUG-001 | Canvas Scaler エラー | 中 | 修正済み | 2026-02-01 ✅ |
| BUG-002 | 敵スポーンのタイミング | 高 | 調査中 | 2026-02-10 |
```

### 2. fixme.md - コード内 FIXME 集約

**用途**: ソースコード内の `// FIXME:` `// TODO:` を集約管理

```csharp
// Assets/Scripts/Game/Systems/SpawnSystem.cs:245
// FIXME: 敵スポーン範囲の計算が不正確。LOD 対応時に改善必要

// Assets/Scripts/Core/Managers/PrefabManager.cs:103
// TODO: メモリリーク対策。未使用 Prefab のアンロード機構を実装
```

集約ファイルで一覧化：
```
| ファイル | 行番号 | 内容 | 優先度 |
|---------|--------|------|--------|
| SpawnSystem.cs | 245 | 敵スポーン範囲計算 | 中 |
| PrefabManager.cs | 103 | メモリリーク対策 | 低 |
```

### 3. backlog.md - 機能要望・改善案

**用途**: Phase 2 以降の機能リクエスト・改善提案を保管

```
## Phase 2 後の検討項目

- [ ] マルチプレイ対応（将来）
- [ ] モバイル（iOS/Android）対応
- [ ] PLATEAU データの動的読み込み
- [ ] ステージエディタ UI 実装
- [ ] パフォーマンス最適化（メモリ管理）
```

---

## 使用パターン

### パターン 1: バグを見つけた場合

```
1. bugs.md に記載
   | BUG-003 | WorldSpace Canvas 表示エラー | 高 | 未修正 | 2026-02-05 |

2. ソースコードに // FIXME: を記載
   // FIXME: BUG-003 WorldSpace Canvas の RenderMode 検出

3. 修正後、bugs.md で状態を変更
   状態: 未修正 → 修正済み (2026-02-02)
```

### パターン 2: FIXME コメントをコードに見つけた場合

```csharp
// FIXME: YamlLoader パフォーマンス最適化
// ストリーミング方式に変更予定（Phase 2）
void LoadYamlAsync()
{
    // ...
}
```

↓ 以下の情報を fixme.md に追加

```
| YamlLoader.cs | 87 | YAML ストリーミング最適化 | Phase 2 | 低 |
```

### パターン 3: 新機能リクエストが来た場合

```
backlog.md に追加：
- [ ] Feature: ゲーム内チュートリアル UI
  優先度: 中
  実装予定: Phase 3
  詳細: プレイヤーの操作ガイド
```

---

## GitHub Issues・Projects との連携

[FUTURE] tasklist/ をベースに GitHub Issues を作成：

```
tasklist/bugs.md（バグ一覧）
    ↓
GitHub Issues（bug label）で一覧化
    ↓
Project ボードで進捗管理
```

---

## チェックリスト

### 定期確認項目（毎週）

- [ ] bugs.md を確認。未修正バグがあるか
- [ ] fixme.md を確認。急ぎの FIXME があるか
- [ ] backlog.md を確認。Phase 2 計画に反映させるべき内容があるか

### Phase 2 開始時

- [ ] backlog.md の内容をロードマップに反映
- [ ] bugs.md・fixme.md を Phase 2 用にリセット
- [ ] GitHub Issues と同期化

---

## 関連ドキュメント

- [docs/vision/roadmap_season3.md](../vision/roadmap_season3.md) - 現在進行中のロードマップ（Season 3 MVP）
- [docs/season3_schedure.md](../season3_schedure.md) - 週単位スケジュール
- [docs/vision/project-statement.md](../vision/project-statement.md) - 長期ビジョン
- [AGENTS.md](../../AGENTS.md) - プロジェクトルール

---

**作成日**: 2026-02-02  
**用途開始予定**: Phase 2（2026-03 以降）
