# 手順メモ
ステージを新規作成
Plateau SDK からターゲットのエリアを選択インポート（時間かかる）
読み込まれたオブジェクトうちいらないものをざっくり削除
カメラの削除
EventSystemの削除
Directional Lightの削除
GamePrefabsをステージに追加
GamePrefabsのPlayerArmatureのプレイヤースタート位置を調整
dem_ で検索
基準となるXXX_dem_XXXの子供のdem_XXXのタグをGroundに
dem_*** を右クリック
AI → 
nav AI>NavMesh Surfaceを追加
NavMesh Surface の Agent typeをターゲット（sweeper）タイプに変更
bake
ステージ名と同名のyamlを \Assets\Resources\staging に配置



# Plateau SDK からのインポート手順
現在のところサーバー経由からの導入より、
G空間情報センターのPlateauデータをダウンロードして、ローカルに展開し、
それをインポートしたほうがよい。サーバー経由だとタイムアウトしてビル情報などが欠落、失敗することが多い。
https://front.geospatial.jp/
TOPからも辿れるがわかりにくいので下記URLから土地名で検索がよい
https://www.geospatial.jp/ckan/dataset/?license_id=plateau
2026/1/21 現在 549 件のデータセット



# TIPSメモ
オブジェクトはシーンに保存されるので狭い範囲を読み込んでも100MBを超える。LOD0,LOD1などを消して１つの建物と地面だけにして5MB





# 環境
地べたマテリアルのBase UV mapping をUV0から Planarへ：コードで自動的になされます NarakuCtrl
シーンのディレクショナルライト(サン)、影、シャドーマップ、解像度の設定→ウルトラ：現在コードではなされていません EnvironmentLight



# スカイボックスの追加＆変更
+ ヒエラルキー→ボリューム→Sky and Fog Global Volume
### Physically Based Sky 

### フォグ
+ アテミュレーションディスタンス：2000  //
+ マックスフォグディスタンス:20000
+ Base height:-80   // 地面の高さに合わせて変更する
+ Maximam:300


