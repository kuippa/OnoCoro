using UnityEngine;
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
        
        private GameObject _eventSystem = null;
        private WaterSurfaceCtrl _waterSurface = null;
        private HashSet<Collider> _collidersInTrigger = new HashSet<Collider>();

        protected override void Awake()
        {
            base.Awake();
            // Naraku は特定の単一タグではなく、複数パターンに対応するため、デフォルトタグを設定
            SetDefaultTargetTag(GameEnum.TagType.Naraku.ToString());
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
                Debug.LogWarning("[NarakuTriggerHandler.OnTriggerEnter] Collider is null");
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
                Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] Collider is null");
                return;
                }

            GameObject playerGO = other.gameObject;
            if (playerGO == null)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] Player GameObject is null");
                return;
            }

            // 位置を調整
            Vector3 resetPosition = DemController.GetClosestPointOnBounds(other);
            if (resetPosition == Vector3.zero)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] DemController returned zero position");
                return;
            }

            // Rigidbody の速度をリセット
            ResetObjectVelocity(other);

            // InputController の速度をリセット
            InputController inputCtrl = playerGO.GetComponent<InputController>();
            if (inputCtrl == null)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnPlayerEnter] InputController not found on player");
                return;
            }

            inputCtrl.ResetVelocity();
            
            // プレイヤーを上に持ち上げて移動
            resetPosition.y += _POPUP_PLAYER_DISTANCE;
            Debug.Log($"[NarakuTriggerHandler.OnPlayerEnter] Moving player to {resetPosition}");
            inputCtrl.CharacterMoveToPosition(resetPosition);
        }

        /// <summary>
        /// 破棄可能なオブジェクト（FireCube, Ash）がナラクに入った処理
        /// </summary>
        private void OnDestructibleEnter(Collider other)
        {
            if (other == null)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnDestructibleEnter] Collider is null");
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
                Debug.LogWarning("[NarakuTriggerHandler.OnRainDropEnter] Collider is null");
                return;
            }

            // EventSystem と WaterSurfaceCtrl を取得
            _eventSystem = GameObjectTreat.GetEventSystem(_eventSystem);
            if (_eventSystem == null)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnRainDropEnter] EventSystem not found");
                return;
            }

            _waterSurface = GameObjectTreat.GetOrAddComponent<WaterSurfaceCtrl>(_eventSystem);
            if (_waterSurface == null)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnRainDropEnter] WaterSurfaceCtrl not found");
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
                Debug.LogWarning("[NarakuTriggerHandler.OnWaterEnter] Collider is null");
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
                Debug.LogWarning("[NarakuTriggerHandler.OnGenericObjectEnter] Collider is null");
                return;
            }

            // 速度をリセット
            ResetObjectVelocity(other);
            
            // 位置を調整
            Vector3 resetPosition = DemController.GetClosestPointOnBounds(other);
            if (resetPosition == Vector3.zero)
            {
                Debug.LogWarning("[NarakuTriggerHandler.OnGenericObjectEnter] DemController returned zero position");
                return;
            }

            other.gameObject.transform.position = resetPosition;
        }

        /// <summary>
        /// オブジェクトの Rigidbody 速度をリセット
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

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // デバッグモード時は追加の減衰を適用
            if (GameConfig._APP_GAME_MODE == GlobalConst.GAME_MODE_DEBUG)
            {
                rb.linearDamping = 2f;
            }
        }
    }
}
