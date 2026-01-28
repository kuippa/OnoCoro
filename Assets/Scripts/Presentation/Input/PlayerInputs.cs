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
		private TabMenuCtrl _tabMenuCtrl = null;
		private EscMenuCtrl _escMenuCtrl = null;


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
			
			if(!IsMenuOpen())
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
			if (IsMenuOpen())
			{
				return;
			}
			ClickCtrl.OnLeftClick(value);
		}

		public void OnRightClick(InputValue value)
		{
			if (IsMenuOpen())
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
			if (!IsMenuOpen() && EventSystem.current.currentSelectedGameObject != null)
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
            _escMenuCtrl = GetEscMenuCtrl();
            _escMenuCtrl.ToggleEscMenuWindow(!_escMenuCtrl.GetEscMenuWindowStatus());
        }

		private EscMenuCtrl GetEscMenuCtrl()
		{
			if (_escMenuCtrl != null)
			{
				return _escMenuCtrl;
			}
            GameObject esc_menu = GameObject.Find("UIEscMenu");
            _escMenuCtrl = esc_menu.GetComponent<EscMenuCtrl>();
			return _escMenuCtrl;
		}

		private TabMenuCtrl GetTabMenuCtrl()
		{
			if (_tabMenuCtrl != null)
			{
				return _tabMenuCtrl;
			}
            GameObject tab_menu = GameObject.Find("UITabMenu");
            _tabMenuCtrl = tab_menu.GetComponent<TabMenuCtrl>();
			return _tabMenuCtrl;
		}

		internal bool IsMenuOpen()
		{
			bool tabMenuOpen = false;
			if (_tabMenuCtrl == null)
			{
				_tabMenuCtrl = GetTabMenuCtrl();
			}
			if (_tabMenuCtrl != null)
			{
				tabMenuOpen = _tabMenuCtrl.GetTabMenuWindowStatus();
			}

			bool escMenuOpen = false;
			if (_escMenuCtrl != null)
			{
				escMenuOpen = _escMenuCtrl.GetEscMenuWindowStatus();
			}

			if (tabMenuOpen || escMenuOpen)
			{
				return true;
			}
			return false;
		}


		public void OnTabMenu(InputValue value)
		{
			_tabMenuCtrl = GetTabMenuCtrl();
			_tabMenuCtrl.ToggleTabMenuWindow();
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
