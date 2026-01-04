# 手順メモ
ステージを新規作成
Plateau SDK からターゲットのエリアを選択インポート（時間かかる）
読み込まれたオブジェクトうちいらないものをざっくり削除
カメラの削除
EventSystemの削除
GamePrefabsをステージに追加
基準となるXXX_dem_XXXの子供のdem_XXXのタグをGroundに
nav AI>NavMesh Surfaceを追加
nav NavMesh の Agent typeをターゲット（sweeper）が動けるタイプに変更
bake
ステージ名と同名のyamlを \Assets\Resources\staging に配置




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


