using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CommonsUtility;
// using UnityEngine.Rendering.Universal.Internal;

public class MouseOverTipsCtrl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    float _time = 0.0f;
    bool _onMouseOver = false;
    GameObject _tooltip = null;
    const float _tooltip_x_buffer = 40.0f;
    const float _tooltip_y_buffer = -20.0f;


    [SerializeField]
    public GameObject _pointer = null;


    // // 3Dオブジェクトなどでマウスオーバーした時に
    // void OnMouseOver()
    // {
    //     //If your mouse hovers over the GameObject with the script attached, output this message
    //     Debug.Log("Mouse is over GameObject.");
    // }

    // Imageなどでマウスオーバーした時に
    public void OnPointerEnter( PointerEventData eventData )
    {
        // Debug.Log("Mouse is over OnPointerEnter." + this.gameObject.name);
        _time = 0.0f;
        _onMouseOver = true;
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        // Debug.Log("Mouse is over OnPointerExit.");
        _time = 0.0f;
        _onMouseOver = false;
        RemoveTooltip();
    }

    private string GetTooltipText()
    {
        string text = "";
        Transform unit = this.transform.Find("txtAlt");
        if (unit != null)
        {
            Text text1 = unit.gameObject.GetComponent<Text>();
            text = text1.text;
        }
        // Debug.Log("text: " + text + this.name);

        return text;
    }

    private void SetTooltip()
    {
        _onMouseOver = false;
        string tooltipText = GetTooltipText();
        if (_tooltip != null || tooltipText == "")
        {
            return;
        }

        GameObject tooltips = Resources.Load<GameObject>("Prefabs/UI/UIToolTips");
        GameObject unit = Instantiate(tooltips);

        GameObject UIInfo = GameObject.Find(GlobalConst.UI_INFO_OBJ_NAME);
        if (UIInfo != null)
        {
            unit.transform.SetParent(UIInfo.transform);
        }
        else
        {
            unit.transform.SetParent(this.transform.parent);
            // unit.transform.SetParent(gameObject.transform.root);
            // unit.transform.SetParent(this.transform);
        }
        // tag UIToolTips を付与する
        unit.tag = "UIToolTips";
        GameObject tooltip = unit.transform.Find("ToolTip").gameObject;
        tooltip.transform.localScale = new Vector3(1, 1, 1);
        RectTransform rect = tooltip.GetComponent<RectTransform>();
        float width = rect.sizeDelta.x;
        Vector2 mousePos = GetPointerPosition();
        rect.anchoredPosition = new Vector2(mousePos.x + width / 2 + _tooltip_x_buffer, mousePos.y + _tooltip_y_buffer);
        // Debug.Log("mousePos:" + mousePos);
        // unit 子供オブジェクトの txtTips の text を変更する
        tooltip.transform.Find("txtTips").gameObject.GetComponent<Text>().text = tooltipText;
        _tooltip = unit;
    }

    // private void UpdateTooltip()
    // {
    //     GameObject tooltipParent = _tooltip.transform.parent.gameObject;
    //     // Debug.Log("tooltipParent: " + tooltipParent.activeInHierarchy + " " + tooltipParent.activeSelf);
    //     if (tooltipParent.activeInHierarchy == false)
    //     {
    //         _time = 0.0f;
    //         _onMouseOver = false;
    //         RemoveTooltip();
    //     }
    // }

    private void RemoveTooltip()
    {
        if (_tooltip != null)
        {
            Destroy(_tooltip);
        }
    }

    private void debug_pointer()
    {
        if (_pointer == null)
        {
            return;
        }
        // Cursor.visible = true;
        // Cursor.lockState = CursorLockMode.None;

        // Vector3 point = new Vector3();
        // Event   currentEvent = Event.current;
        // Vector2 mousePos = new Vector2();
        // Camera cam = Camera.main;

        // mousePos.x = currentEvent.mousePosition.x;
        // mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;
        // point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));


        // Vector2 pos = Input.mousePosition;
        // // Vector3 setPos = Camera.main.ScreenToWorldPoint(pos);
        // Vector3 setPos = pos;

        // setPos = new Vector3(setPos.x, setPos.y, 1);
        // _pointer.transform.position = Input.mousePosition;
        // _pointer.transform.position = setPos;
        // _pointer.GetComponent<RectTransform>().anchoredPosition = setPos;

        // _pointer.GetComponent<RectTransform>().position = setPos;
        // _pointer.GetComponent<RectTransform>().rect.Set(setPos.x, setPos.y, 0, 0);

        // _pointer.GetComponent<RectTransform>().localPosition = setPos;

        // _pointer.transform.position = point;

        // RectTransform canvasRect = this.gameObject.GetComponent<RectTransform>();
        // RectTransform canvasRect = this.transform.root.gameObject.GetComponent<RectTransform>();

        // RectTransform canvasRect = this.transform.Find("Canvas").gameObject.GetComponent<RectTransform>();
        // RectTransform canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();

        RectTransform canvasRect = this.transform.root.gameObject.GetComponentInChildren<RectTransform>();


        Canvas canvas = canvasRect.GetComponent<Canvas>();
        // Canvas canvas = canvasRect.GetComponentInChildren<Canvas>();
        Vector2 MousePos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out MousePos);
        _pointer.GetComponent<RectTransform>().anchoredPosition = new Vector2(MousePos.x, MousePos.y);

        // Debug.Log("_pointer: " + Input.mousePosition + " setPos: " + setPos);
        // Debug.Log("_pointer: " + Input.mousePosition + " point: " + point);
        // Debug.Log("_pointer: " + Input.mousePosition + " MousePos: " + MousePos);
    }

    private Vector2 GetPointerPosition()
    {
        RectTransform canvasRect = this.transform.root.gameObject.GetComponentInChildren<RectTransform>();
        Canvas canvas = canvasRect.GetComponent<Canvas>();
        Vector2 MousePos = Vector2.zero;
        // TODO: Input.mousePosition でいいのか？
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out MousePos);
        // _pointer.GetComponent<RectTransform>().anchoredPosition = new Vector2(MousePos.x, MousePos.y);

        return MousePos;
    }

    // void OnGUI()
    // {
    //     Debug.Log("OnGUI");
    // }

    void FixedUpdate()
    {
        if (_onMouseOver)
        {
            _time += Time.deltaTime;
            if (_time > GlobalConst.TOOL_TIP_TIME)
            {
                // Debug.Log("Mouse is over GameObject over 3s.");
                SetTooltip();
            }
        }
        // if (_tooltip != null)
        // {
        //     UpdateTooltip();    // 親が非表示になったら削除する
        // }

        // debug_pointer();
    }

}
