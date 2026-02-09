using UnityEngine;
using CommonsUtility;

public class SimpleSwitchBox : TriggerHandler
{
    // シリアライズされた変数はインスペクターから変更可能
    [SerializeField] public bool _SwitchBoxState; 

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

    protected override void OnTargetEnter()
    {
        ToggleSwitchBox();
    }

    protected override void OnTargetExit()
    {
        // スイッチはトグルなので離脱時は処理しない
    }

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

    protected override void Awake()
    {
        base.Awake();
        SetDefaultTargetTag(GameEnum.UnitType.Player.ToString());
        
        _OnSwitchBox = transform.Find("btn_on").gameObject;
        _OffSwitchBox = transform.Find("btn_off").gameObject;

        this._SwitchBoxState = !this._SwitchBoxState;
        ToggleSwitchBox();
    }




}
