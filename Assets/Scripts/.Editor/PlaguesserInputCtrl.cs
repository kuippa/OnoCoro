using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Video;
using System.Xml.Serialization;
// using System.Diagnostics;

public class PlaguesserInputCtrl : MonoBehaviour
{
    CinemachineVirtualCamera _vcamera;
    Cinemachine3rdPersonFollow _cinebody;    
    CinemachineVirtualCamera _birdcamera;
    Cinemachine3rdPersonFollow _birdbody;    
    // float _scroll = 0.0f;
    const bool MOVE_REVERSE = false;    // インプットの挙動が鏡写しだった場合にtrueにする
    const float MOVE_SPEED = 1.0f;  // 移動速度の定数
    const float MOVE_VELOCITY = 0.1f;  // 移動距離の定数
    const float CAMERA_DIST_MIN = -1.55f;
    const float CAMERA_DIST_MID = 20f;
    const float CAMERA_DIST_BIRD = 200f;
    const float CAMERA_DIST_MAX = 320f;
    const float CAMERA_BASE_DIST = 1.6f;
    [SerializeField]
    public GameObject _cameraTarget;


    private Vector3 MoveDirection(KeyCode keyCode)
    {
        Vector3 ret = Vector3.zero;
        float sign = 1.0f;
        if (!MOVE_REVERSE)
        {
            sign = -1.0f;
        }
        float movesize = MOVE_SPEED * MOVE_VELOCITY * sign;

        switch(keyCode)
        {
            case KeyCode.LeftArrow:
            case KeyCode.A:
                ret = new Vector3(movesize,0.0f,0.0f);
                break;
            case KeyCode.RightArrow:
            case KeyCode.D:
                ret = new Vector3(movesize * -1,0.0f,0.0f);
                break;
            case KeyCode.UpArrow:
            case KeyCode.W:
                ret = new Vector3(0.0f,0.0f,movesize * -1);
                break;
            case KeyCode.DownArrow:
            case KeyCode.S:
                ret = new Vector3(0.0f,0.0f,movesize);
                break;
            default:
                ret = Vector3.zero;
                break;

        }

        return ret;
    }


