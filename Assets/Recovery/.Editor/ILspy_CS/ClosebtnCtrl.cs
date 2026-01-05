// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// ClosebtnCtrl
using UnityEngine;
using UnityEngine.UI;

public class ClosebtnCtrl : MonoBehaviour
{
	[SerializeField]
	private GameObject _okBtn;

	[SerializeField]
	private GameObject _closeBtn;

	[SerializeField]
	private GameObject _closeWindow;

	private void Awake()
	{
		if (!(_okBtn == null) && !(_closeWindow == null))
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
		_closeWindow.SetActive(value: false);
	}
}
