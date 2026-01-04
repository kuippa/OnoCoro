using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnum
{

    // TODO: Utilityに移動
    internal enum EnemyType
    {
        Garbage,
        // Litter,
        EnemyLitters,
        Player,
        Ground,
        Big,
        Boss,
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




    // internal static EnemyType GetEnemyType(string enemyType)
    // {
    //     switch (enemyType)
    //     {
    //         // case "Litter":
    //             // return EnemyType.Litter;
    //         case "Garbage":
    //             return EnemyType.Garbage;
    //         case "EnemyLitters":
    //             return EnemyType.EnemyLitters;
    //         case "Big":
    //             return EnemyType.Big;
    //         case "Boss":
    //             return EnemyType.Boss;
    //         default:
    //             return EnemyType.Garbage;
    //     }
    // }


}
