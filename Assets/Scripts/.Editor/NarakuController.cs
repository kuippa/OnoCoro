// using System.Collections;
// using System.Collections.Generic;
using System.Linq;
// using Unity.VisualScripting;
using UnityEngine;
using Debug = CommonsUtility.Debug;
// using UnityEngine.UI;
// using UnityEngine.Rendering;
using CommonsUtility;
using StarterAssets;
// using UnityEngine.UIElements;
// using UnityEngine.Events;
// using System.Runtime;
// using NUnit.Framework;
// using Unity.Android.Types;

/// <summary>
/// Naraku（奈落）システムの管理
/// 責務：Naraku ウィンドウの初期化と状態管理のみ
/// トリガー処理は NarakuTriggerHandler に委譲
/// </summary>
public class NarakuController : MonoBehaviour
{
    private const float _NARAKU_DISTANCE = 30f;  // 奈落ごとの距離
    private static readonly Vector2 _NARAKU_BASIC_SIZE = new Vector2(1500f, 15f);   // ナラクの基本サイズ
    private Vector3 _dem_center_pos = Vector3.zero;
    private GameObject _dem = null;   // DEM(Digital Elevation Model) 航空レーザ測量 地形データ

    private void InitWindow()
    {
        GameObject naraku = this.gameObject;
        if (naraku == null)
        {
            // Debug.Log("InitWindow " + "naraku is null");
        }
        _dem = DemController.GetDemObject();
        ChangeMaterialUVToPlanar(_dem); // マテリアルのUVをPlanarに変更
        RectTransform naraku_rect = naraku.GetComponent<RectTransform>();
        Vector3 setPos = naraku_rect.anchoredPosition;
        naraku_rect.sizeDelta = _NARAKU_BASIC_SIZE;
        Transform jigoku = transform.parent;
        jigoku.position = _dem_center_pos;
        int naraku_idx = GetCurrentNarakuIndex();
        float dem_height = DemController.GetDemHeight(_dem);

        // Debug.Log("InitWindow " + "dem_height:" + dem_height + " _NARAKU_DISTANCE:" + _NARAKU_DISTANCE + " naraku_idx:" + naraku_idx + "_dem_center_pos.y:" + _dem_center_pos.y);
        setPos = new Vector3(_dem_center_pos.x, _dem_center_pos.y - dem_height - (_NARAKU_DISTANCE * naraku_idx), _dem_center_pos.z);
        naraku_rect.anchoredPosition = setPos;
    }

    private void ChangeMaterialUVToPlanar(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.Log("ChangeMaterialUVToPlanar " + "targetObject is null");
            return;
        }

        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.Log("No Renderer component found on " + targetObject.name);
            return;
        }
        Material material = renderer.material;
        if (material == null)
        {
            Debug.Log("No material found on " + targetObject.name);
            return;
        }            

        // material.shader.name == "HDRP/Lit"
        // Debug.Log("Material shader name " + material.shader.name);
        // UV Mapping modeを変更
        // [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Planar, 4, Triplanar, 5)] _UVBase("UV Set for base", Float) = 0
        // _UVDetailsMappingMask

        var newUVMapping = 4f;  // 4はPlanar mappingを表す
        float currentUVMapping = material.GetFloat("_UVBase");
        // Debug.Log("Material UV mapping changed to Planar for " + targetObject.name + " currentUVMapping:" + currentUVMapping);
        if (currentUVMapping == newUVMapping)
        {
            return;
        }
        material.SetFloat("_UVBase", 4f);
        // currentUVMapping = material.GetFloat("_UVBase");
        // Debug.Log("Material UV mapping changed to Planar for " + targetObject.name + " currentUVMapping:" + currentUVMapping);
    }

    private void SetDemUV(GameObject dem)
    {
        // 未実装：DEMのUVを地形高さに応じて変更
        // 詳細は InitWindow の ChangeMaterialUVToPlanar を参照
    }

    private int GetCurrentNarakuIndex()
    {
        // 全てのNarakuタグを持つオブジェクトを取得
        GameObject[] narakuObjects = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Naraku.ToString());
        // 現在のオブジェクトのインデックスを探す
        for (int i = 0; i < narakuObjects.Length; i++)
        {
            if (narakuObjects[i] == this.gameObject)
            {
                this.gameObject.name = "Naraku_" + i;
                return i + 1;
            }
        }
        return 1;
    }

    void Update()
    {
        // TODO: 一定時間経過後、ナラク以下まで落ちているオブジェクトがあれば削除する
    }
}
