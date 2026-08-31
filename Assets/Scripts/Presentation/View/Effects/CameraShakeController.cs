using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// カメラシェイクコントローラー（Season 3 W2 Task 1）
    ///
    /// Earthquake が計算する揺れオフセット（静的プロパティ）を毎フレーム
    /// メインカメラの位置に加算する。Cinemachine Brain がカメラ位置を
    /// LateUpdate で確定させた後に上書きするため、実行順を大きく遅らせている。
    ///
    /// シーンへの手動配置は不要（起動時に自己構築し DontDestroyOnLoad で常駐）。
    /// 参照方向は Presentation → Game（Earthquake）で層規約に適合。
    /// </summary>
    [DefaultExecutionOrder(_EXECUTION_ORDER)]
    public class CameraShakeController : MonoBehaviour
    {
        /// <summary>CinemachineBrain の LateUpdate より後に実行するための順序値</summary>
        private const int _EXECUTION_ORDER = 30000;

        private const string _HOST_OBJECT_NAME = "CameraShakeController";

        /// <summary>
        /// 追従の速さ。大きいほど目標値に速く追いつく（小さいほどなめらかで鈍い）。
        /// フレームレートに依存しないよう指数補間の係数として使う
        /// </summary>
        private const float _SMOOTHING_RATE = 40f;

        private Camera _cachedCamera = null;

        /// <summary>実際にカメラへ加算しているオフセット（補間後）</summary>
        private float _appliedOffset = 0f;

        /// <summary>
        /// 起動時に自動生成（シーン配置不要のブートストラップ）
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new GameObject(_HOST_OBJECT_NAME);
            host.AddComponent<CameraShakeController>();
            DontDestroyOnLoad(host);
        }

        private void LateUpdate()
        {
            if (!Earthquake.IsShaking)
            {
                // 揺れが終わったら補間値も戻す（次の地震に前回の残りを持ち込まない）
                _appliedOffset = 0f;
                return;
            }

            Camera targetCamera = GetMainCamera();
            if (targetCamera == null)
            {
                return;
            }

            // Earthquake 側は _INTERVAL(0.02秒)ごとにしかオフセットを更新しないため、
            // そのまま加算すると高フレームレートで階段状に見える。
            // フレーム間で目標値へ寄せてなめらかにする
            _appliedOffset = Mathf.Lerp(
                _appliedOffset,
                Earthquake.CurrentVerticalOffset,
                1f - Mathf.Exp(-_SMOOTHING_RATE * Time.deltaTime));

            targetCamera.transform.position += new Vector3(0f, _appliedOffset, 0f);
        }

        /// <summary>
        /// メインカメラを取得（シーン遷移で破棄された場合は再取得）
        /// </summary>
        private Camera GetMainCamera()
        {
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
            }
            return _cachedCamera;
        }
    }
}
