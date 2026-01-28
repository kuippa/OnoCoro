# Resources.Load 統一化プロジェクト

**目的**: すべての `Resources.Load` 呼び出しを専用のマネージャー/ユーティリティに統一化  
**開始日**: 2026-01-28  
**目標完了日**: 2026-02-10  
**ステータス**: 計画中

---

## 📊 概要サマリー

| 分類 | 件数 | 対応 |
|------|-----|------|
| 既に管理済み | 2 | PrefabManager, SpriteResourceLoader |
| 新規ユーティリティで対応（優先：🔴 高） | 2 | TextureResourceLoader, TextAssetLoader |
| 既存マネージャー修正（優先：🟡 中） | 3 | MaterialManager, UIHelper, PlateauUIManager |
| EditorScript（優先：🟢 低） | 4 | PlateauInfo.cs など |
| コメントアウト済み（優先：🟢 低） | 8 | 将来実装時に対応 |

**合計**: 34 個の Resources.Load 呼び出し

---

## 🔴 優先度 HIGH: 新規ユーティリティ作成（影響範囲小）

### Task 1.1: TextureResourceLoader.cs 新規作成

**目的**: Texture2D リソース読み込みの一元化  
**対象ファイル**:
- `Assets/Scripts/Presentation/Input/PlayerInputs.cs` (Line 165)

**現在のコード**:
```csharp
// PlayerInputs.cs Line 165
cursorTexture = Resources.Load<Texture2D>("imgs/icons/iconaddedlocal");
```

**修正内容**:
```csharp
// 新規作成: Assets/Scripts/Core/Utilities/TextureResourceLoader.cs
public static class TextureResourceLoader
{
    // SpriteResourceLoader と同じパターンで実装
    // - パス検証
    // - null チェック
    // - デバッグログ出力
    // - DebugFindAvailableTexturePaths() メソッド
}

// PlayerInputs.cs で使用
cursorTexture = TextureResourceLoader.LoadTexture("imgs/icons/iconaddedlocal");
```

**実装チェックリスト**:
- [ ] TextureResourceLoader.cs 作成
- [ ] LoadTexture() メソッド実装
- [ ] ValidateResourcePath() メソッド実装
- [ ] DebugFindAvailableTexturePaths() メソッド実装
- [ ] PlayerInputs.cs で置き換え
- [ ] ビルド・PlayMode テスト確認

**工数**: 2-3 時間  
**ブロッカー**: なし

---

### Task 1.2: TextAssetLoader.cs 新規作成

**目的**: TextAsset（テキストリソース）読み込みの一元化  
**対象ファイル**:
1. `Assets/Scripts/Core/Utilities/CommonsCalcs.cs` (Line 21)
2. `Assets/Scripts/Core/Utilities/XMLparser.cs` (Line 13)

**現在のコード**:
```csharp
// CommonsCalcs.cs Line 21
TextAsset versiontxt = Resources.Load<TextAsset>("BuildDate");

// XMLparser.cs Line 13
TextAsset xml = Resources.Load<TextAsset>("xml/Building_usage");
```

**修正内容**:
```csharp
// 新規作成: Assets/Scripts/Core/Utilities/TextAssetLoader.cs
public static class TextAssetLoader
{
    // SpriteResourceLoader と同じパターンで実装
    // - パス検証
    // - null チェック
    // - デバッグログ出力
}

// CommonsCalcs.cs で使用
TextAsset versiontxt = TextAssetLoader.LoadTextAsset("BuildDate");

// XMLparser.cs で使用
TextAsset xml = TextAssetLoader.LoadTextAsset("xml/Building_usage");
```

**実装チェックリスト**:
- [ ] TextAssetLoader.cs 作成
- [ ] LoadTextAsset() メソッド実装
- [ ] CommonsCalcs.cs で置き換え
- [ ] XMLparser.cs で置き換え
- [ ] ビルド・PlayMode テスト確認

**工数**: 2-3 時間  
**ブロッカー**: なし

---

## 🟡 優先度 MEDIUM: 既存マネージャー修正（影響範囲中）

### Task 2.1: MaterialManager.cs 修正

**目的**: Material リソース読み込みの一元化・重複排除  
**対象ファイル**:
1. `Assets/Scripts/Core/Managers/MaterialManager.cs` (Line 14, 26)
2. `Assets/Scripts/Data/Plateau/Integration/PlateauBuildingInteractor.cs` (Line 69)

**現在の状況**:

