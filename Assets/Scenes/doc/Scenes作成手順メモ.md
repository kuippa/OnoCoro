# 手順メモ
ステージを新規作成
Plateau SDK からターゲットのエリアを選択インポート（時間かかる）
読み込まれたオブジェクトうちいらないものをざっくり削除
カメラの削除
EventSystemの削除
GamePrefabsをステージに追加
基準となるXXX_dem_XXXのタグをGroundに
nav AI>NavMesh Surfaceを追加
nav Agent typeをターゲットが動けるタイプに変更
bake
ステージ名と同名のyamlを \Assets\Resources\staging に配置




# TIPSメモ
オブジェクトはシーンに保存されるので狭い範囲を読み込んでも100MBを超える。LOD0,LOD1などを消して１つの建物と地面だけにして5MB









