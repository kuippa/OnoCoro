using UnityEngine;
using UnityEditor;
using System.Collections;
using CommonsUtility;

public class RainDropsCtrl : MonoBehaviour
{
// puddle については、雨が降るときに生成するようにする

    private Vector3 _rainDropPosition = Vector3.zero;
    private bool _is_rain = true;
    private GameObject _puddles_holder = null;
    private float _checkRadius = 10f; // チェックする半径
    private int _requiredRainCount = 5; // 必要なRainタグのオブジェクト数
    private int _check_count = 0; // チェック回数
    private const float RAIN_CHECK = 5.0f;
    private const float STATIONARY_DISTANCE = 5f;    // 雨粒が同じところに留まっている距離
    private const float PUDDLE_ABOVE_DISTANCE = 0.25f;    // 地面からの距離
    private const int PUDDLE_MAX_COUNT = 10;        // 水たまりの最大数
    private const int MAX_CHECK_COUNT = 10;         // 判定回数が上限に達したら雨粒を消す

    private bool MakePuddle()
    {
        // 生成する水たまりの数を数える
        float rainDropCount = 0;
        GameObject[] rainDrops = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Puddle.ToString());
        rainDropCount = rainDrops.Length;

        // 15個以上の水たまりはエラー発生のため生成しない 
        // IndexOutOfRangeException: Index was outside the bounds of the array. が発生するため
        if (rainDropCount > PUDDLE_MAX_COUNT)
        {
            return false;
        }

        GameObject prefab = PrefabManager.GetPrefab(PrefabManager.PrefabType.Puddle);
        if (prefab == null)
        {
            Debug.LogWarning("Puddle prefab が見つかりません");
            return false;
        }
        Vector3 setPoint = DemController.GetDemAbovePosition(this.gameObject, PUDDLE_ABOVE_DISTANCE);
        Quaternion setRotation = Quaternion.Euler(0, 0, 0);
        GameObject unit = Instantiate(prefab, setPoint, setRotation);
        unit.tag = GameEnum.TagType.Puddle.ToString();
        unit.name = GameEnum.TagType.Puddle.ToString() + Time.time.ToString();
        unit.transform.SetParent(GetPuddleHolder().transform);

        // Debug.Log("MakePuddle" + this.name + " " + rainDropCount);
        return true;
    }


    private GameObject GetPuddleHolder()
    {
        if (_puddles_holder == null)
        {
            _puddles_holder = GameObject.Find("puddles");
            if (_puddles_holder == null)
            {
                _puddles_holder = new GameObject("puddles");
            }
        }
        return _puddles_holder;
    }

    private bool CheckExistRainsInRadius()
    {
        bool ret = false;
        // 一定の距離内にあるオブジェクトを取得
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, _checkRadius);
        int rainCount = 0;

        // 取得したオブジェクトのタグをチェック
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(GameEnum.TagType.RainDrop.ToString()))
            {
                rainCount++;
            }
        }

        // Rainタグのオブジェクトが5個以上ある場合の処理
        if (rainCount >= _requiredRainCount)
        {
            // Debug.Log("一定の距離内にRainタグのオブジェクトが" + rainCount + "個あります。");
            ret = true;
        }
        return ret;
    }

    private void OnTriggerCheck()
    {
        _check_count++;
        if ((float)_check_count > MAX_CHECK_COUNT * GameSpeedCtrl.GetGameSpeed())
        {
            _is_rain = false;
            GameObjectTreat.DestroyAll(this.gameObject);
            return;
        }

        if (!this.gameObject.activeSelf)
        {
            _is_rain = false;
            return;
        }

        if (_rainDropPosition != Vector3.zero && Vector3.Distance(_rainDropPosition, this.gameObject.transform.position) <= STATIONARY_DISTANCE)
        {
            if (GameConfig._STAGE_PADDLE_MODE && CheckExistRainsInRadius())
            {
                // Debug.Log("同じ場所に一定時間停止" + this.name);
                if (MakePuddle())
                {
                    _is_rain = false;
                    GameObjectTreat.DestroyAll(this.gameObject);
                }
                return;
            }
            if (GameConfig._STAGE_RAIN_ABSORB_MODE)
            {
                ChangeColliderSize();
            }
        }

        _rainDropPosition = this.gameObject.transform.position;
        // Debug.Log("OnTriggerCheck" + this.name);
    }

    private void ChangeColliderSize()
    {
        Transform absorbColliderTransform = this.gameObject.transform.Find("absorbcollider");
        if (absorbColliderTransform == null)
        {
            Debug.LogWarning("absorbcollider が見つかりません: " + this.name);
            return;
        }

        RainAbsorbCtrl rainAbsorbCtrl = absorbColliderTransform.gameObject.GetComponent<RainAbsorbCtrl>();
        if (rainAbsorbCtrl == null)
        {
            Debug.LogWarning("RainAbsorbCtrl コンポーネントが見つかりません: " + absorbColliderTransform.name);
            return;
        }

        rainAbsorbCtrl.ChangeColliderSize(0.7f);
    }

    IEnumerator RainDrops()
    {
        while (_is_rain)
        {
            yield return new WaitForSeconds(RAIN_CHECK);        
            OnTriggerCheck();
        }

    }

    void Awake()
    {
        // RainDrops();
    }

    void Start()
    {
        StartCoroutine(RainDrops());        
    }

}
