using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonsUtility;
using StarterAssets;
using Debug = UnityEngine.Debug;

namespace CommonsUtility
{
    /// <summary>
    /// 奈落（Naraku）のトリガーハンドラー
    /// 複数の異なるオブジェクト入場パターンに対応
    /// TriggerEnter/Exit を直接オーバーライドして独自ロジックを実装
    /// </summary>
    public class NarakuTriggerHandler : TriggerHandler
    {
        private const float _POPUP_PLAYER_DISTANCE = 30f;   // 落ちたプレイヤーを上に持ち上げる距離
        private const float _NARAKU_DISTANCE = 30f;         // Naraku ごとの Y 間隔（NarakuController と同値）
        private const string _PLAYER_ARMATURE_NAME = "PlayerArmature";
        
        private GameObject _eventSystem = null;
        private WaterSurfaceManager _waterSurface = null;
        private HashSet<Collider> _collidersInTrigger = new HashSet<Collider>();

        protected override void Awake()
        {
            base.Awake();
            // Naraku は特定の単一タグではなく、複数パターンに対応するため、デフォルトタグを設定
            SetDefaultTargetTag(GameEnum.TagType.Naraku.ToString());
            AdjustYPositionByPlayerSpawn();
        }

        /// <summary>
        /// Ground タグのY座標を取得（優先度1）
        /// すべての Ground オブジェクト中で最大 Y 値を返す
        /// </summary>
        private float GetGroundBaselineY()
        {
            GameObject[] groundObjects = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Ground.ToString());
            if (groundObjects == null || groundObjects.Length == 0)
            {
                return float.MinValue;
            }

            float maxY = float.MinValue;
            foreach (GameObject ground in groundObjects)
            {
                if (ground != null)
                {
                    float groundY = ground.transform.position.y;
                    if (groundY > maxY)
                    {
                        maxY = groundY;
                    }
                }
            }

            return maxY;
        }

        /// <summary>
        /// PlayerArmature のY座標を取得（優先度2・フォールバック）
        /// </summary>
        private float GetPlayerArmatureY()
        {
            GameObject playerArmature = GameObject.Find(_PLAYER_ARMATURE_NAME);
            if (playerArmature == null)
            {
                return float.MinValue;
            }

            return playerArmature.transform.position.y;
        }

        /// <summary>
        /// プレイヤーの初期スポーン Y 座標をもとに、この Naraku の Y 位置を調整する。
        /// Ground タグのY座標を優先、存在しない場合は PlayerArmature を使用
        /// (narakuIdx + 1) で必ず _NARAKU_DISTANCE 分下に配置する
        /// </summary>
        private void AdjustYPositionByPlayerSpawn()
        {
            // ステップ 1: Ground Y座標を取得（優先）
            float baselineY = GetGroundBaselineY();
            if (baselineY == float.MinValue)
            {
                // Ground が見つからない場合は PlayerArmature を使用（フォールバック）
                baselineY = GetPlayerArmatureY();
                if (baselineY == float.MinValue)
                {
                    // Debug.LogWarning($"[NarakuTriggerHandler.AdjustYPositionByPlayerSpawn] Ground も PlayerArmature も見つかりません。{gameObject.name} の Y 位置を調整しません。");
                    return;
                }
            }

            int narakuIdx = GetNarakuIndex();
            // (narakuIdx + 1) で必ず _NARAKU_DISTANCE 分下に配置
            float targetY = baselineY - (_NARAKU_DISTANCE * narakuIdx);

            Vector3 currentPos = transform.position;
            currentPos.y = targetY;
            transform.position = currentPos;

            // Debug.Log($"[NarakuTriggerHandler.AdjustYPositionByPlayerSpawn] {gameObject.name} Y 位置を調整: {targetY:F1} (基準Y: {baselineY:F1}, Idx: {narakuIdx}, 計算: {baselineY:F1} - (30 * {narakuIdx + 1}))");
        }