    private void Move()
    {
        // 左に移動
        if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey (KeyCode.A)) {
            _cameraTarget.transform.Translate (MoveDirection(KeyCode.LeftArrow));
        }
        // 右に移動
        if (Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.D)) {
            _cameraTarget.transform.Translate (MoveDirection(KeyCode.RightArrow));
        }
        // 前に移動
        if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.W)) {
            _cameraTarget.transform.Translate (MoveDirection(KeyCode.UpArrow));
        }
        // 後ろに移動
        if (Input.GetKey (KeyCode.DownArrow) || Input.GetKey (KeyCode.S)) {
            _cameraTarget.transform.Translate (MoveDirection(KeyCode.DownArrow));
        }

        CameraMove();
        CameraRotate();
    }

    private void CameraRotate()
    {
        // マウスのドラッグでカメラを回転させる
        if (Input.GetMouseButton(0))
        {
            Debug.Log("mouse");
            float mouseInputX = Input.GetAxis("Mouse X");
            float mouseInputY = Input.GetAxis("Mouse Y");
            _cameraTarget.transform.RotateAround(_cameraTarget.transform.position, Vector3.up, mouseInputX * 200f * Time.deltaTime);
            _cameraTarget.transform.RotateAround(_cameraTarget.transform.position, _cameraTarget.transform.right, mouseInputY * 200f * Time.deltaTime);
        }
    }

    private void CameraMove()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0)
        {
            return;
        }
        ChangeCameraMode(scroll);
    }

    private void ChangeCameraMode(float moveVec)
    {

        // FPS & TPS
        if (_cinebody.CameraDistance <= CAMERA_DIST_MID)
        {
            _birdcamera.Priority = 0;				// カメラ切り替え
            float camera_distance = GetCameraDistance(_cinebody.CameraDistance, moveVec);
            _cinebody.ShoulderOffset = new Vector3(1, 1, 0);
            _cinebody.CameraSide = 0.5f;
            _cinebody.CameraDistance = camera_distance;
            if (camera_distance > CAMERA_DIST_MID)
            {
                _birdbody.CameraDistance = camera_distance;
                // ChangeCameraMode(moveVec);
                return;
            }
        }
        // BIRD VIEW
        else
        {
            _birdcamera.Priority = 20;				// カメラ切り替え
            float camera_distance = GetCameraDistance(_birdbody.CameraDistance, moveVec);
            _birdbody.ShoulderOffset = new Vector3(1, _birdbody.CameraDistance, 0);
            _birdbody.CameraSide = 0.5f;
            _birdbody.CameraDistance = camera_distance;
            if (camera_distance <= CAMERA_DIST_MID)
            {
                _cinebody.CameraDistance = CAMERA_DIST_MID;
                // ChangeCameraMode(moveVec);
                return;
            }
        }
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

    private float GetCameraDistance(float cam_dist, float moveVec)
    {
        float set_val = CAMERA_BASE_DIST * GetCameraMoveVec(moveVec);

        // FPS
        if (cam_dist + set_val * 1/2 <= 0f)
        {
            set_val = cam_dist + set_val * 1/2;
        }
        // // TPS
        else if (cam_dist + set_val < CAMERA_DIST_MID)
        {
            set_val = cam_dist + set_val;
        }
        // BIRD VIEW
        else
        {
            set_val = cam_dist + set_val * 5;
        } 

        if (set_val <= CAMERA_DIST_MIN)
        {
            set_val = CAMERA_DIST_MIN;
        }
        else if (set_val >= CAMERA_DIST_MAX)
        {
            set_val = CAMERA_DIST_MAX;
        }

        return set_val;

    }


    void Awake()
    {
        if (_cameraTarget == null)
        {
            GameObject cameraTarget = GameObject.Find("cameraTarget");
            if (cameraTarget == null)
            {
                _cameraTarget = this.gameObject;
            }
            else
            {
                _cameraTarget = cameraTarget;
            }
        }

        if (_cameraTarget.tag == "MainCamera")
        {
            // cubeオブジェクトを追加する
            _cameraTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // cubeオブジェクトの名前を設定する
            _cameraTarget.name = "cameraTarget";
            // cubeオブジェクトの位置を設定する
            _cameraTarget.transform.position = new Vector3(0, 0, 0);
        }
        // _cameraTarget.gameObject.SetActive(false);

        Camera mainCamera = Camera.main;
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
        }        

        GameObject followCamera = GameObject.Find("FollowCamera");
        if (followCamera == null)
        {
            followCamera = new GameObject("FollowCamera");
            _vcamera = followCamera.AddComponent<CinemachineVirtualCamera>();
            _vcamera.Priority = 10;
            _vcamera.Follow = _cameraTarget.transform;
        }
        else
        {
            _vcamera = GameObject.Find("FollowCamera").GetComponent<CinemachineVirtualCamera>();
        }

        _cinebody = _vcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (_cinebody == null)
        {
            _cinebody = _vcamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        }
        _cinebody.ShoulderOffset = new Vector3(0.5f, 1, 0);
        _cinebody.CameraDistance = 3.0f;
        _cinebody.CameraSide = 0.5f;

        GameObject birdCamera = GameObject.Find("BirdViewCamera");
        if (birdCamera == null)
        {
            birdCamera = new GameObject("BirdViewCamera");
            _birdcamera = birdCamera.AddComponent<CinemachineVirtualCamera>();
            // _birdcamera.name = "BirdViewCamera";
            _birdcamera.Priority = 0;
            _birdcamera.Follow = _cameraTarget.transform;
            _birdcamera.LookAt = _cameraTarget.transform;
            Debug.Log(_birdcamera.Follow);

            // _birdcamera.LookAt = _birdcamera.transform;
            _birdcamera.m_Lens.FieldOfView = 60f;
            _birdcamera.m_Lens.FarClipPlane = 1000;
        }
        else
        {
            _birdcamera = birdCamera.GetComponent<CinemachineVirtualCamera>();
        }
        _birdcamera.Priority = 0;
        // _birdcamera.m_LookAt = _cameraTarget.transform;

        _birdbody = _birdcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (_birdbody == null)
        {
            _birdbody = _birdcamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        }
        _birdbody.ShoulderOffset = new Vector3(0.5f, -0.4f, 0);

        _birdbody.CameraDistance = 2.0f;
        _birdbody.CameraSide = 0.5f;

        // _birdcamera.GetCinemachineComponent(CinemachineCore.Stage.Aim).

        // _birdcamera の Aim を composer に設定する

        // CinemachineCore.Stage.Aim をcomposerに設定する
        CinemachineComposer composer = _birdcamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer == null)
        {
            // composer.m_TrackedObjectOffset = new Vector3(0, 0, 0);
            // composer = _birdcamera.AddCinemachineComponent<CinemachineComposer>();
            // composer を設定する
            // composer. = _cameraTarget.transform;
            _birdcamera.AddCinemachineComponent<CinemachineComposer>();

            Debug.Log("composer null"+ composer);
        }
        // else
        // {
        //     composer = _birdcamera.AddCinemachineComponent<CinemachineComposer>();
        //     composer.m_TrackedObjectOffset = new Vector3(0, 0, 0);
        //     Debug.Log("composer"+ composer);

        // }

        // Debug.Log("composer"+ composer);
        // composer.


        // CinemachineCore.Stage.Aim を"Composer"に設定する

        // _birdcamera.Set


        // Cinemachine3rdPersonAim birdAim = _birdcamera.GetCinemachineComponent<Cinemachine3rdPersonAim>();
        // if (birdAim != null)
        // {
        //     birdAim.AimTarget
        // }


    }

    void Update()
    {
        Move();        

    }
}
