#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.EventSystems;
#endif
using UnityEngine;
using Debug = CommonsUtility.Debug;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using CommonsUtility;
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
			CursorManager.SetCursorLockMode(false);
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			CursorManager.SetCursorLockMode(true);
			// マウスカーソルがゲーム画面外の場合は LookInput を発火させない
			if (!IsMouseInGameWindow())
			{
				LookInput(Vector2.zero);  // 前フレームの入力値をクリア
				return;
			}
			
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
					GameSpeedManager.SetGameSpeed(1f);
				}
				else if (current[Key.F4].wasPressedThisFrame)
				{
					GameSpeedManager.SetGameSpeed(2f);
				}
				else if (current[Key.F5].wasPressedThisFrame)
				{
					GameSpeedManager.SetGameSpeed(20f);
				}
			}
		}

		private void HandleMenuShortcut(int number)
		{
			ItemAction.SelectItem(number);
		}

		/// <summary>
		/// マウスカーソルがゲーム画面内にあるかチェック
		/// </summary>
		private bool IsMouseInGameWindow()
		{
			Vector3 mousePos = Input.mousePosition;
			
			// マウス位置がスクリーン座標の範囲内にあるか確認
			bool isInBounds = mousePos.x >= 0 && mousePos.x <= Screen.width &&
							  mousePos.y >= 0 && mousePos.y <= Screen.height;
			
			return isInBounds;
		}

#endif
		public void Awake()
		{
			// カーソル管理の初期化
			CursorManager.Initialize();
		}


	}
	
}
