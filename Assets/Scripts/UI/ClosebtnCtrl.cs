using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ウィンドウの閉じるボタンを制御するクラス
/// OKボタンと閉じるボタンの両方に対応
/// </summary>
public class ClosebtnCtrl : MonoBehaviour
{
    [SerializeField]
    private GameObject _okBtn = null;

    [SerializeField]
    private GameObject _closeBtn = null;

    [SerializeField]
    private GameObject _closeWindow = null;

    private void Awake()
    {
        if (_okBtn != null && _closeWindow != null)
        {
            _okBtn.GetComponent<Button>().onClick.AddListener(onOkClick);
            if (_closeBtn != null)
            {
                _closeBtn.GetComponent<Button>().onClick.AddListener(onOkClick);
            }
        }
    }

    public void onOkClick()
    {
        _closeWindow.SetActive(false);
    }
}
