# Prototype Phase クイックスタート

テストユーザーへのリリース（v0.1.0-alpha）までの実行計画書  
**完全版**: [docs/prototype-phase-roadmap.md](prototype-phase-roadmap.md)

---

## 🎯 目標

✅ 進行不能バグなし  
✅ 5ステージ以上のゲームプレイ可能  
✅ 60 FPS 安定稼動  
✅ ログシステム・バリデーション完備  

**リリース時期**: 2026-02-末  
**工数**: 約 60 人日  

---

## 📋 実行順序

### Week 1-2: コア機能整備

```
□ DebugLogger 実装 & すべての Debug.Log 置換
  └─ LogUtility で5段階ログレベル + ファイル出力
  
□ YamlValidator 実装 & Editor スクリプト作成
  └─ pathmakers, itemlists, stages, goals, boards を検証
  
□ UIタグ導入 & キャンバス動的調整
  └─ 複数解像度テスト（1920×1080, 1280×720等）
  
□ Resources.Load を PrefabManager で統一
  └─ すべての Resources.Load を検索・置換
```

### Week 2-3: ステージ & バランス

```
□ 5ステージ作成（Easy → Very Hard）
  └─ PLATEAU 地形整合性確認
  └─ スポーン・ゴール到達可能性確認
  └─ FPS 60+ 安定確認
  
□ ゲームバランス調整
  └─ タワーコスト・敵難易度・リソース配分
  └─ 10回プレイテスト（クリア率 60-70% 目標）
  
□ カメラ・Z-Fighting 修正
  └─ ズームレベル調整
  └─ 障害物衝突時の上下反転修正
```

### Week 3-4: QA & リリース

```
□ 進行不能バグの排除
  └─ 全ステージクリア可能な状態へ
  └─ 10時間以上連続プレイでクラッシュなし
  
□ パフォーマンス確保
  └─ 60 FPS (最小 45 FPS)
  └─ メモリ < 1GB
  └─ ロード < 3秒/ステージ
  
□ ドキュメント作成（5ファイル）
  └─ README_Prototype.md
  └─ GAMEPLAY_GUIDE.md
  └─ KNOWN_ISSUES.md
  └─ BUG_REPORT_TEMPLATE.md
  └─ LOG_GUIDE.md
  
□ リリース & テストユーザー対応
  └─ v0.1.0-alpha tag 作成
  └─ GitHub Releases で配布
```

---

## 📊 成功指標

| 指標 | 目標値 |
|------|--------|
| ゲームプレイ時間 | 10-15分/ステージ |
| クリア成功率（初見） | 50-60% |
| クラッシュ率 | 0% |
| フレームレート | 60 FPS (45 FPS最小) |
| メモリ使用量 | < 1 GB |
| ステージ数 | 5+（チュートリアル含む） |
| ドキュメント | 5ファイル以上 |
| テストユーザー評価 | 3.0/5.0 以上 |

---

## 🔗 関連ドキュメント

| ドキュメント | 目的 |
|------------|------|
| [prototype-phase-roadmap.md](prototype-phase-roadmap.md) | 詳細実装計画（Phase 1-3） |
| [debug-logger-guide.md](debug-logger-guide.md) | DebugLogger 実装ガイド |
| [yaml-validation-guide.md](yaml-validation-guide.md) | YamlValidator 実装ガイド |
| [.github/instructions/prefab-asset-management.instructions.md](.github/instructions/prefab-asset-management.instructions.md) | PrefabManager ガイド |
| [TODO.md](../TODO.md) | Prototype フェーズ チェックリスト |

---

## 💡 Tips

**デバッグログの確認**:
```
Windows: %USERPROFILE%\AppData\LocalLow\DefaultCompany\OnoCoro\game.log
```

**バリデーション実行**:
```
Editor → Window メニューに YamlValidator を追加
→ StreamingAssets の YAML ファイルを検証
```

**バランステスト**:
```
1. Development Build でビルド
2. すべてのステージを 10回プレイ
3. クリア率・平均時間・選択肢多様性を記録
4. データに基づいてコスト・敵パラメータ調整
```

---

**最終更新**: 2026-01-23  
**対象版**: v0.1.0-alpha (Prototype Phase)
