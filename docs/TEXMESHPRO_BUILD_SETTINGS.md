# TextMeshPro シェーダー設定ガイド (Unity 6.3.10f1)

**最終更新**: 2026-03-08  
**対象バージョン**: Unity 6.3.10f1  
**作成背景**: v0.1.0-alpha ビルド時の TextMeshPro フォント表示問題の解決

---

## 概要

Unity 6.3.10f1 でスタンドアロンビルド（.exe）を実行したとき、TextMeshPro (TMPro) のフォントが表示されない問題が発生することがあります。

**原因**: Player Settings に TextMeshPro シェーダーが「Always Included Shaders」として登録されていない

**解決**: Edit → Project Settings → Player → Graphics → Always Included Shaders に TextMeshPro シェーダーを追加

---

## TextMeshPro シェーダー完全リスト

### シェーダー定義名と対応ファイル

| Shader 定義名（Player Settings で使用） | ファイル名 | 用途 | ビルド必須 |
|-------------------------------------|---------|------|---------|
| **TextMeshPro/Distance Field** | TMP_SDF.shader | UI テキスト通常表示 | [OK] YES |
| **TextMeshPro/Mobile/Distance Field** | TMP_SDF-Mobile.shader | モバイル版テキスト | [OK] YES |
| **TextMeshPro/Sprite** | TMP_Sprite.shader | スプライト表示 | [OK] YES |
| **TextMeshPro/Distance Field Overlay** | TMP_SDF Overlay.shader | Overlay Canvas 用 | [OK] YES |
| **TextMeshPro/Mobile/Distance Field Overlay** | TMP_SDF-Mobile Overlay.shader | モバイル Overlay | [NOTE] YES |
| **TextMeshPro/Distance Field SSD** | TMP_SDF SSD.shader | 高品質テキスト（SSD） | [NOTE] OPTIONAL |
| **TextMeshPro/Mobile/Distance Field SSD** | TMP_SDF-Mobile SSD.shader | モバイル SSD | [NOTE] OPTIONAL |
| **TextMeshPro/Distance Field - Masking** | TMP_SDF-Mobile Masking.shader | テキストマスク用 | [NOTE] OPTIONAL |
| **TextMeshPro/Mobile/Distance Field - Masking** | TMP_SDF-Mobile Masking.shader | モバイルマスク用 | [NOTE] OPTIONAL |
| **TextMeshPro/Distance Field - 2 Pass** | TMP_SDF-Mobile-2-Pass.shader | 2 パスレンダリング | [NOTE] OPTIONAL |
| **TextMeshPro/Mobile/Distance Field - 2 Pass** | TMP_SDF-Mobile-2-Pass.shader | モバイル 2 パス | [NOTE] OPTIONAL |
| **TextMeshPro/Distance Field (Surface)** | TMP_SDF-Surface.shader | Surface Shader | [NOTE] OPTIONAL |
| **TextMeshPro/Mobile/Distance Field (Surface)** | TMP_SDF-Surface-Mobile.shader | モバイル Surface | [NOTE] OPTIONAL |
| **TextMeshPro/Bitmap** | TMP_Bitmap.shader | ビットマップフォント | [NOTE] OPTIONAL |
| **TextMeshPro/Mobile/Bitmap** | TMP_Bitmap-Mobile.shader | モバイルビットマップ | [NOTE] OPTIONAL |
| **TextMeshPro/Bitmap Custom Atlas** | TMP_Bitmap-Custom-Atlas.shader | カスタム Atlas | [NOTE] OPTIONAL |

---

## 推奨設定（プロトタイプビルド）

### 最小限の設定（推奨）

以下の 4 シェーダーを **最低限** 追加してください：

```
1. TextMeshPro/Distance Field
2. TextMeshPro/Mobile/Distance Field
3. TextMeshPro/Sprite
4. TextMeshPro/Distance Field Overlay
```

**理由**: OnoCoro プロトタイプは PC スタンドアロンビルド（Overlay Canvas）を使用しており、これら 4 つで対応可能

### 完全な設定（全テキスト機能対応）

以下セットで全 TextMeshPro 機能を網羅：

