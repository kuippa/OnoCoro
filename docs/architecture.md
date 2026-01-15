# OnoCoro アーキテクチャ定義

このドキュメントは、OnoCoro プロジェクトの全体アーキテクチャを定義します。

**目的**: 構成の一貫性維持・勝手な設計変更の防止・新規参加者の理解促進

---

## アーキテクチャ概要

### システム構成

OnoCoro は、以下の3層構造で設計されています:

```
┌─────────────────────────────────────────┐
│        Presentation Layer (UI)          │
│  - Scene Controllers                    │
│  - UI Components                        │
│  - Input Handlers                       │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         Game Logic Layer                │
│  - Game Managers                        │
│  - Game Systems                         │
│  - Entity Controllers                   │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         Data Layer                      │
│  - Utility Classes                      │
│  - Data Models                          │
│  - File Operations                      │
└─────────────────────────────────────────┘
```

---

## 1. Presentation Layer (プレゼンテーション層)

### 責務

- ユーザーインターフェースの表示・制御
- ユーザー入力の受付
- ゲームロジック層への橋渡し

### 主要コンポーネント

#### Scene Controllers

各シーンの初期化・管理を担当するコントローラ

| クラス | 責務 | 配置 |
|--------|------|------|
| `TitleStartCtrl` | タイトル画面の制御 | TitleScene |
| `GameMainCtrl` | ゲームメイン画面の制御 | GameScene |
| `StageEditorCtrl` | ステージエディタの制御 | EditorScene |

**実装パターン**:
```csharp
public class TitleStartCtrl : MonoBehaviour
{
    // UI Elements (SerializeField)
    [SerializeField] private GameObject _pnlStageSelector;
    [SerializeField] private GameObject _pnlAboutThisGame;
    
    // Constants
    private const string _OBJ_LOADING = "nowloading";
    
    // Unity Lifecycle
    void Awake()
    {
        InitializeUI();
        RegisterEventListeners();
    }
    
    // Private Methods (10-40行)
    private void InitializeUI() { /* ... */ }
    private void RegisterEventListeners() { /* ... */ }
}
```

#### UI Components

再利用可能な UI コンポーネント

- **WindowCloseCtrl**: ウィンドウ閉じるボタン制御
- **GameTimerCtrl**: ゲーム内タイマー表示
- **HealthBarCtrl**: ヘルスバー表示

---

## 2. Game Logic Layer (ゲームロジック層)

### 責務

- ゲームの中核ロジック
- ビジネスルールの実装
- ゲーム状態の管理

### 主要コンポーネント

#### Game Managers

シングルトンパターンで実装されるゲーム全体の管理クラス

| クラス | 責務 | パターン |
|--------|------|---------|
| `GameManager` | ゲーム全体の状態管理 | Singleton |
| `StageManager` | ステージデータ管理 | Singleton |
| `ResourceManager` | リソース（予算・エネルギー）管理 | Singleton |

**Singleton 実装パターン**:
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
```

#### Game Systems

ゲームの特定機能を担当するシステム

- **TowerSystem**: タワー配置・管理
- **EnemySpawnSystem**: 敵の生成・管理
- **WaveSystem**: ウェーブ進行管理
- **ScoreSystem**: スコア計算

#### Entity Controllers

ゲーム内エンティティ（タワー、敵など）の制御

- **TowerController**: タワーの挙動制御
- **EnemyController**: 敵の挙動制御
- **ProjectileController**: 弾丸の挙動制御

---

## 3. Data Layer (データ層)

### 責務

- データの永続化・読み込み
- ファイル操作
- データ変換・検証

### 主要コンポーネント

#### Utility Classes

共通機能を提供する静的クラス

| クラス | 責務 | 主要メソッド |
|--------|------|------------|
| `LoadStreamingAsset` | StreamingAssets 操作 | `GetYamlFileName()`, `YamlFileExists()`, `AllTextStream()` |
| `UIHelper` | UI 操作支援 | `FindGameObject()`, `RegisterButton()`, `ResetScrollbarInPanel()` |
| `FileOperationUtility` | ファイル操作 | `OpenInEditor()`, `LoadImage()` |
| `StageDataManager` | ステージデータ管理 | `LoadStageList()`, `ParseCSV()` |
| `LogUtility` | ログ管理 | `Log()`, `LogWarning()`, `LogError()` |

#### Data Models

ゲームデータを表現するモデルクラス

```csharp
// ステージ情報
public class StageInfo
{
    public string sceneName;
    public string displayName;
    public string description;
    public int difficulty;
}

// タワー情報
public class TowerData
{
    public string towerId;
    public string towerName;
    public int cost;
    public float range;
    public float damage;
}

