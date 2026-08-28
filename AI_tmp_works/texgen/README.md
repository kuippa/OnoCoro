# 廃材テクスチャ生成ツール（CityHack 2026）

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
