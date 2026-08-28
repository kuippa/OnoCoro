using System;
using System.Collections.Generic;
using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

public class PlateauCubeMaker : MonoBehaviour
{
    private const float _CENTER_Y_OFFSET = 5f;
    private const float _BUFFER_Y_OFFSET = 0.5f;
    private const int _MAX_GARBAGE = 200;
    private const int _BURNING_BOOST = 10;

    private static int GetAngleSpacing(int rebuildCost)
    {
        int num = 1;
        if (rebuildCost < 100)
        {
            return 93;
        }
        if (rebuildCost < 200)
        {
            return 43;
        }
        return 23;
    }

    internal void BreakUpBuildingCube(GameObject targetObj, int rebuildCost)
    {
        int num = 0;
        int num2 = 0;
        int num3 = 1;
        Renderer component = targetObj.GetComponent<Renderer>();
        Vector3 center = component.bounds.center;
        Vector3 extents = component.bounds.extents;
        rebuildCost *= 10;
        num3 = GetAngleSpacing(rebuildCost);
        while (num < rebuildCost)
        {
            num += CreateGarbageRoundByAngle(center, extents, num3, num2);
            num2++;
            if (num2 > 200)
            {
                break;
            }
        }
    }

    /// <summary>
    /// 解体廃棄物として、建物の跡地に指定個数の瓦礫を散布する（CityHack 2026）
    ///
    /// 既存の BreakUpBuildingCube は「再建コスト分のスコアが貯まるまで」撒くのに対し、
    /// こちらは DemolitionSystem が算定した廃棄物量に比例した個数を撒く。
    /// 建物は更地化されるため、跡地（建物のフットプリント内側）にも瓦礫を積む
    /// </summary>
    internal void ScatterDemolitionDebris(GameObject targetObj, float debrisTons)
    {
        Renderer renderer = targetObj.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        // [修正 2026-08-28] 既存の BreakUpBuildingCube と同じ方式に揃える。
        // スポーンは建物中心・高さ center.y + 0.5（実際のばらつきは isSwayingPoint が担当）。
        // 独自の接地 Raycast や山積み計算はやめ、既存挙動と揃えることで奈落落ちを防ぐ
        Vector3 center = renderer.bounds.center;
        Vector3 spawnPoint = new Vector3(center.x, center.y + _DEBRIS_SPAWN_HEIGHT, center.z);

        // サイズをバラけさせつつ、ScoreCtrl.GetTotalGarbageScore（サイズから量を算出）を
        // 積算して目標量に達するまで撒く → サイズが変わっても総量が揃う
        int targetAmount = Mathf.CeilToInt(debrisTons * _DEBRIS_AMOUNT_PER_TON);
        int accumulated = 0;
        int spawnCount = 0;

        while (accumulated < targetAmount)
        {
            accumulated += CreateDemolitionDebrisCube(spawnPoint);
            spawnCount++;
            if (spawnCount > _MAX_DEBRIS_SPAWN_COUNT)
            {
                break;
            }
        }
    }

    /// <summary>瓦礫のスポーン高さ（建物 bounds 中心からの相対。既存 BreakUpBuildingCube と同値）</summary>
    private const float _DEBRIS_SPAWN_HEIGHT = 0.5f;

    /// <summary>廃棄物 1t あたりの目標量（GetTotalGarbageScore の積算単位）</summary>
    private const float _DEBRIS_AMOUNT_PER_TON = 1.0f;

    /// <summary>1 棟あたりの瓦礫スポーン回数の上限（処理落ち防止。既存実装と同じ 200）</summary>
    private const int _MAX_DEBRIS_SPAWN_COUNT = 200;

    /// <summary>
    /// 解体廃棄物の瓦礫を 1 個生成し、その量（サイズ由来のスコア）を返す。
    ///
    /// 既存の CreateGarbageCubeNormal と同じ同期生成で、
    /// ScoreCtrl.GetTotalGarbageScore がキューブのサイズから量を算出する。
    /// これによりサイズをランダムに散らしても、積算した総量は目標に揃う。
    /// サイズは通常（0.3m）と大（1.5〜3.0m）を混ぜ、解体瓦礫として視認できる大きさにする
    /// </summary>
    private int CreateDemolitionDebrisCube(Vector3 pos)
    {
        int sizeFlag = GarbageCubeFactory._SIZE_BIG;
        if (Utility.fRandomRange(0f, 1f) < _DEBRIS_SMALL_RATIO)
        {
            sizeFlag = GarbageCubeFactory._SIZE_NORMAL;
        }

        GameObject unit = GarbageCubeFactory.SpawnGarbageCube(pos, sizeFlag, isSwayingPoint: true);
        if (unit == null)
        {
            return GarbageCube.GetBaseScore();  // 生成失敗時も無限ループにしない
        }

        Collider collider = unit.GetComponent<Collider>();
        if (collider == null)
        {
            return GarbageCube.GetBaseScore();
        }
        return ScoreCtrl.GetTotalGarbageScore(collider);
    }

    /// <summary>小さい破片を混ぜる割合</summary>
    private const float _DEBRIS_SMALL_RATIO = 0.3f;

