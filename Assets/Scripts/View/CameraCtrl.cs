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

        const float CAMERA_DIST_BIRD_MIN = 240f;
        const float CAMERA_DIST_BIRD_MAX = 2048f;

        const float CAMERA_BASE_BIRD = 150f;
        const float CAMERA_BASE_DIST = 1.6f;
        const float CAMERA_BASE_FPS = 0.8f;
        const float CAMERA_BASE_TPS = 1.6f;

        const float CAMERA_ZOOM_MIN = -2f;
        const float CAMERA_ZOOM_MAX = 8f;
        const float CAMERA_ZOOM_BARID = 16f;
        const float BIRDCAM_BASEMENT_Y = 200f;


        private static float _zoom_lv = 3f;


        internal static void ChangeCameraMode(float moveVec)
        {
            CinemachineVirtualCamera vcamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
            Cinemachine3rdPersonFollow cinebody;
            cinebody = vcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            _zoom_lv = SetZoomLv(_zoom_lv, moveVec);
            // Debug.Log("_zoom_lv: "  + _zoom_lv);
            SetCameraDistace(_zoom_lv, cinebody, moveVec);

            // FPS
            if (_zoom_lv <= 0f)
            // if (cinebody.CameraDistance <= 0f)
            {
                ChageCameraBird(0);
                cinebody.ShoulderOffset = new Vector3(1, 0, 0);
                cinebody.CameraSide = _FPS_CAMERA_SIDE;
            }
            // TPS
            else if (_zoom_lv > 0f && _zoom_lv < CAMERA_ZOOM_MAX)
            // else if (cinebody.CameraDistance < CAMERA_DIST_MAX)
            {
                ChageCameraBird(0);
                cinebody.ShoulderOffset = new Vector3(1, 1, 0);
                cinebody.CameraSide = _TPS_CAMERA_SIDE;
            }
            // BIRD VIEW
            else
            {
                ChageCameraBird(20);
            }
        }

        internal static void ChageCameraBird(int Priority)
        {
            CinemachineVirtualCamera birdcamera = GameObject.Find("BirdViewCamera").GetComponent<CinemachineVirtualCamera>();
            if (birdcamera.Priority != Priority)
            {
                birdcamera.Priority = Priority;
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


		private static void SetCameraDistace(float zoom_lv, Cinemachine3rdPersonFollow cinebody, float moveVec)
		{
			// float distVec = GetCameraMoveVec(moveVec);
			// float cam_dist = cinebody.CameraDistance;
			// float set_val = CAMERA_BASE_DIST * distVec;
            float set_val = 0f;

			// FPS
			// if (cam_dist + set_val * 1/2 <= 0f)
			if (zoom_lv <= 0f)
			{
				// set_val = cam_dist + set_val * 1/2;
				set_val = zoom_lv * CAMERA_BASE_FPS;
                cinebody.CameraDistance = filterCameraDistace(set_val);
			}
			// TPS
            else if (zoom_lv > 0f && zoom_lv < CAMERA_ZOOM_MAX)
			// else if (cam_dist + set_val < CAMERA_DIST_MAX)
			{
				// set_val = cam_dist + set_val;
				set_val = zoom_lv * CAMERA_BASE_TPS;
                cinebody.CameraDistance = filterCameraDistace(set_val);
			}
			// BIRD VIEW
			else
			{
                // cinebody.CameraDistance = CAMERA_DIST_MAX;
				Transform birdcam = GameObject.Find("BirdCameraRoot").GetComponent<Transform>();
                set_val = filterBirdCameraDistace(BIRDCAM_BASEMENT_Y + CAMERA_BASE_BIRD * (zoom_lv - CAMERA_ZOOM_MAX));
                // set_val = filterBirdCameraDistace(birdcam.localPosition.y + CAMERA_BASE_BIRD * set_val);
				birdcam.localPosition = new Vector3(birdcam.localPosition.x, set_val, birdcam.localPosition.z);
                // if (set_val <= CAMERA_DIST_BIRD_MIN)
                // {
                //     cinebody.CameraDistance = CAMERA_DIST_MAX - CAMERA_BASE_DIST;
                // }
			}
            // Debug.Log("set_val: " + set_val);

		}

        private static float filterCameraDistace(float set_val)
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

        private static float filterBirdCameraDistace(float set_val)
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