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
    /// <param name="targetAmount">DemolitionSystem が算定した瓦礫の目標量（建物種別の係数込み）</param>
    /// <param name="maxCubes">生成するキューブ数の上限（物理負荷の安全弁。YAML の max_cubes）</param>
    /// <param name="noncombustibleRatio">不燃物の割合（0.0〜1.0。YAML の noncombustible_ratio）</param>
    internal void ScatterDemolitionDebris(GameObject targetObj, int targetAmount, int maxCubes, float noncombustibleRatio)
    {
        Renderer renderer = targetObj.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        // [修正 2026-08-28] 建物の外周に円状へばらまくのをやめ、建物跡地の中央に集約する。
        // 生成は既存の CreateGarbageCubeSmall（小サイズ・GarbageCubeSpawner 経由）を共用し、
        // 位置のばらつきは GarbageCubeFactory の isSwayingPoint（±2m）が担当する
        Vector3 center = renderer.bounds.center;
        Vector3 spawnPoint = new Vector3(center.x, center.y + _DEBRIS_SPAWN_HEIGHT, center.z);

        int accumulated = 0;
        int spawnCount = 0;

        while (accumulated < targetAmount)
        {
            // 不燃率に応じて可燃・不燃を振り分ける。
            // 1 個ずつ抽選するので、まばらに混ざって山ができる
            bool isNoBurn = Utility.fRandomRange(0f, 1f) < noncombustibleRatio;
            accumulated += CreateGarbageCubeSmall(spawnPoint, isNoBurn);
            spawnCount++;
            if (spawnCount >= maxCubes)
            {
                // [重要] ここで打ち切ると、YAML の tons_per_sqm / debris_per_ton を
                // いくら増やしても見た目が変わらなくなる。黙って頭打ちにならないよう警告を出す
                Debug.LogWarning(
                    $"[Demolition] {targetObj.name}: 瓦礫が上限 {maxCubes} 個に達したため打ち切りました"
                    + $"（目標 {targetAmount} / 生成 {accumulated}）。"
                    + "さらに増やすには YAML の max_cubes を上げてください");
                break;
            }
        }
    }

    /// <summary>瓦礫のスポーン高さ（建物 bounds 中心からの相対。既存実装と同値）</summary>
    private const float _DEBRIS_SPAWN_HEIGHT = 0.5f;

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

    private int CreateGarbageCubeSmall(Vector3 pos, bool isNoBurn = false)
    {
        GameObject gameManagerObject = GameObjectTreat.GetGameManagerObject();
        GarbageCubeSpawner garbageCubeSpawner = gameManagerObject.GetComponent<GarbageCubeSpawner>();
        if (garbageCubeSpawner == null)
        {
            garbageCubeSpawner = gameManagerObject.AddComponent<GarbageCubeSpawner>();
        }
        garbageCubeSpawner.SpawnGarbageCubeAsync(pos, 1, isSwayingPoint: true, isNoBurn: isNoBurn);

        // 可燃・不燃で基準スコアは同値。換算式を共通に保つため
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
