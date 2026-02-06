using CommonsUtility;
using UnityEngine;
using Debug = CommonsUtility.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 敵ユニットスポーン イベント
    /// 
    /// シリアライズフィールドで指定された敵ユニット名に基づいて
    /// 敵ユニットをスポーンする Environmental イベントコンポーネント
    /// 
    /// 機能：
    /// - シリアライズフィールドで敵ユニット名を設定
    /// - マーカー位置で敵のスポーン地点を指定（複数指定可）
    /// - Start() で自動的に敵がスポーンされる
    /// 
    /// 使用例：
    /// 1. GameObject (例: "EnemySpawner") に本コンポーネントをアタッチ
    /// 2. インスペクターで敵ユニット名（例: "Enemy_01"）を設定
    /// 3. スポーン地点を示すマーカー（空の GameObject）を配置
    /// 4. マーカー名の配列をインスペクターで設定
    /// 5. ゲーム実行時に自動的に敵がスポーンされる
    /// </summary>
    public class EventSpawn : MonoBehaviour
    {
        [SerializeField]
        private GameEnum.ModelsType _enemyUnitType = GameEnum.ModelsType.GarbageCubeBox;
        
        [SerializeField]
        private Vector3 _spawnPointOffset = new Vector3(0f, 1.5f, 0f);
        
        // [SerializeField]
        // private string[] _markerNames = new string[0];
        
        [SerializeField]
        private bool _spawnOnStart = false;
        
        private void Start()
        {
            if (_spawnOnStart)
            {
                if (SpawnEnemyUnit())
                {
                    Destroy(this);
                }
            }
        }


        internal void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == GameEnum.UnitType.Player.ToString())
            {
                if (SpawnEnemyUnit())
                {
                    Destroy(this);
                }
            }
        }

        /// <summary>
        /// ユニットをスポーン
        /// EventLoader.SpawnEnemyUnit() SpawnUnit()と同じロジック
        /// </summary>
        private bool SpawnEnemyUnit()
        {
            SpawnController spawnCtrl = GameObjectTreat.GetSpawnController();
            if (spawnCtrl == null)
            {
                return false;
            }
            
            string unitName = _enemyUnitType.ToString();
            Vector3 spawnPoint = this.transform.position + _spawnPointOffset;
            spawnCtrl.CallUnitByName(unitName, spawnPoint);

            if (EventLogCtrl.Instance != null)
            {
                EventLogCtrl.Instance.ShowEventLog($"SpawnEnemyUnit:{unitName}");
            }

            return true;
        }
    }
}
