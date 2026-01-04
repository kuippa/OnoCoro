// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// SimpleSwitchBox
using UnityEngine;

public class SimpleSwitchBox : MonoBehaviour
{
	[SerializeField]
	public bool _SwitchBoxState;

	private GameObject _OnSwitchBox;

	private GameObject _OffSwitchBox;

	private float _ActiveSwitchY = 0.4f;

	private float _UnActiveSwitchY = 0.2f;

	private Vector3 SetSwitchBoxPosition(Vector3 localpos, float new_y)
	{
		float x = localpos.x;
		float z = localpos.z;
		return new Vector3(x, new_y, z);
	}

	private void OnTriggerEnter(Collider other)
	{
		ToggleSwitchBox();
	}

	private void ToggleSwitchBox()
	{
		_SwitchBoxState = !_SwitchBoxState;
		if (_SwitchBoxState)
		{
			_OnSwitchBox.transform.localPosition = SetSwitchBoxPosition(_OnSwitchBox.transform.localPosition, _ActiveSwitchY);
			_OffSwitchBox.transform.localPosition = SetSwitchBoxPosition(_OffSwitchBox.transform.localPosition, _UnActiveSwitchY);
		}
		else
		{
			_OnSwitchBox.transform.localPosition = SetSwitchBoxPosition(_OnSwitchBox.transform.localPosition, _UnActiveSwitchY);
			_OffSwitchBox.transform.localPosition = SetSwitchBoxPosition(_OffSwitchBox.transform.localPosition, _ActiveSwitchY);
		}
	}

	private void Awake()
	{
		_OnSwitchBox = base.transform.Find("btn_on").gameObject;
		_OffSwitchBox = base.transform.Find("btn_off").gameObject;
		_SwitchBoxState = !_SwitchBoxState;
		ToggleSwitchBox();
	}
}