```csharp
// MaterialManager.cs - 既存コード（staticプロパティ）
public static class MaterialManager
{
    private static Material _material_BG_Green;
    public static Material BG_Green
    {
        get
        {
            if (_material_BG_Green == null)
            {
                _material_BG_Green = Resources.Load<Material>("Materials/BG_Green");
            }
            return _material_BG_Green;
        }
    }

    private static Material _material_BG_RED;
    public static Material BG_RED
    {
        get
        {
            if (_material_BG_RED == null)
            {
                _material_BG_RED = Resources.Load<Material>("Materials/BG_RED");
            }
            return _material_BG_RED;
        }
    }
}

// PlateauBuildingInteractor.cs - 外部での直接ロード
Material source = Resources.Load("Materials/PlateauGenericWood") as Material;
```

**修正内容**:

```csharp
// Step 1: MaterialManager.cs に PlateauGenericWood プロパティを追加
public static Material PlateauGenericWood
{
    get
    {
        if (_material_PlateauGenericWood == null)
        {
            _material_PlateauGenericWood = Resources.Load<Material>("Materials/PlateauGenericWood");
        }
        return _material_PlateauGenericWood;
    }
}

// Step 2: PlateauBuildingInteractor.cs で使用
Material source = MaterialManager.PlateauGenericWood;
```

**実装チェックリスト**:
- [ ] MaterialManager.cs に PlateauGenericWood プロパティ追加
- [ ] bk_PlateauInfo.cs (Line 258) の Resources.Load を確認・修正対象化
- [ ] PlateauInfo.cs (Line 258) の Resources.Load を確認・修正対象化
- [ ] PlateauBuildingInteractor.cs (Line 69) で MaterialManager 使用に変更
- [ ] ビルド・PlayMode テスト確認

**関連ファイル**:
- MaterialManager.cs (修正)
- PlateauBuildingInteractor.cs (修正)
- bk_PlateauInfo.cs (EditorScript - 後回し)
- PlateauInfo.cs (EditorScript - 後回し)

**工数**: 3-4 時間  
**ブロッカー**: MaterialManager の構造確認が必要

---

### Task 2.2: UIHelper.cs 修正

**目的**: UI GameObject ロードを PrefabManager に統一  
**対象ファイル**:
- `Assets/Scripts/Core/Helpers/UIHelper.cs` (Line 209)

**現在のコード**:
```csharp
// UIHelper.cs Line 209
GameObject prefab = Resources.Load<GameObject>(prefabPath);
```

**修正内容**:
```csharp
// UIHelper.cs で PrefabManager 使用に変更
GameObject prefab = PrefabManager.Instance.GetPrefab(prefabPath);
// または static メソッド形式によって調整
```

**実装チェックリスト**:
- [ ] UIHelper.cs 内の Resources.Load<GameObject> の呼び出し内容確認
- [ ] prefabPath パターンを確認（"Prefabs/..." か絶対パスか）
- [ ] PrefabManager との パス形式 統一
- [ ] UIHelper.cs で PrefabManager を使用するように修正
- [ ] ビルド・PlayMode テスト確認

**関連ファイル**:
- UIHelper.cs (修正)
- PrefabManager.cs (確認・必要に応じて拡張)

**工数**: 2-3 時間  
**ブロッカー**: UIHelper.cs と PrefabManager のパス形式統一確認が必要

---

### Task 2.3: PlateauUIManager.cs 修正

**目的**: UI GameObject ロードを PrefabManager に統一  
**対象ファイル**:
- `Assets/Scripts/Data/Plateau/Utilities/PlateauUIManager.cs` (Line 94)

**現在のコード**:
```csharp
// PlateauUIManager.cs Line 94
infoBox = Instantiate(Resources.Load("Prefabs/UI/UIBuildingInfo") as GameObject);
```

**修正内容**:
```csharp
// PlateauUIManager.cs で PrefabManager 使用に変更
GameObject prefab = PrefabManager.Instance.GetPrefab("Prefabs/UI/UIBuildingInfo");
infoBox = Instantiate(prefab);
// または
infoBox = PrefabManager.Instance.InstantiatePrefab("Prefabs/UI/UIBuildingInfo");
```

**実装チェックリスト**:
- [ ] PlateauUIManager.cs の Resources.Load 箇所特定
- [ ] PrefabManager のメソッドシグニチャ確認（Instantiate 対応の有無）
- [ ] PlateauUIManager.cs で PrefabManager を使用するように修正
- [ ] ビルド・PlayMode テスト確認

**関連ファイル**:
- PlateauUIManager.cs (修正)
- PrefabManager.cs (確認・必要に応じて拡張)

**工数**: 2-3 時間  
**ブロッカー**: PrefabManager が Instantiate 対応しているか確認が必要

---

## 🟢 優先度 LOW: EditorScript & コメントアウト済み

