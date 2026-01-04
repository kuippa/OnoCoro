using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBtnInteractable : MonoBehaviour
{

    void Awake()
    {
        // GetComponent<UnityEngine.UI.Button>().interactable = false;

        // this.gameObject.SetActive(true);
        // this.transform.Find("btnOK").gameObject.SetActive(false);
        ChangeBtnEnable(false);

        Debug.Log("Awake");

        Button btnTrue = GameObject.Find("btnTrue").GetComponent<Button>();

        // Button btnTrue = this.transform.Find("btnTrue").GetComponent<Button>();
        btnTrue.onClick.AddListener(() => ChangeBtnEnable(true));
        Button btnFalse = this.transform.Find("btnFalse").GetComponent<Button>();
        // btnFalse.onClick.AddListener(() => ChangeBtnEnable(false));
        Button btnOK = this.transform.Find("btnOK").GetComponent<Button>();
        btnOK.onClick.AddListener(() => OnOkClick());

        Debug.Log("GetPersistentEventCount" + btnFalse.onClick.GetPersistentEventCount());


    }

    public void ChangeBtnEnable(bool enable)
    {
        Button btn = this.transform.Find("btnOK").GetComponent<Button>();
        btn.interactable = enable;        
        // gameObject.SetActive(false);
        // GetComponent<UnityEngine.UI.Button>().interactable = enable;
        // this.transform.Find("btnOK").gameObject.SetActive(enable);

        Button btnFalse = this.transform.Find("btnFalse").GetComponent<Button>();
        Debug.Log("GetPersistentEventCount" + btnFalse.onClick.GetPersistentEventCount());

    }

    public void OnOkClick()
    {
        Button btnFalse = this.transform.Find("btnFalse").GetComponent<Button>();
        btnFalse.onClick.AddListener(() => ChangeBtnEnable(false));

        Debug.Log("OnOkClick");
    }



}
