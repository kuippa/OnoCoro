// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// TabMenuCtrl
using UnityEngine;
using UnityEngine.EventSystems;

public class TabMenuCtrl : MonoBehaviour
{
	internal bool _item_box_open = true;

	private ItemCreateCtrl _item_create_window;

	private InfoWindowCtrl _infoWindowCtrl;

	public void ToggleTabMenuWindow()
	{
		GameObject gameObject = GameObject.Find("UIItem");
		if (gameObject == null)
		{
			ToggleCreateWindow(isOn: false);
			return;
		}
		RectTransform component = gameObject.transform.Find("ItemBox").GetComponent<RectTransform>();
		if (!_item_box_open)
		{
			component.sizeDelta = new Vector2(260f, 252f);
			ToggleCreateWindow(isOn: true);
			RectTransform component2 = component.transform.Find("BG").Find("ItemList").GetComponent<RectTransform>();
			EventSystem.current.SetSelectedGameObject(component2.transform.GetChild(0).gameObject);
		}
		else
		{
			component.sizeDelta = new Vector2(260f, 60f);
			ToggleCreateWindow(isOn: false);
			CloseUIInfoWindow();
			EventSystem.current.SetSelectedGameObject(null);
			SpawnMarkerPointerCtrl.SetMarkerActive(isActive: false);
		}
		component.anchoredPosition = new Vector3(component.anchoredPosition.x, -1f * component.sizeDelta.y / 2f, 0f);
		_item_box_open = !_item_box_open;
	}

	public bool IsTabMenuWindowOpen()
	{
		return _item_box_open;
	}

	public bool GetTabMenuWindowStatus()
	{
		return _item_box_open;
	}

	private void CloseUIInfoWindow()
	{
		if (_infoWindowCtrl == null)
		{
			GameObject gameObject = GameObject.Find("UIInfo");
			if (gameObject == null)
			{
				return;
			}
			_infoWindowCtrl = gameObject.transform.GetComponent<InfoWindowCtrl>();
		}
		_infoWindowCtrl.ToggleInfoWindow(isActive: false);
	}

	private void ToggleCreateWindow(bool isOn)
	{
		if (_item_create_window == null)
		{
			GameObject gameObject = GameObject.Find("UIItemCreate");
			if (gameObject == null)
			{
				return;
			}
			_item_create_window = gameObject.transform.GetComponent<ItemCreateCtrl>();
		}
		_item_create_window.SwitchActive(isOn);
		GameObject gameObject2 = GameObject.Find("UIBG_mask");
		if (isOn)
		{
			gameObject2.transform.GetComponent<CanvasGroup>().alpha = 0.8f;
			gameObject2.transform.GetComponent<CanvasGroup>().blocksRaycasts = true;
		}
		else
		{
			gameObject2.transform.GetComponent<CanvasGroup>().alpha = 0f;
			gameObject2.transform.GetComponent<CanvasGroup>().blocksRaycasts = false;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("UIToolTips");
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
	}

	private void Start()
	{
		ToggleTabMenuWindow();
	}

	private void Awake()
	{
	}
}