### Task 3.1: PlateauInfo.cs / bk_PlateauInfo.cs 修正

**対象ファイル**:
- `Assets/Scripts/.Editor/bk_PlateauInfo.cs` (Line 72, 258, 334)
- `Assets/Scripts/.Editor/PlateauInfo.cs` (Line 72, 258, 334)

**現在のコード**:
```csharp
// Line 72
GameObject UICircularIndicator = Instantiate(Resources.Load("Prefabs/UI/UICircularIndicator")) as GameObject;

// Line 258
Material doom_material = Resources.Load("Materials/PlateauGenericWood") as Material;

// Line 334
infoBox = Instantiate(Resources.Load("Prefabs/UI/UIBuildingInfo") as GameObject);
```

**修正内容**:
- EditorScript なので後回し対応でOK
- Task 2.1 (MaterialManager) と Task 2.3 (PlateauUIManager) の実装後に対応

**実装チェックリスト**:
- [ ] 優先度高・中の Task 完了後に対応

**工数**: 1-2 時間  
**ブロッカー**: Task 2.1, 2.3 の完了待ち

---

### Task 3.2: コメントアウト済み Resources.Load の対応

**対象ファイル** (将来の機能実装時に対応):
1. `FireCubeCtrl.cs` (Line 96) - コメント
2. `SpawnController.cs` (Line 178) - コメント
3. `Flame.cs` (Line 34) - コメント
4. `tmp_TowerSweeper.cs` (Line 25) - コメント
5. `UnitSpawn.cs` (Line 24, 73, 111, 122, 141) - 一部コメント
6. `UnitVFXPrefab.cs` (Line 22)

**対応方針**: 
- 機能の実装/有効化時に PrefabManager へ統一

**工数**: 将来対応

---

## 📅 実装スケジュール

```
Week 1 (2026-01-28 ~ 2026-02-03)
├─ Task 1.1: TextureResourceLoader.cs 作成 (2-3h)
├─ Task 1.2: TextAssetLoader.cs 作成 (2-3h)
└─ Test & Integration (1h)

Week 2 (2026-02-04 ~ 2026-02-10)
├─ Task 2.1: MaterialManager.cs 修正 (3-4h)
├─ Task 2.2: UIHelper.cs 修正 (2-3h)
├─ Task 2.3: PlateauUIManager.cs 修正 (2-3h)
└─ Task 3.1: PlateauInfo.cs 修正 (1-2h)

Week 3+
└─ Task 3.2: コメントアウト済み対応 (将来実装時)
```

---

## 🔍 実装前チェックリスト

**全タスク共通**:
- [ ] AGENTS.md のアクセス修飾子ポリシー確認（internal vs public）
- [ ] coding-standards.md のコーディング標準確認
- [ ] prefab-asset-management.instructions.md で PrefabManager パターン確認

**新規ユーティリティ (Task 1.x)**:
- [ ] SpriteResourceLoader.cs のパターンを参照実装
- [ ] Namespace = `CommonsUtility`
- [ ] アクセス修飾子 = `internal static class`

**既存マネージャー修正 (Task 2.x)**:
- [ ] 既存マネージャーの構造確認
- [ ] 依存関係チェック（アップストリーム）
- [ ] PlayMode テスト項目確認

**EditorScript (Task 3.x)**:
- [ ] EditorScript 判別（.Editor フォルダ配下か確認）
- [ ] 後回し対応は明示的にコメント記載

---

## 📝 進捗トラッキング

| Task | Status | 担当者 | 実施日 | 完了日 |
|------|--------|--------|--------|--------|
| Task 1.1 | [ ] Not Started | - | - | - |
| Task 1.2 | [ ] Not Started | - | - | - |
| Task 2.1 | [ ] Not Started | - | - | - |
| Task 2.2 | [ ] Not Started | - | - | - |
| Task 2.3 | [ ] Not Started | - | - | - |
| Task 3.1 | [ ] Not Started | - | - | - |
| Task 3.2 | [ ] Future | - | - | - |

---

## 📚 参考資料

- [AGENTS.md - アクセス修飾子ポリシー](../AGENTS.md#access-modifier-policy)
- [AGENTS.md - クラス命名規則](../AGENTS.md#クラス命名規則)
- [docs/coding-standards.md](coding-standards.md)
- [docs/prefab-asset-management.instructions.md](prefab-asset-management.instructions.md)
- [SpriteResourceLoader.cs - 参照実装](../Assets/Scripts/Core/Utilities/SpriteResourceLoader.cs)
- [PrefabManager.cs - 参照実装](../Assets/Scripts/Core/Managers/PrefabManager.cs)

---

**Last Updated**: 2026-01-28