    private int CreateGarbageRoundByAngle(Vector3 center, Vector3 extents, int step, int i)
    {
        float radius = GetRadius(extents);
        float num = (float)step * (MathF.PI / 180f);
        float num2 = radius * Mathf.Cos(num * (float)i);
        float num3 = radius * Mathf.Sin(num * (float)i);
        float num4 = Mathf.PerlinNoise(num2 * 0.1f, num3 * 0.1f) * 0.5f;
        num2 += num4;
        num3 += num4;
        Vector3 pos = new Vector3(center.x + num2, center.y + 0.5f, center.z + num3);
        return CreateGarbageCubeSmall(pos);
    }

    private int CreateGarbageCubeNormal(Vector3 pos)
    {
        int num = 0;
        Collider component = GarbageCubeFactory.SpawnGarbageCube(pos).GetComponent<Collider>();
        if (component != null)
        {
            num += ScoreCtrl.GetTotalGarbageScore(component);
        }
        return num;
    }

    private int CreateGarbageCubeSmall(Vector3 pos)
    {
        GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
        GarbageCubeSpawner garbageCubeSpawner = gameManagerObject.GetComponent<GarbageCubeSpawner>();
        if (garbageCubeSpawner == null)
        {
            garbageCubeSpawner = gameManagerObject.AddComponent<GarbageCubeSpawner>();
        }
        garbageCubeSpawner.SpawnGarbageCubeAsync(pos, 1, isSwayingPoint: true);
        return GarbageCube.GetBaseScore();
    }

    internal void DispCubeMarker(GameObject gameObject, Dictionary<string, string> dictInfo)
    {
            float height = GetMesuredHeight(dictInfo);
            GetMeshrenderInfo(gameObject, height);
    }

    private float GetMesuredHeight(Dictionary<string, string> dictInfo)
    {
        float height = 0; 
        // bldg:measuredheight, value: 7.2
        if (dictInfo.ContainsKey("bldg:measuredheight"))
        {
            height = float.Parse(dictInfo["bldg:measuredheight"]);
        }
        return height;
    }

    private void GetMeshrenderInfo(GameObject targetObj, float height = 5f)
    {
        if (targetObj.GetComponent<MeshFilter>() != null)
        {
            Renderer component = targetObj.GetComponent<Renderer>();
            SetCubeMark(component, height);
            CreateCubeRoundByArc(component.bounds.center, component.bounds.extents, 4);
        }
        else
        {
            Debug.Log("MeshFilter not found" + targetObj.name);
        }
    }

    private void SetCubeMark(Renderer renderer, float height)
    {
        Vector3 center = renderer.bounds.center;
        SetCubeAtCenter(center, Color.blue, height);
        SetCubeAtCorner(renderer);
    }

    private void SetCubeAtCorner(Renderer renderer)
    {
        Vector3 center = renderer.bounds.center;
        Vector3 extents = renderer.bounds.extents;
        extents = center + extents;
        extents.y = center.y;
        Vector3 extents2 = renderer.bounds.extents;
        extents2 = center - extents2;
        extents2.y = center.y;
        Vector3 extents3 = renderer.bounds.extents;
        extents3.x = center.x + extents3.x;
        extents3.z = center.z - extents3.z;
        extents3.y = center.y;
        Vector3 extents4 = renderer.bounds.extents;
        extents4.x = center.x - extents4.x;
        extents4.z = center.z + extents4.z;
        extents4.y = center.y;
        SetCube(extents, Color.black);
        SetCube(extents2, Color.cyan);
        SetCube(extents3, Color.magenta);
        SetCube(extents4, Color.yellow);
    }

    private void SetCubeAtCenter(Vector3 center, Color color, float height)
    {
        center.y += height * 0.5f + 0.5f;
        SetCube(center, color);
    }


    private float GetRadius(Vector3 extents)
    {
        float x = extents.x;
        float z = extents.z;
        float r = Mathf.Sqrt(x * x + z * z);
        return r;
    }

    private void CreateCubeRoundByArc(Vector3 center, Vector3 extents, int interval)
    {
        float radius = GetRadius(extents);
        int step = Mathf.FloorToInt(MathF.PI * 2f * radius / (float)interval);
        CreateCubeRoundByAngle(center, extents, step);
    }

    private void CreateCubeRoundByAngle(Vector3 center, Vector3 extents, int step)
    {
        float radius = GetRadius(extents);
        float num = 360f / (float)step * (MathF.PI / 180f);
        for (int i = 0; i < step; i++)
        {
            float num2 = radius * Mathf.Cos(num * (float)i);
            float num3 = radius * Mathf.Sin(num * (float)i);
            Vector3 setPosition = new Vector3(center.x + num2, center.y + 0.5f, center.z + num3);
            SetCube(setPosition, Color.white);
        }
    }

    private void SetMaterialColor(GameObject targetObj, Color color)
    {
        Renderer component = targetObj.GetComponent<Renderer>();
        if (!(component == null) && component.materials.Length <= 1)
        {
            component.material.color = color;
        }
    }

    private void SetCube(Vector3 setPosition, Color color)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.transform.position = setPosition;
        SetMaterialColor(gameObject, color);
        gameObject.AddComponent<Rigidbody>();
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        gameObject.tag = GameEnum.TagType.Garbage.ToString();
    }
}
