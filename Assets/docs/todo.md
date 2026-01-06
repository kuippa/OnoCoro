## FixMe:
PlayModeでitemリストが読み込まれないバグがある。
Navmesh surfaceが地面から少し浮いている
Bloom付きシェーダーマテリアルの不透明度がおかしい。なおしたいがエラーが頻発で解決できず。
静止板がUnit structをもっていなかったので削除できるように追加。
WASDで移動したときマウスポインターが消えて、画面の中央になるが、これのせいでスポーンポイントマーカーを表示しているときの挙動が変。
視界カメラが障害物などとの衝突で上下天地することがある


TODO:
Debug.Log 消しておくか、統合的なログ吐き出し関数へ
Debug.LogErrorが使われている箇所があるので消しておく
同じ優先順位の描画オブジェクトを配置すると「Zファイティング」が起きて画面がちらつく。y座標の位置を微調整すること。



SubstancePainter
テクスチャ作成ツール







## 後回し
パフォーマンスチューニング
    findをやめるなど
    Unity Profiler(Development Build)
    Memory Profiler
    Heap Explorer(個人開発者OSS)
    UnityEditor.AssetPostprocessor
        AssetPostprocessor
        settings.textureCompression = TextureImporterCompression.Compressed;
    Physics.autoSimulation




多言語対応



