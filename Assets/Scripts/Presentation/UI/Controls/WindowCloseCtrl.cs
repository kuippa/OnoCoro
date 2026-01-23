using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class WindowCloseCtrl : MonoBehaviour
{
    private const string _TITLEBAR_NAME = "titlebar";
    void Awake()
    {
        var button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            Debug.LogWarning("WindowCloseCtrl: Button component not found.");
            return;
        }

        // Inspector設定をクリアしてコードで設定
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(CloseWindow);
    }

    private void CloseWindow()
    {
        if (transform.parent == null)
        {
            Debug.LogWarning("WindowCloseCtrl: No parent to close.");
            return;
        }

        if (transform.parent.gameObject.name == _TITLEBAR_NAME)
        {
            if (transform.parent.parent == null)
            {
                return;
            }
            transform.parent.parent.gameObject.SetActive(false);
            return;
        }
        transform.parent.gameObject.SetActive(false);
    }


}