// 敵情報
public class EnemyData
{
    public string enemyId;
    public string enemyName;
    public int health;
    public float speed;
}
```

---

## PLATEAU SDK 統合

### PLATEAU SDK の役割

PLATEAU SDK は、3D都市モデルデータの読み込み・表示を担当します。

### 統合アーキテクチャ

```
┌─────────────────────────────────────────┐
│         OnoCoro Game Logic              │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│       PLATEAU SDK for Unity             │
│  - CityGML Parser                       │
│  - 3D Model Loader                      │
│  - Coordinate Transformer               │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         StreamingAssets                 │
│  - CityGML Files (.gml)                 │
│  - Stage Config (.yaml)                 │
└─────────────────────────────────────────┘
```

### PLATEAU データフロー

1. **ステージ選択**: ユーザーがステージを選択
2. **設定読み込み**: YAML ファイルから CityGML ファイルパスを取得
3. **PLATEAU SDK 呼び出し**: CityGML データをロード
4. **3D モデル生成**: Unity シーンに建物・地形を配置
5. **ゲーム開始**: 配置されたモデルをゲームフィールドとして使用

---

## ファイル構成

### Assets フォルダ構造

```
Assets/
├── Scenes/                    # Unity シーン
│   ├── TitleScene.unity      # タイトル画面
│   ├── GameScene.unity       # ゲームメイン
│   └── EditorScene.unity     # ステージエディタ
│
├── Scripts/                   # C# スクリプト
│   ├── Controllers/          # Scene Controllers
│   │   ├── TitleStartCtrl.cs
│   │   ├── GameMainCtrl.cs
│   │   └── StageEditorCtrl.cs
│   │
│   ├── Managers/             # Game Managers
│   │   ├── GameManager.cs
│   │   ├── StageManager.cs
│   │   └── ResourceManager.cs
│   │
│   ├── Systems/              # Game Systems
│   │   ├── TowerSystem.cs
│   │   ├── EnemySpawnSystem.cs
│   │   └── WaveSystem.cs
│   │
│   ├── Entities/             # Entity Controllers
│   │   ├── TowerController.cs
│   │   ├── EnemyController.cs
│   │   └── ProjectileController.cs
│   │
│   ├── UI/                   # UI Components
│   │   ├── WindowCloseCtrl.cs
│   │   ├── GameTimerCtrl.cs
│   │   └── HealthBarCtrl.cs
│   │
│   ├── Utilities/            # Utility Classes
│   │   ├── LoadStreamingAsset.cs
│   │   ├── UIHelper.cs
│   │   ├── FileOperationUtility.cs
│   │   ├── StageDataManager.cs
│   │   └── LogUtility.cs
│   │
│   └── Models/               # Data Models
│       ├── StageInfo.cs
│       ├── TowerData.cs
│       └── EnemyData.cs
│
├── Prefabs/                  # プレハブ
│   ├── UI/                   # UI プレハブ
│   ├── Towers/               # タワープレハブ
│   ├── Enemies/              # 敵プレハブ
│   └── Effects/              # エフェクトプレハブ
│
├── StreamingAssets/          # ストリーミングアセット
│   ├── Stages/               # ステージデータ
│   │   ├── stagelist.csv    # ステージ一覧
│   │   ├── stage01.yaml     # ステージ1設定
│   │   └── stage02.yaml     # ステージ2設定
│   │
│   └── CityData/             # PLATEAU データ
│       └── *.gml             # CityGML ファイル
│
└── Resources/                # Resources フォルダ
    ├── Textures/            # テクスチャ
    ├── Materials/           # マテリアル
    └── Audio/               # オーディオ
```

---

## データフロー

### ゲーム起動からプレイまでの流れ

```
1. Unity 起動
   ↓
2. TitleScene ロード
   ↓
3. TitleStartCtrl.Awake()
   - UI 初期化
   - ステージリスト読み込み (StageDataManager)
   ↓
4. ユーザーがステージ選択
   ↓
5. GameScene ロード
   ↓
6. GameMainCtrl.Awake()
   - PLATEAU SDK でCityGML読み込み
   - ゲームシステム初期化
   ↓
7. ゲームプレイ
   - ユーザー入力 → TowerSystem → TowerController
   - ウェーブ進行 → WaveSystem → EnemySpawnSystem
   ↓
8. ゲーム終了
   - スコア保存
   - TitleScene に戻る
```

### ステージデータ読み込みフロー

```
1. StageDataManager.LoadStageList()
   ↓
2. LoadStreamingAsset.AllTextStream("stagelist.csv")
   ↓
3. CSV パース → List<StageInfo>
   ↓
4. UI に表示 (TitleStartCtrl)
   ↓
5. ユーザー選択 → StageInfo 取得
   ↓
6. LoadStreamingAsset.YamlFileExists(sceneName)
   ↓
7. YAML 読み込み → ステージ設定取得
   ↓
