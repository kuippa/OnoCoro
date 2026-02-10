# TriggerHandler Refactoring [COMPLETE]

**Created**: 2026-02-10  
**Completed**: 2026-02-11  
**Status**: [COMPLETE] - All 12 TriggerHandler implementations finished

---

## 概要

OnTriggerEnter/Exit の複数発火バグ（複数ゴミキューブ衝突時に ArgumentException 発生）を根本解決するため、TriggerHandler ベースクラスを中心とした大規模リファクタリング工程。

### Root Cause Resolution

**問題**: Player が複数 Collider を持つ場合、各 Collider ペアごとに OnTriggerEnter が複数回発火
```
FireCube1.Collider ↔ Player.Collider1 → OnTriggerEnter [OK]
FireCube1.Collider ↔ Player.Collider2 → OnTriggerEnter [OK] (duplicate! 冪等性破壊)
```

**解決**: HashSet<Collider> による deduplication
```csharp
private HashSet<Collider> _objectsInTrigger = new HashSet<Collider>();
// First time both Player Colliders collide → OnTargetEnter() executes once
```

---

## 完了タスク [COMPLETE] (12/12 - 100%)

### 1. SimpleSwitchBox.cs [OK]
**ファイル**: `Assets/Scripts/Game/Units/Structures/SimpleSwitchBox.cs`  
**実装パターン**: TriggerHandler 継承  
**処理**: `OnTargetEnter()` → `ToggleSwitchBox()`  
**検証**: 圧力版トリガー動作確認済み

### 2. Burning.cs [OK] + BurningTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/Burning.cs`  
**ハンドラー**: `Assets/Scripts/Core/Handlers/BurningTriggerHandler.cs`  
**実装パターン**: MultiTagTriggerHandler 継承  
**修正**: ArgumentException (duplicate key) 根本解決

### 3. Earthquake.cs [OK] + EarthquakeTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/Earthquake.cs`  
**パターン**: Single tag (Player)

### 4. Raining.cs [OK] + RainTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/Raining.cs`  
**パターン**: Single tag (Player)  
**追加修正**:
- DemController 依存性調整（Scale 補正）
- UnitEnemy シーンで Player fallback 実装
- 兼六園・UnitEnemy 両シーン対応

### 5. TowerSweeper.cs [OK] + TowerSweeperTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Units/Towers/TowerSweeper.cs`  
**パターン**: MultiTagTriggerHandler (Garbage, Ash)

### 6. TowerDustBoxCtrl.cs [OK] + TowerDustBoxTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Units/Towers/TowerDustBoxCtrl.cs`  
**パターン**: Per-collider tracking

### 7. SentryGuardCtrl.cs [OK] + SentryGuardTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Units/Towers/SentryGuardCtrl.cs`  
**パターン**: Single tag (EnemyLitter)

### 8. PuddleController.cs [OK] + PuddleTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Systems/Weather/PuddleController.cs`  
**パターン**: MultiTagTriggerHandler (RainDrop, Puddle)

### 9. PowerCubeCtrl.cs [OK] + PowerCubeTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Units/Shared/PowerCubeCtrl.cs`  
**パターン**: Single tag (Player)

### 10. DustBoxCtrl.cs [OK] + DustBoxTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Units/Towers/DustBoxCtrl.cs`  
**パターン**: Single tag (Garbage) with Coroutine deletion

### 11. RainAbsorbController.cs [OK] + RainAbsorbTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/RainAbsorbController.cs`  
**パターン**: MultiTagTriggerHandler (RainDrop, Untagged)

### 12. NarakuController.cs [OK] + NarakuTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/GameManager/NarakuController.cs`  
**ハンドラー**: `Assets/Scripts/Core/Handlers/NarakuTriggerHandler.cs`  
**複雑度**: 高 - 複数パターン対応の独自実装  
**パターン**:
- OnPlayerEnter() → InputController 操作
- OnDestructibleEnter() → FireCube/Ash 削除
- OnRainDropEnter() → WaterSurfaceCtrl 委譲
- OnWaterEnter() → 削除
- OnGenericObjectEnter() → Rigidbody リセット + 位置調整
- NarakuController は InitWindow（ウィンドウ初期化）のみに集約

