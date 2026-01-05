using UnityEngine;
using Unity.Cinemachine;

namespace AppCamera
{

    public static class CameraCtrl
    {
        const float _FPS_CAMERA_SIDE = 0.5f;
        const float _TPS_CAMERA_SIDE = 0.84f;
        const float _BIRD_CAMERA_SIDE = 0.5f;
        const float CAMERA_DIST_MIN = -1.55f;
        const float CAMERA_DIST_MAX = 20f;
        const float CAMERA_DIST_LONG_MIN = 10f;
        const float CAMERA_DIST_LONG_MAX = 250f;

        const float CAMERA_DIST_BIRD_MIN = 240f;
        const float CAMERA_DIST_BIRD_MAX = 2048f;

        const float CAMERA_BASE_BIRD = 150f;
        const float CAMERA_BASE_DIST = 1.6f;
        const float CAMERA_BASE_FPS = 0.8f;
        const float CAMERA_BASE_TPS = 1.6f;
        const float CAMERA_BASE_LONG = 12f;

        const float CAMERA_ZOOM_MIN = -2f;
        const float CAMERA_ZOOM_MAX = 8f;
        const float CAMERA_ZOOM_LONG = 14f;
        const float CAMERA_ZOOM_BARID = 20f;
        const float BIRDCAM_BASEMENT_Y = 200f;


        private static float _zoom_lv = 3f;


        internal static void ChangeCameraMode(float moveVec)
        {
            CinemachineCamera vcamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineCamera>();
            CinemachineCamera birdcamera = GameObject.Find("BirdViewCamera").GetComponent<CinemachineCamera>();
            CinemachineCamera longcamera = GameObject.Find("LongShotCamera").GetComponent<CinemachineCamera>();
            
            var orbitalFollow = vcamera.GetComponent<CinemachineOrbitalFollow>();
            var rotationComposer = vcamera.GetComponent<CinemachineRotationComposer>();

            _zoom_lv = SetZoomLv(_zoom_lv, moveVec);
            SetCameraDistance(_zoom_lv, orbitalFollow, moveVec);

            // FPS
            if (_zoom_lv <= 0f)
            {
                ChangeCameraPriority(0, birdcamera);
                ChangeCameraPriority(0, longcamera);
                if (orbitalFollow != null)
                {
                    orbitalFollow.TargetOffset = Vector3.zero;
                }
            }
            // TPS
            else if (_zoom_lv > 0f && _zoom_lv < CAMERA_ZOOM_MAX)
            {
                ChangeCameraPriority(0, birdcamera);
                ChangeCameraPriority(0, longcamera);
            }
            // LONG SHOT
            else if (_zoom_lv >= CAMERA_ZOOM_MAX && _zoom_lv < CAMERA_ZOOM_LONG)
            {
                ChangeCameraPriority(0, birdcamera);
                ChangeCameraPriority(20, longcamera);
            }
            // BIRD VIEW
            else
            {
                ChangeCameraPriority(20, birdcamera);
                ChangeCameraPriority(0, longcamera);
            }
        }

        internal static void ChangeCameraPriority(int Priority, CinemachineCamera vcamera)
        {
            if (vcamera.Priority != Priority)
            {
                vcamera.Priority = Priority;
            }
        }

        private static float SetZoomLv(float zoom_lv, float moveVec)
        {
            float distVec = GetCameraMoveVec(moveVec);
            zoom_lv += distVec;
            zoom_lv = Mathf.Clamp(zoom_lv, CAMERA_ZOOM_MIN, CAMERA_ZOOM_BARID);
            return zoom_lv;
        }

        private static void SetCameraDistance(float zoom_lv, CinemachineOrbitalFollow orbitalFollow, float moveVec)
        {
            float set_val = 0f;
            
            // FPS
            if (zoom_lv <= 0f)
            {
                set_val = zoom_lv * CAMERA_BASE_FPS;
                if (orbitalFollow != null)
                {
                    orbitalFollow.Orbits.Center.Radius = Mathf.Abs(set_val);
                }
            }
            // TPS
            else if (zoom_lv > 0f && zoom_lv < CAMERA_ZOOM_MAX)
            {
                set_val = zoom_lv * CAMERA_BASE_TPS;
                if (orbitalFollow != null)
                {
                    orbitalFollow.Orbits.Center.Radius = set_val;
                }
            }
            // LONG SHOT
            else if (zoom_lv >= CAMERA_ZOOM_MAX && zoom_lv < CAMERA_ZOOM_LONG)
            {
                set_val = CAMERA_BASE_LONG * (zoom_lv - CAMERA_ZOOM_MAX);
                set_val = Mathf.Clamp(set_val, CAMERA_DIST_LONG_MIN, CAMERA_DIST_LONG_MAX);
                
                CinemachineCamera longcamera = GameObject.Find("LongShotCamera").GetComponent<CinemachineCamera>();
                var longOrbital = longcamera.GetComponent<CinemachineOrbitalFollow>();
                if (longOrbital != null)
                {
                    longOrbital.Orbits.Center.Radius = 20 + set_val;
                    longOrbital.TargetOffset = new Vector3(0, 6 + 1.5f * (zoom_lv - CAMERA_ZOOM_MAX), 0);
                }
            }
            // BIRD VIEW
            else
            {
                Transform birdcam = GameObject.Find("BirdCameraRoot").GetComponent<Transform>();
                set_val = filterBirdCameraDistance(BIRDCAM_BASEMENT_Y + CAMERA_BASE_BIRD * (zoom_lv - CAMERA_ZOOM_LONG));
                birdcam.localPosition = new Vector3(birdcam.localPosition.x, set_val, birdcam.localPosition.z);
            }
        }

        private static float filterBirdCameraDistance(float set_val)
        {
            return Mathf.Clamp(set_val, CAMERA_DIST_BIRD_MIN, CAMERA_DIST_BIRD_MAX);
        }

        private static float GetCameraMoveVec(float moveVec)
        {
            return moveVec < 0 ? 1 : -1;
        }
    }
}