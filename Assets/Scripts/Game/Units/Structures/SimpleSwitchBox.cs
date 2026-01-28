using System;
using UnityEngine;

public class SimpleSwitchBox : MonoBehaviour
{
    // シリアライズされた変数はインスペクターから変更可能
    [SerializeField] public Boolean _SwitchBoxState; 

    private GameObject _OnSwitchBox;
    private GameObject _OffSwitchBox;

    private float _ActiveSwitchY = 0.4f;
    private float _UnActiveSwitchY = 0.2f;

    private Vector3 SetSwitchBoxPosition(Vector3 localpos, float new_y )
    {
        float x = localpos.x;
        float y = new_y;
        float z = localpos.z;
        return new Vector3(x, y, z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: 対象をプレイヤーのみに限定する？
        ToggleSwitchBox();
    }

    // private void OnMouseDown()
    // {
    //     ToggleSwitchBox();
    // }

    private void ToggleSwitchBox()
    {
        this._SwitchBoxState = !this._SwitchBoxState;
        if (this._SwitchBoxState)
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

    void Awake()
    {
        
        _OnSwitchBox = transform.Find("btn_on").gameObject;
        _OffSwitchBox = transform.Find("btn_off").gameObject;

        this._SwitchBoxState = !this._SwitchBoxState;
        ToggleSwitchBox();
    }




}
