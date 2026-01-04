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
        Extinguishing,
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
        Extinguishing,
    }


    internal enum LayerType
    {
        Ground,
    }

    internal enum UnitType
    {
        Player,
    }

    internal enum UIType
    {
        SpawnMarker,
    }

}
