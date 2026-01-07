# ILSpyファイルと現在プロジェクトファイルの比較結果

## 実行日時
2026年1月8日

## 分析対象
- ILSpyフォルダ: `Assets/Recovery/.Editor/ILspy_CS/`
- プロジェクトフォルダ: `Assets/Scripts/`
- 分析したファイル数: 101ファイル（.csファイル、除外ファイルを除く）

## 結果サマリー

### ファイル分類

1. **機能的に変更なし（nochange_pass に移動可能）**: **0ファイル**
   - 該当なし

2. **小さな変更のみ（空メソッドやコメントのみの違い）**: **1ファイル**
   - EnvironmentCameraCtrl.cs
     - Awakeメソッドが空で、コメントアウトされたコードのみの違い
     - 機能的には同等と見なせる可能性があるが、コメントアウトされたコードには開発意図が含まれている可能性がある

3. **機能的な変更あり**: **70ファイル**
   - すべてのファイルで以下のような違いが確認された：
     - ItemStruct初期化のパラメータ値の違い（例: "BIT" → `GlobalConst.SHORT_SCORE1_SCALE`）
     - 変数名の違い（ILSpyによる逆コンパイルで自動生成された名前 vs 元の変数名）
     - ロジックの改善・追加
     - コメントやデバッグコードの追加
     - using文の違い
     - フォーマットの違い

4. **対応するファイルが見つからない**: **30ファイル**
   - 以下のファイルは現在のプロジェクトに存在しない：
     - BloomPathCtrl.cs
     - ClosebtnCtrl.cs
     - CommonsUtility.GlobalConst.cs
     - CommonsUtility.Utility.cs
     - CoroutineRunner.cs
     - DustBox.cs
     - DustBoxCtrl.cs
     - EnvironmentLight.cs
     - GameSpeedCtrl.cs
     - IItemStructProvider.cs
     - IUnitStructProvider.cs
     - MarkerIndicatorCtrl.cs
     - MaterialManager.cs
     - NavMeshCtrl.cs
     - PathMakerCtrl.cs
     - PlateauUtility.cs
     - RainAbsorbCtrl.cs
     - SentryGuard.cs
     - SentryGuardCtrl.cs
     - SignPowerOutageCtrl.cs
     - TowerDustBox.cs
     - TowerSentryGuard.cs
     - UnitEnemy.cs
     - UnitFireDisaster.cs
     - UnitPathBloom.cs
     - WaterSurfaceCtrl.cs
     - WaterTurret.cs
     - WaterTurretCtrl.cs
     - WeatherCtrl.cs
     - WindCtrl.cs

## 詳細な違いの例

### 典型的な違いのパターン

1. **変数名の違い**
   ```csharp
   // ILSpy (自動生成)
   GameObject obj = ...;
   int num = 0;
   foreach (GameObject item2 in list) { ... }
   
   // 現在のプロジェクト (元のコード)
   GameObject _plateauInfo = ...;
   int i = 0;
   foreach (var obj in list) { ... }
   ```

2. **定数値の変更**
   ```csharp
   // ILSpy
   new ItemStruct("Loupe", "LoupeID", ..., "BIT", 0f, ...)
   
   // 現在のプロジェクト
   new ItemStruct("Loupe", "LoupeID", ..., GlobalConst.SHORT_SCORE1_SCALE, 0.0f, ...)
   ```

3. **ロジックの改善**
   ```csharp
   // ILSpy
   if (obj == null || component == null) { return; }
   
   // 現在のプロジェクト
   if (_plateauInfo == null || plateauInfo == null) {
       // Debug.Log("PlateauInfo is null");
       return;
   }
   ```

4. **メソッドの実装の違い**
   - Awake()メソッドの実装が大幅に異なる（例: BuildingBreak.cs）
   - LINQ使用 vs foreachループ
   - オブジェクト検索方法の違い

## 推奨事項

### nochange_passフォルダに移動すべきファイル

**該当なし**

現在のプロジェクトのファイルは、ILSpyから復元されたファイルと比較して、すべて何らかの形で改善・修正されています。以下の理由により、機能的に同一と見なせるファイルはありません：

1. **変数名の改善**: ILSpyの自動生成名（`item`, `item2`, `num`など）から、意味のある名前に変更されている
2. **定数の使用**: ハードコードされた文字列や値が、定数（`GlobalConst`）に置き換えられている
3. **コメントとデバッグコードの追加**: 開発意図を示すコメントやデバッグコードが追加されている
4. **ロジックの改善**: より読みやすく、メンテナブルなコードに改善されている

### 注意が必要なファイル

**EnvironmentCameraCtrl.cs** は唯一、実質的な機能変更がほとんどないファイルですが、コメントアウトされたコードには将来の実装予定（カメラの視界距離制御）が含まれているため、そのままプロジェクトに保持することを推奨します。

### 見つからないファイルの扱い

30個のファイルが現在のプロジェクトに見つかりませんでした。これらは以下のいずれかの可能性があります：
- 削除された機能
- 名前が変更されたファイル
- 別の場所に移動されたファイル
- まだ実装されていない機能

これらのファイルについては、個別に確認し、必要に応じてプロジェクトに追加するか、不要であれば削除することを推奨します。

## 結論

**ILspy_CSフォルダ内のすべてのファイルは、現在のプロジェクトファイルと比較して機能的な違いがあるため、nochange_passフォルダに移動すべきファイルはありません。**

現在のプロジェクトのファイルは、ILSpyによる逆コンパイル後に、以下のような改善が施されています：
- コードの可読性向上
- 定数の使用による保守性向上
- コメントによるドキュメンテーション
- ロジックの改善

したがって、ILspy_CSフォルダのファイルは参照用として保持し、現在のプロジェクトのファイルを継続して使用することを強く推奨します。

## 詳細な差分ファイル

個別のファイルの詳細な差分は、以下のディレクトリに保存されています：
`g:\unity\OnoCoro2026\diff_analysis\`

各ファイルの `.diff.txt` ファイルを確認することで、具体的な違いを確認できます。
