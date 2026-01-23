using UnityEngine;
using Unity.Cinemachine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AppCamera
{
    /// <summary>
    /// カメラモード
    /// </summary>
    public enum CameraMode
    {
        FPS,        // ファーストパーソン
        TPS,        // サードパーソン
        LongShot,   // ロングショット
        BirdView    // バードビュー
    }

    public static class CameraController
    {
        // カメラサイド定数
        const float _FPS_CAMERA_SIDE = 0.5f;
        const float _TPS_CAMERA_SIDE = 0.84f;
        const float _BIRD_CAMERA_SIDE = 0.5f;
        
        // カメラ距離範囲
        const float CAMERA_DIST_MIN = -1.55f;
        const float CAMERA_DIST_MAX = 20f;
        const float CAMERA_DIST_LONG_MIN = 10f;
        const float CAMERA_DIST_LONG_MAX = 250f;
        const float CAMERA_DIST_BIRD_MIN = 30f;
        const float CAMERA_DIST_BIRD_MAX = 2048f;

        // カメラベース距離
        const float CAMERA_BASE_BIRD = 25f;
        const float CAMERA_BASE_FPS = 0.8f;
        const float CAMERA_BASE_TPS = 1.6f;
        const float CAMERA_BASE_LONG = 12f;

        // ズームレベル範囲
        const float CAMERA_ZOOM_MIN = -2f;
        const float CAMERA_ZOOM_MAX = 8f;
        const float CAMERA_ZOOM_LONG = 14f;
        const float CAMERA_ZOOM_BIRD = 25f;
        const float BIRDCAM_BASEMENT_Y = 20f;

        // 状態変数
        private static float _zoom_lv = 5f;
        private static CameraMode _currentMode = CameraMode.TPS;
        private static Vector3 _camera_offset = new Vector3(1f, 1f, 0f);
        private static bool _isJumpEasing = false;
        private static bool _isInitialized = false;

        // ショルダーオフセットデフォルト値
        private static Vector3 _defaultShoulderOffsetFPS = new Vector3(0f, 0.5f, 0.6f);
        private static Vector3 _defaultShoulderOffsetTPS = new Vector3(0.5f, 0.8f, 0.5f);

        // カメラ距離のスムーズ補間用
        private static float _targetCameraDistance = 0f;
        private static float _currentCameraDistance = 0f;
        private const float CAMERA_LERP_SPEED = 8f; // 補間速度（高いほど速い）
        
        // カメラオブジェクトキャッシュ
        private static CinemachineCamera _playerCamera;
        private static CinemachineCamera _birdCamera;
        private static CinemachineCamera _longCamera;
        private static Transform _birdCameraRoot;

        /// <summary>
        /// カメラシステムを初期化（初回のみ実行）
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized) return;
            
            // カメラオブジェクトをキャッシュ
            CacheCamera();
            
            if (_playerCamera == null) return;
            
            var thirdPersonFollow = _playerCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow != null)
            {
                // 現在のカメラ距離から初期ズームレベルを逆算
                float currentDistance = thirdPersonFollow.CameraDistance;
                
                // TPS範囲として初期化
                if (currentDistance > 0.01f)
                {
                    _zoom_lv = currentDistance / CAMERA_BASE_TPS;
                    _zoom_lv = Mathf.Clamp(_zoom_lv, CAMERA_ZOOM_MIN, CAMERA_ZOOM_BIRD);
                }
                
                // 補間用の変数を初期化
                _targetCameraDistance = currentDistance;
                _currentCameraDistance = currentDistance;
                
                // 初期モードを設定
                _currentMode = GetCameraModeFromZoomLevel(_zoom_lv);
                
                Debug.Log($"[CameraCtrl] Initialized: zoom_lv={_zoom_lv:F2}, distance={currentDistance:F2}, mode={_currentMode}");
            }
            
            _isInitialized = true;
        }

        /// <summary>
        /// カメラオブジェクトをキャッシュ
        /// </summary>
        private static void CacheCamera()
        {
            if (_playerCamera == null)
            {
                GameObject playerCameraObj = GameObject.Find("PlayerFollowCamera");
                _playerCamera = playerCameraObj?.GetComponent<CinemachineCamera>();
            }
            
            if (_birdCamera == null)
            {
                GameObject birdCameraObj = GameObject.Find("BirdViewCamera");
                _birdCamera = birdCameraObj?.GetComponent<CinemachineCamera>();
            }
            
            if (_longCamera == null)
            {
                GameObject longCameraObj = GameObject.Find("LongShotCamera");
                _longCamera = longCameraObj?.GetComponent<CinemachineCamera>();
            }
            
            if (_birdCameraRoot == null)
            {
                GameObject birdCameraRootObj = GameObject.Find("BirdCameraRoot");
                _birdCameraRoot = birdCameraRootObj?.transform;
            }
        }

        /// <summary>
        /// ズームレベルからカメラモードを判定
        /// </summary>
        private static CameraMode GetCameraModeFromZoomLevel(float zoomLevel)
        {
            if (zoomLevel <= 0f)
                return CameraMode.FPS;
            else if (zoomLevel < CAMERA_ZOOM_MAX)
                return CameraMode.TPS;
            else if (zoomLevel < CAMERA_ZOOM_LONG)
                return CameraMode.LongShot;
            else
                return CameraMode.BirdView;
        }

        internal static void ChangeCameraJumpOffset(bool isReset)
        {
            Initialize();
            CacheCamera();
            
            if (_playerCamera == null) return;

            var thirdPersonFollow = _playerCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow == null) return;

            float jumpDistanceOffset = 0.4f;
            
            if (isReset)
            {
                // カメラ距離をリセット
                float targetDistance = _camera_offset.magnitude;
                if (Mathf.Abs(thirdPersonFollow.CameraDistance - (targetDistance + jumpDistanceOffset)) < 0.01f)
                {
                    thirdPersonFollow.CameraDistance = targetDistance;
                    _targetCameraDistance = targetDistance;
                }
            }
            else
            {
                // ジャンプ時はカメラを少し引く
                float currentDistance = thirdPersonFollow.CameraDistance;
                if (Mathf.Abs(currentDistance - (_camera_offset.magnitude + jumpDistanceOffset)) > 0.01f)
                {
                    _camera_offset = new Vector3(0, 0, currentDistance);
                    thirdPersonFollow.CameraDistance += jumpDistanceOffset;
                    _targetCameraDistance = currentDistance + jumpDistanceOffset;
                }
            }
        }

        /// <summary>
        /// FPSモード時のカメラパラメータを設定
        /// </summary>
        private static void SetFPSCameraParameters(CinemachineCamera vcamera, CinemachineThirdPersonFollow thirdPersonFollow)
        {
            if (thirdPersonFollow == null || vcamera == null) return;
            
            thirdPersonFollow.ShoulderOffset = _defaultShoulderOffsetFPS;
            thirdPersonFollow.CameraSide = _FPS_CAMERA_SIDE;
            thirdPersonFollow.VerticalArmLength = 0.0f;
            
            var lens = vcamera.Lens;
            lens.NearClipPlane = 1.25f;
            vcamera.Lens = lens;
        }
        
        /// <summary>
        /// TPSモード時のカメラパラメータを設定
        /// </summary>
        private static void SetTPSCameraParameters(CinemachineCamera vcamera, CinemachineThirdPersonFollow thirdPersonFollow)
        {
            if (thirdPersonFollow == null || vcamera == null) return;
            
            thirdPersonFollow.ShoulderOffset = _defaultShoulderOffsetTPS;
            thirdPersonFollow.CameraSide = _TPS_CAMERA_SIDE;
            thirdPersonFollow.VerticalArmLength = 0.4f;
            
            var lens = vcamera.Lens;
            lens.NearClipPlane = 0.7f;
            vcamera.Lens = lens;
        }


        internal static void ChangeCameraMode(float moveVec)
        {
            Initialize();
            CacheCamera();
            
            if (_playerCamera == null) return;

            // ズームレベルを更新
            _zoom_lv = UpdateZoomLevel(_zoom_lv, moveVec);
            
            // 新しいカメラモードを判定
            CameraMode newMode = GetCameraModeFromZoomLevel(_zoom_lv);
            
            // モードが変更された場合は補間をリセット
            if (newMode != _currentMode)
            {
                var thirdPersonFollow = _playerCamera.GetComponent<CinemachineThirdPersonFollow>();
                if (thirdPersonFollow != null)
                {
                    _currentCameraDistance = thirdPersonFollow.CameraDistance;
                }
                _currentMode = newMode;
            }
            
            // カメラ距離とパラメータを設定
            ApplyCameraMode(_currentMode, _playerCamera);
            
            // 優先度を設定
            SetCameraPriorities(_currentMode);
        }

        /// <summary>
        /// ズームレベルを更新
        /// </summary>
        private static float UpdateZoomLevel(float currentZoomLevel, float moveVec)
        {
            float direction = moveVec < 0 ? 1f : -1f;
            float newZoomLevel = currentZoomLevel + direction;
            return Mathf.Clamp(newZoomLevel, CAMERA_ZOOM_MIN, CAMERA_ZOOM_BIRD);
        }

        /// <summary>
        /// カメラモードに応じた設定を適用
        /// </summary>
        private static void ApplyCameraMode(CameraMode mode, CinemachineCamera camera)
        {
            switch (mode)
            {
                case CameraMode.FPS:
                    ApplyFPSMode(camera);
                    break;
                case CameraMode.TPS:
                    ApplyTPSMode(camera);
                    break;
                case CameraMode.LongShot:
                    ApplyLongShotMode();
                    break;
                case CameraMode.BirdView:
                    ApplyBirdViewMode();
                    break;
            }
        }

        /// <summary>
        /// FPSモードを適用
        /// </summary>
        private static void ApplyFPSMode(CinemachineCamera camera)
        {
            var thirdPersonFollow = camera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow == null) return;
            
            // カメラ距離を計算（zoom_lvが負の値なので絶対値を取る）
            float targetDistance = Mathf.Abs(_zoom_lv * CAMERA_BASE_FPS);
            targetDistance = Mathf.Clamp(targetDistance, 0f, CAMERA_BASE_FPS * Mathf.Abs(CAMERA_ZOOM_MIN));
            
            UpdateCameraDistanceSmooth(thirdPersonFollow, targetDistance);
            SetFPSCameraParameters(camera, thirdPersonFollow);
        }

        /// <summary>
        /// TPSモードを適用
        /// </summary>
        private static void ApplyTPSMode(CinemachineCamera camera)
        {
            var thirdPersonFollow = camera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow == null) return;
            
            float targetDistance = _zoom_lv * CAMERA_BASE_TPS;
            targetDistance = Mathf.Clamp(targetDistance, 0f, CAMERA_ZOOM_MAX * CAMERA_BASE_TPS);
            
            UpdateCameraDistanceSmooth(thirdPersonFollow, targetDistance);
            SetTPSCameraParameters(camera, thirdPersonFollow);
        }

        /// <summary>
        /// ロングショットモードを適用
        /// </summary>
        private static void ApplyLongShotMode()
        {
            if (_longCamera == null) return;
            
            var longThirdPersonFollow = _longCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (longThirdPersonFollow == null) return;
            
            float distanceValue = CAMERA_BASE_LONG * (_zoom_lv - CAMERA_ZOOM_MAX);
            distanceValue = Mathf.Clamp(distanceValue, CAMERA_DIST_LONG_MIN, CAMERA_DIST_LONG_MAX);
            
            longThirdPersonFollow.CameraDistance = 20f + distanceValue;
        }

        /// <summary>
        /// バードビューモードを適用
        /// </summary>
        private static void ApplyBirdViewMode()
        {
            if (_birdCameraRoot == null) return;
            
            float heightValue = BIRDCAM_BASEMENT_Y + CAMERA_BASE_BIRD * (_zoom_lv - CAMERA_ZOOM_LONG - 1);
            heightValue = Mathf.Clamp(heightValue, CAMERA_DIST_BIRD_MIN, CAMERA_DIST_BIRD_MAX);
            
            _birdCameraRoot.localPosition = new Vector3(
                _birdCameraRoot.localPosition.x,
                heightValue,
                _birdCameraRoot.localPosition.z
            );

            // Debug.Log($"[CameraCtrl] BirdView Height Set: {_birdCameraRoot.localPosition.y:F2}");
        }

        /// <summary>
        /// カメラ距離をスムーズに更新
        /// </summary>
        private static void UpdateCameraDistanceSmooth(CinemachineThirdPersonFollow thirdPersonFollow, float targetDistance)
        {
            // ターゲット値が大きく変わった場合は現在値をリセット
            if (Mathf.Abs(_targetCameraDistance - targetDistance) > 0.01f)
            {
                _currentCameraDistance = thirdPersonFollow.CameraDistance;
            }
            
            _targetCameraDistance = targetDistance;
            _currentCameraDistance = thirdPersonFollow.CameraDistance;
            
            float newDistance = Mathf.Lerp(_currentCameraDistance, _targetCameraDistance, Time.deltaTime * CAMERA_LERP_SPEED);
            thirdPersonFollow.CameraDistance = newDistance;
        }

        /// <summary>
        /// カメラの優先度を設定
        /// </summary>
        private static void SetCameraPriorities(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.FPS:
                case CameraMode.TPS:
                    SetCameraPriority(_playerCamera, 20);
                    SetCameraPriority(_longCamera, 0);
                    SetCameraPriority(_birdCamera, 0);
                    break;
                    
                case CameraMode.LongShot:
                    SetCameraPriority(_playerCamera, 0);
                    SetCameraPriority(_longCamera, 20);
                    SetCameraPriority(_birdCamera, 0);
                    break;
                    
                case CameraMode.BirdView:
                    SetCameraPriority(_playerCamera, 0);
                    SetCameraPriority(_longCamera, 0);
                    SetCameraPriority(_birdCamera, 20);
                    break;
            }
        }

        /// <summary>
        /// 個別カメラの優先度を設定
        /// </summary>
        private static void SetCameraPriority(CinemachineCamera camera, int priority)
        {
            if (camera != null && camera.Priority != priority)
            {
                camera.Priority = priority;
            }
        }

    }
}