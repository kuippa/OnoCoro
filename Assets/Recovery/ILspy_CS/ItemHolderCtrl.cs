// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ItemHolderCtrl
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemHolderCtrl : MonoBehaviour, IDragHandler, IEventSystemHandler, IEndDragHandler, IDropHandler
{
	private const string _PLACEMENT_NAME = "Item_";

	private ItemStruct _item;

	private Vector2 _prevPos;

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 position = new Vector3(eventData.position.x, eventData.position.y, base.transform.position.z);
		base.transform.position = position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			if (!(base.name == item.gameObject.GetComponentInParent<ItemHolderCtrl>().name) && !(item.gameObject.GetComponentInParent<ItemHolderCtrl>() == null) && item.gameObject.GetComponentInParent<ItemHolderCtrl>().name.StartsWith("Item_"))
			{
				int siblingIndex = base.transform.GetSiblingIndex();
				int siblingIndex2 = item.gameObject.GetComponentInParent<ItemHolderCtrl>().transform.GetSiblingIndex();
				ChangeSiblingIndex(siblingIndex2);
				item.gameObject.GetComponentInParent<ItemHolderCtrl>().ChangeSiblingIndex(siblingIndex);
				break;
			}
		}
		LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
	}

	public void OnDrop(PointerEventData eventData)
	{
	}

	private void Awake()
	{
		EventTrigger eventTrigger = base.gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.Select
		};
		entry.callback.AddListener(delegate
		{
			OnClickItemHolder();
		});
		eventTrigger.triggers.Add(entry);
	}

	private void ChangeIcon(Sprite sprite)
	{
		Image componentInChildren = base.transform.Find("pnlHolder").Find("Item_icon").GetComponentInChildren<Image>();
		componentInChildren.sprite = sprite;
		if (componentInChildren.sprite != null)
		{
			Color color = componentInChildren.color;
			color.a = 1f;
			componentInChildren.color = color;
		}
		else
		{
			Color color2 = componentInChildren.color;
			color2.a = 0f;
			componentInChildren.color = color2;
		}
	}

	private int GetFreeItemIndex()
	{
		if (_item.Name == null)
		{
			return base.transform.GetSiblingIndex();
		}
		return -1;
	}

	internal int GetAppendableItemIndex(ItemStruct item)
	{
		int freeItemIndex = GetFreeItemIndex();
		if (freeItemIndex == -1 && _item.Name == item.Name)
		{
			return base.transform.GetSiblingIndex();
		}
		return freeItemIndex;
	}

	private void SetAltText(ItemStruct item)
	{
		Transform transform = base.transform.Find("pnlHolder").Find("Item_icon").Find("txtAlt");
		if (transform != null)
		{
			transform.gameObject.GetComponent<Text>().text = item.Info;
		}
	}

	internal void AddItemToHolder(ItemStruct item)
	{
		if (_item.Name == item.Name)
		{
			_item.AddStack(item.Stack);
			SetStackText();
			if (_item.Stack == 0)
			{
				_item = default(ItemStruct);
				ChangeIcon(null);
				SetAltText(_item);
				SetActiveImageFlag(active: false);
			}
		}
		else
		{
			_item = item;
			Sprite sprite = Resources.Load<Sprite>(item.ItemIconPath);
			ChangeIcon(sprite);
			SetAltText(item);
		}
	}

	internal ItemStruct GetItem()
	{
		return _item;
	}

	internal void ChangeSiblingIndex(int index)
	{
		base.transform.SetSiblingIndex(index);
		base.name = "Item_" + base.transform.GetSiblingIndex();
		base.transform.Find("txtIndex").GetComponentInChildren<Text>().text = (index + 1).ToString();
		SetStackText();
	}

	private void OnClickItemHolder()
	{
		int siblingIndex = base.transform.GetSiblingIndex();
		Debug.Log("SelectItemByClick: " + base.name + " :" + siblingIndex);
		ItemAction.SelectItem(siblingIndex + 1);
	}

	internal ItemStruct SelectItem()
	{
		ItemStruct item = GetItem();
		Button component = GetComponent<Button>();
		if (component == null)
		{
			return item;
		}
		if (item.Name != null)
		{
			if (!LoupeCtrl.IsLoupe(item.Name))
			{
				SpawnMarkerPointerCtrl.SetMarkerActive(isActive: true);
			}
			component.Select();
			SetActiveImageFlag(active: true);
		}
		else
		{
			SpawnMarkerPointerCtrl.SetMarkerActive(isActive: false);
			component.Select();
			component.OnDeselect(null);
			SetActiveImageFlag(active: false);
		}
		return item;
	}

	private void SetStackText()
	{
		if (_item.Stack > 1)
		{
			base.transform.Find("txtStack").GetComponentInChildren<Text>().text = _item.Stack.ToString();
		}
		else
		{
			base.transform.Find("txtStack").GetComponentInChildren<Text>().text = "";
		}
	}

	private void SetActiveImageFlag(bool active)
	{
		GameObject[] allActiveImageObjects = GetAllActiveImageObjects();
		GameObject gameObject = base.transform.Find("pnlHolder").Find("active_Image").gameObject;
		GameObject[] array = allActiveImageObjects;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2 == null)
			{
				continue;
			}
			Image component = gameObject2.GetComponent<Image>();
			if (!(component == null))
			{
				if (gameObject == gameObject2)
				{
					component.enabled = active;
				}
				else
				{
					component.enabled = false;
				}
			}
		}
	}

	private GameObject[] GetAllActiveImageObjects()
	{
		return (from t in Object.FindObjectsOfType<Transform>()
			where t.name == "active_Image"
			select t.gameObject).ToArray();
	}
}
