# Phase 1.5: 初期化順序管理設計書

**バージョン**: 1.0  
**作成日**: 2026-01-29  
**対象**: OnoCoro Prototype Phase  
**ステータス**: 設計段階 → **実装開始（2026-01-29）**

---

## 🚀 現在の実装状況

### 完了したコンポーネント（9/22）

#### Panels（5/5 ✅）
- `EscMenuCtrl` - ESC メニュー表示制御
- `TabMenuCtrl` - タブメニュー管理
- `NoticeCtrl` - 通知ポップアップ表示
- `DebugInfoCtrl` - デバッグ情報パネル
- `SpawnMarkerPointerCtrl` - スポーンマーカー表示

#### Dialogs（4/4 ✅）
- `EventLogCtrl` - イベントログダイアログ
- `GameTimerCtrl` - ゲームタイマー表示
- `InfoWindowCtrl` - 情報ウィンドウ
- `MessageBoxCtrl` - メッセージボックス

#### 対象外（プロトタイプ期間）
- **HUD**（8個）: オンデマンド初期化のため実装予定なし
- **Controls**（5個）: オンデマンド初期化のため実装予定なし

---

## 📊 初期化フロー（実装版）

### Awake Phase → Start Phase → 初期化完了

```
【時間: 0ms】
Awake() - 参照取得のみ
├─ Panels（5個）
│  ├─ EscMenuCtrl.Awake() → base.Awake() + 参照取得
│  ├─ TabMenuCtrl.Awake() → base.Awake() + 参照取得
│  ├─ NoticeCtrl.Awake() → base.Awake() + 参照取得
│  ├─ DebugInfoCtrl.Awake() → base.Awake() + 参照取得
│  └─ SpawnMarkerPointerCtrl.Awake() → base.Awake() + 参照取得
│
└─ Dialogs（4個）
   ├─ EventLogCtrl.Awake() → base.Awake() + 参照取得
   ├─ GameTimerCtrl.Awake() → base.Awake() + 参照取得
   ├─ InfoWindowCtrl.Awake() → base.Awake() + 参照取得
   └─ MessageBoxCtrl.Awake() → base.Awake() + 参照取得

【時間: 50-100ms】
Start() - 初期化開始（UIControllerBase が自動制御）
├─ Panels & Dialogs（9個）
│  └─ protected virtual void Start()
│     └─ StartCoroutine(InitializeAsync())
│        ├─ yield return Initialize()（各クラスで override）
│        ├─ IsInitialized = true に自動設定
│        └─ Debug.Log("[UIControllerBase] {ClassName} 初期化完了")
│
└─ 各コンポーネントの実装
   ├─ EscMenuCtrl.Initialize() → yield return null
   ├─ TabMenuCtrl.Initialize() → yield return null
   ├─ NoticeCtrl.Initialize() → yield return null
   ├─ DebugInfoCtrl.Initialize() → yield return null
   ├─ SpawnMarkerPointerCtrl.Initialize() → yield return null
   ├─ EventLogCtrl.Initialize() → yield return null
   ├─ GameTimerCtrl.Initialize() → yield return null
   ├─ InfoWindowCtrl.Initialize() → yield return null
   └─ MessageBoxCtrl.Initialize() → yield return null

【時間: 100-200ms】
初期化完了
└─ IsInitialized = true × 9個
   └─ InitializationManager が動的検出
      └─ GetComponentsInChildren<IInitializable>() で 9個検出
         └─ コンソール: "[InitializationManager] 検出: 9個"
```

---

## 🔧 UIControllerBase - 実装パターン

### パターン A：必須初期化（Panels & Dialogs）

```csharp
// 基本的な使い方
public class EscMenuCtrl : UIControllerBase
{
    private Button _escButton;
    
    // [1] Awake - 参照取得のみ
    protected override void Awake()
    {
        base.Awake();  // 重要: base.Awake() を最初に呼ぶ
        _escButton = GetComponentInChildren<Button>();
    }
    
    // [2] Initialize - 初期化処理
    protected override IEnumerator Initialize()
    {
        _escButton.onClick.AddListener(OnEscButtonClicked);
        yield return null;  // 必ず yield する
    }
    
    // [3] IsInitialized は自動設定（手動で設定不要）
    // Start() が自動で InitializeAsync() を呼び出す
    // Initialize() 完了後、UIControllerBase が IsInitialized = true に設定
}
```

**対象コンポーネント**:
- Panels: EscMenuCtrl, TabMenuCtrl, NoticeCtrl, DebugInfoCtrl, SpawnMarkerPointerCtrl
- Dialogs: EventLogCtrl, GameTimerCtrl, InfoWindowCtrl, MessageBoxCtrl

**IsInitializationRequired**: デフォルト値 `true` をそのまま使用（オーバーライド不要）

### パターン B：オンデマンド初期化（HUD & Controls）- 将来実装

```csharp
// HUD や Controls 向け（実装予定）
public class MyHUDCtrl : UIControllerBase
{
    // IsInitializationRequired を false にオーバーライド
    protected override bool IsInitializationRequired => false;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override IEnumerator Initialize()
    {
        yield return null;  // 呼ばれない
    }
    
    // Start() で即座に IsInitialized = true に設定される
    // Initialize() は呼ばれない
}
```

**対象コンポーネント**（実装予定）:
- HUD: MarkerIndicatorCtrl, MarkerPointerCtrl, PathMakerCtrl, ScoreCtrl, TelopCtrl, TooltipInfoCtrl, MouseOverTipsCtrl, CircularIndicator
- Controls: ClickCtrl, ClosebtnCtrl, OkbtnCtrl, WindowCloseCtrl, WindowDragCtrl

---

## ⚙️ Script Execution Order（設定必須）

| 優先度 | クラス | 役割 |
|--------|--------|------|
| **-100** | `InitializationManager` | 最優先：初期化全体の進行管理 |
| **-50** | `GamePrefabs` | 次優先：プレファブロード（非同期） |
| **0** | Panels, Dialogs, EventLoader, YamlLoader | デフォルト |
| **100+** | FireCubeCtrl, GarbageCubeCtrl など | 遅延実行：ゲームロジック系 |

