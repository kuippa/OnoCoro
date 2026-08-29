---
title: Built-in 向けアセットを HDRP に取り込む
description: アセットストアのモデルを入れてマテリアルがマゼンタになったときの直し方
---

# Built-in 向けアセットを HDRP に取り込む

## 症状

アセットストアからモデルを入れたら、マテリアルが**全部マゼンタ（紫）**になる。

## 原因

本プロジェクトは **HDRP**。アセットが **Built-in Render Pipeline** 向けだと、
マテリアルが Built-in の Standard シェーダーを参照しており、HDRP では描画できない。

数年前のアセットはたいてい Built-in 向けなので、これは珍しいことではない。

## 直し方

1. Project ウィンドウでそのアセットの `Materials` フォルダを開く
2. マテリアルを**全選択**（Ctrl + A）
3. **`Edit > Rendering > Materials > Convert Selected Materials using HDRP upgraders`**

[WARN] 同じ階層にある `Convert All Materials using HDRP upgraders` は
**プロジェクト内の全マテリアルが対象**になる。自作の HDRP マテリアルまで
走ってしまうので、必ず **Selected**（選択したぶんだけ）を使うこと。

[NOTE] `Upgrade HDRP Materials to Latest Version` は名前が似ているが別物。
これは「HDRP マテリアルの版上げ」で、Built-in からの変換ではない。

## 変換後に確認すること

アップグレーダは Standard の Albedo / Normal / Metallic を HDRP/Lit の
対応スロットへ移してくれるが、万能ではない。以下は目視で確認する。

- **テクスチャが入っているか** … Base Map に元の Albedo が来ているか
- **透過が効いているか** … Standard の Cutout / Transparent は
  Surface Type の再設定が要ることがある（目・まぶた・葉・髪などで起きやすい）
- **明るさ・質感** … Smoothness の解釈差で濃すぎ / 薄すぎになることがある

崩れるマテリアルが少数なら、変換に頼らず **HDRP/Lit を手で割り当てて
Base Map にテクスチャを入れ直す**ほうが早い。Textures フォルダは残っている。

## 自分でマテリアルを作る場合

- シェーダーは **HDRP > Lit**
- テクスチャを入れるスロットは **Surface Inputs > Base Map**
  （Built-in の Albedo にあたる。名前が違うので探しにくい）
- **Base Color は白にする**。HDRP では Base Color が Base Map に乗算されるため、
  色が付いたままだとテクスチャの色が濁る

## 確認方法（マゼンタの原因切り分け）

マテリアルの `.mat` をテキストで開き、`m_Shader` を見る。

```
m_Shader: {fileID: 46, guid: 0000000000000000f000000000000000, type: 0}
```

この `guid: 0000...f000...` は Unity 組み込みリソースを指し、`fileID: 46` は
Built-in の Standard シェーダー。これが出ていれば Built-in 向けアセットで確定。

## 実績

- 2026-08-29 `Assets/Samples/LittleFriends-CartoonAnimals-Lite`
  （2023 年のアセット・猫モデル）を上記手順で変換して復旧

---

## 関連ドキュメント

- [../project-rules/folder-structure.md](../project-rules/folder-structure.md) - フォルダ構成
