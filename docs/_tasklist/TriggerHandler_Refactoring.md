# TriggerHandler Refactoring Progress

**Created**: 2026-02-10  
**Status**: ACTIVE - TriggerHandler hierarchy implementation  
**Session**: Message 45 (Burning.cs Dictionary fix completed)

---

## 概要

OnTriggerEnter/Exit の複数発火バグ（複数ゴミキューブ衝突時に ArgumentException 発生）を根本解決するため、TriggerHandler ベースクラスを中心とした大規模リファクタリング工程。

### Root Cause Resolution

**問題**: Player が複数 Collider を持つ場合、各 Collider ペアごとに OnTriggerEnter が複数回発火
```
FireCube1.Collider ↔ Player.Collider1 → OnTriggerEnter ✅
FireCube1.Collider ↔ Player.Collider2 → OnTriggerEnter ✅  (duplicate! 冪等性破壊)
```

**解決**: HashSet<Collider> による deduplication
```csharp
private HashSet<Collider> _objectsInTrigger = new HashSet<Collider>();
// First time both Player Colliders collide → OnTargetEnter() executes once
```

---

## 完了タスク ✅ (4 個)

### 1. SimpleSwitchBox.cs [COMPLETE ✅]
**ファイル**: `Assets/Scripts/Game/Units/Structures/SimpleSwitchBox.cs`  
**変更**: TriggerHandler 継承実装  
**処理**: `OnTargetEnter()` → `ToggleSwitchBox()`  
**検証**: 圧力版トリガー動作確認済み

### 2. Burning.cs [COMPLETE ✅] + BurningTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/Burning.cs`  
**ハンドラー**: `Assets/Scripts/Core/Handlers/BurningTriggerHandler.cs`  
**変更**: 
- OnTriggerEnter/Exit 削除
- 4 つの internal メソッド実装:
  - `OnGarbageEnter(Collider)`
  - `OnGarbageExit(Collider)` 
  - `OnFireCubeEnter(Collider)`
  - `OnWaterEnter(Collider)`
  - `OnBuildingEnter(Collider)`

**最新修正 (Message 45)**:
- `OnGarbageEnter()` で `_dict_burn_garbage.ContainsKey()` チェック追加 → 重複 Add 防止
- `OnGarbageExit()` で `_dict_burn_garbage.Remove()` 実装 → Dictionary から削除
- **Fix**: ArgumentException "An item with the same key has already been added" 解決 ✅

**MultiTagTriggerHandler と連携**:
- BurningTriggerHandler は MultiTagTriggerHandler 継承
- GameEnum.TagType による型安全な tag 検出
- `OnTargetEnter(Collider, GameEnum.TagType)` override で tag ごとの処理分岐

### 3. Earthquake.cs [COMPLETE ✅] + EarthquakeTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/Earthquake.cs`  
**ハンドラー**: `Assets/Scripts/Core/Handlers/EarthquakeTriggerHandler.cs`  
**変更**:
- OnTriggerEnter/Exit 削除（EventSystem は Collider 不要のため dead code）
- EarthquakeTriggerHandler で trigger 検出 → `Earthquake.EventEarthQuake()` 呼び出し
- `GetOrAddComponent<Earthquake>()` パターンで auto-initialize

### 4. Raining.cs [COMPLETE ✅] + RainTriggerHandler.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/Raining.cs`  
**ハンドラー**: `Assets/Scripts/Core/Handlers/RainTriggerHandler.cs`  
**変更**:
- Raining: TriggerHandler 継承→削除、純粋 MonoBehaviour に戻す
- Update() loop のみ（RainDrop 生成責務）
- RainTriggerHandler で trigger 検出 → `WeatherController.ChangeWeather()` 呼び出し
- Weather toggle zone や SimpleSwichBox に RainTriggerHandler attach

---

## 未完了タスク 🔄 (8 個)

### 優先度: 🔴 HIGH (2 個)

#### [ ] NarakuController.cs (Narak 管理)
**ファイル**: `Assets/Scripts/Game/Events/Environmental/NarakuController.cs`  
**複雑度**: ⭐⭐⭐ (最高)  
**理由**: 
- 複数 tag 監視（可能性: Garbage, FireCube, Water, Building等）
- 複数の責務（煉獄レベル管理、ゴミ燃焼、水処理 etc）
- MultiTagTriggerHandler 対応必須

**実装方針**:
1. `NarakuTriggerHandler` を MultiTagTriggerHandler 継承で作成
2. NarakuController から OnTriggerEnter/Exit 削除
3. tag ごとに internal メソッド分割
4. GetOrAddComponent<NarakuController>() パターン

**現在の状態**: 未着手（Message 35 で分析完了）

#### [ ] TowerSweeper.cs (掃除機タワー)
**ファイル**: `Assets/Scripts/Game/Units/Towers/TowerSweeper.cs`  
**複雑度**: ⭐⭐ (中)  
**理由**: 複数タグ（Garbage, Dust 等）を監視？
**現在の状態**: TowerSweeper 改修チェックボックスは [--] スキップマーク（理由確認待ち）

