using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

using Cinemachine;


namespace StarterAssets
{
	public class CameraInputs : MonoBehaviour
	{
		[Header("CameraInputs Input Values")]
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

			// SetCursorPointer(false);
			MoveInput(value.Get<Vector2>());
		}

#endif
		public void MoveInput(Vector2 newMoveDirection)
		{
			// move = newMoveDirection;

            // Vector3 inputDirection = new Vector3(newMoveDirection.x, 0.0f, newMoveDirection.y).normalized;
            Vector3 inputDirection = new Vector3(newMoveDirection.x/10, 0.0f, newMoveDirection.y/10).normalized;
        	float _targetRotation = 0.0f;
			GameObject _mainCamera;
			// _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			_mainCamera = GameObject.FindGameObjectWithTag("FreeLookCamera");
			// float _rotationVelocity = 0;
			// float RotationSmoothTime = 0.12f;
            // if (newMoveDirection != Vector2.zero)
            // {
            //     _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
            //                       _mainCamera.transform.eulerAngles.y;
            //     float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
            //         RotationSmoothTime);

            //     // rotate to face input direction relative to camera position
            //     transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            // }

            // Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
			CharacterController _controller;
            _controller = GetComponent<CharacterController>();
			// float _speed = 2;
			// float _verticalVelocity = 2f;

            // // move the player
            // _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
            //                  new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f));
            _controller.Move(inputDirection);



		} 

		public void Awake()
		{
			// initMousePointer();
		}


	}
	
}