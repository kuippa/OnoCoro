# Unity 6.3 + Vector Graphics 3.0 での画像表示問題の解決方法

## 発見日
2026年1月15日

## 問題
Unity 6.3 で `com.unity.vectorgraphics` 3.0 を使用した際、SVGから変換した画像が表示されない

## 解決方法

### ✅ **解決策: スプライトメッシュタイプを使用に設定**

画像（Texture）のインポート設定で以下を有効にする：

```
Inspector設定:
1. 画像を選択
2. Texture Type: Sprite (2D and UI)
3. ▼ Advanced を展開
4. ☑ Use Sprite Mesh Type: ON (チェックを入れる)
5. Apply をクリック
```

### 重要なポイント
- **Sprite Mesh Type を使用** にチェックを入れることで画像が表示される
- この設定はデフォルトでOFFになっている
- Vector Graphics 3.0特有の問題と思われる
- ネット上にはまだ情報がない（2026年1月15日時点）

## 詳細設定

### 推奨設定値
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100
Mesh Type: Full Rect (または Tight)
☑ Use Sprite Mesh Type: ON  ← 重要
Extrude Edges: 0
Pivot: Center
Generate Physics Shape: OFF (必要に応じて)
```

## 適用対象
- Unity 6.0 以降
- Vector Graphics Package 3.0.0
- SVG → Sprite 変換時
- PNG/JPG でも同様の設定が有効

## 関連ファイル
- `SignPowerOutage` プレファブ（電力不足マーカー）
- その他の2D UI Sprite アイコン

## 参考
- Unity公式: https://docs.unity3d.com/Manual/class-TextureImporter.html
- Sprite Mesh については Unity 6.x で仕様変更された可能性が高い

## 備考
従来の Unity バージョンでは不要だった設定。Unity 6系での新しい仕様と思われる。
