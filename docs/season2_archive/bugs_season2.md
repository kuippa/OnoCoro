# バグ報告・修正追跡

**更新日**: 2026-02-02  
**状態**: 初期化  
**担当者**: Team

---

## バグ一覧

| バグID | 内容 | 重大度 | 状態 | 報告日 | 修正予定日 | 備考 |
|--------|------|--------|------|--------|----------|------|
| BUG-001 | Canvas Scaler のスクリーンスペース強制 | 高 | 修正済み ✅ | 2026-01-26 | 2026-01-27 | WorldSpace Canvas 対応済み |
| | | | | | | |


## バグメモ
Litterがなにかの拍子にゴミをまきちらかさなることがあるようだ 再現性低い 監視モードのIN OUTが原因？


---

## バグ詳細

### BUG-001: WorldSpace Canvas の RenderMode 上書き

**症状**: 3D UI（SignPowerOutage）が全画面に表示される

**原因**: UICanvasManager.ApplyStandardScalerSettings() がすべての Canvas を ScreenSpaceOverlay に変更

**修正**: WorldSpace Canvas を検出してスキップ

```csharp
// FIXED: WorldSpace Canvas は RenderMode を変更しない
if (canvas.renderMode == RenderMode.WorldSpace)
{
    return;
}
```

**テスト**: ✅ UICanvasManagerTest.cs で検証済み

**修正日**: 2026-01-27

---

## 修正待ちバグ

[現在なし]

---

## テンプレート

```markdown
### BUG-XXX: [タイトル]

**症状**: [ユーザーが見る現象]

**原因**: [根本原因]

**修正**: [修正内容]

**テスト**: [テスト方法]

**修正日**: [修正完了日]
```

---

## バグ分類

| 重大度 | 説明 | 対応 |
|--------|------|------|
| **極高** | ゲーム起動不可・クラッシュ | 即座に対応 |
| **高** | ゲーム進行不能・大幅な見た目異常 | Phase 1 内で対応 |
| **中** | プレイに支障あり・部分的な異常 | Phase 1-2 内で対応 |
| **低** | 軽微な不具合・UI の微調整 | Phase 2-3 で対応 |

---

**関連**: [README.md](README.md)、[fixme.md](fixme.md)、[backlog.md](backlog.md)
