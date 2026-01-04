// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ClickCtrl
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickCtrl : MonoBehaviour
{
	private const float _click_limit_distance = 20f;

	private bool _isProcessingClick;

	private static ClickCtrl _instance;

	public static ClickCtrl Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindFirstObjectByType<ClickCtrl>();
				if (_instance == null)
				{
					_instance = new GameObject("ClickCtrl").AddComponent<ClickCtrl>();
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public static void OnRightClick(InputValue value)
	{
		if (!Instance._isProcessingClick)
		{
			Instance._isProcessingClick = true;
			Instance.StartCoroutine(Instance.ProcessRightClickNextFrame());
		}
	}

	private IEnumerator ProcessRightClickNextFrame()
	{
		yield return null;
		if (!CheckAndCloseNoticeWindow())
		{
			Instance._isProcessingClick = false;
			yield break;
		}
		if (IsPointerOverUIObject())
		{
			Instance._isProcessingClick = false;
			yield break;
		}
		LoupeCtrl.ActLoupe();
		Instance._isProcessingClick = false;
	}

	public static void OnLeftClick(InputValue value)
	{
		if (!Instance._isProcessingClick)
		{
			Instance._isProcessingClick = true;
			Instance.StartCoroutine(Instance.ProcessLeftClickNextFrame());
		}
	}

	private bool IsPointerOverUIObject()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Mouse.current.position.ReadValue()
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		if (list.Count > 0)
		{
			Debug.Log("raycastResults.Count:" + list.Count + list[0].gameObject.name);
		}
		return list.Count > 0;
	}

	private IEnumerator ProcessLeftClickNextFrame()
	{
		yield return null;
		if (!CheckAndCloseNoticeWindow())
		{
			Instance._isProcessingClick = false;
			yield break;
		}
		if (IsPointerOverUIObject())
		{
			Debug.Log("UI上でのクリック");
			Instance._isProcessingClick = false;
			yield break;
		}
		if (LoupeCtrl.IsLoupe(ItemAction.GetSelectedItemName()))
		{
			Debug.Log("ルーペモード中のクリック");
			LoupeCtrl.ActLoupe();
		}
		else if (SpawnMarkerPointerCtrl.IsMarkerActive())
		{
			Debug.Log("マーカーアクティブ中のクリック");
			ItemAction.ActItemUse();
		}
		else
		{
			Debug.Log("マーカー非アクティブ中のクリック");
		}
		Instance._isProcessingClick = false;
	}

	private static bool CheckAndCloseNoticeWindow()
	{
		GameObject gameObject = GameObject.Find("UINotice");
		if (gameObject != null)
		{
			NoticeCtrl component = gameObject.GetComponent<NoticeCtrl>();
			if (component.IsNoticeWindowActive())
			{
				component.ToggleNoticeWindow(isOn: false);
				return false;
			}
		}
		return true;
	}

	private static void CreateBonFire(Ray PointRay, RaycastHit hit)
	{
		Debug.DrawRay(PointRay.origin, PointRay.direction * 10f, Color.red, 5f);
		GameObject original = Resources.Load<GameObject>("Prefabs/Bonfire");
		Vector3 position = hit.collider.gameObject.transform.position;
		MeshFilter component = hit.collider.gameObject.GetComponent<MeshFilter>();
		if (component != null)
		{
			position += Vector3.up * component.mesh.bounds.size.y;
		}
		else
		{
			position += Vector3.up;
		}
		GameObject gameObject = Object.Instantiate(rotation: hit.collider.gameObject.transform.localRotation, original: original, position: position);
		Transform parent = hit.collider.gameObject.transform.parent;
		if (parent != null)
		{
			gameObject.transform.SetParent(parent.transform);
		}
	}

	private static void DebugMeshInfo(RaycastHit hit, Vector3 setPoint)
	{
		Vector3 vector = setPoint;
		Debug.Log("localPosition:" + vector.ToString());
		Debug.Log("position:" + hit.collider.gameObject.transform.position.ToString());
		MeshFilter component = hit.collider.gameObject.GetComponent<MeshFilter>();
		Debug.Log("MeshFilter.bounds.size.y:" + component.mesh.bounds.size.y);
		Transform transform = GameObject.Find("PlayerArmature").transform;
		Debug.Log("amarture :" + transform.localPosition.ToString());
		Debug.Log("MeshFilter size:" + component.mesh.bounds.size.ToString());
		Renderer component2 = hit.collider.gameObject.GetComponent<Renderer>();
		Debug.Log("Renderer size:" + component2.bounds.size.ToString());
		Debug.Log("MeshFilter extents:" + component.mesh.bounds.extents.ToString());
		Debug.Log("MeshFilter center:" + component.mesh.bounds.center.ToString());
		Debug.Log("Renderer center:" + component2.bounds.center.ToString());
		Debug.Log("MeshFilter max:" + component.mesh.bounds.max.ToString());
		Debug.Log("MeshFilter min:" + component.mesh.bounds.min.ToString());
		Debug.Log("Renderer max:" + component2.bounds.max.ToString());
		Debug.Log("Renderer min:" + component2.bounds.min.ToString());
		Vector3 localPosition = hit.collider.gameObject.transform.localPosition;
		Debug.Log("localPosition - center:" + (localPosition - component.mesh.bounds.center).ToString());
		Debug.Log("localPosition + center:" + (localPosition + component.mesh.bounds.center).ToString());
		Debug.Log("localPosition - size:" + (localPosition - component.mesh.bounds.size).ToString());
		Debug.Log("localPosition + size:" + (localPosition + component.mesh.bounds.size).ToString());
		Debug.Log("Contains:" + component.mesh.bounds.Contains(setPoint));
		Debug.Log("ClosestPoint:" + component.mesh.bounds.ClosestPoint(setPoint).ToString());
		Debug.Log("Submeshes: " + component.mesh.subMeshCount);
		Debug.Log("isReadable: " + component.mesh.isReadable);
	}
}