8. PLATEAU SDK で CityGML ロード
```

---

## 依存関係

### レイヤー間の依存関係

- **Presentation Layer** → **Game Logic Layer** ✅ OK
- **Game Logic Layer** → **Data Layer** ✅ OK
- **Data Layer** → **Presentation Layer** ❌ NG
- **Presentation Layer** → **Data Layer** ⚠️ Utility Classes のみ許可

### 外部ライブラリ依存

```
OnoCoro
├── Unity Engine (6.3)
├── PLATEAU SDK for Unity
│   └── glTFast (3Dモデルロード)
├── Cinemachine (カメラ制御)
├── Unity Input System (入力管理)
└── TextMesh Pro (テキスト表示)
```

---

## 設計パターン

### 使用している設計パターン

| パターン | 使用箇所 | 目的 |
|---------|---------|------|
| **Singleton** | GameManager, StageManager | ゲーム全体の状態を一元管理 |
| **Observer** | イベントシステム | UI とロジックの疎結合化 |
| **Strategy** | タワー攻撃ロジック | タワータイプごとの攻撃方法の切り替え |
| **Object Pool** | 弾丸・エフェクト | パフォーマンス最適化 |
| **Factory** | 敵生成 | 敵タイプに応じたインスタンス生成 |
| **Static Utility** | UIHelper, LoadStreamingAsset | 共通機能の集約 |

### 避けるべきアンチパターン

❌ **God Object**: すべてを1つのクラスに詰め込まない
❌ **Spaghetti Code**: 複雑な依存関係を作らない
❌ **Magic Numbers**: 定数化を徹底
❌ **Deep Nesting**: Early Return で平坦化

---

## パフォーマンス考慮事項

### 最適化戦略

1. **Object Pooling**: 頻繁に生成・破棄されるオブジェクトをプール管理
2. **LOD (Level of Detail)**: 距離に応じた描画品質の調整
3. **Culling**: カメラ外のオブジェクトの描画スキップ
4. **Batching**: 描画コールの削減

### 推奨ハードウェア

| コンポーネント | 推奨スペック |
|--------------|------------|
| CPU | Intel i7 / AMD Ryzen 7 以上 |
| RAM | 16 GB 以上 |
| GPU | NVIDIA GeForce RTX 2070 以上 |
| ストレージ | SSD 100 GB 以上 |

---

## セキュリティ考慮事項

### ファイル操作の安全性

- **パストラバーサル対策**: `Path.Combine()` を使用
- **存在チェック**: `File.Exists()` で事前確認
- **例外処理**: `try-catch` で適切なエラーハンドリング

### ユーザー入力の検証

- **入力値の範囲チェック**: min/max の制約
- **不正な文字列の排除**: サニタイズ処理
- **null チェック**: 明示的な null 確認

---

## 拡張性

### 新機能追加時の指針

1. **適切なレイヤーに配置**: 責務に応じた層に配置
2. **既存のパターンに従う**: Singleton, Observer などを活用
3. **ユーティリティクラスの活用**: 共通処理は集約
4. **ドキュメントの更新**: このファイルを更新

### プラグインアーキテクチャ

将来的には、以下のようなプラグイン構造を検討:

```
┌─────────────────────────────────────────┐
│         OnoCoro Core                    │
└─────────────────────────────────────────┘
            ↓          ↓          ↓
     ┌──────────┐ ┌──────────┐ ┌──────────┐
     │ Plugin A │ │ Plugin B │ │ Plugin C │
     │ (Towers) │ │(Enemies) │ │ (Maps)   │
     └──────────┘ └──────────┘ └──────────┘
```

---

## テスト戦略

### テスト方針

| テストレベル | 対象 | ツール |
|------------|------|--------|
| **Unit Test** | Utility Classes, Data Models | Unity Test Framework |
| **Integration Test** | System 間の連携 | Unity Test Framework |
| **Play Mode Test** | UI, ゲームプレイ | Unity Test Runner |
| **Manual Test** | ゲーム全体の挙動 | 手動テスト |

---

## 制約事項

### 技術的制約

- **Unity Version**: 6.3 固定（変更不可）
- **PLATEAU SDK**: 必須（削除不可）
- **Windows 環境**: PowerShell 前提（Linux/macOS コマンド使用不可）

### 設計上の制約

- **関数の行数**: 40行以内
- **ネストの深さ**: 2段階まで
- **ファイルサイズ**: 100MB 未満（Git 管理対象）

---

## 改訂履歴

| 日付 | バージョン | 変更内容 |
|------|----------|---------|
| 2026-01-15 | 1.0.0 | 初版作成 |

---

## 参考資料

- [Unity Architecture Best Practices](https://unity.com/how-to/programming-unity)
- [PLATEAU SDK Documentation](https://www.mlit.go.jp/plateau/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
