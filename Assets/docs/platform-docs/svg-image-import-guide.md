# SVG 画像インポートガイド

## 概要

Unity 6.3 で SVG ファイルを UI に表示するには、特殊なインポート設定が必要です。本ガイドは、公式ドキュメントに不備があるため、実際に機能する手順をまとめています。

**対象**: `Assets/Resources/imgs/icons/` 内のすべての `.svg` ファイル（全36ファイル）

---

## 必要な環境

- Unity 6.3以上
- Unity.VectorGraphics 3.0.0-preview.2 · July 31, 2025 パッケージ（インストール済み）
- UI Image コンポーネント（Canvas/Panel 配下）

---

## SVG 画像のインポート手順

### 手順 1: SVG ファイルの Asset Type を変更

1. **Unity エディタで SVG ファイルを選択**
   - 例: `Assets/Resources/imgs/icons/spaghetti-monster-flying-solid.svg`

2. **Inspector パネルで Asset Type を確認**
   ```
   Asset Type: [Default] → [UI SVGImage] に変更
   ```

3. **Apply ボタンをクリック**
   - Unity が自動的にSVG をスプライトアセットに変換

4. **サブアセット確認**
   - SVG ファイル左側の三角形 ▶ をクリック
   - `spaghetti-monster-flying-solidSprite` が表示される
   - これが Sprite メッシュアセット

**注意**: 
- `UI SVGImage` 選択後、サブアセットが自動生成されるまで数秒待つ
- Assets フォルダの refresh が必要な場合は F5 を押す

---

### 手順 2: UI の Image コンポーネントで Sprite を設定

1. **Image コンポーネント (Source Image) を設定**
   - Inspector で Image コンポーネントを展開
   - `Source Image` フィールドに `spaghetti-monster-flying-solidSprite` をドラッグ
   - または Picker ボタン ⊙ をクリックしてアセットを選択

2. **Image Type を確認**
   ```
   Image Type: Simple
   ```

3. **「スプライトメッシュを使用」チェックボックスをオン**
   - Image コンポーネント内の以下オプションを確認
   ```
   ☑ Use Sprite Mesh
   ```
   - このチェックが **必須** です
   - チェックしないと SVG が正常に表示されません

**重要な注意**:
- **ソース画像がない場合、「Use Sprite Mesh」オプションは非表示になります**
- 例：ItemHolder の Item_icon は動的に画像を設定するため、最初にソース画像がありません
- この場合、コード側から Image コンポーネントのプロパティを設定する必要があります（手順3参照）
- Editor から手動で最初のダミー画像を設定しておくか、C# コード側で初期化してください

## 手順 3: コード側での「Use Sprite Mesh」設定

動的に画像を設定する場合（ItemHolder など）、コード側から Image コンポーネントのプロパティを初期化します：

### Reflection を使用した設定

```csharp
// Image コンポーネントの取得
Image image = GetComponent<Image>();

// SVG スプライトを読み込む
Sprite sprite = SpriteResourceLoader.LoadSprite(imagePath);

if (sprite != null)
{
    image.sprite = sprite;
    
    // Reflection を使用して「Use Sprite Mesh」プロパティをONに設定
    EnableSpriteMeshForImage(image);
}

/// <summary>
/// Image コンポーネントで「Use Sprite Mesh」を有効にする
/// SVG スプライトの正常な表示に必須
/// </summary>
private void EnableSpriteMeshForImage(Image image)
{
    if (image == null) return;
    
    try
    {
        // Unity 6.3 VectorGraphics: useSpriteMesh プロパティ
        var property = image.GetType().GetProperty("useSpriteMesh");
        if (property != null && property.CanWrite)
        {
            property.SetValue(image, true);
            Debug.Log("Use Sprite Mesh enabled for Image");
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Failed to enable sprite mesh: {ex.Message}");
    }
}
```

### SpriteResourceLoader の自動設定（推奨）

`SpriteResourceLoader.SetSpriteToImage()` を使用すれば、自動的に「Use Sprite Mesh」が有効化されます：

