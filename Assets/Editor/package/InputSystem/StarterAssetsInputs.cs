using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

using Cinemachine;


namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		// public bool cursorLocked = false;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			// 他の移動キーを押しながら移動中にさらに呼び出された場合

			SetCursorPointer(false);
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			// todo マウスが動くたびに呼ばれているのでコール制限をかける

			// Debug.Log("Look");
			SetCursorPointer(true);
			// todo ポインター表示まで少しディレイがあってもいいかもしれない

			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnClick(InputValue value)
		{
			// todo:mouse Down とUPの二回走っている！
			// downのときのみに抑制
			Debug.Log("click");

// UnityEngine.InputSystem.UI
			// UnityEngine.InputSystem.UI.
			// UIPointerBehavior
			// SprintInput(value.isPressed);

			// クリックされた位置の検出
			Vector2 mousePosision = Mouse.current.position.ReadValue();
	
			Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosision);	 // マウスをクリックしたときのメインカメラの位置
			// Vector3 viewPoint = Camera.main.ScreenToViewportPoint(mousePosision);
			
			Ray PointRay = Camera.main.ScreenPointToRay(mousePosision);

			RaycastHit hit;
			int layerMask = 1 << 8;

			// if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
			// if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, 50f, layerMask))
			// if (Physics.Raycast(PointRay, out hit, 50f, layerMask))
			// Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), Color.yellow, 5.0f);
			// Physics.RaycastAll

			// if (Physics.RaycastAll(worldPoint, , out hit, 50f))
			if (Physics.Raycast(PointRay, out hit, 20f))
			{
				// Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow, 5f);
				// Debug.DrawRay(Camera.main.transform.position, mousePosision, Color.green, 5f);
				Debug.DrawRay(PointRay.origin, PointRay.direction * 10, Color.red, 5f);

				// GameObject prefab = Resources.Load<GameObject>("Prefabs/Cube");
				GameObject prefab = Resources.Load<GameObject>("Prefabs/Bonfire");
				// Vector3 setPoint = hit.collider.gameObject.transform.position + hit.collider.gameObject.transform.up * 2;
				Vector3 setPoint = hit.collider.gameObject.transform.position;
				// Vector3 setPoint = hit.collider.gameObject.transform.localPosition;

				setPoint += Vector3.up;
				GameObject instance = Instantiate(prefab, setPoint, Quaternion.identity);
				Debug.Log("Did Hit:" + hit.collider.gameObject.name + setPoint);

				Transform parent = hit.collider.gameObject.transform.parent;
				if (parent != null)
				{
					instance.transform.SetParent(parent.transform);
				}
				

				// hit.collider.gameObject.transform.position が結構とんでもない位置を返している。
				// 親（LOD2）の位置がそもそもおかしい -3502.517 -26.75882 -51782.59
				// lod1_53392633_bldg_6677 が x軸方向に-90回転して表示しているので座標が合わない

				// 困ったもんだ・・・

				// ex.Did Hit:BLD_9b519583-dd03-44fa-ae02-5c81743fdf88(-3502.52, -26.76, -51782.59)

				// parent positionからの差分で？


			}
			else
			{
				// GameObject prefab = Resources.Load<GameObject>("Prefabs/Bonfire");
				GameObject prefab = Resources.Load<GameObject>("Prefabs/Cube");
				Vector3 setPoint = worldPoint + Camera.main.transform.forward * 10;
				// if (setPoint.z < 0)
				// {
				// 	setPoint.z = 1;
				// }
				GameObject instance = Instantiate(prefab, setPoint, Quaternion.identity);

				Debug.Log("non Hit ");
			}


			// Debug.Log("click" + worldPoint);


		}

		public void OnZoom(InputValue value)
		{
			Vector2 mouseScroll = Mouse.current.scroll.ReadValue();
			if (mouseScroll == Vector2.zero)
			{
				return;
			}
			CinemachineVirtualCamera vcamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
			Cinemachine3rdPersonFollow cinebody;
			cinebody = vcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
			// Debug.Log($"[frame: {Time.frameCount}] Mouse.current.scroll = {mouseScroll.ToString("F3")}");
			ChangeCameraMode(cinebody, mouseScroll.y);
		}

