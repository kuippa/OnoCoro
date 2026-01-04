using UnityEngine;
using CommonsUtility;
using System.Reflection;

public class ExtinguishingCtrl : MonoBehaviour
{
    // [SerializeField]
    private float _shoot_speed = 22f;
    private float _t = 0f;
    private const float _INTERVAL_TIME = 0.4f;
    private const float _MAX_SEARCH_RANGE = 30f; // 
    private const string _WATER_SPHERE_PARENT_NAME = "WaterSphere";

    private float _sinValue = 0f;
    private bool _reSearch = false;

    // private GameObject _hose;
    private GameObject _nozzle;
    private GameObject _target;
    private static int _waterUnitIndex = 0; // 静的変数でインデックスを管理
    private bool _isDelete = false; // 削除処理実行中かどうか


    private GameObject CreateWaterBullet()
    {
        // 3D 球を生成する
		GameObject prefab = Resources.Load<GameObject>("Prefabs/WorkUnit/WaterSphere");
        Vector3 setPoint = _nozzle.transform.position;
        // Quaternion hoseRotation = _nozzle.transform.rotation;
        // Vector3 offset = new Vector3(0, -0.2f, 0.2f);
        // setPoint += hoseRotation * offset;
        Quaternion setRotation = Quaternion.Euler(0, 0, 0);
        GameObject water_unit = Instantiate(prefab, setPoint, setRotation);
        water_unit.name = GameEnum.TagType.Water.ToString() + _waterUnitIndex++.ToString();
        water_unit.tag = GameEnum.TagType.Water.ToString();

        Transform parentTransform = GameObjectTreat.GetParentTransform(_WATER_SPHERE_PARENT_NAME);
        water_unit.transform.SetParent(parentTransform);

        return water_unit;
    }

    private void WaterShoot()
    {
        if (_sinValue > 360f)
        {
            _sinValue = 0f;
            _reSearch = true;
        }
        float sinValue = Mathf.Sin(_sinValue * Mathf.Deg2Rad);
        if (sinValue < 0)
        {
            return;
        }

        // 3D 球を生成する
        GameObject water_unit = CreateWaterBullet();


        Rigidbody rb = water_unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = water_unit.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // ターゲットの中心位置を計算
        Vector3 targetSize = new Vector3(
            0,
            _target.transform.localScale.y - 0.2f,  // 少し下目にする。根本から消火するため
            0
        );
        Vector3 targetCenter = _target.transform.position - targetSize;
        // 射出オブジェクトの中心位置を計算
        Vector3 waterUnitCenter = water_unit.transform.position;

        // 目標地点までの距離を計算
        Vector3 toTarget = targetCenter - waterUnitCenter;
        float distance = toTarget.magnitude - (0.6f + 1f*sinValue); // 少し手前に出す

        // 45度+αの角度で射出するための初速度を計算
        float angle = (45f+(20f*sinValue)) * Mathf.Deg2Rad;
        float gravity = Physics.gravity.magnitude;
        float initialVelocity = Mathf.Sqrt((distance * gravity) / Mathf.Sin(2 * angle));

        // 初速度ベクトルを計算
        Vector3 velocity = toTarget.normalized * initialVelocity * Mathf.Cos(angle);
        velocity.y = initialVelocity * Mathf.Sin(angle);

        // Rigidbodyに初速度を設定
        rb.linearVelocity = velocity;
    }

    private void LookRotateTarget()
    {
        if (_target == null || _target.transform == null)
        {
            return;
        }

        Vector3 direction = _target.transform.position - this.gameObject.transform.position;
        direction.y = 0; // Y軸方向を固定
        Quaternion rotation = Quaternion.LookRotation(direction);
        float rotationSpeed = 1f; // 回転速度 0-1の間で指定
        if (Quaternion.Angle(this.gameObject.transform.rotation, rotation) < 0.1f)
        {
            // 回転が完了したときにイベントを発生させる
            WaterShoot();
        }
        else
        {
            this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, rotation, rotationSpeed);
        }
    }

    // 一番近いターゲットを探す
    private bool SearchTargets()
    {
        if (_target != null && _reSearch == false)
        {
            return true;
        }
        _reSearch = false;
        GameObject[] targets = GameObject.FindGameObjectsWithTag(GameEnum.TagType.FireCube.ToString());
        if (targets.Length == 0)
        {
            return false;
        }

        float minDistance = _MAX_SEARCH_RANGE;
        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _target = target;
            }
        }
        return true;
    }

    public void CreateExtinguishingUnit(Vector3 setPoint)
    {
        this.gameObject.tag = GameEnum.TagType.Extinguishing.ToString();
        int idx = GameObjectTreat.IndexObjectByTag(this.gameObject.tag);
        this.gameObject.name = GameEnum.ModelsType.Extinguishing.ToString() + idx.ToString();
        this.gameObject.AddComponent<Extinguishing>();
        this.gameObject.GetComponent<Extinguishing>()._item_struct.ItemID = this.name;
        this.gameObject.GetComponent<Extinguishing>()._unit_struct.UnitID = this.name;
        this.gameObject.transform.position = setPoint;
    }

    internal void StartDeleteUnitProcess()
    {
        _isDelete = true;
    }

    internal void DeleteUnitProcess()
    {
        UnitStruct unitStruct = this.GetComponent<Extinguishing>().GetUnitStruct();
        ScoreCtrl.UpdateAndDisplayScore((int)unitStruct.DeleteCost, unitStruct.ScoreType);
        GameObjectTreat.DestroyAll(this.gameObject);
    }

    void OnDestroy()
    {
        // #if UNITY_EDITOR
        //     Debug.Log(this.GetType().FullName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        // #endif
        GameObjectTreat.DestroyAll(this.gameObject);
    }

    void Awake()
    {
        // _hose = this.gameObject.transform.Find("hose").gameObject;
        _nozzle = this.gameObject.transform.Find("ExtinguishingCylinder/nozzle").gameObject;

        GameObject unit = this.gameObject;
        unit.AddComponent<Extinguishing>();
        unit.GetComponent<Extinguishing>()._item_struct.ItemID = unit.name;
        unit.GetComponent<Extinguishing>()._unit_struct.UnitID = unit.name;


    }

    void Update()
    {
        _t += Time.deltaTime;        
        if (_t > _INTERVAL_TIME  && !_isDelete)
        {
            _sinValue += _shoot_speed;
            _t = 0;
            if (SearchTargets())
            {
                LookRotateTarget();
            }
        }

    }

}
