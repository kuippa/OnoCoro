# テクスチャ生成ツール（CityHack 2026）

コードでテクスチャを生成する。色味や粒度を変えたくなったら定数をいじって再生成する
（Unity 側は PNG が差し替わるだけで自動再インポートされる）。

| スクリプト | 型名 | 生成物 |
| --- | --- | --- |
| GenDebrisTex.cs | `DebrisTexGen` | 廃材テクスチャ `Debris*.png` |
| GenWaterGauge.cs | `WaterGaugeGen` | 水位標 `WaterGauge*.png` |

---

## 廃材テクスチャ

`Assets/Resources/Textures/Debris*.png` を生成するコード。
色味やタイリングの粒度を変えたくなったら、`GenDebrisTex.cs` の定数をいじって
下記コマンドで再生成する（Unity 側は PNG が差し替わるだけで自動再インポートされる）。

## 実行方法（PowerShell）

```powershell
$src = Get-Content "AI_tmp_works\texgen\GenDebrisTex.cs" -Raw -Encoding UTF8
Add-Type -TypeDefinition $src -ReferencedAssemblies "System.Drawing"
[DebrisTexGen]::Generate("G:\unity\OnoCoro2026\Assets\Resources\Textures")
```

[NOTE] `-Encoding UTF8` は必須。省略すると日本語コメントが文字化けして
コンパイルエラーになる（Windows PowerShell 5.1 は既定で ANSI として読む）。

## 生成物

| ファイル | 用途 | 想定する構造種別 |
| --- | --- | --- |
| DebrisConcrete.png | コンクリート殻 | concrete（RC・SRC 造） |
| DebrisWood.png | 木材・角材 | wood（木造） |
| DebrisMetal.png | 錆びた金属 | steel（鉄骨造） |
| DebrisMixed.png | 混合廃材（汎用） | 種別を問わない既定 |

いずれも 64x64・上下左右がつながる（タイリング可能）。1 枚 6〜10KB。

## 実装メモ

- 格子をモジュロで巡回参照させることでシームレスな値ノイズを作っている
- 4/8/16 の 3 オクターブを重ねて、粗い塊感と細かいざらつきを両立させている
- 木材は 16px ごとに継ぎ目の暗い線を入れて板の集まりに見せている
- 金属の錆はしきい値（0.55）を超えた領域だけに乗せて斑にしている

---

## 水位標テクスチャ

呼び出し方は廃材と同じで、スクリプト名と型名だけ異なる。

```powershell
$src = Get-Content "AI_tmp_works\texgen\GenWaterGauge.cs" -Raw -Encoding UTF8
Add-Type -TypeDefinition $src -ReferencedAssemblies "System.Drawing"
[WaterGaugeGen]::Generate("G:\unity\OnoCoro2026\Assets\Resources\Textures")
```

| ファイル | 用途 |
| --- | --- |
| WaterGaugePole.png | 支柱の目盛り（20cm ごとの紅白帯・1m ごとの数字） |
| WaterGaugeBoard.png | 「想定浸水深」の標識板 |

[IMPORTANT] 目盛りの長さは `GAUGE_METERS` で決まる。
支柱の高さと揃えないとゲーム内の水深と目盛りがずれる。
また数字が入っているため**縦にタイリングしてはいけない**。

設置時の要件は
[cityhack2026-water-gauge.md](../../docs/_tasklist/detailed/cityhack2026-water-gauge.md) を参照。
