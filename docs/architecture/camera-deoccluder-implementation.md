# CinemachineDeoccluder 実装ガイド

地面下へのカメラ潜り対策（Deoccluder コライダー配置・フォグ設定）

**Last Updated**: 2026-03-08  
**Status**: 実装手順待ち（TODO リストに記載）  
**関連**: [camera-exposure-settings.md](camera-exposure-settings.md)、[TODO.md](../../TODO.md)

---

## 目次

- [問題概要](#問題概要)
- [解決方法](#解決方法)
- [実装手順](#実装手順)
- [CinemachineDeoccluder 詳細設定](#cinemachinedeoccluder-詳細設定)
- [フォグ設定](#フォグ設定)
- [検証チェックリスト](#検証チェックリスト)

---

## 問題概要

### 症状

- カメラが地面メッシュを通り抜けて、地面下が表示される
- キャラクター視点モード（FPS/TPS）で顕著
- 地面メッシュに Convex=OFF の MeshCollider のみ配置

### 原因分析

MeshCollider (Convex=OFF) は複雑な形状に対応するが、CinemachineDeoccluder がこれを凸形状と判定できない場合がある。また、地面下に物理オブジェクトがないため Deoccluder の Raycast が機能しない。

---

## 解決方法

**デュアルコライダー戦略**: 地面の上に MeshCollider（キャラクター移動用）、下に Capsule Collider（Deoccluder 対象）を配置

### メリット

- キャラクターは起伏のある地面を正常に歩行可能（MeshCollider Convex=OFF）
- Deoccluder は単純な凸形状（Capsule）で確実に機能
- パフォーマンス影響最小（2つ目の Collider は簡易形状）

---

## 実装手順

### Step 1: 地面 GameObject の確認

**対象**: 石川県金沢市兼六園ステージの `Ground` GameObject（またはベース地面メッシュ）

確認項目：
```
Hierarchy → 検索: "Ground" (または "Terrain", "環境GML_*")
    Inspector → MeshCollider
        [OK] Convex: OFF (キャラクター移動優先)
        [OK] Is Trigger: OFF
        [OK] Rigidbody: Body Type=Kinematic 存在
```

### Step 2: Deoccluder 用 Capsule Collider の作成

**オプションA: 既存 GameObject に追加（推奨・シンプル）**

1. 地面 Ground GameObject を選択
2. `Add Component` → Capsule Collider
3. 以下のように設定：

```
Capsule Collider
├── Direction: Y-Axis
├── Radius: 0.5
├── Height: 地面メッシュの Y 方向の高さ
│   例: 地面最高部分 + 1.0 (バッファ)
├── Center: (0, -地面メッシュ中心 Y 値, 0)
│   ※ 地面の下の空間を囲むように配置
└── Material: None
```

**オプションB: 別 GameObject として配置（推奨・保守性向上）**

1. Hierarchy → Ground GameObject 子要素として作成
   ```
   Ground
   ├── Mesh (既存の MeshCollider)
   └── DeoccluderCollider (新規 GameObject)
   ```

2. DeoccluderCollider GameObject 設定：
   ```
   Position: (0, 地面メッシュの Y 中心 - Height/2, 0)
   Scale: (1, 1, 1)
   ```

3. DeoccluderCollider に Capsule Collider 追加：
   ```
   Direction: Y-Axis
   Radius: 0.5 → 0.8 (拡大めシンプル)
   Height: 10.0 (地面メッシュを十分に包含)
   Center: (0, 0, 0)
   Is Trigger: OFF ✓
   ```

4. Rigidbody 追加（必須）：
   ```
   Body Type: Kinematic
   Gravity: OFF
   Constraints: 全チェック
   ```

### Step 3: CameraController の Deoccluder 設定メソッド追加

ファイル: `Assets/Scripts/Presentation/View/Cameras/CameraController.cs`

以下のメソッドをクラスに追加：

```csharp
/// <summary>
/// CinemachineDeoccluder を特級カメラに追加・設定
/// Convex MeshCollider との競合を避けるため、
/// 地面下に配置した Capsule Collider を対象とする
/// </summary>
private static void SetupCinemachineDeoccluder(CinemachineCamera camera)
{
    if (camera == null)
    {
        return;
    }
    
    var deoccluder = camera.GetComponent<CinemachineDeoccluder>();
    if (deoccluder == null)
    {
        deoccluder = camera.gameObject.AddComponent<CinemachineDeoccluder>();
    }
    
    // 基本設定
    deoccluder.m_AvoidObstacles = true;
    deoccluder.m_DistanceLimit = 0f;
    deoccluder.m_CameraRadius = 0.3f;
    
    // ダンピング（カメラの滑らかさ）
    deoccluder.m_DampingWhenOccluded = 0.5f;
    deoccluder.m_Damping = 1.0f;
    
    // 衝突時の策略
    // PullCameraForward: カメラを目標に引き寄せる（地面突き抜け対策に最適）
    deoccluder.m_Strategy = CinemachineDeoccluder.ResolutionStrategy.PullCameraForward;
    
    // 衝突レイヤーマスク設定（地面コライダーを含むレイヤー）
    int groundLayerMask = LayerMask.GetMask("Ground");
    if (groundLayerMask == 0)
    {
        // フォールバック: "Default" レイヤーも含める
        groundLayerMask = LayerMask.GetMask("Ground", "Default");
    }
    deoccluder.m_CollideAgainst = groundLayerMask;
}
```

### Step 4: 各カメラモード初期化時に Deoccluder を設定

既存のカメラ設定メソッド内に呼び出しを追加：

```csharp
private static void SetFPSCameraParameters(CinemachineCamera cinemachineCamera)
{
    // 既存の設定...
    // var lens = cinemachineCamera.Lens;
    // lens.NearClipPlane = 1.25f;
    // ...
    
    // [NEW] Deoccluder 設定追加
    SetupCinemachineDeoccluder(cinemachineCamera);
}

private static void SetTPSCameraParameters(CinemachineCamera cinemachineCamera)
{
    // 既存の設定...
    
    // [NEW] Deoccluder 設定追加
    SetupCinemachineDeoccluder(cinemachineCamera);
}

private static void SetLongShotCameraParameters(CinemachineCamera cinemachineCamera)
{
    // 既存の設定...
    
    // [NEW] Deoccluder 設定追加
    SetupCinemachineDeoccluder(cinemachineCamera);
}

private static void SetBirdViewCameraParameters(CinemachineCamera cinemachineCamera)
{
    // 既存の設定...
    
    // [NEW] Deoccluder 設定追加
    SetupCinemachineDeoccluder(cinemachineCamera);
}
```

### Step 5: フォグ負方向設定

**目的**: カメラが地面下に潜ってしまった場合の視認性確保

ファイル: `Assets/Prefabs/Environments/Rendering Volume` または `Assets/Resources/...`

Volume Profile の Fog セクション：

```
Fog
├── State: Override (有効化) ✓
├── Fog Distance: 500 (既存値)
└── [NEW] Fog Bounds
    ├── Min: -500 (↓ 負方向フォグ)
    ├── Max: 500 (↑ 正方向フォグ)
    └── Active: ON
```

**または** (より詳細な制御が必要な場合)

Environment Volume の Directional の Fog Bounds の Y 値を負にする：

```yaml
volumeProfile.components[FogComponent]:
  fogBounds:
    min: Vector3(0, -1000, 0)  # Y が負
    max: Vector3(0, 1000, 0)
```

---

## CinemachineDeoccluder 詳細設定

### 設定項目別解説

| 項目 | 型 | 推奨値 | 説明 |
|------|-----|--------|------|
| **m_AvoidObstacles** | bool | `true` | Deoccluder 有効化フラグ |
| **m_DistanceLimit** | float | 0f | 目標距離制限（0=無制限） |
| **m_Strategy** | enum | `PullCameraForward` | 衝突時の解決戦略 |
| **m_CameraRadius** | float | 0.3f | カメラ衝突判定半径（m） |
| **m_DampingWhenOccluded** | float | 0.5f | 衝突時のダンピング（0=なし, 1=最大） |
| **m_Damping** | float | 1.0f | 通常時のダンピング |
| **m_CollideAgainst** | LayerMask | LayerMask.GetMask("Ground") | 衝突対象レイヤー |

### Strategy 戦略の選択肢

```
[OK] PullCameraForward (推奨)
     - カメラを目標に引き寄せる
     - 地面突き抜け対策に最適
     - 自然な動き

[OK] SlowDownWhenApproaching
     - 移動を遅延させる
     - より現実的だが複雑

[OK] IgnoreObstacles
     - 衝突を無視（使用推奨外）

[OK] PreserveCameraDistance
     - カメラ距離を優先
     - Z-Facing の原因の可能性
```

### 検証方法

Editor Play Mode で：

```
1. セッティング後のカメラ確認
2. FPS モードで地面下を潜ろうとしても Raycast が反応
3. カメラが地面下に潜らない
4. ダンピングによる自然な動き
```

---

## フォグ設定

### 目的

地面下に潜った場合のビジュアル対応。Deoccluder が物理的にカメラを押し戻すため、フォグは補助的役割。

### 設定パターン

**パターン1: 内部フォグ（推奨）**
```yaml
Weather Event:
  event: weather
  value: sunny, 0.00, 0.25, 500  # 霧の視界距離
```

**パターン2: Fog Bounds 設定**
```csharp
var fogComponent = volumeProfile.GetComponent<Fog>();
if (fogComponent != null)
{
    fogComponent.m_FogBounds.value = new Vector4(-500, 500, 0, 0);
}
```

---

## 検証チェックリスト

実装完了時の確認項目：

- [ ] DeoccluderCollider GameObject が Ground 下に配置されている
- [ ] Capsule Collider の Direction = Y-Axis
- [ ] Capsule Collider の Radius > 0 (推奨: 0.5 ～ 0.8)
- [ ] Capsule Collider の Height が地面メッシュを十分カバー
- [ ] Capsule Collider の Is Trigger = OFF
- [ ] Rigidbody が存在し Body Type = Kinematic
- [ ] CameraController.SetupCinemachineDeoccluder() が実装済み
- [ ] 全カメラモード (FPS/TPS/LongShot/BirdView) で SetupCinemachineDeoccluder() 呼び出し
- [ ] レイヤーマスク "Ground" が定義されている

### 動作確認

```
Editor Play Mode → 石川県金沢市兼六園ステージ読み込み
→ FPS/TPS モードで地面のくぼみに向かってカメラ移動
→ カメラが地面下に潜らないこと確認
→ ダンピングにより滑らかにカメラが調整される
→ フォグによって視界調整（補助的役割）
```

---

## 参考資料

- Unity 公式: [CinemachineDeoccluder]
- AGENTS.md: [Class Naming Convention](../../AGENTS.md#class-naming-convention)
- このドキュメント関連: [camera-exposure-settings.md](camera-exposure-settings.md)

**次フェーズ**: 実装完了後、この手順を `.md` 化してプロジェクトリファレンスとして保持

