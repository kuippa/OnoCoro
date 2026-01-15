// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MouseOverTipsCtrl
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOverTipsCtrl : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private float _time;

	private bool _onMouseOver;

	private GameObject _tooltip;

	private const float _tooltip_x_buffer = 40f;

	private const float _tooltip_y_buffer = -20f;

	[SerializeField]
	public GameObject _pointer;

	public void OnPointerEnter(PointerEventData eventData)
	{
		_time = 0f;
		_onMouseOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_time = 0f;
		_onMouseOver = false;
		RemoveTooltip();
	}

	private string GetTooltipText()
	{
		string result = "";
		Transform transform = base.transform.Find("txtAlt");
		if (transform != null)
		{
			result = transform.gameObject.GetComponent<Text>().text;
		}
		return result;
	}

	private void SetTooltip()
	{
		_onMouseOver = false;
		string tooltipText = GetTooltipText();
		if (!(_tooltip != null) && !(tooltipText == ""))
		{
			GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/UIToolTips"));
			GameObject gameObject2 = GameObject.Find("UIInfo");
			if (gameObject2 != null)
			{
				gameObject.transform.SetParent(gameObject2.transform);
			}
			else
			{
				gameObject.transform.SetParent(base.transform.parent);
			}
			gameObject.tag = "UIToolTips";
			GameObject obj = gameObject.transform.Find("ToolTip").gameObject;
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			RectTransform component = obj.GetComponent<RectTransform>();
			float x = component.sizeDelta.x;
			Vector2 pointerPosition = GetPointerPosition();
			component.anchoredPosition = new Vector2(pointerPosition.x + x / 2f + 40f, pointerPosition.y + -20f);
			obj.transform.Find("txtTips").gameObject.GetComponent<Text>().text = tooltipText;
			_tooltip = gameObject;
		}
	}

	private void RemoveTooltip()
	{
		if (_tooltip != null)
		{
			Object.Destroy(_tooltip);
		}
	}

	private void debug_pointer()
	{
		if (!(_pointer == null))
		{
			RectTransform componentInChildren = base.transform.root.gameObject.GetComponentInChildren<RectTransform>();
			Canvas component = componentInChildren.GetComponent<Canvas>();
			Vector2 localPoint = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(componentInChildren, Input.mousePosition, component.worldCamera, out localPoint);
			_pointer.GetComponent<RectTransform>().anchoredPosition = new Vector2(localPoint.x, localPoint.y);
		}
	}

	private Vector2 GetPointerPosition()
	{
		RectTransform componentInChildren = base.transform.root.gameObject.GetComponentInChildren<RectTransform>();
		Canvas component = componentInChildren.GetComponent<Canvas>();
		Vector2 localPoint = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(componentInChildren, Input.mousePosition, component.worldCamera, out localPoint);
		return localPoint;
	}

	private void FixedUpdate()
	{
		if (_onMouseOver)
		{
			_time += Time.deltaTime;
			if (_time > 0.7f)
			{
				SetTooltip();
			}
		}
	}
}
