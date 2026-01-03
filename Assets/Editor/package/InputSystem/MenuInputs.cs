using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif


namespace StarterAssets
{
    public class MenuInputs : MonoBehaviour
    {
        private bool _item_box_open = false;
        // private GameObject _item_create_window = null;
        private ItemCreateCtrl _item_create_window = null;
        private GameObject _esc_menu_window = null;


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		// public void OnMove(InputValue value)
		// {
		// 	// 他の移動キーを押しながら移動中にさらに呼び出された場合

		// 	SetCursorPointer(false);
		// 	MoveInput(value.Get<Vector2>());
		// }

		public void OnEscMenu(InputValue value)
		{
            Debug.Log("OnEscMenu");
            // EscMenuCtrl escMenuCtrl = GameObject.Find("UIEscMenu").GetComponent<EscMenuCtrl>();

            GameObject esc_menu = GameObject.Find("UIEscMenu");
           EscMenuCtrl escMenuCtrl = esc_menu.GetComponent<EscMenuCtrl>();

            // esc_menu.transform.Find("menuWindow").gameObject.SetActive(true);
            // esc_menu の 子供オブジェクト menuWindow を取得
            _esc_menu_window = esc_menu.transform.Find("menuWindow").gameObject;
            ToggleEscMenuWindow(!_esc_menu_window.activeSelf, escMenuCtrl);
        }

        private void ToggleEscMenuWindow(bool isOn, EscMenuCtrl escMenuCtrl)
        {
            if (_esc_menu_window != null)
            {
                // _esc_menu_window.SetActive(isOn);
                escMenuCtrl.ToggleEscMenuWindow(isOn);
            }
        }

		public void OnTabMenu(InputValue value)
		{
            // Debug.Log("OnTabMenu");
            GameObject itemtoolbar = GameObject.Find("UIItem");
            if (itemtoolbar != null)
            {
                RectTransform rect = itemtoolbar.transform.Find("ItemBox").GetComponent<RectTransform>();
                // Debug.Log(rect.transform.localPosition.x + " : " + rect.transform.localPosition.y + " h:" + rect.sizeDelta.y + " anchoredPosition:" + rect.anchoredPosition);
                // 250 と 60でトグル
                if (_item_box_open) {
                    rect.sizeDelta = new Vector2(260f, 252f);
                    ToggleCreateWindow(true);
                }
                else
                {
                    rect.sizeDelta = new Vector2(260f, 60f);
                    ToggleCreateWindow(false);
                    CloseUIInfoWindow();
                }
                // Debug.Log(rect.transform.localPosition.x + " : " + rect.transform.localPosition.y + " h:" + rect.sizeDelta.y + " anchoredPosition:" + rect.anchoredPosition);

                // rect.transform.localPosition = new Vector3(125f, -30f);
                // rect.transform.position = new Vector3(125f, -30f, 0f);
                // rect.transform.position = new Vector3(rect.transform.localPosition.x, rect.transform.localPosition.y, 0f);
                // rect.transform.localPosition = new Vector3(rect.transform.localPosition.x, rect.transform.localPosition.y, 0f);
                rect.anchoredPosition = new Vector3(rect.anchoredPosition.x, -1*rect.sizeDelta.y/2, 0f);
                _item_box_open = !_item_box_open;
            }
			// JumpInput(value.isPressed);
		}

        private void CloseUIInfoWindow()
        {
            GameObject uiInfo = GameObject.Find("UIInfo");
            if (uiInfo != null)
            {
                uiInfo.SetActive(false);
            }
        }


        private void ToggleCreateWindow(bool isOn)
        {
            // GameObject createWindow = ItemCreateCtrl._item_create_window;
            // if (createWindow != null)
            // {
            //     createWindow.SetActive(isOn);
            // }

            if (_item_create_window == null)
            {
                GameObject parentGameObject = GameObject.Find("GameInterface");
                if (parentGameObject == null)
                {
                    Debug.Log("GameInterface not found");
                    return;
                }
                GameObject createWindow = parentGameObject.transform.Find("UIItemCreate").gameObject;
                // _item_create_window = createWindow;
                _item_create_window = createWindow.transform.GetComponent<ItemCreateCtrl>();

            }

            // _item_create_window.SetActive(isOn);
            // _item_create_window.SwitchActive(isOn);
            _item_create_window.SwitchActive(!_item_create_window.gameObject.activeSelf);

        }



#endif
    

    }
}
