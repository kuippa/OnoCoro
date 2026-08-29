using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnum
{
    internal enum TagType
    {
        Garbage,
        PowerCube,
        Litter,
        EnemyLitters,
        TowerSweeper,
        TowerDock,
        Player,
        Ground,
        Naraku,
        Untagged,
        RainDrop,
        Puddle,
        StopPlate,
        FireCube,
        Ash,
        Water,
        WaterTurret,
        DustBox,
        SentryGuard,
        Holder,
        PathBloom,
        Hydrant,
        Cistern,
        Cat,            // 巨大猫（CityHack 2026: 経路上の建物を解体する敵ユニット）
        GarbageNoBurn,  // 不燃ゴミ（CityHack 2026: コンクリートがら・金属など）
    }

    internal enum ModelsType
    {
        GarbageCube,
        GarbageCubeNoBurn,
        GarbageCubeBox,
        GarbageCubeBig,
        Litter,
        Sweeper,
        PowerCube,
        StopPlate,
        FireCube,
        WaterTurret,
        DustBox,
        SentryGuard,

        // 防災施策（Season 3 W2: 統計効果型インフラ）
        Hydrant,   // 消火栓
        Cistern,   // 防火水槽
        Plaza,     // 避難広場

        // CityHack 2026
        Cat,       // 巨大猫（経路上の建物を解体する敵ユニット）
    }

    internal enum LayerType
    {
        Ground,
        AreaIgnoreRaycast,
    }

    internal enum UnitType
    {
        Player,
    }

    internal enum UIType
    {
        SpawnMarker,
    }

    /// <summary>
    /// Path Marker の名前に含まれる識別文字列定数
    /// YAML で定義される path_marker_start, path_marker_goal の命名規則に対応
    /// </summary>
    internal static class PathMarkerNameParts
    {
        internal const string START = "start";
        internal const string GOAL = "goal";

        internal const string ALL = "all";
    }

    /// <summary>
    /// シーン内の重要なゲームオブジェクト名の定数化
    /// GameObject.Find() の対象になるルートレベルのオブジェクト名を一元管理
    /// </summary>
    internal static class GameObjectNames
    {
        internal const string GAME_PREFABS = "GamePrefabs";
        internal const string PLAYER_ARMATURE = "PlayerArmature";
    }

}
