# CHANGELOG

OnoCoro プロジェクトの変更履歴です。

---

## [0.0.20-prototype] - 2026-03-10

**前回リリース**: 0.0.6 (2026-01-26)  
**Status**: Prototype / Recovery Phase 2.2  
**Build Number**: 20  
**Tag**: `v0.0.20-prototype`

### [NEW] 新機能

#### 起動パフォーマンス診断
- **LogWithTimestamp()** - 初期化各フェーズでタイムスタンプ記録
- **LogWithMilliseconds()** - 各フェーズの実行時間を計測
- **InitializationManager 計測** - Phase 2-3 の ResourceLoaders / Managers / UIComponents ごとにタイミング記録
- **パフォーマンスログ出力** - Application.persistentDataPath に YYYYMMDD_onoco.log として自動出力

#### ドキュメントの充実
- **versioning.md** - バージョン管理・タグ付ルール、BuildDate.txt フォーマット、GitHub リリースタグ規則
- **debugging-and-logging.md** - デバッグログ・パフォーマンス診断ガイド
- **Release documentation** - README.md / RELEASE_NOTES.md / aboutthisgame.txt の統一化

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
- **GameConfig ログ設定** - LogFilePath / LogFileName を Application.persistentDataPath に統一
- **Debug.cs 拡張** - LogWithTimestamp / LogWithMilliseconds メソッド追加

#### Documentation
- **AGENTS.md** - セッション情報要件の統一
- **versioning.md** - バージョン管理規則の体系化
- **aboutthisgame.txt** - プロトタイプ版表記・Unity バージョン更新

### [DOCS] ドキュメント

#### 新規作成
- `docs/architecture/camera-exposure-settings.md` - カメラ・Depth of Field トラブルシューティング
- `docs/architecture/camera-deoccluder-implementation.md` - CinemachineDeoccluder 実装ガイド

#### 更新
- AGENTS.md - スリム化・ドキュメント構造整理
- prototype-phase-roadmap.md - Phase 2-3 進捗追跡
- TODO.md - carrera地面潜り対策 TODO 追加

### [PERF] パフォーマンス

- 起動遅延診断インフラストラクチャ実装（詳細な初期化タイミング記録）
- Phase 2 段階での計測準備完了

### [KNOWN_ISSUES] 既知の問題

- [ ] ⏳ **低スペック環境での起動遅延** - Phase 2.2 で診断中 (詳細は README.md 参照)
- [ ] マップの端から落ちる可能性がある
- [ ] Fire イベント延焼範囲の表示が未実装
- [ ] ユニットアップグレード機能が未実装
- [ ] ゲーム効果音・BGM が未実装
- [ ] セーブ機能が未実装

### 技術基盤

| 項目 | バージョン |
|------|----------|
| **Unity** | 6.3.10f1 |
| **PLATEAU SDK** | Latest |
| **Cinemachine** | Unity Standard |

詳細なパッケージ構成と開発環境設定は [docs/BUILD_ENVIRONMENT.md](docs/BUILD_ENVIRONMENT.md) を参照。

---

## [0.0.6] - 2026-01-26

Phase 0 初期ビルド版。エディター参照削除、設定更新実施。

---

## このリリースに含まれるシーン

- ✅ TitleScene - タイトル・シーン選択画面
- ✅ 石川県金沢市兼六園 - チュートリアルステージ（推奨）
- ✅ 三鷹井の頭 - Wave テストステージ

---

## このリリースでのテスト焦点

### ✓ Prototype 段階で確認済み
- [x] マルチシーン遷移・UI初期化
- [x] itemlist シーン別更新
- [x] YAML イベント・パス読み込み
- [x] ユニット作成・配置
- [x] Enemy Litter スポーン・移動
- [x] カメラ制御（FPS/TPS/LongShot/BirdView）
- [x] 起動タイミング・パフォーマンスログ記録

### ⏳ テストユーザー向け検証必要事項
- [ ] GPU 互換性（Intel/NVIDIA/AMD）
- [ ] 低仕様環境での FPS 安定性（特に新宿都庁、東京駅）
- [ ] 長時間連続プレイ（メモリリーク検証）
- [ ] 起動遅延の詳細分析（ログ提供 / フィードバック希望）

---

## テストユーザー向け報告方法

GitHub Issues で以下の形式で報告をお願いします。**ログファイルの添付が特に重要です**：

```
**Title**: [BUG] 現象を一行で説明

**Environment**:
- OS: Windows XXX
- GPU: モデル
- ビルド番号: v0.0.20-prototype

**Logs**:
- Player.log: %AppData%/../LocalLow/Hagurachaya/Onokoro/
- Performance Log: YYYYMMDD_onoco.log (same directory)

**Reproduction**:
1. 手順1
2. 手順2

**Expected vs Actual**:
期待: xxx
実際: yyy
```

ログファイル位置: `C:\Users\[ユーザー名]\AppData\LocalLow\Hagurachaya\Onokoro\`

---

## インストール・起動方法

1. `OnoCoro-v0.0.20-prototype.zip` を解凍
2. `OnoCoro.exe` を実行
3. シーン選択画面から「石川県金沢市兼六園」（チュートリアル）を選択
4. TAB キーでユニット作成メニュー表示
5. ユニットを配置してテスト

---

**公式リポジトリ**: https://github.com/kuippa/OnoCoro  
**ライセンス**: MIT License
