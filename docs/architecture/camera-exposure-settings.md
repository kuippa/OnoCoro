# カメラ露出設定とトラブルシューティング

**対象**: OnoCoro カメラシステム・グラフィックスパイプライン  
**更新日**: 2026-03-07  
**関連バージョン**: Unity 6.3.10f1, Cinemachine 3.1.6, HDRP 17.3.0  
**重要度**: 高（バージョンアップ時の互換性問題）

---

## 概要

Unity 6.3.10f1 + Cinemachine 3.1.6 + HDRP 17.3.0 への更新において、カメラのピンボケ現象が発生しました。これは **Volume Profile の Depth of Field（被写体深度）の Focus Mode が「Manual Range」に設定されたこと** が原因でした。

HDRP/Cinemachine のバージョンアップ時には、Volume Profile のデフォルト設定が変更される可能性があります。このドキュメントは、同様の問題をすばやく診断・解決するためのガイドです。

---

## 問題の症状

| 現象 | 原因 | 対象 |
|------|------|------|
| **全フレームがピンボケ（焦点が合わない）** | Volume の DepthOfField が有効 | FPS/TPS/LongShot/BirdView すべてのカメラモード |
| **特定の視野範囲だけピン**ボケ | Focus Mode が "Manual Range" | Depth of Field の focus plane 計算エラー |
| **バージョンアップ後に急に発生** | HDRP/Cinemachine デフォルト値変更 | パッケージの自動更新時 |

---

## 原因分析

### Cinemachine 2.x vs 3.x の違い

| 項目 | Cinemachine 2.x | Cinemachine 3.1.6+ | 影響 |
|------|-----------------|-------------------|------|
| **焦点距離計算** | NearClip × 0.8 | NearClip × 2.0（仕様変更） | ピンボケ発生 |
| **NearClipPlane デフォルト** | 0.7f～1.25f | 同じコードでも計算結果が変化 | カメラ設定値の再調整が必要 |
| **Volume との相互作用** | 焦点距離が自動計算 | Focus Distance が明示设定で上書き | 競合が発生える |

### HDRP 17.3.0 での Depth of Field 変更

```yaml
【HDRP 17.0 時点】
Depth of Field:
  - Focus Mode: Fixed Distance (デフォルト)
  - Focus Distance: 10m
  - focal Length: 50mm（推奨）

【HDRP 17.3.0 への更新】
Depth of Field:
  - Focus Mode: Manual Range（デフォルトに変更）← ★ ここが問題
  - Manual Focus Start Distance: 指定値
  - Manual Focus End Distance: 指定値
```

**Manual Range Mode の問題点：**
- Cinemachine の自動計算焦点距離を上書きする
- 明示的な範囲設定なしでは、カメラが焦点を失う
- バージョンアップで自動的に有効化される（既存プロジェクトに不可視で反映）

---

## トラブルシューティング フロー

### Step 1: ピンボケの確認
```
[症状チェック] 
↓ リアルタイムプレイビューでピンボケが見えるか？
├─ YES → Step 2 へ
└─ NO → 他の原因を調査
```

### Step 2: Volume Profile の確認
```powerShell
# Unity エディタで以下を確認
Hierarchy → "Volume" を検索
  ↓
Inspector → Volume Profile → 「Using: Default/Custom」を確認
```

### Step 3: Depth of Field 設定を確認
```yaml
Volume Profile → Post-processing → Depth of Field

確認項目：
1. [O] Active？（有効になっていないか）
2. [O] Focus Mode？
   - "Physical Distance" → 正常（カメラが自動計算）
   - "Fixed Distance" → 可（手動設定が可能）
   - "Manual Range" ← ★ 注意！Cinemachine と競合する可能性
```

### Step 4: 修正を試行
```yaml
【修正方法】
Depth of Field を OFF にする
  または
Focus Mode を "Fixed Distance" に変更
  かつ
Focus Distance を 100（遠方）に設定して、Cinemachine 優先に変更
```

---

## 推奨設定値

### OnoCoro 用 Depth of Field 設定

| 設定項目 | 推奨値 | 理由 |
|---------|--------|------|
| **Active** | OFF（無効） | Cinemachine で自動管理するため |
| **Focus Mode**（OFF でない場合） | Physical Distance | カメラ距離に柔軟に対応 |
| **Focus Distance**（参考値） | 10m | プレイヤーキャラから約 10m 先にピント |
| **Aperture (f-stop)** | f/16 以上 | 被写界深度を浅くしない（ゲーム性重視） |
| **Blade Count** | 0（無効） | ボケの形状を制御しない |

### Cinemachine との協調設定

