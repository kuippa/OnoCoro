// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// AppCamera.CameraCtrl
using Cinemachine;
using UnityEngine;

public static class CameraCtrl
{
	private const float _FPS_CAMERA_SIDE = 0.5f;

	private const float _TPS_CAMERA_SIDE = 0.84f;

	private const float _BIRD_CAMERA_SIDE = 0.5f;

	private const float CAMERA_DIST_MIN = -1.55f;

	private const float CAMERA_DIST_MAX = 20f;

	private const float CAMERA_DIST_LONG_MIN = 10f;

	private const float CAMERA_DIST_LONG_MAX = 250f;

	private const float CAMERA_DIST_BIRD_MIN = 240f;

	private const float CAMERA_DIST_BIRD_MAX = 2048f;

	private const float CAMERA_BASE_BIRD = 150f;

	private const float CAMERA_BASE_DIST = 1.6f;

	private const float CAMERA_BASE_FPS = 0.8f;

	private const float CAMERA_BASE_TPS = 1.6f;

	private const float CAMERA_BASE_LONG = 12f;

	private const float CAMERA_ZOOM_MIN = -2f;

	private const float CAMERA_ZOOM_MAX = 8f;

	private const float CAMERA_ZOOM_LONG = 14f;

	private const float CAMERA_ZOOM_BARID = 20f;

	private const float BIRDCAM_BASEMENT_Y = 200f;

	private static float _zoom_lv = 3f;

	private static Vector3 _camera_offset = new Vector3(1f, 1f, 0f);

	private static bool _isJumpEasing = false;

	internal static void ChangeCameraJumpOffset(bool isReset)
	{
		Vector3 vector = new Vector3(0.4f, 0f, 0f);
		Cinemachine3rdPersonFollow cineBody = GetCineBody();
		if (isReset)
		{
			if (vector == cineBody.ShoulderOffset)
			{
				cineBody.ShoulderOffset = _camera_offset;
			}
		}
		else if (vector != cineBody.ShoulderOffset)
		{
			_camera_offset = cineBody.ShoulderOffset;
			cineBody.ShoulderOffset = vector;
		}
	}

	private static void EasingCameraOffset(Vector3 set_offset, Cinemachine3rdPersonFollow cinebody)
	{
		_isJumpEasing = true;
		float num = set_offset.x - cinebody.ShoulderOffset.x;
		if (num != 0f)
		{
			float x = cinebody.ShoulderOffset.x + num * 0.1f;
			if (num <= 0.1f)
			{
				x = set_offset.x;
				_isJumpEasing = false;
			}
			cinebody.ShoulderOffset = new Vector3(x, cinebody.ShoulderOffset.y, cinebody.ShoulderOffset.z);
		}
	}

	private static Cinemachine3rdPersonFollow GetCineBody()
	{
		return GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>();
	}

	internal static void ChangeCameraMode(float moveVec)
	{
		CinemachineVirtualCamera component = GameObject.Find("BirdViewCamera").GetComponent<CinemachineVirtualCamera>();
		CinemachineVirtualCamera component2 = GameObject.Find("LongShotCamera").GetComponent<CinemachineVirtualCamera>();
		Cinemachine3rdPersonFollow cineBody = GetCineBody();
		_zoom_lv = SetZoomLv(_zoom_lv, moveVec);
		SetCameraDistance(_zoom_lv, cineBody, moveVec);
		if (_zoom_lv <= 0f)
		{
			ChangeCameraPriority(0, component);
			ChangeCameraPriority(0, component2);
			cineBody.ShoulderOffset = new Vector3(1f, 0f, 0f);
			cineBody.CameraSide = 0.5f;
		}
		else if (_zoom_lv > 0f && _zoom_lv < 8f)
		{
			ChangeCameraPriority(0, component);
			ChangeCameraPriority(0, component2);
			cineBody.ShoulderOffset = new Vector3(1f, 1f, 0f);
			cineBody.CameraSide = 0.84f;
		}
		else if (_zoom_lv >= 8f && _zoom_lv < 14f)
		{
			ChangeCameraPriority(0, component);
			ChangeCameraPriority(20, component2);
		}
		else
		{
			ChangeCameraPriority(20, component);
			ChangeCameraPriority(0, component2);
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
		float cameraMoveVec = GetCameraMoveVec(moveVec);
		zoom_lv += cameraMoveVec;
		if (zoom_lv < -2f)
		{
			zoom_lv = -2f;
		}
		else if (zoom_lv > 20f)
		{
			zoom_lv = 20f;
		}
		return zoom_lv;
	}

	private static void SetCameraDistance(float zoom_lv, Cinemachine3rdPersonFollow cinebody, float moveVec)
	{
		float num = 0f;
		if (zoom_lv <= 0f)
		{
			num = zoom_lv * 0.8f;
			cinebody.CameraDistance = filterCameraDistance(num);
		}
		else if (zoom_lv > 0f && zoom_lv < 8f)
		{
			num = zoom_lv * 1.6f;
			cinebody.CameraDistance = filterCameraDistance(num);
		}
		else if (zoom_lv >= 8f && zoom_lv < 14f)
		{
			num = 12f * (zoom_lv - 8f);
			num = Mathf.Clamp(num, 10f, 250f);
			Cinemachine3rdPersonFollow cinemachineComponent = GameObject.Find("LongShotCamera").GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>();
			cinemachineComponent.CameraDistance = 20f + num;
			cinemachineComponent.ShoulderOffset = new Vector3(0f, 6f + 1.5f * (zoom_lv - 8f), 0f);
		}
		else
		{
			Transform component = GameObject.Find("BirdCameraRoot").GetComponent<Transform>();
			num = filterBirdCameraDistance(200f + 150f * (zoom_lv - 14f));
			component.localPosition = new Vector3(component.localPosition.x, num, component.localPosition.z);
		}
	}

	private static float filterCameraDistance(float set_val)
	{
		if (set_val < -1.55f)
		{
			set_val = -1.55f;
		}
		else if (set_val > 20f)
		{
			set_val = 20f;
		}
		return set_val;
	}

	private static float filterBirdCameraDistance(float set_val)
	{
		if (set_val < 240f)
		{
			set_val = 240f;
		}
		else if (set_val > 2048f)
		{
			set_val = 2048f;
		}
		return set_val;
	}

	private static float GetCameraMoveVec(float moveVec)
	{
		float num = 0f;
		if (moveVec < 0f)
		{
			return 1f;
		}
		return -1f;
	}
}
