#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.EventSystems;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using AppCamera;
using UnityEngine.AI;

namespace StarterAssets
// namespace InputActions
{
	public class PlayerInputs : MonoBehaviour
	{
        private GameObject _esc_menu_window = null;
		const float _click_limit_distance = 20.0f;

		[Header("Character Input Values")]
		public Vector2 _move;
		public Vector2 _look;
		public bool _jump;
		public bool _sprint;

		[Header("Movement Settings")]
		public bool _analogMovement;

		[Header("Mouse Cursor Settings")]
		// public bool cursorLocked = true;
		// public bool _cursorLocked = false;
		public bool _TabMenuOpen = false;
		public bool _EscMenuOpen = false;

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
			if(!_TabMenuOpen && !_EscMenuOpen)
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

		public void OnLeftClick(InputValue value)
		{
			if (_TabMenuOpen || _EscMenuOpen)
			{
				return;
			}
			ClickCtrl.OnLeftClick(value);
		}

		public void OnRightClick(InputValue value)
		{
			if (_TabMenuOpen || _EscMenuOpen)
			{
				return;
			}
			ClickCtrl.OnRightClick(value);
		}

		public void OnZoom(InputValue value)
		{
			Vector2 mouseScroll = Mouse.current.scroll.ReadValue();
			if (mouseScroll == Vector2.zero)
			{
				return;
			}
    		if (SpawnMarkerPointerCtrl.IsMarkerActive())
			{
				SpawnMarkerPointerCtrl.RotateMarker(mouseScroll.y);
			}
			else
			{
				CameraController.ChangeCameraMode(mouseScroll.y);
			}
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			if (!_TabMenuOpen && EventSystem.current.currentSelectedGameObject != null)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			_move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			_look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			_jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			_sprint = newSprintState;
		}

		// private void OnApplicationFocus(bool hasFocus)
		// {
		// 	// SetCursorState(hasFocus?!_cursorLocked:_cursorLocked);
		// 	SetCursorState(hasFocus);
		// }

		// void OnApplicationPause(bool pauseStatus)
		// {
		// 	Debug.Log("OnApplicationPause");
		// 	// isPaused = pauseStatus;
		// }		

		// private void SetCursorState(bool newState)
		// {
		// 	// Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
		// 	Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		// 	// Cursor.lockState = CursorLockMode.None;		// カーソルの動作を変更させません。
		// 	// Cursor.lockState = CursorLockMode.Locked;	// カーソルが消える(ゲームウィンドウの中央にカーソルをロックします。)			
		// 	// Cursor.lockState = CursorLockMode.Confined;		// カーソルがゲーム内からでれなくなる カーソルをゲーム内のみ表示させます。
		// }

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

		private void ChangeMousePointer()
		{
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

		public void OnEscMenu(InputValue value)
		{
            GameObject esc_menu = GameObject.Find("UIEscMenu");
            EscMenuCtrl escMenuCtrl = esc_menu.GetComponent<EscMenuCtrl>();
            _esc_menu_window = esc_menu.transform.Find("menuWindow").gameObject;
            ToggleEscMenuWindow(!_esc_menu_window.activeSelf, escMenuCtrl);
        }

        private void ToggleEscMenuWindow(bool isOn, EscMenuCtrl escMenuCtrl)
        {
            if (_esc_menu_window != null)
            {
				// _TabMenuOpen = !isOn;
                escMenuCtrl.ToggleEscMenuWindow(isOn);
				_EscMenuOpen = escMenuCtrl.GetEscMenuWindowStatus();
            }
        }

		public void OnTabMenu(InputValue value)
		{
            GameObject tab_menu = GameObject.Find("UITabMenu");
            TabMenuCtrl tabMenuCtrl = tab_menu.GetComponent<TabMenuCtrl>();
            tabMenuCtrl.ToggleTabMenuWindow();
			_TabMenuOpen = tabMenuCtrl.GetTabMenuWindowStatus();
		}

		public void OnMenuShortCut(InputValue value)
		{
			// キーボードからの入力を取得
			var keyboard = Keyboard.current;
			if (keyboard == null) return;

			// 数字キー（1から5）をチェック
			for (int i = 1; i <= 5; i++)
			{
				if (keyboard[Key.Digit1 + i - 1].wasPressedThisFrame)
				{
					// Debug.Log($"数字 {i} が入力されました");
					HandleMenuShortcut(i);
					return;
				}
			}
		}

		public void OnFunctionMenu(InputValue value)
		{
			Keyboard current = Keyboard.current;
			if (current != null)
			{
				if (current[Key.F3].wasPressedThisFrame)
				{
					GameSpeedCtrl.SetGameSpeed(1f);
				}
				else if (current[Key.F4].wasPressedThisFrame)
				{
					GameSpeedCtrl.SetGameSpeed(2f);
				}
				else if (current[Key.F5].wasPressedThisFrame)
				{
					GameSpeedCtrl.SetGameSpeed(20f);
				}
			}
		}

		private void HandleMenuShortcut(int number)
		{
			ItemAction.SelectItem(number);
		}

#endif
		public void Awake()
		{
			// initMousePointer();
		}


	}
	
}