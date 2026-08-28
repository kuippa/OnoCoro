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
        
        /// <summary>ユニット配置イベント（デバッグモード用、mode: debug 時に処理）</summary>
        spawn_unit_debug,
        
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

        /// <summary>建物破壊＋倒壊した各建物から出火（地震連動火災。倒壊数を出火数に反映）</summary>
        building_break_fire,

        /// <summary>建物を解体して更地化し、延床面積×構造別原単位で算定した瓦礫を散布（CityHack 2026）</summary>
        building_demolish,
        
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
        
        /// <summary>パス上のユニット完全消滅時にパス強調表示解除（EventLoader で処理）</summary>
        off_bloom_path_complete,
        
        /// <summary>桜を咲かせるイベント（TreeSakura で処理）</summary>
        bloom_sakura,
        
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

        /// <summary>ルート名定義セクション（pathmakers の複数シーケンスを名前で参照可能にする）</summary>
        internal const string RouteNames = "routenames";

        /// <summary>ゴール条件セクション</summary>
        internal const string Goals = "goals";

        /// <summary>ゲームオーバー条件セクション</summary>
        internal const string GameOvers = "gameovers";

        /// <summary>タイマーイベントセクション</summary>
        internal const string Events = "events";

        /// <summary>ボード設定セクション</summary>
        internal const string Boards = "boards";

        /// <summary>年サイクル定義セクション（Season 3 ターンベース化）</summary>
        internal const string Years = "years";
    }

    /// <summary>
    /// YAML の years セクションで使用可能なフィールド名（Season 3）
    /// </summary>
    internal enum YearCommandFields
    {
        /// <summary>year - 年番号（1 始まり連番）</summary>
        year,

        /// <summary>duration - 年の長さ（秒）。経過で年終了</summary>
        duration,

        /// <summary>note - 年の説明（任意）</summary>
        note,

        /// <summary>schedule - スポーンパターンの編成リスト（任意）</summary>
        schedule,

        /// <summary>events - 年内の単発イベントリスト（任意）</summary>
        events,

        /// <summary>baseline - 消火なし時の想定火災延焼棟数（任意・W3 Task 4。未指定なら K×N）</summary>
        baseline
    }

    /// <summary>
    /// YAML の years.schedule エントリで使用可能な予約フィールド名（Season 3）
    /// これ以外のキーはすべてスロット束縛（スロット名: 値）として扱われる
    /// </summary>
    internal enum ScheduleCommandFields
    {
        /// <summary>pattern - 参照するスポーンパターン ID（patterns/*.yaml の pattern_id）</summary>
        pattern,

        /// <summary>at - 年内の開始オフセット（秒）</summary>
        at
    }

    /// <summary>
    /// スポーンパターンファイル（staging/patterns/*.yaml）のトップレベルキー（Season 3）
    /// </summary>
    internal static class YamlPatternKeys
    {
        /// <summary>パターン ID（システム内で一意）</summary>
        internal const string PatternId = "pattern_id";

        /// <summary>パターン内イベントリスト（time はパターン内相対秒）</summary>
        internal const string Events = "events";
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