**設定手順**:
1. Unity Editor で `Edit` → `Project Settings` → `Script Execution Order` を開く
2. 各クラスをドラッグして順序を設定（+ ボタンで新規追加）
3. Unity を再起動して設定が保存されたか確認

---

## 📋 目次

1. [概要](#概要)
2. [現在の問題](#現在の問題)
3. [提案する解決方法](#提案する解決方法)
4. [初期化フロー](#初期化フロー)
5. [実装ステップ](#実装ステップ)
6. [各コンポーネントの修正方針](#各コンポーネントの修正方針)
7. [スケジュール](#スケジュール)
8. [リスク & 対策](#リスク--対策)

---

## 概要

### 目的

**低スペック環境（旧型マシン）における初期化タイムアウトによる NullReferenceException を排除**

現在、GamePrefabs の初期化中に EventLoader や YamlLoader が早期に起動し、未初期化の参照にアクセスして落ちる問題が顕在化しています。

### スコープ

| 項目 | 内容 |
|------|------|
| **対象コンポーネント** | InitializationManager, GamePrefabs, EventLoader, YamlLoader |
| **工数** | 3-4 日（実装 2-2.5 日 + テスト 1-1.5 日） |
| **優先度** | 🔴 CRITICAL（Phase 1.4 UI 改善より優先） |
| **ブロッカー** | Phase 2・3 ブロッカー（低スペック環境での再現） |

### ビジネス影響

- ✅ テストユーザー（旧型マシン利用者）が prototype v0.1.0-alpha をプレイ可能に
- ✅ Phase 2・3 スケジュール の確実化
- ✅ 本版リリース時のトラブル削減

---

## 現在の問題

### 問題シナリオ

```
【現在の初期化フロー（問題あり）】

Time: 0ms
├─ Scene Load
│  └─ MonoBehaviour.Awake() 呼び出し
│     ├─ GamePrefabs.Awake()
│     │  └─ プレファブロード・キャッシュ初期化開始（～500ms on 低スペック）
│     │
│     ├─ EventLoader.Awake()
│     │  └─ instance 設定のみ
│     │
│     └─ YamlLoader.Awake()
│        └─ instance 設定のみ
│
Time: 10-50ms（並行実行）
├─ GamePrefabs 初期化中...（500ms 必要）
│
├─ EventLoader.OnEnable()
│  └─ ❌ 即座にロード開始
│     └─ GamePrefabs._gamePrefabs がまだ null
│        └─ NullReferenceException ☠️
│
└─ YamlLoader.OnEnable()
   └─ ❌ 即座にロード開始
      └─ GameConfig がまだ uninitialized
         └─ IndexOutOfRangeException ☠️

Time: 500ms
└─ GamePrefabs.OnAwake() 完了
   └─ 遅すぎる！既に落ちてる
```

### 根本原因

1. **同期的初期化の前提の欠落** - Awake/OnEnable の実行順序は保証されない
2. **依存関係の明示化なし** - GamePrefabs → EventLoader/YamlLoader の依存を制御する機構がない
3. **Script Execution Order の未設定** - InitializationManager が存在しても使われていない

### 影響範囲

| マシン仕様 | 症状 |
|-----------|------|
| **高スペック（SSD+CPU core 8+）** | ほぼ問題なし（初期化完了が早い） |
| **低スペック（HDD+CPU core 2-4）** | ❌ 初期化中に EventLoader が起動→ NullRef 例外 |
| **仮想環境** | ❌ 最悪（初期化時間予測不可） |

---

## 提案する解決方法

### アーキテクチャ概要

```
【提案：Event-driven 初期化制御】

InitializationManager
├─ 責務: 初期化順序の制御・通知
├─ 機構: Readiness Event（OnGamePrefabsReady など）
└─ 特徴: 各コンポーネントが「準備完了」を待つ

GamePrefabs
├─ 責務: Game オブジェクトプレファブの一元管理
├─ 初期化: Awake() → プレファブロード → NotifyGamePrefabsReady()
└─ 特徴: InitializationManager への通知が責務

EventLoader
├─ 責務: ステージイベントの遅延ロード
├─ 待機: OnEnable() ではなく Start() で InitializationManager.OnGamePrefabsReady を購読
└─ 特徴: 初期化完了まで待機

YamlLoader
├─ 責務: YAML ファイルの遅延ロード
├─ 待機: 同様に OnGamePrefabsReady を購読
└─ 特徴: 初期化完了まで待機
```

### 実装パターン

#### パターン A: Event-driven（推奨）

**メリット**:
- ✅ 疎結合（各コンポーネントが独立）
- ✅ 拡張性（新規ローダー追加が容易）
- ✅ テスト性（モックイベント供給可）

**デメリット**:
- 🟡 イベント購読の追加実装が必要

```csharp
// InitializationManager.cs
public static event System.Action OnGamePrefabsReady;

internal static void NotifyGamePrefabsReady()
{
    OnGamePrefabsReady?.Invoke();
}

// EventLoader.cs
private void Start()
{
    InitializationManager.OnGamePrefabsReady += LoadEvents;
}

private void OnDestroy()
{
    InitializationManager.OnGamePrefabsReady -= LoadEvents;
}
```

#### パターン B: Wait-based（シンプル）

**メリット**:
- ✅ 実装が単純
- ✅ C# コルーチン標準パターン

**デメリット**:
- 🟡 ポーリング（WaitUntil）の処理コスト

```csharp
// EventLoader.cs
private IEnumerator Start()
{
    yield return new WaitUntil(() => InitializationManager.IsGamePrefabsReady);
    LoadEvents();
}
```

### 推奨: パターン A（Event-driven）+ パターン B（Fallback）

```csharp
// EventLoader.cs - ハイブリッドアプローチ
private void Start()
{
    // Event 購読（初期化完了時）
    InitializationManager.OnGamePrefabsReady += LoadEvents;
    
    // Fallback: 既に初期化完了している場合はすぐに呼び出し
    if (InitializationManager.IsGamePrefabsReady)
    {
        LoadEvents();
    }
}
```

---

## シーン構成とコンポーネント呼び出し図

### ハイレベルシーン構成（UnitFireDisaster シーン例）

```
【UnitFireDisaster シーン構成】

シーン (UnitFireDisaster)
├─ GameObject: [Manager]
│  ├─ Component: InitializationManager ← 初期化制御の中心
│  ├─ Component: GameConfig
│  ├─ Component: LanguageManager
│  └─ Component: GameSpeedManager
│
├─ GameObject: [GamePrefabs]
│  └─ Component: GamePrefabs ← プレファブロード・通知
│     └─ 呼び出し: PrefabManager.LoadAllGamePrefabs()
│
├─ GameObject: [EventSystem]
│  ├─ Component: EventLoader ← イベント遅延ロード
│  │  └─ 購読: InitializationManager.OnGamePrefabsReady
│  │
│  └─ Component: YamlLoader ← YAML 遅延ロード
│     └─ 購読: InitializationManager.OnGamePrefabsReady
│
├─ GameObject: [GameManager]
│  ├─ Component: FireCubeCtrl ← 炎キューブ管理
│  │  └─ 待機: IsInitialized フラグ確認
│  │  └─ 読み込み元: GamePrefabs.GetGamePrefabs()
│  │
│  └─ Component: GarbageCubeCtrl ← ゴミキューブ管理
│     └─ 待機: IsInitialized フラグ確認
│     └─ 読み込み元: GamePrefabs.GetGamePrefabs()
│
├─ GameObject: [UnitSpawn]
│  ├─ Component: UnitFireDisaster ← ステージセットアップ
│  │  ├─ 待機: InitializationManager.IsInitialized
│  │  ├─ 呼び出し: SettingCubes() → GarbageCubeCtrl.SpawnGarbageCubeAsync()
│  │  ├─ 呼び出し: SettingFireCubes() → FireCubeCtrl.SpawnFireCubeAsync()
│  │  └─ 呼び出し: GameObjectTreat.GetGameManagerObject()
│  │
│  └─ Component: GameTimerCtrl ← ゲームタイマー
│     ├─ 待機: InitializationManager.IsInitialized
│     └─ 参照: EventLoader.GetTimerEvents()
│
└─ その他シーン要素（Camera、Lights など）
```

---

### 初期化フローシーケンス図

```
【初期化フロー】

Timeline (Parallel Execution)
┌─────────────────────────────────────────────────────────────────┐
│ Time: 0ms - Awake Phase                                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Thread/Order: -100 (Script Execution Order)                    │
│  ┌─ InitializationManager.Awake()                               │
│  │  └─ (最小限の処理のみ)                                        │
│  │     - instance 設定                                          │
│  │     - dictionaries 初期化                                     │
│  │     - ✅ 完了                                                │
│  │                                                               │
│  Thread/Order: -50                                              │
│  ┌─ GamePrefabs.Awake()                                         │
│  │  └─ instance 設定のみ                                        │
│  │     - DontDestroyOnLoad 設定                                 │
│  │     - ✅ 完了                                                │
│  │                                                               │
│  Thread/Order: 0 (Default)                                      │
│  ├─ EventLoader.Awake()                                         │
│  │  └─ instance 設定のみ                                        │
│  │     - ✅ 完了                                                │
│  │                                                               │
│  ├─ YamlLoader.Awake()                                          │
│  │  └─ instance 設定のみ                                        │
│  │     - ✅ 完了                                                │
│  │                                                               │
│  ├─ FireCubeCtrl.Awake()                                        │
│  │  └─ コンポーネント参照取得のみ                                 │
│  │     - Transform, Rigidbody など                              │
│  │     - ✅ 完了                                                │
│  │                                                               │
│  └─ GarbageCubeCtrl.Awake()                                     │
│     └─ コンポーネント参照取得のみ                                 │
│        - Transform, Rigidbody など                              │
│        - ✅ 完了                                                │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Time: 10-50ms - OnEnable Phase                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  OnEnable() 呼び出し（各コンポーネント）                          │
│  - EventLoader.OnEnable()                                        │
│    └─ ❌ 削除 または 最小限に                                    │
│    └─ 初期化待機へ移行 (Start へ)                                │
│                                                                   │
│  - YamlLoader.OnEnable()                                         │
│    └─ ❌ 削除 または 最小限に                                    │
│    └─ 初期化待機へ移行 (Start へ)                                │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Time: 50-100ms - Start Phase (Script Execution Order)           │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Order: -100 (最優先)                                            │
│  ┌─ InitializationManager.Start()                               │
│  │  ├─ yield return null (全 Awake 完了確認)                    │
│  │  └─ StartCoroutine(InitializeAllComponents())                │
│  │     └─ [処理開始] InitializeResourceLoaders()                │
│  │     └─ [処理開始] InitializeManagers()                       │
│  │     └─ [処理開始] InitializeUIComponents()                   │
│  │                                                               │
│  Order: -50                                                      │
│  ┌─ GamePrefabs.Start()                                         │
│  │  ├─ StartCoroutine(InitializePrefabs())                      │
│  │  │  └─ PrefabManager.LoadAllGamePrefabs() [時間: 200-500ms] │
│  │  │     ├─ TextureResourceLoader.Load()                      │
│  │  │     ├─ MaterialManager.Load()                            │
│  │  │     └─ PrefabManager.InstantiateFromResources()          │
│  │  │        └─ ✅ 完了後                                       │
│  │  │           NotifyGamePrefabsReady() 呼び出し               │
│  │  │           ↓ ↓ ↓ (Event 発火)                             │
│  │  │                                                            │
│  │  │ ┌────────────────────────────────────────────────────┐  │
│  │  │ │ イベント: OnGamePrefabsReady                       │  │
│  │  │ │ ⚡ Event 発火！購読者に通知開始                      │  │
│  │  │ └────────────────────────────────────────────────────┘  │
│  │  │                                                            │
│  │  └─ ✅ 完了                                                  │
│  │                                                               │
│  Order: 100 (遅延実行)                                           │
│  ├─ EventLoader.Start()                                         │
│  │  ├─ InitializationManager.OnGamePrefabsReady += LoadEvents   │
│  │  ├─ if (IsGamePrefabsReady) { LoadEvents(); } [Fallback]    │
│  │  │  └─ ❌ 初期化完了まで待機状態                            │
│  │  │     (Event 受信待機)                                     │
│  │  │                                                            │
│  │  └─ ✅ 待機中...                                            │
│  │                                                               │
│  ├─ YamlLoader.Start()                                          │
│  │  ├─ InitializationManager.OnGamePrefabsReady += LoadYaml     │
│  │  ├─ if (IsGamePrefabsReady) { LoadYaml(); } [Fallback]      │
│  │  │  └─ ❌ 初期化完了まで待機状態                            │
│  │  │     (Event 受信待機)                                     │
│  │  │                                                            │
│  │  └─ ✅ 待機中...                                            │
│  │                                                               │
│  ├─ FireCubeCtrl.Start()                                        │
│  │  ├─ yield return new WaitUntil(() =>                        │
│  │  │    InitializationManager.IsInitialized)                  │
│  │  │  └─ ❌ 初期化完了まで待機状態                            │
│  │  │                                                            │
│  │  └─ ✅ 待機中...                                            │
│  │                                                               │
│  └─ GarbageCubeCtrl.Start()                                     │
│     ├─ yield return new WaitUntil(() =>                        │
│     │  InitializationManager.IsInitialized)                    │
│     │  └─ ❌ 初期化完了まで待機状態                            │
│     │                                                            │
│     └─ ✅ 待機中...                                            │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Time: 200-500ms - Async Resource Loading (GamePrefabs)          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  【並行実行 - InitializationManager コルーチン】                  │
│                                                                   │
│  ┌─ InitializeResourceLoaders()                                 │
│  │  ├─ TextureResourceLoader 初期化                              │
│  │  │  └─ Resources.LoadAll("Textures/") → cache                │
│  │  │  └─ yield return null (負荷分散)                          │
│  │  │     MarkStepAsInitialized("TextureLoader")                │
│  │  │                                                            │
│  │  └─ MaterialManager 初期化                                     │
│  │     └─ Resources.LoadAll("Materials/") → cache               │
│  │     └─ yield return null                                     │
│  │        MarkStepAsInitialized("MaterialManager")              │
│  │                                                               │
│  ┌─ InitializeManagers()                                        │
│  │  ├─ GameConfig 確認                                           │
│  │  │  └─ yield return null                                     │
│  │  │                                                            │
│  │  ├─ FireCubeCtrl 確認                                        │
│  │  │  └─ yield return new WaitUntil(() => fireCube.IsInit...) │
│  │  │     MarkStepAsInitialized("FireCubeCtrl")                 │
│  │  │                                                            │
│  │  └─ GarbageCubeCtrl 確認                                     │
│  │     └─ yield return new WaitUntil(() => garbageCube.IsInit)  │
│  │        MarkStepAsInitialized("GarbageCubeCtrl")              │
│  │                                                               │
│  └─ InitializeUIComponents()                                    │
│     └─ (現在は最小限)                                            │
│        yield return null                                        │
│                                                                   │
│  ✅ 全ステップ完了 → isInitialized = true                       │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Time: 500ms - GamePrefabs ロード完了 & イベント発火              │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  【GamePrefabs 側】                                              │
│  ┌─ InitializePrefabs() コルーチン完了                           │
│  │  └─ Debug.Log("[GamePrefabs] プレファブロード完了")           │
│  │                                                               │
│  │  ⚡⚡⚡ NotifyGamePrefabsReady() 呼び出し ⚡⚡⚡             │
│  │  │                                                            │
│  │  │  _isGamePrefabsReady = true                               │
│  │  │  OnGamePrefabsReady?.Invoke()                             │
│  │  │                                                            │
│  │  └─ ✅ 完了                                                  │
│  │                                                               │
│  【Event 購読者たち - 通知受信】                                  │
│  ├─ EventLoader.LoadEvents() 実行 ✅                            │
│  │  ├─ ReadEventYaml() 開始                                     │
│  │  ├─ _timer_events 構築                                       │
│  │  ├─ _board_data 読み込み                                     │
│  │  └─ GameTimerCtrl に参照設定                                 │
│  │                                                               │
│  ├─ YamlLoader.LoadYaml() 実行 ✅                               │
│  │  ├─ YamlValidator 実行                                       │
│  │  ├─ 各ステージの YAML ファイル読み込み                       │
│  │  └─ リポジトリに結果保存                                      │
│  │                                                               │
│  ├─ FireCubeCtrl.InitializePool() 完了 ✅                       │
│  │  └─ オブジェクトプール 100 個初期化完了                      │
│  │     isInitialized = true                                     │
│  │                                                               │
│  └─ GarbageCubeCtrl.InitializePool() 完了 ✅                    │
│     └─ オブジェクトプール 100 個初期化完了                      │
│        isInitialized = true                                     │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Time: 550-600ms - シーン初期化 & ゲーム開始                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  【FireCubeCtrl & GarbageCubeCtrl - InitializePool 完了後】      │
│  ├─ FireCubeCtrl (IsInitialized = true)                         │
│  │  └─ Start() コルーチン続行 (WaitUntil 解除)                  │
│  │     ├─ yield return InitializePool()                         │
│  │     ├─ isInitialized = true                                  │
│  │     └─ ✅ 完了                                               │
│  │                                                               │
│  └─ GarbageCubeCtrl (IsInitialized = true)                      │
│     └─ Start() コルーチン続行 (WaitUntil 解除)                  │
│        ├─ yield return InitializePool()                         │
│        ├─ isInitialized = true                                  │
│        └─ ✅ 完了                                               │
│                                                                   │
│  【UnitFireDisaster - ゲーム開始】                               │
│  └─ Start() コルーチン続行 (WaitUntil 解除)                     │
│     ├─ ChangeDemMeshSize()                                      │
│     ├─ SettingWalls()                                           │
│     ├─ SettingWaterTurret()                                     │
│     ├─ SettingCubes(50, distance) → GarbageCube スポーン       │
│     ├─ SettingFireCubes(50, distance) → FireCube スポーン       │
│     └─ ✅ ステージ完全準備完了 🎮 ゲーム開始！                   │
│                                                                   │
│  【GameTimerCtrl - タイマー開始】                                │
│  └─ Start() コルーチン続行 (WaitUntil 解除)                     │
│     ├─ EventLoader からのタイマーイベント取得                    │
│     ├─ タイマー開始                                              │
│     └─ ✅ イベント駆動開始                                       │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

### コンポーネント依存関係図（DAG - Directed Acyclic Graph）

```
【依存関係グラフ】

InitializationManager (中心ハブ)
├─ イベント: OnGamePrefabsReady
│
├→ GamePrefabs (1段目)
│  ├→ PrefabManager
│  │  ├→ TextureResourceLoader
│  │  ├→ MaterialManager
│  │  └→ Resources.Load() API
│  │
│  └→ InitializationManager (通知)
│     ┌─ Event 発火 ⚡
│     │
│     ├→ EventLoader (2段目)
│     │  ├→ YamlRepository
│     │  ├→ ReadEventYaml()
│     │  └→ GameTimerCtrl に参照設定
│     │
│     ├→ YamlLoader (2段目)
│     │  ├→ YamlValidator
│     │  ├→ StageRepository
│     │  └→ PathmakerRepository
│     │
│     ├→ FireCubeCtrl (2段目)
│     │  ├→ GamePrefabs.GetGamePrefabs() [プレファブ取得]
│     │  ├→ ObjectPool 初期化
│     │  └→ Instantiate(firePrefab)
│     │
│     └→ GarbageCubeCtrl (2段目)
│        ├→ GamePrefabs.GetGamePrefabs() [プレファブ取得]
│        ├→ ObjectPool 初期化
│        └→ Instantiate(garbageCubePrefab)

【シーン開始層】
UnitFireDisaster (3段目)
├─ InitializationManager.IsInitialized 待機
├─ GameObjectTreat.GetGameManagerObject()
├─ GarbageCubeCtrl.SpawnGarbageCubeAsync() 呼び出し
├─ FireCubeCtrl.SpawnFireCubeAsync() 呼び出し
└─ SettingCubes(), SettingWalls() など実行

GameTimerCtrl (3段目)
├─ InitializationManager.IsInitialized 待機
├─ EventLoader.GetTimerEvents() から参照取得
└─ タイマー開始
```

---

## 初期化フロー

### 新しい初期化フロー（提案）

```
【改善後の初期化フロー】

Time: 0ms - Awake Phase
├─ InitializationManager.Awake() [-100] ⬅️ Script Execution Order 最優先
│  └─ (最小限の処理のみ)
│
├─ GamePrefabs.Awake() [0]
│  ├─ プレファブロード開始（非同期）
│  └─ (他には何もしない)
│
├─ EventLoader.Awake() [0]
│  └─ instance 設定のみ
│
└─ YamlLoader.Awake() [0]
   └─ instance 設定のみ

Time: 10ms - OnEnable Phase
├─ EventLoader.OnEnable()
│  └─ ❌ 削除 または 最小限に（初期化待機へ移行）
│
└─ YamlLoader.OnEnable()
   └─ ❌ 削除 または 最小限に（初期化待機へ移行）

Time: 50-100ms - Start Phase
├─ InitializationManager.Start() [-99] ⬅️ 最優先で開始
│  ├─ 全 Awake() 完了を確認（yield return null）
│  └─ InitializeAllComponents() コルーチン開始
│
├─ GamePrefabs.Start() (Start Phase には入らない - Awake で処理完了目安)
│
├─ EventLoader.Start() [100] ⬅️ 遅延実行
│  └─ WaitUntil(() => InitializationManager.IsGamePrefabsReady)
│     └─ ✅ GamePrefabs ロード完了時に LoadEvents() 呼び出し
│
└─ YamlLoader.Start() [100]
   └─ WaitUntil(() => InitializationManager.IsGamePrefabsReady)
      └─ ✅ ロード完了時に LoadYaml() 呼び出し

Time: 500ms - GamePrefabs ロード完了
├─ GamePrefabs が NotifyGamePrefabsReady() 呼び出し
│  └─ InitializationManager.OnGamePrefabsReady?.Invoke()
│
├─ EventLoader が Event 受信
│  └─ LoadEvents() 実行 ✅
│
└─ YamlLoader が Event 受信
   └─ LoadYaml() 実行 ✅

Time: 600ms - 全初期化完了
└─ InitializationManager.IsInitialized = true
   └─ ゲーム進行開始 ✅
```

### Script Execution Order 設定

```csharp
【推奨値】

-100: InitializationManager ⬅️ 絶対最優先
-50: GamePrefabs ⬅️ 次優先
  0: EventLoader, YamlLoader（デフォルト）
 50: その他のシステム
100: GameTimerCtrl, UnitFireDisaster などシーン依存
```

---

## 実装ステップ

### Week 1: InitializationManager 実装完成

**目的**: 既存の雛形を実装完成させる

**タスク 1-1: InitializationManager.cs 拡張**

```csharp
// 追加する実装
internal static class InitializationManager
{
    // [1] Readiness フラグ
    private static bool _isGamePrefabsReady = false;
    public static bool IsGamePrefabsReady => _isGamePrefabsReady;
    
    // [2] Event - パターン A 対応
    public static event System.Action OnGamePrefabsReady;
    
    // [3] 初期化通知メソッド
    internal static void NotifyGamePrefabsReady()
    {
        _isGamePrefabsReady = true;
        OnGamePrefabsReady?.Invoke();
        Debug.Log("[InitializationManager] GamePrefabs 初期化完了");
    }
    
    // [4] 初期化確認メソッド（安全弁）
    internal static void WaitForGamePrefabsReady()
    {
        if (!_isGamePrefabsReady)
        {
            throw new System.InvalidOperationException(
                "[InitializationManager] GamePrefabs がまだ初期化されていません");
        }
    }
}
```

**時間見積**: 0.5 日

---

### Week 2: GamePrefabs.cs 作成

**目的**: Entry point クラスを実装し、InitializationManager と統合

**タスク 2-1: GamePrefabs.cs 新規作成**

```csharp
// 新規作成: Assets/Scripts/Core/Managers/GamePrefabs.cs
internal class GamePrefabs : MonoBehaviour
{
    private static GamePrefabs _instance;
    
    // ゲーム用プレファブの参照
    private Dictionary<string, GameObject> _gamePrefabs = new();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        StartCoroutine(InitializePrefabs());
    }
    
    private IEnumerator InitializePrefabs()
    {
        Debug.Log("[GamePrefabs] プレファブロード開始");
        
        // プレファブの読み込み（Resources.Load など）
        yield return PrefabManager.LoadAllGamePrefabs(_gamePrefabs);
        
        Debug.Log("[GamePrefabs] プレファブロード完了");
        
        // ✅ 重要: InitializationManager に通知
        InitializationManager.NotifyGamePrefabsReady();
    }
    
    public static Dictionary<string, GameObject> GetGamePrefabs()
    {
        if (_instance == null || _instance._gamePrefabs.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[GamePrefabs] GamePrefabs がまだ初期化されていません");
        }
        return _instance._gamePrefabs;
    }
}
```

**時間見積**: 1 日

---

### Week 3: EventLoader & YamlLoader 修正

**目的**: 初期化待機メカニズムを追加

**タスク 3-1: EventLoader.cs 修正**

```csharp
// 修正前（危険）
public class EventLoader : MonoBehaviour
{
    private void OnEnable()  // ❌ 初期化完了前に呼ばれる
    {
        LoadEvents();  // ❌ GamePrefabs がまだ null
    }
}

// 修正後（安全）
public class EventLoader : MonoBehaviour
{
    private void OnEnable()
    {
        // OnEnable では何もしない
        // 初期化待機は Start で行う
    }
    
    private void Start()
    {
        // パターン A: Event 購読
        InitializationManager.OnGamePrefabsReady += LoadEvents;
        
        // パターン B: Fallback（既に完了している場合）
        if (InitializationManager.IsGamePrefabsReady)
        {
            LoadEvents();
        }
    }
    
    private void OnDestroy()
    {
        InitializationManager.OnGamePrefabsReady -= LoadEvents;
    }
    
    private void LoadEvents()
    {
        Debug.Log("[EventLoader] イベント読み込み開始");
        // 既存の LoadEvents() ロジック
    }
}
```

**時間見積**: 0.5 日

**タスク 3-2: YamlLoader.cs 修正（存在確認後）**

```csharp
// 修正パターン（EventLoader と同じ）
private void Start()
{
    InitializationManager.OnGamePrefabsReady += LoadYaml;
    if (InitializationManager.IsGamePrefabsReady)
    {
        LoadYaml();
    }
}
```

**時間見積**: 0.5 日

---

### Week 4: テスト & ドキュメント

**目的**: 低スペック環境での動作確認とドキュメント整備

**タスク 4-1: 機能テスト**

- [ ] 高スペック環境での動作確認（初期化順序 OK）
- [ ] 低スペック環境（VM など）での動作確認
- [ ] Exception ハンドリング確認

**タスク 4-2: ドキュメント更新**

- [ ] InitializationManager_Usage.md に Phase 1.5 パターン追加
- [ ] Script Execution Order 設定ガイド
- [ ] トラブルシューティング追加

**時間見積**: 1 日

---

## 各コンポーネントの修正方針

### InitializationManager

| 項目 | 内容 |
|------|------|
| **ファイル** | `Assets/Scripts/Core/Managers/InitializationManager.cs` |
| **修正** | Event・Readiness フラグの追加実装 |
| **設定** | Script Execution Order: -100 |
| **責務** | 初期化順序制御・通知 |
| **テスト** | IsGamePrefabsReady フラグの遷移確認 |

### GamePrefabs（新規作成）

| 項目 | 内容 |
|------|------|
| **ファイル** | `Assets/Scripts/Core/Managers/GamePrefabs.cs`（新規） |
| **配置** | シーンに GameObject を作成、このスクリプトをアタッチ |
| **初期化** | Start() コルーチン内でプレファブロード |
| **通知** | NotifyGamePrefabsReady() 呼び出し |
| **設定** | Script Execution Order: -50（InitializationManager の次） |
| **依存** | PrefabManager |

### EventLoader

| 項目 | 内容 |
|------|------|
| **ファイル** | `Assets/Scripts/Game/Events/System/EventLoader.cs` |
| **修正** | OnEnable → Start へ移行、Event 購読追加 |
| **待機** | InitializationManager.OnGamePrefabsReady を購読 |
| **設定** | Script Execution Order: 100（遅延実行） |
| **タイミング** | LoadEvents() は GamePrefabs ロード完了後 |

### YamlLoader（ファイル確認後）

| 項目 | 内容 |
|------|------|
| **ファイル** | `Assets/Scripts/.../YamlLoader.cs` |
| **修正** | EventLoader と同じパターン |
| **待機** | InitializationManager.OnGamePrefabsReady を購読 |

---

## スケジュール

### Phase 1.5: 初期化順序管理

```
【全体期間】 3-4 日

Week 1 (1日)
├─ InitializationManager.cs 拡張
│  └─ Event / Readiness フラグ追加
│  └─ NotifyGamePrefabsReady() 実装
│  └─ Script Execution Order -100 設定
└─ 時間: 0.5 日

Week 2 (1日)
├─ GamePrefabs.cs 新規作成
│  └─ PrefabManager との統合
│  └─ InitializationManager への通知
│  └─ Script Execution Order -50 設定
└─ 時間: 1 日

Week 3 (1日)
├─ EventLoader.cs 修正
│  └─ Start() へ初期化待機移行
│  └─ Event 購読追加
│  └─ Script Execution Order 100 設定
├─ YamlLoader.cs 修正（同パターン）
│  └─ 時間: 0.5 日
└─ 時間: 1 日

Week 4 (0.5-1日)
├─ 機能テスト（高・低スペック環境）
│  └─ 時間: 0.5 日
└─ ドキュメント更新
   └─ 時間: 0.5 日

【合計】 3-4 日 = 24-32 工数

【その後】
→ Phase 1.4 UI 改善（1920×1080標準化）へ進行
```

---

## GamePrefabs にアタッチされるコンポーネント一覧

### コンポーネント構成

GamePrefabs オブジェクトにアタッチされているコントローラークラスの完全リスト（合計 22 個）

#### Panels（パネル UI）- 5 個

| # | クラス名 | ファイルパス | 責務 |
|---|---------|-------------|------|
| 1 | EscMenuCtrl | Presentation/UI/Panels/ | ESC メニュー表示制御 |
| 2 | TabMenuCtrl | Presentation/UI/Panels/ | タブメニュー管理 |
| 3 | NoticeCtrl | Presentation/UI/Panels/ | 通知ポップアップ表示 |
| 4 | DebugInfoCtrl | Presentation/UI/Panels/ | デバッグ情報パネル |
| 5 | SpawnMarkerPointerCtrl | Presentation/UI/Panels/ | スポーンマーカー表示 |

#### HUD（常時表示 UI）- 8 個

| # | クラス名 | ファイルパス | 責務 |
|---|---------|-------------|------|
| 6 | MarkerIndicatorCtrl | Presentation/UI/HUD/ | マーカーインジケーター |
| 7 | MarkerPointerCtrl | Presentation/UI/HUD/ | マーカーポインター |
| 8 | PathMakerCtrl | Presentation/UI/HUD/ | パス表示制御 |
| 9 | ScoreCtrl | Presentation/UI/HUD/ | スコア表示 |
| 10 | TelopCtrl | Presentation/UI/HUD/ | テロップ表示 |
| 11 | TooltipInfoCtrl | Presentation/UI/HUD/ | ツールチップ情報 |
| 12 | MouseOverTipsCtrl | Presentation/UI/HUD/ | マウスオーバーヒント |
| 13 | CircularIndicator | Presentation/UI/HUD/ | 円形インジケーター |

#### Controls（UI制御部品）- 5 個

| # | クラス名 | ファイルパス | 責務 |
|---|---------|-------------|------|
| 14 | ClickCtrl | Presentation/UI/Controls/ | クリック検出・処理 |
| 15 | ClosebtnCtrl | Presentation/UI/Controls/ | 閉じるボタン制御 |
| 16 | OkbtnCtrl | Presentation/UI/Controls/ | OK ボタン制御 |
| 17 | WindowCloseCtrl | Presentation/UI/Controls/ | ウィンドウ閉じる処理 |
| 18 | WindowDragCtrl | Presentation/UI/Controls/ | ウィンドウドラッグ制御 |

#### Dialogs（ダイアログ）- 4 個

| # | クラス名 | ファイルパス | 責務 |
|---|---------|-------------|------|
| 19 | EventLogCtrl | Presentation/UI/Dialogs/ | イベントログダイアログ |
| 20 | GameTimerCtrl | Presentation/UI/Dialogs/ | ゲームタイマー表示 |
| 21 | InfoWindowCtrl | Presentation/UI/Dialogs/ | 情報ウィンドウ |
| 22 | MessageBoxCtrl | Presentation/UI/Dialogs/ | メッセージボックス |

### 初期化戦略

これらのコンポーネントは **Awake 時点で最小限の初期化** を実行し、**Start 時点で IsInitialized フラグを true に設定** する予定です。

---

## UIControllerBase - 共通ベースクラス設計

### 目的

22 個のコントローラークラスに対して **IsInitialized フラグ**を統一的に管理するための共通基底クラス

### 設計

```csharp
/// <summary>
/// UI コントローラーの共通基底クラス
/// すべてのコントローラークラス（*Ctrl）はこれを継承
/// 
/// 責務:
/// - IsInitialized フラグの管理
/// - 初期化完了通知
/// - InitializationManager との連携
/// </summary>
public abstract class UIControllerBase : MonoBehaviour
{
    /// <summary>
    /// 初期化が完了したかどうか
    /// 各子クラスの Start() で true に設定される
    /// </summary>
    public bool IsInitialized { get; protected set; } = false;

    /// <summary>
    /// 子クラスが初期化を開始する前の前処理
    /// 各子クラスで必要に応じてオーバーライド
    /// </summary>
    protected virtual void Awake()
    {
        // Awake 時は IsInitialized = false のまま
        IsInitialized = false;
    }

    /// <summary>
    /// 子クラスの初期化メイン処理
    /// Start() または InitializeAsync() で呼び出される
    /// </summary>
    protected abstract IEnumerator Initialize();

    /// <summary>
    /// Start() からの呼び出しパターン
    /// コルーチンを開始し、完了後に IsInitialized = true
    /// </summary>
    protected virtual void Start()
    {
        StartCoroutine(InitializeAsync());
    }

    /// <summary>
    /// 初期化コルーチン
    /// Initialize() → IsInitialized = true の順序で実行
    /// </summary>
    protected virtual IEnumerator InitializeAsync()
    {
        yield return Initialize();
        IsInitialized = true;
        
        Debug.Log($"[UIControllerBase] {this.GetType().Name} 初期化完了");
    }

    /// <summary>
    /// 初期化完了を待機（外部用）
    /// 例: yield return new WaitUntil(() => uiCtrl.IsInitialized)
    /// </summary>
    public bool IsReady => IsInitialized;
}
```

### 実装パターン例

#### パターン 1: 最小限の初期化（多くのコントローラー向け）

```csharp
// 例: EscMenuCtrl
public class EscMenuCtrl : UIControllerBase
{
    private Button _escButton;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Awake では参照取得のみ
        _escButton = GetComponentInChildren<Button>();
    }
    
    protected override IEnumerator Initialize()
    {
        // 初期化処理（必要に応じて）
        _escButton.onClick.AddListener(OnEscButtonClicked);
        
        yield return null;  // 非同期処理がなければ即座に完了
    }
    
    private void OnEscButtonClicked()
    {
        // ボタンクリック時の処理
    }
}
```

#### パターン 2: リソース読み込みが必要な場合（高負荷コントローラー向け）

```csharp
// 例: GameTimerCtrl
public class GameTimerCtrl : UIControllerBase
{
    private Text _timerText;
    
    protected override void Awake()
    {
        base.Awake();
        _timerText = GetComponentInChildren<Text>();
    }
    
    protected override IEnumerator Initialize()
    {
        // EventLoader の準備完了を待機
        yield return new WaitUntil(() => InitializationManager.IsGamePrefabsReady);
        
        // イベントデータ取得
        var timerEvents = EventLoader.Instance.GetTimerEvents();
        yield return null;
        
        // タイマー初期化完了
    }
}
```

#### パターン 3: GamePrefabs への依存がある場合

```csharp
// 例: MarkerIndicatorCtrl
public class MarkerIndicatorCtrl : UIControllerBase
{
    protected override IEnumerator Initialize()
    {
        // GamePrefabs 準備完了まで待機
        yield return new WaitUntil(() => InitializationManager.IsGamePrefabsReady);
        
        // GamePrefabs から必要なプレファブ取得
        var prefabs = GamePrefabs.GetGamePrefabs();
        yield return null;
    }
}
```

### 導入計画

| フェーズ | タスク | 時間 | ステータス |
|---------|-------|------|----------|
| **Phase 1.5-0** | IInitializable インターフェース作成 | 0.25 日 | ✅ 完了 |
| **Phase 1.5-1** | UIControllerBase.cs 作成・実装 | 0.5 日 | ✅ 完了 |
| **Phase 1.5-2** | 各コントローラー（22個）を UIControllerBase に変更 | 1.5 日 | ⏳ 実装待ち |
| **Phase 1.5-3** | InitializationManager に動的検出ロジック追加 | 0.5 日 | ✅ 完了 |
| **Phase 1.5-4** | テスト・検証 | 0.5 日 | ⏳ テスト待ち |
| **合計** | | **3.25 日** | 25% 完了 |

### 利点

- [OK] **統一性**: すべてのコントローラーで IsInitialized フラグが同じ動作
- [OK] **拡張性**: 新規コントローラー追加時も自動的にフラグ管理
- [OK] **テスト性**: モッククラスで IsInitialized 動作をシミュレート可能
- [OK] **保守性**: 共通ロジックが一箇所に集約

---

## 動的管理アーキテクチャ（Phase 1.5-3 実装済み）

### IInitializable インターフェース

```csharp
/// <summary>
/// 初期化管理インターフェース
/// InitializationManager で動的に検出されるための標準インターフェース
/// </summary>
public interface IInitializable
{
    /// <summary>初期化が完了したかどうか</summary>
    bool IsInitialized { get; }
    
    /// <summary>コンポーネント名を取得（ログ出力用）</summary>
    string GetComponentName();
}
```

### UIControllerBase 実装

```csharp
// UIControllerBase が IInitializable を実装
public abstract class UIControllerBase : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; protected set; } = false;
    
    public string GetComponentName()
    {
        return this.GetType().Name;
    }
    
    // ... その他のメソッド
}
```

### InitializationManager 動的検出ロジック

```csharp
private IEnumerator InitializeManagers()
{
    Debug.Log("[InitializationManager] GamePrefabs のコントローラーを自動検出");
    
    // [1] GamePrefabs オブジェクト取得
    GameObject gamePrefabsObj = GameObjectTreat.GetGameManagerObject();
    if (gamePrefabsObj == null)
    {
        Debug.LogWarning("[InitializationManager] GamePrefabs が見つかりません");
        yield break;
    }
    
    // [2] IInitializable を実装したすべてのコンポーネントを検出
    IInitializable[] controllers = gamePrefabsObj.GetComponentsInChildren<IInitializable>();
    
    if (controllers.Length == 0)
    {
        Debug.LogWarning("[InitializationManager] IInitializable を実装したコンポーネントがありません");
        yield break;
    }
    
    Debug.Log($"[InitializationManager] {controllers.Length} 個のコントローラーを検出しました");
    
    // [3] 各コントローラーの初期化完了を個別に監視
    foreach (IInitializable controller in controllers)
    {
        yield return new WaitUntil(() => controller.IsInitialized);
        
        string componentName = controller.GetComponentName();
        MarkStepAsInitialized(componentName);
        Debug.Log($"[InitializationManager] [OK] {componentName} 初期化完了");
    }
    
    Debug.Log("[InitializationManager] すべてのコントローラー初期化完了");
}
```

### メリット（実装済み）

| 項目 | メリット |
|------|---------|
| **拡張性** | [OK] 新規コントローラー追加時に InitializationManager を修正不要 |
| **保守性** | [OK] コントローラー削除時に自動的に反映 |
| **動的検出** | [OK] GetComponentsInChildren で実行時に検出可能 |
| **ログ出力** | [OK] 何個検出されたかコンソールで確認可能 |
| **デバッグ** | [OK] 各コントローラーの初期化タイミングを可視化 |

---

## リスク & 対策

### リスク 1: Script Execution Order が保存されない

**症状**: ビルド後に初期化順序がおかしくなる

**対策**:
- [ ] Project Settings の Screenshot を取得
- [ ] `.gitignore` で `ProjectSettings/` が除外されていないか確認
- [ ] 設定後に Unity を再起動してセーブ確認

### リスク 2: GamePrefabs が見つからない

**症状**: NotifyGamePrefabsReady() が呼ばれない

**対策**:
- [ ] シーンに GamePrefabs GameObject があるか確認
- [ ] Script Execution Order が正しく設定されているか確認
- [ ] コンソールで "[GamePrefabs] プレファブロード完了" ログを確認

### リスク 3: EventLoader がロードできない

**症状**: ロード開始の Event が発火しない

**対策**:
- [ ] EventLoader.Start() が呼ばれているか確認
- [ ] Event 購読が正しくセットアップされているか確認
- [ ] デバッグログを追加して動作確認

### リスク 4: YamlLoader が見つからない

**症状**: YamlLoader.cs が workspace に存在しない

**対策**:
- [ ] `grep_search` で YamlLoader を検索
- [ ] 存在しない場合は、EventLoader パターンのみ実装
- [ ] 後で YamlLoader 相当のコンポーネント発見時に修正

---

## Checklist

### 実装前

- [ ] AGENTS.md・coding-standards.md 確認
- [ ] InitializationManager_Usage.md 確認
- [ ] PrefabManager と MaterialManager の依存関係確認

### 実装中

- [ ] Event-driven パターンに統一
- [ ] Null チェック徹底（Recovery Phase 準拠）
- [ ] 関数長が 40 行以下か確認
- [ ] Magic number なし

### テスト

- [ ] 高スペック環境での動作確認
- [ ] 低スペック環境（VM）での動作確認
- [ ] 初期化順序ログの確認
- [ ] Exception handling の確認

### ドキュメント

- [ ] Script Execution Order ガイド作成
- [ ] トラブルシューティング追加
- [ ] サンプルコード更新

---

## 参考資料

- [AGENTS.md - クラス命名規則](../AGENTS.md#クラス命名規則)
- [AGENTS.md - 初期化順序ルール](../AGENTS.md#development-workflow)
- [InitializationManager_Usage.md](../Assets/Scripts/Core/Managers/InitializationManager_Usage.md)
- [coding-standards.md](./coding-standards.md)

---

**作成者**: GitHub Copilot  
**最終更新**: 2026-01-29  
**ステータス**: 📋 設計段階（実装待ち）
