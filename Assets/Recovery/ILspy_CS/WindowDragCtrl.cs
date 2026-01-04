// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// WindowDragCtrl
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragCtrl : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	public void OnDrag(PointerEventData eventData)
	{
		Vector2 vector = base.transform.parent.position - base.transform.position;
		base.transform.parent.position = eventData.position + vector;
	}
}
