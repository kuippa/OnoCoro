using UnityEngine;

public class ExtinguishingCtrl : MonoBehaviour
{
    private float _t = 0f;
    private const float _INTERVAL_TIME = 0.8f;

    private GameObject _hose;
    private GameObject _target;


    private void WaterShoot()
    {
        // 消火
        Debug.Log("WaterShoot");

        // 3D 球を生成する
        GameObject water_unit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Vector3 setPoint = _hose.transform.position;
        setPoint.x += 0.1f;
        water_unit.transform.position = setPoint;

        Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
        water_unit.transform.localScale = scale;
        water_unit.name = "WaterUnit";
        // unit.water_unit = GameEnum.TagType.RainDrop.ToString();

        Rigidbody rb = water_unit.AddComponent<Rigidbody>();
        Vector3 direction = (_target.transform.position - this.gameObject.transform.position).normalized;
        // direction.y += 0.3f; // 少し放物線を描く
        float force = 15f; // 力の大きさを調整
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void LookRotateTarget()
    {
        Vector3 direction = _target.transform.position - this.gameObject.transform.position;
        direction.y = 0; // Y軸方向を固定
        Quaternion rotation = Quaternion.LookRotation(direction);
        // float rotationSpeed = Time.deltaTime * 1.0f; // 回転速度 0-1の間で指定
        // float rotationSpeed = 0.2f; // 回転速度 0-1の間で指定
        float rotationSpeed = 1f; // 回転速度 0-1の間で指定
        // 回転が完了したかチェック
        if (Quaternion.Angle(this.gameObject.transform.rotation, rotation) < 0.1f)
        {
            // 回転が完了したときにイベントを発生させる
            WaterShoot();
        }
        else
        {
            // Debug.Log("LookRotateTarget" + rotationSpeed);
            this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, rotation, rotationSpeed);
        }

    }

    private bool SearchTargets()
    {
        _target = null;
        GameObject[] targets = GameObject.FindGameObjectsWithTag(GameEnum.TagType.FireCube.ToString());

        if (targets.Length == 0)
        {
            return false;
        }
        // 一番近いターゲットを探す
        float minDistance = 1000f;
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


    void Awake()
    {
        // _target = GameObject.FindGameObjectWithTag("Player");
        // hose = GameObject.Find("Hose");
        // 子どもオブジェクトのhoseを取得
        _hose = this.gameObject.transform.Find("hose").gameObject;

    }

    void Update()
    {
        // 一定間隔で子供要素を生成
        _t += Time.deltaTime;        
        if (_t > _INTERVAL_TIME)
        {
            _t = 0;
            if (SearchTargets())
            {
                LookRotateTarget();
            }
        }

    }

}
