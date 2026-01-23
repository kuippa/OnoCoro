using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OnoCoro.Presentation.View
{
    /// <summary>
    /// 環境（PLATEAU）カメラ制御
    /// 
    /// 【TODO - Alpha Phase 実装予定】
    /// - PLATEAU建築物などの大規模シーン用カメラ制御
    /// - 視界距離（Far Clip Plane）の動的制御
    /// - LOD（Level of Detail）との連携
    /// - パフォーマンス最適化の監視
    /// 
    /// 【実装予定機能】
    /// - far clip plane の動的調整（2000m程度の大規模シーン対応）
    /// - ユーザー設定オプション化
    /// - フォグ距離の自動制御
    /// - シャドウマップ距離の最適化
    /// </summary>
    public class EnvironmentCameraController : MonoBehaviour
    {
        private void Awake()
        {
            // Debug.Log("EnvironmentCameraCtrl Awake");
            // GameObject cameraObj = Camera.main.gameObject;
            // Camera camera = cameraObj.GetComponent<Camera>();
            // far clip plane

            // TODO: 視界距離の制御をオプションで変更できるように実装する
            // Debug.Log("camera.far: " + camera.farClipPlane + ", camera.farClipPlane: " + camera.farClipPlane);
            // camera.farClipPlane = 2000.0f;
            // Debug.Log("camera.far: " + camera.farClipPlane + ", camera.farClipPlane: " + camera.farClipPlane);
        }
    }
}
