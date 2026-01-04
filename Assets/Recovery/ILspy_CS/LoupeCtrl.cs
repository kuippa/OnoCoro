// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// LoupeCtrl
using UnityEngine;

public class LoupeCtrl : MonoBehaviour
{
	public static LoupeCtrl instance;

	private static ItemStruct _item;

	internal static bool IsLoupe(string item_name)
	{
		if (item_name == _item.Name)
		{
			Debug.Log("IsLoupe: " + item_name + "  _item.Name: " + _item.Name);
			return true;
		}
		return false;
	}

	internal static void ActLoupe()
	{
		GameObject gameObject = GameObject.Find("Plateau");
		if (gameObject == null)
		{
			return;
		}
		if (gameObject.GetComponent<PlateauInfoManager>().IsPlateauObject())
		{
			gameObject.GetComponent<PlateauInfoManager>().DisplayPlateauInfo();
			return;
		}
		GameObject gameObject2 = GameObject.Find("UIInfo");
		if (gameObject2 != null)
		{
			gameObject2.GetComponent<InfoWindowCtrl>().GetTargetUnit();
		}
	}

	private void OnDestory()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		ItemStruct itemStruct = default(ItemStruct);
		itemStruct = base.gameObject.AddComponent<Loupe>()._item_struct;
		base.gameObject.GetComponentInChildren<ItemListCtrl>().SetItemStruct(itemStruct);
		_item = itemStruct;
	}
}