---

## インフラストラクチャ実装 [COMPLETE]

### Core/Handlers/ フォルダ構成

[OK] **TriggerHandler.cs** (base class)
```csharp
internal abstract class TriggerHandler : MonoBehaviour
{
    // HashSet<Collider> deduplication
    // SetTargetTag(), SetDelayedExitTime() configuration
    // OnTargetEnter/Exit() abstract methods
}
```

[OK] **MultiTagTriggerHandler.cs** (multi-tag extension)
```csharp
internal abstract class MultiTagTriggerHandler : TriggerHandler
{
    // SetTargetTags(params GameEnum.TagType[])
    // TryParse string → GameEnum.TagType conversion
    // OnTargetEnter(Collider, GameEnum.TagType) abstract override
    // OnTargetExit(Collider, GameEnum.TagType) abstract override
}
```

[OK] **RainTriggerHandler.cs** (weather trigger)  
[OK] **EarthquakeTriggerHandler.cs** (earthquake trigger)  
[OK] **BurningTriggerHandler.cs** (multi-tag burning)  

---

## 主要な修正と最適化

### Issue 1: Rain Particle Generation Bug (Session 46 - Message 9)

**問題**: 兼六園では rain drop が見えるが, UnitEnemy シーンでは見えない

**原因**: DemController.GetDemRndAbovePosition() が UnitEnemy で Vector3.zero を返す（DEM terrain データ不在）

**解決**:
- Raining.RainDrops() に DemController 戻り値チェック追加
- fallback: Player position + ABOVE_POSITION（DEM 不在時）
- 両シーン互換性確認済み

### Issue 2: DemController Scale Not Applied (Session 46 - Message 10-11)

**問題**: Rain particles が narrow range に集中（Terrain Scale X=100, Z=100 未反映）

**原因**: GetDemRndAbovePosition() が Y-axis のみ scale 適用、X,Z は未適用

**解決**:
```csharp
float scaledMeshSizeX = meshSize.x * scale.x;
float scaledMeshSizeZ = meshSize.z * scale.z;
// Random.Range() に scaledSize を適用
```

### Issue 3: NarakuTriggerHandler Compilation (Session 46 - Message 12-14)

**エラー**: 
- CS0115: No suitable override (abstract method signature mismatch)
- CS0103: _collidersInTrigger フィールド欠落
- CS1513: Missing closing brace, indentation errors

**解決**:
- Abstract method override を OnTriggerEnter/Exit に修正（Collider parameter 対応）
- HashSet declaration 追加
- Multi-method indentation correction

---

## 検証状況

[OK] 兼六園 scene: DEM を使用した rain drop 配置
[OK] UnitEnemy scene: Player fallback を使用した rain drop 配置
[OK] Multiple collider deduplication across all handlers
[OK] NarakuTriggerHandler multi-tag detection
[OK] All 12 handler implementations compilation success

---

## Changed Files Summary

**12 Controllers/Systems Modified**:
- SimpleSwitchBox, Burning, Earthquake, Raining
- TowerSweeper, TowerDustBoxCtrl, SentryGuardCtrl
- PuddleController, PowerCubeCtrl, DustBoxCtrl
- RainAbsorbController, NarakuController

**12 New Handler Classes Created**:
- Core/Handlers/ フォルダに全て配置
- TriggerHandler 継承ベース
- MultiTagTriggerHandler 対応クラス

**Core Infrastructure**:
- TriggerHandler (abstract base)
- MultiTagTriggerHandler (multi-tag support)
- DemController (scale correction applied)

---

## Archive Notes (Session 46)

**Session Duration**: 2026-02-10 to 2026-02-11  
**Total Tasks**: 12/12 Complete  
**Key Achievements**:
- [OK] Eliminated multiple OnTriggerEnter calls (deduplication working)
- [OK] 117 lines removed from NarakuController (single responsibility restored)
- [OK] 12 dedicated handler classes implemented
- [OK] Multi-scene rain generation working (DEM + fallback)
- [OK] Type-safe enum-based tag detection across all systems

**Next Phase**: Testing and validation for new feature development

---

**Last Updated**: 2026-02-11  
**Status**: [COMPLETE] - All TriggerHandler refactoring finished