        /// <summary>
        /// Naraku タグを持つ全オブジェクト中でのインデックスを返す（1始まり）。
        /// オブジェクトを名前でソートして順番を決定
        /// 見つからない場合は 1 を返す。
        /// </summary>
        private int GetNarakuIndex()
        {
            GameObject[] narakuObjects = GameObject.FindGameObjectsWithTag(GameEnum.TagType.Naraku.ToString());
            
            if (narakuObjects == null || narakuObjects.Length == 0)
            {
                return 1;
            }
            
            // 名前でソート
            System.Array.Sort(narakuObjects, (a, b) => a.name.CompareTo(b.name));
            
            for (int i = 0; i < narakuObjects.Length; i++)
            {
                if (narakuObjects[i] == this.gameObject)
                {
                    return i + 1;
                }
            }
            return 1;
        }

        protected override void OnTargetEnter()
        {
            // 使用しない - OnTriggerEnter で直接処理
        }

        protected override void OnTargetExit()
        {
            // 使用しない - OnTriggerExit で直接処理
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnTriggerEnter] Collider is null");
                return;
            }

            string otherTag = other.gameObject.tag;
            bool wasAdded = _collidersInTrigger.Add(other);
            
            if (!wasAdded)
            {
                return;  // 既に処理済み
            }

            // タグに応じた処理を振り分け
            if (otherTag == GameEnum.TagType.Player.ToString())
            {
                OnPlayerEnter(other);
            }
            else if (otherTag == GameEnum.TagType.FireCube.ToString() || otherTag == GameEnum.TagType.Ash.ToString())
            {
                OnDestructibleEnter(other);
            }
            else if (otherTag == GameEnum.TagType.RainDrop.ToString())
            {
                OnRainDropEnter(other);
            }
            else if (otherTag == GameEnum.TagType.Water.ToString())
            {
                OnWaterEnter(other);
            }
            else if (otherTag == GameEnum.TagType.Ground.ToString())
            {
                // Ground タグは特に処理しない
            }
            else
            {
                // その他のオブジェクト（GarbageCube など）
                OnGenericObjectEnter(other);
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (other != null)
            {
                _collidersInTrigger.Remove(other);
            }
        }

        /// <summary>
        /// プレイヤーがナラクに入った処理
        /// </summary>
        private void OnPlayerEnter(Collider other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] Collider is null");
                return;
            }

            GameObject playerGO = other.gameObject;
            if (playerGO == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] Player GameObject is null");
                return;
            }

            // 位置を調整
            Vector3 resetPosition = DemController.GetClosestPointOnBounds(other, out bool succeeded);
            if (resetPosition == Vector3.zero)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] DemController returned zero position");
                return;
            }

            if (!succeeded)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] 全試行失敗。DEM センターへフォールバック");
                resetPosition = DemController.GetDemCenterSafePosition(other.bounds.size.y);
            }

            // Rigidbody の速度をリセット
            ResetObjectVelocity(other);

            // InputController の速度をリセット
            InputController inputCtrl = playerGO.GetComponent<InputController>();
            if (inputCtrl == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] InputController not found on player");
                return;
            }

            inputCtrl.ResetVelocity();
            
            // 高い GameSpeed でも確実に naraku から抜けるよう、より高い位置に配置
            float popupDistance = _POPUP_PLAYER_DISTANCE;
            float gameSpeedMultiplier = Mathf.Max(1f, GameSpeedManager.GetGameSpeed() / 5f);
            popupDistance *= gameSpeedMultiplier;

            resetPosition.y += popupDistance;
            // Debug.Log($"[NarakuTriggerHandler.OnPlayerEnter] プレイヤーを Y+{popupDistance:F1} に移動 (GameSpeed: {GameSpeedManager.GetGameSpeed():F1}x, 乗数: {gameSpeedMultiplier:F2})");
            inputCtrl.CharacterMoveToPosition(resetPosition);
        }

        /// <summary>
        /// 破棄可能なオブジェクト（FireCube, Ash）がナラクに入った処理
        /// </summary>
        private void OnDestructibleEnter(Collider other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnDestructibleEnter] Collider is null");
                return;
            }

            GameObjectTreat.DestroyAll(other.gameObject);
        }

        /// <summary>
        /// 雨粒（RainDrop）がナラクに入った処理
        /// </summary>
        private void OnRainDropEnter(Collider other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnRainDropEnter] Collider is null");
                return;
            }

            // EventSystem と WaterSurfaceCtrl を取得
            _eventSystem = GameObjectTreat.GetEventSystem(_eventSystem);
            if (_eventSystem == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnRainDropEnter] EventSystem not found");
                return;
            }

            _waterSurface = GameObjectTreat.GetOrAddComponent<WaterSurfaceManager>(_eventSystem);
            if (_waterSurface == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnRainDropEnter] WaterSurfaceManager not found;");
                return;
            }

            // 雨粒を水面に通知してから削除
            _waterSurface.RainDropIntoNaraku(other.gameObject);
            GameObjectTreat.DestroyAll(other.gameObject);
        }

        /// <summary>
        /// 水（Water）がナラクに入った処理
        /// </summary>
        private void OnWaterEnter(Collider other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnWaterEnter] Collider is null");
                return;
            }

            GameObjectTreat.DestroyAll(other.gameObject);
        }

        /// <summary>
        /// その他のオブジェクト（GarbageCube など）がナラクに入った処理
        /// </summary>
        private void OnGenericObjectEnter(Collider other)
        {
            if (other == null)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnGenericObjectEnter] Collider is null");
                return;
            }

            // 速度をリセット
            ResetObjectVelocity(other);
            
            // 位置を調整
            Vector3 resetPosition = DemController.GetClosestPointOnBounds(other, out bool succeeded);
            if (resetPosition == Vector3.zero)
            {
                // Debug.LogWarning("[NarakuTriggerHandler.OnGenericObjectEnter] DemController returned zero position");
                return;
            }

            if (!succeeded)
            {
                // Debug.LogWarning($"[NarakuTriggerHandler.OnGenericObjectEnter] {other.gameObject.name}: 全{DemController.MaxIteration}回試行失敗。初期スポーン位置へフォールバック");
                resetPosition = GetFallbackPosition(other);
            }

            other.gameObject.transform.position = resetPosition;
        }

        /// <summary>
        /// DEM 検出全失敗時のフォールバック地点を返す。
        /// SpawnOriginTracker があれば初期スポーン位置、なければ DEM センターを使用。
        /// </summary>
        private Vector3 GetFallbackPosition(Collider other)
        {
            SpawnOriginTracker spawnTracker = other.gameObject.GetComponent<SpawnOriginTracker>();
            if (spawnTracker != null && spawnTracker.HasSpawnOrigin())
            {
                return spawnTracker.SpawnOrigin;
            }

            return DemController.GetDemCenterSafePosition(other.bounds.size.y);
        }

        /// <summary>
        /// オブジェクトの Rigidbody 速度をリセット（高速GameSpeed対応）
        /// </summary>
        private void ResetObjectVelocity(Collider other)
        {
            if (other == null)
            {
                return;
            }

            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                return;
            }

            // 速度・角速度をリセット
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 高い GameSpeed でも落下を続けないよう、複数の対策を実施
            // 方法 1: useGravity を一度無効化してから有効化（重力リセット）
            rb.useGravity = false;
            rb.useGravity = true;

            // 方法 2: constraints で Y 軸の動きを一時的に固定
            RigidbodyConstraints originalConstraints = rb.constraints;
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            
            // 次フレームで constraints を解除するコルーチンを開始
            if (this != null && this.gameObject.activeInHierarchy)
            {
                StartCoroutine(ReleaseConstraintsNextFrame(rb, originalConstraints));
            }

            // Debug.Log($"[NarakuTriggerHandler.ResetObjectVelocity] {other.gameObject.name}: 重力・速度をリセット (GameSpeed: {GameSpeedManager.GetGameSpeed():F1}x)");
        }

        /// <summary>
        /// 次フレームで Rigidbody の constraints を元に戻すコルーチン
        /// </summary>
        private IEnumerator ReleaseConstraintsNextFrame(Rigidbody rb, RigidbodyConstraints originalConstraints)
        {
            yield return null;  // 1フレーム待機
            
            if (rb != null)
            {
                rb.constraints = originalConstraints;
                // Debug.Log($"[NarakuTriggerHandler.ReleaseConstraintsNextFrame] {rb.gameObject.name}: constraints を解除");
            }
        }
    }
}