---

### 優先度: 🟡 MEDIUM (3 個)

#### [ ] TowerDustBoxCtrl.cs
**ファイル**: `Assets/Scripts/Game/Units/Towers/TowerDustBoxCtrl.cs`  
**複雑度**: ⭐ (低)  
**監視対象**: Dust (single tag)  
**実装方針**: 標準 TriggerHandler 継承

#### [ ] SentryGuardCtrl.cs
**ファイル**: `Assets/Scripts/Game/Units/Towers/SentryGuardCtrl.cs`  
**複雑度**: ⭐ (低)  
**監視対象**: Enemy (single tag)  
**実装方針**: 標準 TriggerHandler 継承

#### [ ] PuddleController.cs
**ファイル**: `Assets/Scripts/Game/Events/Environmental/PuddleController.cs`  
**複雑度**: ⭐ (低)  
**監視対象**: Player? (single tag)  
**実装方針**: 標準 TriggerHandler 継承

---

### 優先度: 🟢 LOW (3 個)

#### [ ] PowerCubeCtrl.cs, DustBoxCtrl.cs, RainAbsorbController.cs
**複雑度**: ⭐ (単純)  
**実装方針**: 各自 TriggerHandler 継承、internal メソッド分割

---

## インフラストラクチャ（完了）

### Core/Handlers/ フォルダ構成

✅ **TriggerHandler.cs** (base class)
```csharp
internal abstract class TriggerHandler : MonoBehaviour
{
    // HashSet<Collider> deduplication
    // SetTargetTag(), SetDelayedExitTime() configuration
    // OnTargetEnter/Exit() abstract methods
}
```

✅ **MultiTagTriggerHandler.cs** (multi-tag extension)
```csharp
internal abstract class MultiTagTriggerHandler : TriggerHandler
{
    // SetTargetTags(params GameEnum.TagType[])
    // TryParse string → GameEnum.TagType conversion
    // OnTargetEnter(Collider, GameEnum.TagType) abstract override
    // OnTargetExit(Collider, GameEnum.TagType) abstract override
}
```

✅ **RainTriggerHandler.cs** (weather trigger)
✅ **EarthquakeTriggerHandler.cs** (earthquake trigger)
✅ **BurningTriggerHandler.cs** (multi-tag burning)

---

## 最新ステータス (Message 45)

### 問題修正 ✅
**症状**: `ArgumentException: An item with the same key has already been added. Key: GarbageCube210`

**原因**: 
```
Burning.OnGarbageExit() で _list_near_garbage だけ削除
→ _dict_burn_garbage には key が残存
→ 2 回目の衝突で Add() → duplicate key error
```

**修正内容**:
1. **Burning.OnGarbageEnter()** 
   - `_dict_burn_garbage.Add()` 前に `ContainsKey()` チェック
   
2. **Burning.OnGarbageExit()**
   - `_list_near_garbage` 削除のみでなく
   - `_dict_burn_garbage.Remove()` も実装

**検証**: 複数 FireCube 所有時も GarbageCube 衝突が安全に処理される

---

## 次のステップ (推奨順序)

### Session 46 以降の作業計画

1. **NarakuController 調査** (優先度: 最高)
   - ソース分析して tag 種別確認
   - MultiTagTriggerHandler 対応確認
   
2. **TowerSweeper 状態確認**
   - [--] スキップマークの理由確認
   
3. **TowerDustBoxCtrl / SentryGuardCtrl**
   - 単純な TriggerHandler 継承実装

4. **Remaining classes**
   - デバッグ・テスト検証

---

## Git Status (Before Commit)

**Changed Files**:
- `Assets/Scripts/Game/Events/Environmental/Burning.cs` (Message 45 修正)
- `Assets/Scripts/Core/Handlers/BurningTriggerHandler.cs`
- `Assets/Scripts/Core/Handlers/MultiTagTriggerHandler.cs`
- `Assets/Scripts/Core/Handlers/TriggerHandler.cs`
- `Assets/Scripts/Core/Handlers/RainTriggerHandler.cs`
- `Assets/Scripts/Core/Handlers/EarthquakeTriggerHandler.cs`
- `Assets/Scripts/Game/Units/Structures/SimpleSwitchBox.cs`
- `Assets/Scripts/Game/Events/Environmental/Earthquake.cs`
- `Assets/Scripts/Game/Events/Environmental/Raining.cs`

**Commit Message** (推奨):
```
feat: Implement TriggerHandler hierarchy for safe collision detection

- Add TriggerHandler base class with HashSet deduplication
- Add MultiTagTriggerHandler for multi-tag scenarios
- Create RainTriggerHandler, EarthquakeTriggerHandler
- Create BurningTriggerHandler for multi-tag burning logic
- Refactor SimpleSwitchBox, Burning, Earthquake, Raining
- Fix Burning.OnGarbageExit() Dictionary cleanup bug
- Prevents ArgumentException on repeated collision cycles
```

---

**Last Updated**: 2026-02-10 (Message 45)  
**Assigned to**: Next Session (46+)  
**Review**: NarakuController complexity assessment recommended
