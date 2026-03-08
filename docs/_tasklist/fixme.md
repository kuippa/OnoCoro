# FIXME・TODO コメント集約

**更新日**: 2026-02-02  
**対象**: ソースコード内の `// FIXME:` `// TODO:` コメント  
**集約頻度**: 随時（大きな変更時に更新）

---

## 概要

ソースコード内に記載された `// FIXME:` や `// TODO:` コメントを一覧化し、優先度順に追跡します。

**用途**:
- コード改善の必要性を可視化
- Phase 2 以降の実装計画の参考資料
- リファクタリング・パフォーマンス最適化の優先順位付け

---

## FIXME・TODO 一覧

### 高優先度（Phase 1-2 内で対応）

| ファイル | 行番号 | 内容 | タイプ | 関連バグ | 対応予定 |
|---------|--------|------|--------|---------|---------|
| Environment Volume | - | [HDRI Sky のキューブマップが欠如、SpaceEmission も欠如] | FIXME | - | Phase 1.5 |
| UICanvasManager.cs | (参考欄) | [複数解像度対応時の動的更新] | TODO | - | Phase 2 |
| InitializationManager.cs | (参考) | [FontManager 初期化] | TODO | - | Phase 1.4～1.5 |
| YamlLoader.cs | (参考) | [ストリーミング読み込み最適化] | FIXME | - | Phase 2 |

### 中優先度（Phase 2-3 での実装推奨）

| ファイル | 行番号 | 内容 | タイプ | 対応予定 |
|---------|--------|------|--------|---------|
| PrefabManager.cs | - | [メモリリーク対策・未使用 Prefab のアンロード] | FIXME | Phase 2 |
| SpawnSystem.cs | - | [敵スポーン範囲計算の精度向上] | FIXME | Phase 2 |

### 低優先度（Phase 3 以降・コンテキスト許す限り）

| ファイル | 内容 | タイプ | 対応予定 |
|---------|------|--------|---------|
| (TBD) | UI の微調整・エフェクト改善 | TODO | Phase 3+ |

---

## テンプレート

**FIXME コメントの標準形式**:

```csharp
// FIXME: [問題の概要]
// 背景: [なぜこれが問題か]
// 対応: Phase X で [対応内容] を実装予定
// 参考: [関連 Issue・ドキュメント]
public void SomeMethod()
{
    // 暫定実装
}
```

**TODO コメントの標準形式**:

```csharp
// TODO: [実装予定の機能]
// 優先度: [高/中/低]
// Phase: [Phase X]
// 備考: [追加情報]
void IncompleteFeature()
{
    // スタブ実装
}
```

---

## スキャン方法

### VS Code での FIXME/TODO 検索

```powershell
# ワークスペース全体で FIXME を検索
# Ctrl+Shift+F で検索パネルを開き、以下を入力：
// FIXME:
// TODO:
```

### Git Grep での検索

```powershell
cd g:\unity\OnoCoro2026
git grep "FIXME\|TODO" -- "*.cs"
```

---

## 定期更新スケジュール

| 時期 | 作業 |
|------|------|
| **週 1 回** | コード内の新規 FIXME/TODO をスキャン |
| **Phase 末** | FIXME 完了状況を確認し、次 Phase に反映 |
| **リリース前** | すべての FIXME/TODO を確認し、必要に応じて対応 |

---

## 完了後の処理

FIXME を修正した場合：

1. コードから `// FIXME:` を削除
2. このファイルから対応行を削除（またはアーカイブ）
3. git commit メッセージで参照:
   ```
   fix: FIXME を解決 - ストリーミング読み込み実装 (#123)
   ```

---

## 関連ドキュメント

- [README.md](README.md) - tasklist 層概要
- [bugs.md](bugs.md) - バグ報告・修正追跡
- [backlog.md](backlog.md) - 機能要望・改善案
- [AGENTS.md](../AGENTS.md#coding-standards) - コーディング規約

