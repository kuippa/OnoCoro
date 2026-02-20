namespace CommonsUtility
{
    /// <summary>
    /// YAML ファイルで使用可能なセクション種別
    /// </summary>
    internal enum YamlSectionType
    {
        /// <summary>メタデータ（stagename, stageid など）</summary>
        Metadata,
        
        /// <summary>ステージ定義（name, BIT, CLK）</summary>
        Stages,
        
        /// <summary>配置可能ユニット一覧</summary>
        ItemLists,
        
        /// <summary>ゲーム上のマーカー配置</summary>
        PathMakers,
        
        /// <summary>ゴール条件</summary>
        Goals,
        
        /// <summary>ゲームオーバー条件</summary>
        GameOvers,
        
        /// <summary>タイマーイベント</summary>
        Events,
        
        /// <summary>ゲームボード設定</summary>
        Boards
    }

    /// <summary>
    /// ゴール条件のタイプ
    /// ユーザーが YAML で使用可能な値のみを制限
    /// </summary>
    internal enum GoalType
    {
        /// <summary>すべての敵を倒す</summary>
        AllEnemiesDefeated,
        
        /// <summary>スコア閾値到達</summary>
        ScoreThreshold,
        
        /// <summary>時間制限内クリア</summary>
        TimeLimit,
        
        /// <summary>ウェーブ完了</summary>
        WavesCompleted
    }

    /// <summary>
    /// ゲームオーバー条件のタイプ
    /// </summary>
    internal enum GameOverType
    {
        /// <summary>HP がゼロになる</summary>
        HealthDefeated,
        
        /// <summary>敵がゴールに到達</summary>
        EnemyReachedGoal,
        
        /// <summary>時間切れ</summary>
        TimeExpired,
        
        /// <summary>ユニット配置不可</summary>
        NoPlaceableUnits,
        
        /// <summary>フィールド上のごみ数が閾値を超過</summary>
        GarbageOverflow
    }

    /// <summary>
    /// YAML イベント種別（EventLoader.ActionEvent() で処理されるゲーム進行時イベント）
    /// UnityEngine.EventType との衝突を避けるため YamlEventType に命名
    /// 実装と定義を完全に一致させ、YAML バリデーションを強化
    /// </summary>
    internal enum YamlEventType
    {
        // スポーン関連イベント
        /// <summary>ユニット配置イベント（SpawnController で処理）</summary>
        spawn_unit,
        
        /// <summary>敵ユニット配置イベント（SpawnController で処理）</summary>
        spawn_enemy_unit,
        
        // 環境イベント
        /// <summary>天候変更イベント（WeatherController で処理）</summary>
        weather,
        
        /// <summary>太陽高度変更イベント（WeatherController で処理）</summary>
        solar,
        
        /// <summary>風設定イベント（WindController で処理）</summary>
        wind,
        
        /// <summary>水面高さ変更イベント（WaterSurfaceCtrl で処理）</summary>
        watersurface,
        
        // 災害イベント
        /// <summary>地震イベント（Earthquake コンポーネント で処理）</summary>
        earthquake,
        
        /// <summary>建物破壊イベント（BuildingBreak コンポーネント で処理）</summary>
        building_break,
        
        // UI 通知イベント
        /// <summary>通知メッセージ表示（NoticeCtrl で処理）</summary>
        notice,
        
        /// <summary>大型テロップ表示（TelopCtrl で処理）</summary>
        telop,
        
        /// <summary>小型テロップ表示（TelopCtrl で処理）</summary>
        subtelop,
        
        // パス・ビジュアルイベント
        /// <summary>パス強調表示開始（BloomPathController で処理）</summary>
        bloom_path,
        
        /// <summary>パス強調表示解除（BloomPathController で処理）</summary>
        off_bloom_path,
        
        // 未実装
        /// <summary>火山噴火イベント（未実装）</summary>
        volcano
    }

    /// <summary>
    /// ボード設定項目
    /// </summary>
    internal enum BoardConfigType
    {
        /// <summary>ボードサイズ（"256x256" など）</summary>
        BoardSize,
        
        /// <summary>同時スポーン敵数</summary>
        SpawnPoints,
        
        /// <summary>最大ウェーブ数</summary>
        MaxWaves,
        
        /// <summary>難易度修正値</summary>
        DifficultyModifier
    }

    /// <summary>
    /// YAML でゴール定義に使用可能なフィールド名
    /// </summary>
    internal enum GoalCommandFields
    {
        /// <summary>goal_type - ゴール条件のタイプ</summary>
        goal_type,
        
        /// <summary>threshold - 達成条件の閾値</summary>
        threshold,
        
        /// <summary>description - ゴールの説明</summary>
        description
    }

    /// <summary>
    /// YAML でゲームオーバー定義に使用可能なフィールド名
    /// </summary>
    internal enum GameOverCommandFields
    {
        /// <summary>gameover_type - ゲームオーバー条件のタイプ</summary>
        gameover_type,
        
        /// <summary>threshold - トリガーの閾値</summary>
        threshold
    }

    /// <summary>
    /// YAML でイベント定義に使用可能なフィールド名
    /// </summary>
    internal enum TimedEventCommandFields
    {
        /// <summary>time - イベント発火時刻（秒）</summary>
        time,
        
        /// <summary>event - イベント種別</summary>
        @event
    }

    /// <summary>
    /// YAML ファイルのトップレベルセクションキー
    /// ユーザーが YAML に記述するセクション名をここで一元管理する
    /// </summary>
    internal static class YamlSectionKeys
    {
        /// <summary>ステージ基本情報セクション</summary>
        internal const string Stages = "stages";

        /// <summary>ステージ説明（スカラー値）</summary>
        internal const string StageNotice = "stagenotice";

        /// <summary>配置可能ユニット一覧セクション</summary>
        internal const string ItemLists = "itemlists";

        /// <summary>パスマーカー配置セクション</summary>
        internal const string PathMakers = "pathmakers";

        /// <summary>ゴール条件セクション</summary>
        internal const string Goals = "goals";

        /// <summary>ゲームオーバー条件セクション</summary>
        internal const string GameOvers = "gameovers";

        /// <summary>タイマーイベントセクション</summary>
        internal const string Events = "events";

        /// <summary>ボード設定セクション</summary>
        internal const string Boards = "boards";
    }

    /// <summary>
    /// YAML フィールドで使用可能な特殊値キーワード
    /// ユーザーが YAML に記述するリテラル文字列をここで一元管理する
    /// </summary>
    internal static class YamlValueKeywords
    {
        /// <summary>Y 座標を Raycast で自動検出するキーワード（例: pos: 0, auto, 135）</summary>
        internal const string AutoHeight = "auto";
    }

    /// <summary>
    /// YAML でボード設定に使用可能なフィールド名
    /// </summary>
    internal enum BoardCommandFields
    {
        /// <summary>code - ボードコード（ReadMeText0 など）</summary>
        code,
        
        /// <summary>text - ボード表示テキスト</summary>
        text,
        
        /// <summary>pos - 立て看板の座標（オプション、"x, y, z" 形式）</summary>
        pos
    }
}
