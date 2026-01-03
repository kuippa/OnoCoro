using UnityEngine;
using System.Collections.Generic;
using CommonsUtility;

public class PlateauCubeMaker : MonoBehaviour
{
    const float _CENTER_Y_OFFSET = 5.0f;
    const float _BUFFER_Y_OFFSET = 0.5f;

    internal void BreakUpBuildingCube(GameObject targetObj, int rebuildCost)
    {
        // rebuildCost 分のゴミを生成
        int total_score = 0;
        int i = 0;
        
        // MeshFilter meshFilter = targetObj.GetComponent<MeshFilter>();
        // if (meshFilter == null)
        // {
        //     return;
        // }
        Renderer renderer = targetObj.GetComponent<Renderer>();
        Vector3 center = renderer.bounds.center;
        Vector3 extents = renderer.bounds.extents;

        while (total_score < rebuildCost)
        {
            total_score += CreateGarbageRoundByAngle(center, extents, 24, i);
            i++;
            if (i > 100)
            {
                Debug.Log("i > 100");
                break;
            }
        }
    }

    private int CreateGarbageRoundByAngle(Vector3 center, Vector3 extents, int step, int i)
    {
        int total_score = 0;
        float r = GetRadius(extents);
        float angle = 360.0f / step;
        float radian = angle * Mathf.Deg2Rad;
        // for (int i = 0; i < step; i++)
        // {
            float x1 = r * Mathf.Cos(radian * i);
            float z1 = r * Mathf.Sin(radian * i);
            Vector3 pos = new Vector3(center.x + x1, center.y + _BUFFER_Y_OFFSET, center.z + z1);
            // GameObject garbage = GarbageCubeCtrl.SpawnGarbageCube(pos, GarbageCubeCtrl._SIZE_SMALL, true);
            GameObject garbage = GarbageCubeCtrl.SpawnGarbageCube(pos, GarbageCubeCtrl._SIZE_NORMAL, false);
            Collider collider = garbage.GetComponent<Collider>();
            if (collider != null)
            {
                total_score += ScoreCtrl.GetTotalGarbageScore(collider);
            }
//         }
        return total_score;
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

    private void GetMeshrenderInfo(GameObject targetObj, float height = _CENTER_Y_OFFSET)
    {
        MeshFilter meshFilter = targetObj.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Renderer renderer = targetObj.GetComponent<Renderer>();
            SetCubeMark(renderer, height);
            // CreateCubeRoundByAngle(renderer.bounds.center, renderer.bounds.extents, 16);
            CreateCubeRoundByArc(renderer.bounds.center, renderer.bounds.extents, 4);
        }
        else
        {
            Debug.Log("MeshFilter not found"+ targetObj.name);
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
        // 四隅をポイント
        Vector3 extents1 = renderer.bounds.extents;
        extents1 = center + extents1;
        extents1.y = center.y;
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

        // SetCube(extents1, Color.black);
        // SetCube(extents2, Color.cyan);
        // SetCube(extents3, Color.magenta);
        // SetCube(extents4, Color.yellow);
    }

    private void SetCubeAtCenter(Vector3 center, Color color, float height)
    {
        center.y += height  * 0.5f + _BUFFER_Y_OFFSET;
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
        float r = GetRadius(extents);
        float arc = 2.0f * Mathf.PI * r;
        int step = Mathf.FloorToInt(arc / interval);
        CreateCubeRoundByAngle(center, extents, step);
    }

    private void CreateCubeRoundByAngle(Vector3 center, Vector3 extents, int step)
    {
        float r = GetRadius(extents);
        float angle = 360.0f / step;
        float radian = angle * Mathf.Deg2Rad;
        for (int i = 0; i < step; i++)
        {
            float x1 = r * Mathf.Cos(radian * i);
            float z1 = r * Mathf.Sin(radian * i);
            Vector3 pos = new Vector3(center.x + x1, center.y + _BUFFER_Y_OFFSET, center.z + z1);
            SetCube(pos, Color.white);
            // SetBonFire(pos);
        }
    }

    private void SetMaterialColor(GameObject targetObj, Color color)
    {
        Renderer renderer = targetObj.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }
        if (renderer.materials.Length > 1)
        {
            // Debug.Log("material 複数パターン");
            return;
        }
        renderer.material.color = color;
    }

    private void SetCube(Vector3 setPosition, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = setPosition;
        SetMaterialColor(cube, color);
        cube.AddComponent<Rigidbody>();
        cube.GetComponent<Rigidbody>().useGravity = true;
        cube.tag = GameEnum.TagType.Garbage.ToString();
    }


}
