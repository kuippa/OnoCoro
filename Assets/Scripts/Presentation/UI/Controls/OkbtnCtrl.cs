using UnityEngine;
using UnityEngine.UI;
using CommonsUtility;
using Debug = CommonsUtility.Debug;

public class OkbtnCtrl : MonoBehaviour
{
    [SerializeField]
    private GameObject _okBtn = null;
    [SerializeField]
    private GameObject _closeBtn = null;
    [SerializeField]
    private GameObject _closeWindow = null;


    private void Awake()
    {
        if (_okBtn == null || _closeWindow == null)
        {
            return;
        }
        _okBtn.GetComponent<Button>().onClick.AddListener(onOkClick);
        if (_closeBtn != null)
        {
            _closeBtn.GetComponent<Button>().onClick.AddListener(onOkClick);
        }
    }

    public void onOkClick()
    {
        GameObjectTreat.DestroyAll(_closeWindow);
    }

}