```
1. TextMeshPro/Distance Field
2. TextMeshPro/Mobile/Distance Field
3. TextMeshPro/Sprite
4. TextMeshPro/Distance Field Overlay
5. TextMeshPro/Mobile/Distance Field Overlay
6. TextMeshPro/Distance Field SSD
7. TextMeshPro/Distance Field (Surface)
```

---

## セットアップ手順（ステップバイステップ）

### Step 1: Player Settings を開く

```
[メニュー] Edit → Project Settings → Player
```

### Step 2: Graphics セクションまでスクロール

- **Graphics** カテゴリを展開
- **Always Included Shaders** を見つける

### Step 3: シェーダーを追加

#### 推奨設定の場合（最小 4 個）:

1. **Size** フィールドを `4` に設定
2. 以下を順序通り入力：
   ```
   Element 0: TextMeshPro/Distance Field
   Element 1: TextMeshPro/Mobile/Distance Field
   Element 2: TextMeshPro/Sprite
   Element 3: TextMeshPro/Distance Field Overlay
   ```

#### 完全設定の場合（7 個以上）:

1. **Size** フィールドを `7` に設定
2. 上記推奨設定 4 個 + 追加 3 個を入力

### Step 4: 保存・再ビルド

1. **Ctrl + S** で ProjectSettings を保存
2. **File → Build and Run** で新規ビルド実行
3. ビルド完了後、.exe を実行

---

## トラブルシューティング

### 症状 1: フォントが完全に表示されない

**確認事項:**
- [ ] Always Included Shaders に `TextMeshPro/Distance Field` が存在？
- [ ] Shader 定義名が「ファイル名」ではなく「Shader "..."」の形式？
- [ ] Build フォルダが古い？ → 削除して Clean Build

**解決方法:**
```powershell
# Build フォルダを削除
Remove-Item -Path "G:\unity\OnoCoro2026\Build" -Recurse -Force

# Unity Editor で再ビルド
# File → Build Settings → Build and Run
```

### 症状 2: 一部のシーンだけフォント表示されない

**原因**: Overlay Canvas で使用時は `TextMeshPro/Distance Field Overlay` が必須

**確認事項:**
- [ ] Always Included Shaders に `TextMeshPro/Distance Field Overlay` が存在？
- [ ] シーンの Canvas が「Render Mode: Overlay」になっている？

**解決方法:**
1. `TextMeshPro/Distance Field Overlay` を追加
2. 再ビルド

### 症状 3: ビルド時にシェーダーコンパイルエラー

**エラーメッセージ例**:
```
Shader error in 'TextMeshPro/Distance Field': ...
```

**原因**: HDRP との互換性問題（Unity 6.3.10f1 固有）

**解決方法:**

#### Option A: Surface Shader を使用（推奨）
```
TextMeshPro/Distance Field → TextMeshPro/Distance Field (Surface) に変更
```

#### Option B: HDRP 設定を確認
```
Edit → Project Settings → Graphics → 
Scriptable Render Pipeline Settings → HDRP Asset → 
Material Quality / Shader Settings を確認
```

### 症状 4: パフォーマンス低下（フレームレート低下）

**原因**: Distance Field SSD（高品質）を不要に使用中

**解決方法**:
1. `TextMeshPro/Distance Field SSD` を削除
2. 通常の `TextMeshPro/Distance Field` のみを使用
3. 再ビルド

---

## ビルド後の検証

### 方法 1: Visual Studio で確認

```
Build フォルダ開く
  → OnoCoro_Data/resources.assets を検査
  → TextMeshPro シェーダーが含まれているか確認
```

### 方法 2: Unity Editor で確認

```
Edit → Project Settings → Player → Graphics → 
Always Included Shaders で登録内容を再確認
```

### 方法 3: .exe 実行時のログ確認

```powershell
# スタンドアロンビルド実行時のログ
G:\unity\OnoCoro202603_build_prot\OnoCoro_Data\output_log.txt
```

ログ内容に以下が含まれていれば正常：
```
[TMP] Loading TMP_Settings.asset
[TMP] Initializing TextMeshPro
Shader 'TextMeshPro/Distance Field' loaded
```

