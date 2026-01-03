using UnityEngine;
using Cinemachine;

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
        const float CAMERA_ZOOM_BARID = 20f;    // 16f
        const float BIRDCAM_BASEMENT_Y = 200f;


        private static float _zoom_lv = 3f;


        internal static void ChangeCameraMode(float moveVec)
        {
            CinemachineVirtualCamera vcamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
            CinemachineVirtualCamera birdcamera = GameObject.Find("BirdViewCamera").GetComponent<CinemachineVirtualCamera>();
            CinemachineVirtualCamera longcamera = GameObject.Find("LongShotCamera").GetComponent<CinemachineVirtualCamera>();
            // vcamera.Priority = 10;
            Cinemachine3rdPersonFollow cinebody;
            cinebody = vcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            _zoom_lv = SetZoomLv(_zoom_lv, moveVec);
            // Debug.Log("_zoom_lv: "  + _zoom_lv);
            SetCameraDistance(_zoom_lv, cinebody, moveVec);

            // FPS
            if (_zoom_lv <= 0f)
            {
                ChangeCameraPriority(0, birdcamera);
                ChangeCameraPriority(0, longcamera);
                cinebody.ShoulderOffset = new Vector3(1, 0, 0);
                cinebody.CameraSide = _FPS_CAMERA_SIDE;
            }
            // TPS
            else if (_zoom_lv > 0f && _zoom_lv < CAMERA_ZOOM_MAX)
            {
                ChangeCameraPriority(0, birdcamera);
                ChangeCameraPriority(0, longcamera);
                cinebody.ShoulderOffset = new Vector3(1, 1, 0);
                cinebody.CameraSide = _TPS_CAMERA_SIDE;
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

        internal static void ChangeCameraPriority(int Priority, CinemachineVirtualCamera vcamera)
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
            if (zoom_lv < CAMERA_ZOOM_MIN)
            {
                zoom_lv = CAMERA_ZOOM_MIN;
            }
            else if (zoom_lv > CAMERA_ZOOM_BARID)
            {
                zoom_lv = CAMERA_ZOOM_BARID;
            }
            return zoom_lv;
        }

		private static void SetCameraDistance(float zoom_lv, Cinemachine3rdPersonFollow cinebody, float moveVec)
		{
            float set_val = 0f;
			// FPS
			if (zoom_lv <= 0f)
			{
				set_val = zoom_lv * CAMERA_BASE_FPS;
                cinebody.CameraDistance = filterCameraDistance(set_val);
			}
			// TPS
            else if (zoom_lv > 0f && zoom_lv < CAMERA_ZOOM_MAX)
			{
				set_val = zoom_lv * CAMERA_BASE_TPS;
                cinebody.CameraDistance = filterCameraDistance(set_val);
			}
            // LONG SHOT
            else if (zoom_lv >= CAMERA_ZOOM_MAX && zoom_lv < CAMERA_ZOOM_LONG)
            {
                set_val = CAMERA_BASE_LONG * (zoom_lv - CAMERA_ZOOM_MAX);
                set_val = Mathf.Clamp(
                    set_val,
                    CAMERA_DIST_LONG_MIN,
                    CAMERA_DIST_LONG_MAX
                );
                // Debug.Log("set_val: " + set_val);
                CinemachineVirtualCamera longcamera = GameObject.Find("LongShotCamera").GetComponent<CinemachineVirtualCamera>();
                Cinemachine3rdPersonFollow longcamcinebody = longcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();            
                longcamcinebody.CameraDistance = 20 + set_val;
                longcamcinebody.ShoulderOffset = new Vector3(0, 6 + 1.5f * (zoom_lv - CAMERA_ZOOM_MAX), 0);
            }
			// BIRD VIEW
			else
			{
				Transform birdcam = GameObject.Find("BirdCameraRoot").GetComponent<Transform>();
                // set_val = filterBirdCameraDistance(BIRDCAM_BASEMENT_Y + CAMERA_BASE_BIRD * (zoom_lv - CAMERA_ZOOM_MAX));
                set_val = filterBirdCameraDistance(BIRDCAM_BASEMENT_Y + CAMERA_BASE_BIRD * (zoom_lv - CAMERA_ZOOM_LONG));
				birdcam.localPosition = new Vector3(birdcam.localPosition.x, set_val, birdcam.localPosition.z);
			}
		}

        private static float filterCameraDistance(float set_val)
        {
            if (set_val < CAMERA_DIST_MIN)
            {
                set_val = CAMERA_DIST_MIN;
            }
            else if (set_val > CAMERA_DIST_MAX)
            {
                set_val = CAMERA_DIST_MAX;
            }
            return set_val;
        }

        private static float filterBirdCameraDistance(float set_val)
        {
            if (set_val < CAMERA_DIST_BIRD_MIN)
            {
                set_val = CAMERA_DIST_BIRD_MIN;
            }
            else if (set_val > CAMERA_DIST_BIRD_MAX)
            {
                set_val = CAMERA_DIST_BIRD_MAX;
            }
            return set_val;
        }

		private static float GetCameraMoveVec(float moveVec)
		{
			float ret = 0;
			if (moveVec < 0) {
				ret = 1;
			}
			else
			{
				ret = -1;
			}
			return ret;
		}


    }
}