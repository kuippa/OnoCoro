using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WindowDragCtrl : MonoBehaviour, IDragHandler, IBeginDragHandler, IEventSystemHandler
{
    private Vector2 _dragStartPosition;
    private Vector2 _panelStartPosition;
    private RectTransform _parentRectTransform;
    private Canvas _canvas;

    void Start()
    {
        if (transform.parent != null)
        {
            _parentRectTransform = transform.parent.GetComponent<RectTransform>();
        }
        _canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// ドラッグ開始時の処理
    /// ドラッグ開始位置とパネルの初期位置を記録
    /// </summary>
    /// <param name="eventData">ポインターイベントデータ</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_parentRectTransform == null)
        {
            return;
        }

        _dragStartPosition = eventData.position;
        _panelStartPosition = _parentRectTransform.anchoredPosition;
    }

    /// <summary>
    /// ドラッグ中の処理
    /// ドラッグの移動量だけパネルを移動
    /// </summary>
    /// <param name="eventData">ポインターイベントデータ</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (_parentRectTransform == null || _canvas == null)
        {
            return;
        }

        Vector2 currentPosition = eventData.position;
        Vector2 dragDelta = currentPosition - _dragStartPosition;
        
        // Canvasのスケールを考慮
        float scaleFactor = _canvas.scaleFactor;
        dragDelta = dragDelta / scaleFactor;

        _parentRectTransform.anchoredPosition = _panelStartPosition + dragDelta;
    }
}
