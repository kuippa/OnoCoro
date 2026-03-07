# ビルド環境 (Build Environment)

OnoCoro 開発プロジェクトの公式ビルド環境仕様。開発者は以下の設定でプロジェクトを構築してください。

## Unity エディターバージョン

| 項目 | 値 |
|------|-----|
| **Unity Version** | **6.3.10f1** |
| **Editor Revision** | e35f0c77bd8e |
| **Project Settings** | `ProjectSettings/ProjectVersion.txt` で確認可能 |

### バージョン指定の理由

- [OK] Focal distance 計算が安定 (3.x との互換性) - Cinemachine の NearClip 計算が統一
- [OK] HDRP 17.3.0 との完全互換
- [NOTE] 6.3.2f1 からアップグレード - Depth of Field の初期値が変更

---

## 必須パッケージ

### PLATEAU SDK (地理空間データ処理)

| パッケージ | バージョン | 用途 |
|-----------|-----------|------|
| **PLATEAU SDK for Unity** | `git` (GitHub) | CityGML ファイル読み込み、座標変換 |
| **PLATEAU SDK Toolkits** | `git` (GitHub) | 3D レンダリング、データ処理ユーティリティ |

### グラフィックス・レンダリング

| パッケージ | バージョン | 用途 |
|-----------|-----------|------|
| **HDRP (High Definition Render Pipeline)** | 17.3.0 | 高品質レンダリング、Depth of Field |
| **HDRP Config** | 17.3.0 | HDRP プロジェクト設定 |
| **Visual Effect Graph** | 17.3.0 | パーティクルエフェクト (水システム、汚染可視化) |
| **Cinemachine** | 3.1.6 | カメラ制御、スムーズズーム |
| **Post-processing** | 3.5.3 | 画面効果（色補正、グロー） |
| **UI Toolkit (uGUI)** | 2.0.0 | UI レイアウト |
| **Vector Graphics** | 3.0.0-preview.7 | 2D グラフィックス |

### ゲームシステム

| パッケージ | バージョン | 用途 |
|-----------|-----------|------|
| **Input System** | 1.18.0 | キーボード・マウス入力 |
| **Timeline** | 1.8.11 | シーン遷移・イベントシーケンシング |
| **AI Navigation** | 2.0.11 | 予約済み (将来的な敵 AI) |
| **Visual Scripting** | 1.9.10 | 予約済み (ビジュアルロジック) |

### 開発ツール

| パッケージ | バージョン | 用途 |
|-----------|-----------|------|
| **Recorder** | 5.1.5 | ゲームプレイ映像録画 |
| **IDE Support (Rider)** | 3.0.39 | JetBrains Rider 統合 |
| **IDE Support (Visual Studio)** | 2.0.27 | Visual Studio 統合 |
| **Test Framework** | 1.6.0 | ユニットテスト |
| **Development** | 1.0.2 | デバッグツール、Profiler |

### その他

| パッケージ | バージョン | 用途 |
|-----------|-----------|------|
| **Collab Proxy** | 2.11.4 | Unity Collaborate (オプション) |
| **Multiplayer Center** | 1.0.1 | オンラインマルチプレイ (予約済み) |
| **XR Management** | 4.5.4 | VR サポート (予約済み) |
| **Extended NUnit** | 2.0.5 | テストフレームワーク拡張 |

---

## 完全パッケージリスト (Packages/manifest.json)

全パッケージはプロジェクトの `Packages/manifest.json` に記録されています。

### カテゴリ別パッケージ一覧

**Core Modules (1.0.0):**
- com.unity.modules.accessibility
- com.unity.modules.adaptiveperformance
- com.unity.modules.ai
- com.unity.modules.androidjni
- com.unity.modules.animation
- com.unity.modules.assetbundle
- com.unity.modules.audio
- com.unity.modules.cloth
- com.unity.modules.director
- com.unity.modules.imageconversion
- com.unity.modules.imgui
- com.unity.modules.jsonserialize
- com.unity.modules.particlesystem
- com.unity.modules.physics
- com.unity.modules.physics2d
- com.unity.modules.screencapture
- com.unity.modules.terrain
- com.unity.modules.terrainphysics
- com.unity.modules.tilemap
- com.unity.modules.ui
- com.unity.modules.uielements
- com.unity.modules.umbra
- com.unity.modules.unityanalytics
- com.unity.modules.unitywebrequest (及び関連)
- com.unity.modules.vectorgraphics
- com.unity.modules.vehicles
- com.unity.modules.video
- com.unity.modules.vr
- com.unity.modules.wind
- com.unity.modules.xr

---

## 開発環境推奨スペック

### 最小要件

| 項目 | 仕様 |
|------|------|
| **CPU** | Intel Core i5-8400 相当以上 |
| **RAM** | 16 GB 以上 |
| **ストレージ** | SSD 100 GB 以上 (空き容量) |
| **GPU** | NVIDIA GTX 960 / AMD R9 Fury 相当 |
| **OS** | Windows 10 21H2 以上 |

### 推奨スペック