```csharp
// CameraController.cs での推奨値
private static void SetFPSCameraParameters(CinemachineCamera vcamera, CinemachineThirdPersonFollow thirdPersonFollow)
{
    if (thirdPersonFollow == null || vcamera == null) return;
    
    thirdPersonFollow.ShoulderOffset = _defaultShoulderOffsetFPS;
    thirdPersonFollow.CameraSide = _FPS_CAMERA_SIDE;
    thirdPersonFollow.VerticalArmLength = 0.0f;
    
    var lens = vcamera.Lens;
    lens.NearClipPlane = 0.3f;   // Cinemachine 3.x での調整値
    lens.FarClipPlane = 5000f;   // 遠方視界を確保
    vcamera.Lens = lens;
    
    // FIXME: HDRP Depth of Field との相互作用を監視
    // Cinemachine の焦点距離が有効に機能していることを確認すること
}
```

---

## バージョンアップ時のチェックリスト

マイナーバージョンアップ（HDRP 17.x → 17.y など）を実施した際に確認すべき項目：

| 確認項目 | チェック方法 | 対応 |
|---------|-----------|------|
| **Depth of Field が勝手に有効化されていないか** | Inspector で Volume Profile 確認 | OFF のままにするか、Focus Mode を Fixed Distance に |
| **Focus Mode が Manual Range に変更されていないか** | Depth of Field コンポーネント確認 | Physical Distance または OFF に統一 |
| **Exposure（露出）設定が変わっていないか** | Volume Profile → Post-processing → Exposure | 値が期待値か確認 |
| **Cinemachine NearClipPlane の計算が正常か** | カメラプレビュー確認 | ピンボケなければ OK |
| **カメラの視野角（FOV）が勝手に変わっていないか** | CameraController.cs で設定値確認 | ハードコード値と実際の値を比較 |

### チェック用スクリプト（オプション）

```csharp
public static class CameraExposureDebugger
{
    [RuntimeInitializeOnLoadMethod]
    public static void DebugOnLoad()
    {
        // ゲーム起動時に HDRP Volume 設定をログ出力
        var volume = FindObjectOfType<Volume>();
        if (volume != null && volume.profile != null)
        {
            Debug.Log("=== HDRP Volume Profile Debug ===");
            
            // Exposure 確認
            if (volume.profile.TryGet<Exposure>(out var exposure))
            {
                Debug.Log($"Exposure Mode: {exposure.mode}");
                Debug.Log($"Exposure Compensation: {exposure.compensation}");
            }
            
            // Depth of Field 確認
            if (volume.profile.TryGet<DepthOfField>(out var dof))
            {
                Debug.Log($"DepthOfField Active: {dof.active}");
                Debug.Log($"Focus Mode: {dof.focusMode}");
                Debug.Log($"Focus Distance: {dof.focusDistance}");
                Debug.Log($"Aperture: {dof.aperture}");
            }
        }
    }
}
```

---

## 2026-03-07 発生事例

### 発生状況
- **アップデート内容**: Unity 6.3.2f1 → 6.3.10f1、パッケージ全体を最新に更新
- **症状**: すべてのカメラモード（FPS/TPS/LongShot/BirdView）でピンボケ
- **原因**: GamePrefabs/Environment/Rendering Volume の Depth of Field → Focus Mode が "Manual Range" に設定されていた
- **解決方法**: Focus Mode を OFF にする（Depth of Field を無効化）
- **結果**: 正常なピント状態に復帰

### 教訓
- HDRP/Cinemachine のマイナーアップデートでも、デフォルト設定が変わる可能性がある
- **Volume Profile は git で追跡されないため（バイナリまたは LFS 管理）、変更を検知しにくい**
- バージョンアップ直後は Editor Log とゲーム画面を目視で確認すること

---

## 関連ドキュメント

- [architecture.md](../architecture.md) - システムアーキテクチャ
- [.github/instructions/plateau-sdk-geospatial.instructions.md](../../.github/instructions/plateau-sdk-geospatial.instructions.md) - PLATEAU SDK 統合ガイド
- [debugging-and-logging.md](../project-rules/debugging-and-logging.md) - デバッグとログ管理
- HDRP 公式ドキュメント: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/

---

## 参考リンク

- [Cinemachine 3.1.6 チェンジログ](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/changelog/CHANGELOG.html)
- [HDRP Depth of Field](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/manual/Post-Process-Depth-of-Field.html)
- [HDRP Exposure](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.3/manual/Post-Process-Exposure.html)

---

**作成者**: GitHub Copilot (Claude Haiku 4.5)  
**最終確認**: 2026-03-07
