using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WindowDragCtrl : MonoBehaviour , IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // print($"OnDrag : {eventData}");
        // this.transform.position = eventData.position;
        // Debug.Log($"this.transform.position : {this.transform.position} + this.transform.parent.position : {this.transform.parent.position}" );
        // // this.transform.position : (368.50, 424.00, 0.00) + this.transform.parent.position : (368.50, 309.00, 0.00)
        // Debug.Log($"eventData.position : {eventData.position}");

        Vector2 pos = this.transform.parent.position - this.transform.position;
        this.transform.parent.position = eventData.position + pos;

        // TODO: 端をクリックしてもドラッグ開始で中心に来てしまうので調整



        // クリックした相対位置を取得
        // Vector2 pos = this.transform.position - eventData.position;

        // this.GetComponentInParent.<RectTransform>().position = eventData.position;
    }

        // // header にOnDragを登録
        // GameObject header = GameObject.Find("header");
        // // header.GetComponent<RectTransform>().SetAsLastSibling();
        // // header.GetComponent<RectTransform>().SetAsFirstSibling();
        // EventTrigger trigger = header.AddComponent<EventTrigger>();
        // EventTrigger.Entry entry = new EventTrigger.Entry();
        // entry.eventID = EventTriggerType.Drag;
        // entry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        // trigger.triggers.Add(entry);

    // public void OnDrag(PointerEventData eventData)
    // {

    //     //Create a ray going from the camera through the mouse position
    //     Ray ray = Camera.main.ScreenPointToRay(eventData.position);
    //     //Calculate the distance between the Camera and the GameObject, and go this distance along the ray
    //     Vector3 rayPoint = ray.GetPoint(Vector3.Distance(transform.position, Camera.main.transform.position));
    //     //Move the GameObject when you drag it
    //     transform.position = rayPoint;

    //     // if (eventData.pointerEnter != null && eventData.pointerEnter.transform as RectTransform != null)
    //     //     m_DraggingPlane = eventData.pointerEnter.transform as RectTransform;

    //     // var rt = m_DraggingIcon.GetComponent<RectTransform>();
    //     // Vector3 globalMousePos;
    //     // if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
    //     // {
    //     //     rt.position = globalMousePos;
    //     //     rt.rotation = m_DraggingPlane.rotation;
    //     // }

    //     // // マウスがヒットした地面の座標を取得
    //     // RaycastHit hit;
    //     // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     // if (Physics.Raycast(ray, out hit))
    //     // {
    //     //     // マウスがヒットした地面の座標を取得
    //     //     if (hit.collider.gameObject.layer != LayerMask.NameToLayer(EnemyEnum.LayerType.Ground.ToString()))
    //     //     {
    //     //         return;
    //     //     }
    //     //     // マーカーの座標をマウスの座標にする
    //     //     transform.position = hit.point;
    //     // }
    // }

}
