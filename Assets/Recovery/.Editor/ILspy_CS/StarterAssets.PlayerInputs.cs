// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// StarterAssets.PlayerInputs
using AppCamera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
	private GameObject _esc_menu_window;

	private const float _click_limit_distance = 20f;

	[Header("Character Input Values")]
	public Vector2 _move;

	public Vector2 _look;

	public bool _jump;

	public bool _sprint;

	[Header("Movement Settings")]
	public bool _analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool _TabMenuOpen;

	public bool _EscMenuOpen;

	public void OnMove(InputValue value)
	{
		SetCursorPointer(newState: false);
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		SetCursorPointer(newState: true);
		if (!_TabMenuOpen && !_EscMenuOpen)
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
		if (!_TabMenuOpen && !_EscMenuOpen)
		{
			ClickCtrl.OnLeftClick(value);
		}
	}

	public void OnRightClick(InputValue value)
	{
		if (!_TabMenuOpen && !_EscMenuOpen)
		{
			ClickCtrl.OnRightClick(value);
		}
	}

	public void OnZoom(InputValue value)
	{
		Vector2 vector = Mouse.current.scroll.ReadValue();
		if (!(vector == Vector2.zero))
		{
			if (SpawnMarkerPointerCtrl.IsMarkerActive())
			{
				SpawnMarkerPointerCtrl.RotateMarker(vector.y);
			}
			else
			{
				CameraCtrl.ChangeCameraMode(vector.y);
			}
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

	private void SetCursorPointer(bool newState)
	{
		CursorLockMode cursorLockMode = ((!newState) ? CursorLockMode.Locked : CursorLockMode.None);
		if (Cursor.lockState != cursorLockMode)
		{
			Cursor.lockState = cursorLockMode;
		}
	}

	private void ChangeMousePointer()
	{
		CursorMode cursorMode = CursorMode.Auto;
		Vector2 zero = Vector2.zero;
		Texture2D texture = Resources.Load<Texture2D>("imgs/icons/iconaddedlocal");
		if (Cursor.lockState != CursorLockMode.Locked)
		{
			Debug.Log("Cursor.SetCursor");
			Cursor.SetCursor(texture, zero, cursorMode);
		}
	}

	public void OnEscMenu(InputValue value)
	{
		GameObject gameObject = GameObject.Find("UIEscMenu");
		EscMenuCtrl component = gameObject.GetComponent<EscMenuCtrl>();
		_esc_menu_window = gameObject.transform.Find("menuWindow").gameObject;
		ToggleEscMenuWindow(!_esc_menu_window.activeSelf, component);
	}

	private void ToggleEscMenuWindow(bool isOn, EscMenuCtrl escMenuCtrl)
	{
		if (_esc_menu_window != null)
		{
			escMenuCtrl.ToggleEscMenuWindow(isOn);
			_EscMenuOpen = escMenuCtrl.GetEscMenuWindowStatus();
		}
	}

	public void OnTabMenu(InputValue value)
	{
		TabMenuCtrl component = GameObject.Find("UITabMenu").GetComponent<TabMenuCtrl>();
		component.ToggleTabMenuWindow();
		_TabMenuOpen = component.GetTabMenuWindowStatus();
	}

	public void OnMenuShortCut(InputValue value)
	{
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return;
		}
		for (int i = 1; i <= 5; i++)
		{
			if (current[(Key)(41 + i - 1)].wasPressedThisFrame)
			{
				HandleMenuShortcut(i);
				break;
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

	public void Awake()
	{
	}
}