| 項目 | 仕様 |
|------|------|
| **CPU** | Intel Core i7-12700K / AMD Ryzen 7 5800X 相当 |
| **RAM** | 32 GB 以上 |
| **ストレージ** | NVMe SSD 200 GB 以上 (空き容量) |
| **GPU** | NVIDIA RTX 3080 / AMD RX 6800 XT 相当 |
| **OS** | Windows 11 23H2 以上 |
| **モニター** | 3440x1440 (ウルトラワイド推奨) または 3840x2160 |

### ネットワーク

- [OK] インターネット接続 (Git, npm パッケージ取得用)
- [OK] 1 Mbps 以上の下り速度推奨

---

## セットアップ手順

### 1. このドキュメントで前提条件を確認

- Unity 6.3.10f1 がインストール済みか確認
- パッケージマネージャーで上記パッケージがロードされているか確認
- `Packages/manifest.json` を開いて依存関係を確認

```powershell
# PowerShell で manifest.json を確認
Get-Content Packages\manifest.json | Select-String -Pattern "com.unity|com.synesthesias"
```

### 2. 依存パッケージのインストール

プロジェクトを Unity Editor で開くと、パッケージマネージャーが自動的にダウンロード・インストールします。

```
[メニュー] → Window → TextureFormat Package Manager
            または
            → Package Manager タブで確認
```

### 3. PLATEAU SDK の初期化

```csharp
// Assets/Scripts/Core/Managers/PlateauManager.cs で実装
// SDK 初期化は起動時に自動実行
```

### 4. ビルド設定を確認

**File → Build Settings で対象プラットフォームを指定:**

| 設定項目 | 値 |
|--------|-----|
| **Target Platform** | PC, Mac & Linux Standalone |
| **Architecture** | x86_64 |
| **Build System** | Visual Studio 2022 |
| **Scene List** | Main.unity, Title.unity, Stage*.unity |

### 5. プロジェクト設定を確認

**Edit → Project Settings:**

| セクション | 設定 |
|-----------|------|
| **Player** | Company Name: "PLATEAU" |
| | Product Name: "OnoCoro" |
| | Version: "0.1.0-alpha" |
| **Graphics** | Scriptable Render Pipeline: HDRP |
| **Quality** | Default Quality: Ultra (推奨) |

### 6. エディタースクリプト実行

```powershell
# Assets/Editor/BuildTools.cs の初期化スクリプト実行
# または Unity Editor で Tools → Rebuild Project を選択
```

---

## トラブルシューティング

### パッケージ読み込みエラー

[NOTE] GitHub リポジトリから PLATEAU SDK をダウンロードできない場合：

```powershell
# キャッシュをクリア
Remove-Item -Path "Library\PackageCache" -Recurse -Force
Remove-Item -Path ".git\index.lock" -Force

# プロジェクト再オープン
```

### Cinemachine Focal Distance エラー

[NOTE] 旧プロジェクト (Unity 3.x) の Cinemachine 2.x カメラプリセットは互換性がない場合があります。

**解決方法:**
1. Camera プリセットを削除
2. `CameraManager.cs` で NearClip = FocalDistance × 2.0 で統一を確認
3. `docs/architecture/camera-exposure-settings.md` を参照

### HDRP Depth of Field 初期値

[NOTE] HDRP 17.3.0 では Depth of Field デフォルトが "Manual Range" に変更されました。

**解決方法:**
1. HDRP Settings Asset を確認
2. `HDRP_Default_Settings.asset` を参照
3. Depth of Field を Disabled で初期化

### TextMeshPro フォント表示されない

[NOTE] スタンドアロンビルド (.exe) 実行時に UI テキストが表示されない場合、Player Settings の Always Included Shaders が未設定の可能性があります。

**解決方法:**
1. **詳細ガイド**: [docs/TEXMESHPRO_BUILD_SETTINGS.md](TEXMESHPRO_BUILD_SETTINGS.md) を参照
2. Edit → Project Settings → Player → Graphics → Always Included Shaders に以下を追加：
   - `TextMeshPro/Distance Field`
   - `TextMeshPro/Mobile/Distance Field`
   - `TextMeshPro/Sprite`
   - `TextMeshPro/Distance Field Overlay`
3. Build フォルダを削除して Clean Build を実施
4. 再ビルド

---

## 参考リンク

| ドキュメント | 目的 |
|-----------|------|
| [AGENTS.md](../../AGENTS.md) | プロジェクト開発ガイド全般 |
| [docs/architecture.md](../architecture.md) | システム構成図 |
| [docs/project-rules/coding-csharp.md](../project-rules/coding-csharp.md) | C# コーディング標準 |
| [docs/TEXMESHPRO_BUILD_SETTINGS.md](TEXMESHPRO_BUILD_SETTINGS.md) | **TextMeshPro シェーダー設定ガイド** |
| [.github/instructions/](../../.github/instructions/) | Recovery フェーズ実装ガイド |
| [docs/architecture/camera-exposure-settings.md](../architecture/camera-exposure-settings.md) | カメラ設定詳細 |

---

## 更新履歴

| 日付 | 変更内容 |
|------|--------|
| 2026-03-08 | 初版作成: Unity 6.3.10f1 パッケージ完全リスト |

**最終更新:** 2026-03-08
**維持者:** GitHub Copilot (OnoCoro Development)