---

## HDRP 17.3.0 との互換性

OnoCoro は **HDRP 17.3.0** を使用しています。

### 既知の互換性問題

| 問題 | 症状 | 解決方法 |
|------|------|--------|
| **Overlay Canvas Depth** | テキストが背景に隠れる | Canvas の Sorting Order を確認 |
| **Dynamic Batching** | パフォーマンス低下 | Edit → Project Settings → Player → Dynamic Batching を無効化 |
| **Shader Variants** | ビルドサイズ増加 | `TextMeshPro/Distance Field (Surface)` を使用 |
| **Depth of Field** | テキストがぼやける | HDRP Settings で Depth of Field を Disabled に設定（既実装） |

**OnoCoro での設定** (既適用):
- Depth of Field: Disabled
- HDRP バージョン: 17.3.0
- Render Pipeline: High Definition Render Pipeline

詳細: [docs/BUILD_ENVIRONMENT.md](BUILD_ENVIRONMENT.md#パッケージ仕様)

---

## 参考資料

| ドキュメント | リンク | 目的 |
|-----------|-------|------|
| **BUILD_ENVIRONMENT.md** | [docs/BUILD_ENVIRONMENT.md](BUILD_ENVIRONMENT.md) | 完全なビルド環境仕様 |
| **Unity 6 Release Notes** | https://docs.unity3d.com/6.0/Documentation/Manual/whats-new.html | Unity 6.3 の新機能 |
| **TextMeshPro Manual** | https://docs.unity3d.com/Manual/com.unity.textmeshpro.html | TextMeshPro 公式ドキュメント |
| **HDRP Documentation** | https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition/ | HDRP 公式ドキュメント |

---

## よくある質問 (FAQ)

### Q1: モバイルビルドの場合はどのシェーダーを使う？

**A**: `TextMeshPro/Mobile/*` シェーダーを使用してください。

推奨:
```
1. TextMeshPro/Mobile/Distance Field
2. TextMeshPro/Mobile/Distance Field Overlay
3. TextMeshPro/Sprite
```

### Q2: ビットマップフォントを使う場合は？

**A**: `TextMeshPro/Bitmap` を追加してください。

```
1. TextMeshPro/Distance Field（通常テキスト用）
2. TextMeshPro/Bitmap（ビットマップフォント用）
3. TextMeshPro/Sprite
4. TextMeshPro/Distance Field Overlay
```

### Q3: この設定で VR ビルドに対応できる？

**A**: 部分的に対応できます。VR 専用シェーダーは別途設定が必要な場合があります。

詳細は Unity VR ドキュメントを参照してください。

### Q4: ビルドサイズが大幅に増えた。削減方法は？

**A**: 使わないシェーダーを削除してください。

必須のみ:
```
1. TextMeshPro/Distance Field
2. TextMeshPro/Sprite
3. TextMeshPro/Distance Field Overlay
```

### Q5: Editor では表示されるのに、ビルド後は表示されない

**A**: ほぼ確実に Always Included Shaders の漏れです。

**確認**:
1. `TextMeshPro/Distance Field` が追加されているか？
2. Shader 名が正確か？（スペルや大文字小文字）
3. Build フォルダを削除して Clean Build したか？

---

## バージョン履歴

| 日付 | バージョン | 変更内容 |
|------|----------|--------|
| 2026-03-08 | 1.0 | 初版作成。Unity 6.3.10f1 用シェーダー設定完成 |

---

## 追記：OnoCoro プロトタイプビルドでの設定状況

**現在の設定** (v0.1.0-alpha):

[OK] **推奨設定を適用済み**
- TextMeshPro/Distance Field
- TextMeshPro/Mobile/Distance Field
- TextMeshPro/Sprite
- TextMeshPro/Distance Field Overlay

[OK] **HDRP 17.3.0 完全互換**

[OK] **Depth of Field 無効化済み**

[OK] **ビルド環境ドキュメント統合済み**

---

**作成者**: GitHub Copilot (OnoCoro Development)  
**ライセンス**: MIT (OnoCoro プロジェクトに準ずる)
