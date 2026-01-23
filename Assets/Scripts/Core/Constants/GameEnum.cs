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
    }

    internal enum ModelsType
    {
        GarbageCube,
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

}
