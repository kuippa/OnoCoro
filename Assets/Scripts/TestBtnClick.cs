using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBtnClick : MonoBehaviour
{
    // public Button yourButton;

    // Start is called before the first frame update
    void Start()
    {
		Button btn1 = GameObject.Find("Item_1").GetComponent<Button>();
		// Button btn = yourButton.GetComponent<Button>();
		btn1.onClick.AddListener(onTestClick);


        Debug.Log(btn1.transform.GetSiblingIndex());
        btn1.transform.SetSiblingIndex(2);
        Debug.Log(btn1.transform.GetSiblingIndex());



    }

    public void onTestClick()
    {

        Debug.Log("test");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
