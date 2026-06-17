using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// バードビュー時のカメラ姿勢安定化（BUG-S3-015）
    ///
    /// バードビューはほぼ真下を向くため、LookRotation/Aim が特異点（ジンバルロック）に近づき、
    /// ノースアップ処理と相まって高速パン時にロールが 180° 反転（上を向く）することがある。
    /// CinemachineBrain がカメラを計算した後（LateUpdate の後段）に、特異点の無い安定向き
    /// （真下を向き、上参照を水平な北=forward に固定）へ強制上書きしてノースアップを保証する。
    ///
    /// CameraShakeController と同じ「Brain 後に上書き」方式。シーン配置不要（起動時に自己構築）。
    /// </summary>
    [DefaultExecutionOrder(_EXECUTION_ORDER)]
    public class CameraOrientationStabilizer : MonoBehaviour
    {
        // CinemachineBrain・CameraShakeController(30000) より後に実行
        private const int _EXECUTION_ORDER = 30001;
        private const string _HOST_OBJECT_NAME = "CameraOrientationStabilizer";

        // 真下を向き、画面の上を北（world forward）に固定したノースアップ姿勢（特異点なし）
        private static readonly Quaternion _NORTH_UP_TOP_DOWN = Quaternion.LookRotation(Vector3.down, Vector3.forward);

        private Camera _cachedCamera = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new GameObject(_HOST_OBJECT_NAME);
            host.AddComponent<CameraOrientationStabilizer>();
            DontDestroyOnLoad(host);
        }

        private void LateUpdate()
        {
            if (CameraController.CurrentMode != CameraMode.BirdView)
            {
                return;
            }

            Camera targetCamera = GetMainCamera();
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.transform.rotation = _NORTH_UP_TOP_DOWN;
        }

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
