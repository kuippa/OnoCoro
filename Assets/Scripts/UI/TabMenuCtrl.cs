using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using CommonsUtility;

public class TabMenuCtrl : MonoBehaviour
{
    internal bool _item_box_open = true;
    private ItemCreateCtrl _item_create_window = null;
    private InfoWindowCtrl _infoWindowCtrl = null;

    public void ToggleTabMenuWindow()
    {
        GameObject itemtoolbar = GameObject.Find("UIItem");
        if (itemtoolbar != null)
        {
            RectTransform rect = itemtoolbar.transform.Find("ItemBox").GetComponent<RectTransform>();
            // 250 と 60でトグル
            if (!_item_box_open) {
                rect.sizeDelta = new Vector2(260f, 252f);
                ToggleCreateWindow(true);
                // ITemBox -> BG -> ItemList -> Item_0  Item_0 にフォーカスを当てる
                RectTransform item_list = rect.transform.Find("BG").Find("ItemList").GetComponent<RectTransform>();
                EventSystem.current.SetSelectedGameObject(item_list.transform.GetChild(0).gameObject);
            }
            else
            {
                rect.sizeDelta = new Vector2(260f, 60f);
                ToggleCreateWindow(false);
                CloseUIInfoWindow();
                EventSystem.current.SetSelectedGameObject(null);
                SpawnMarkerPointerCtrl.SetMarkerActive(false);                
            }
            rect.anchoredPosition = new Vector3(rect.anchoredPosition.x, -1*rect.sizeDelta.y/2, 0f);
            _item_box_open = !_item_box_open;
        }
    }

    public bool GetTabMenuWindowStatus()
    {
        return _item_box_open;
    }

    private void CloseUIInfoWindow()
    {
        if (_infoWindowCtrl == null)
        {
            GameObject uiInfo = GameObject.Find("UIInfo");
            if (uiInfo == null)
            {
                return;
            }
            _infoWindowCtrl = uiInfo.transform.GetComponent<InfoWindowCtrl>();
        }
        _infoWindowCtrl.ToggleInfoWindow(false);
    }

    private void ToggleCreateWindow(bool isOn)
    {
        if (_item_create_window == null)
        {
            GameObject createWindow = GameObject.Find("UIItemCreate");
            if (createWindow == null)
            {
                return;
            }
            _item_create_window = createWindow.transform.GetComponent<ItemCreateCtrl>();
        }
        _item_create_window.SwitchActive(isOn);

        // tooltipも消す
        GameObject[] tooltips = GameObject.FindGameObjectsWithTag("UIToolTips");
        foreach (GameObject tooltip in tooltips)
        {
            Destroy(tooltip);
        }

    }

    void Awake()
    {
        ToggleTabMenuWindow();
    }
}