#endif

		private void ChangeCameraMode(Cinemachine3rdPersonFollow cinebody,float moveVec)
		{
			CinemachineVirtualCamera birdcamera = GameObject.Find("BirdViewCamera").GetComponent<CinemachineVirtualCamera>();
			birdcamera.Priority = 0;
			SetCameraDistace(cinebody, moveVec);

			// FPS
			if (cinebody.CameraDistance <= 0f)
			{
				cinebody.ShoulderOffset = new Vector3(1, 0, 0);
				cinebody.CameraSide = 0.5f;
			}
			// TPS
			else if (cinebody.CameraDistance < 20f)
			{
				cinebody.ShoulderOffset = new Vector3(1, 1, 0);
				cinebody.CameraSide = 0.84f;
			}
			// BIRD VIEW
			else
			{
				birdcamera.Priority = 20;				// カメラ切り替え
				cinebody.ShoulderOffset = new Vector3(1, cinebody.CameraDistance, 0);
				cinebody.CameraSide = 0.5f;
			}
		}

		private void SetCameraDistace(Cinemachine3rdPersonFollow cinebody, float moveVec)
		{
			const float CAMERA_DIST_MIN = -1.55f;
			const float CAMERA_DIST_MID = 20f;
			const float CAMERA_DIST_BIRD = 200f;
			const float CAMERA_DIST_MAX = 520f;
			const float CAMERA_BASE_DIST = 1.6f;
			float distVec = GetCameraMoveVec(moveVec);
			float cam_dist = cinebody.CameraDistance;
			float set_val = CAMERA_BASE_DIST * distVec;

			// FPS
			if (cam_dist + set_val * 1/2 <= 0f)
			{
				set_val = cam_dist + set_val * 1/2;
			}
			// TPS
			else if (cam_dist + set_val < CAMERA_DIST_MID)
			{
				set_val = cam_dist + set_val;
			}
			// BIRD VIEW
			else
			{
				set_val = cam_dist + set_val * 50;
				Transform birdcam = GameObject.Find("BirdCameraRoot").GetComponent<Transform>();
				birdcam.position = new Vector3(0, CAMERA_DIST_BIRD + set_val, 0);
				// TPSに切り替える
				if (cam_dist < 0 && set_val <= CAMERA_DIST_BIRD)
				{
					set_val = CAMERA_DIST_MID;
				}
			}

			if (set_val <= CAMERA_DIST_MIN)
			{
				set_val = CAMERA_DIST_MIN;
			}
			else if (set_val >= CAMERA_DIST_MAX)
			{
				set_val = CAMERA_DIST_MAX;
			}

			cinebody.CameraDistance = set_val;

		}

		private float GetCameraMoveVec(float moveVec)
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


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			// true onFocus false lost Forcus
			Debug.Log("hasFocus:" + hasFocus);

			SetCursorState(cursorLocked);
		}
		// void OnApplicationPause(bool pauseStatus)
		// {
		// 	isPaused = pauseStatus;
		// }		

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
			// Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
			// Cursor.lockState = CursorLockMode.None;		// カーソルの動作を変更させません。
			// Cursor.lockState = CursorLockMode.Locked;	// カーソルが消える(ゲームウィンドウの中央にカーソルをロックします。)			
			// Cursor.lockState = CursorLockMode.Confined;		// カーソルがゲーム内からでれなくなる カーソルをゲーム内のみ表示させます。
		}

		private void SetCursorPointer(bool newState)
		{
			CursorLockMode lockMode;
			// Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.Locked;
			lockMode = newState ? CursorLockMode.None : CursorLockMode.Locked;

			// 複数のキーを押しながら移動など多重呼び出しを回避するため
			if (Cursor.lockState == lockMode)
			{
				return;
			}
			Cursor.lockState  = lockMode;



		}

		private void initMousePointer()
		{
			// TODO:: initへ移動、初回のみ

    		Texture2D cursorTexture;
    		CursorMode cursorMode = CursorMode.Auto;	// プラットフォームがサポートしているハードウェアカーソルを使用します
			// CursorMode cursorMode = CursorMode.ForceSoftware;		//ソフトウェアカーソルを使用します
    		Vector2 hotSpot = Vector2.zero;	// マウスポインター位置を
			// cursorTexture = Resources.Load<Texture2D>("imgs/icons/test");
			cursorTexture = Resources.Load<Texture2D>("imgs/icons/iconaddedlocal");
			// TODO:: 使える画像とそうでないのがある。
			// SVG com.unity.vectorgraphics 入れてみたけど、まだうまくうごかない

			// GameObject instance = Instantiate(Resources.Load("enemy", typeof(GameObject))) as GameObject;
			// Addressables.LoadAssetAsync<GameObject>("ExamplePrefab");
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Debug.Log("Cursor.SetCursor");
				Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
			}
		}

		public void Awake()
		{
			// initMousePointer();
		}


	}
	
}