```csharp
// SpriteResourceLoader が自動的に useSpriteMesh = true を設定
Image image = GetComponent<Image>();
SpriteResourceLoader.SetSpriteToImage(image, imagePath);
```

**プロパティ詳細**:
- **プロパティ名**: `useSpriteMesh` 
- **型**: `bool`
- **デフォルト値**: `false`
- **用途**: SVG スプライトメッシュレンダリングの有効化

---

## C# コード側での読み込み

SVG スプライトを `Resources.Load<Sprite>()` で読み込む場合：

```csharp
// SVG スプライトを読み込む（サブアセット名で指定）
Sprite sprite = Resources.Load<Sprite>("imgs/icons/spaghetti-monster-flying-solidSprite");

if (sprite == null)
{
    Debug.LogError("Failed to load SVG sprite: spaghetti-monster-flying-solidSprite");
    return;
}

// Image コンポーネントに設定
Image image = GetComponent<Image>();
image.sprite = sprite;
```

**重要なポイント**:
- パスには `Sprite` サフィックスが必須
- 元のファイル名: `imgs/icons/spaghetti-monster-flying-solid`
- スプライトアセット名: `imgs/icons/spaghetti-monster-flying-solidSprite`

---

## トラブルシューティング

### 現象: スプライトが表示されない

**チェック項目**:
1. ☑ `Use Sprite Mesh` チェックボックスがオンか？
2. ☑ `Source Image` に正しいスプライトが設定されているか？
3. ☑ Image コンポーネントの Color が透明（alpha = 0）になっていないか？

### 現象: サブアセット（Sprite）が生成されない

**対処**:
1. SVG ファイルを右クリック → **Re-import**
2. Asset Type を再度確認（`UI SVGImage` に設定）
3. Unity エディタを再起動
4. Assets フォルダ → **Reimport All**

### 現象: 画像が歪む、ぼやける

**対処**:
- SVG Pixels Per Unit を調整
  ```
  SVG Settings → SVG Pixels Per Unit: 100 → 200
  ```
  値を増やすと解像度が上がります

---

## 一括処理の計画

### 対象ファイル（36ファイル）

```
biohazard-solid.svg
bomb-solid.svg
building-solid.svg
car-on-solid.svg
eye-solid.svg
fire-extinguisher-solid.svg
fire-flame-curved-solid.svg
fire-solid.svg
gear-solid.svg
hammer-solid.svg
hockey-puck-solid.svg
house-chimney-crack-solid.svg
house-crack-solid.svg
house-fire-solid.svg
house-flood-water-solid.svg
magnifying-glass-minus-solid.svg
magnifying-glass-plus-solid.svg
meteor-solid.svg
plug-circle-bolt-solid.svg
plug-circle-plus-solid.svg
poo-solid.svg
poop-solid.svg
skull-solid.svg
snowplow-solid.svg
spaghetti-monster-flying-solid.svg
spider-solid.svg
spinner-solid.svg
splotch-solid.svg
steam.svg
virus-covid-solid 1.svg
virus-covid-solid.svg
volume-high-solid.svg
volume-low-solid.svg
volume-xmark-solid.svg
wrench-solid.svg
xmark-solid.svg
```

### 実装手順

**Phase 1: Unity エディタで .meta ファイル修正**
- すべての `.svg.meta` ファイルを `UI SVGImage` に設定
- Unity が自動的にスプライトアセットを生成

**Phase 2: C# コード修正**
- `Resources.Load<Sprite>()` の呼び出し箇所を確認
- パスに `Sprite` サフィックスを追加
- null チェックを強化

**Phase 3: UI プレハブ修正**
- `Item_holder` など UI プレハブを開く
- Image コンポーネントの `Use Sprite Mesh` にチェック
- スプライトアセットを手動で割り当て

---

## 参考資料

- Unity VectorGraphics パッケージ: `com.unity.vectorgraphics`
- SVG スプライトメッシュ: Unity 6.3 + VectorGraphics 統合機能

