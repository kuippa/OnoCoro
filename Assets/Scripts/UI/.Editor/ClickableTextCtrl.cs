using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClickableTextCtrl : MonoBehaviour
{
    public Renderer rend;
    TextMeshProUGUI textmeshPro;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        // this.GetComponent<UnityEngine.UI.Text>().color = Color.white;
        TextMeshProUGUI textmeshPro = this.GetComponent<TextMeshProUGUI>();
        textmeshPro.color = Color.white;
        // this.GetComponent<>().color = Color.green;
        // イベントリスナーの追加
        // this.GetComponent<TextMeshProUGUI>().onClick.AddListener(OnClick);


    }

    public void OnMouseEnter()
    {
        this.GetComponent<UnityEngine.UI.Text>().color = Color.red;
    }

    public void OnMouseExit()
    {
        this.GetComponent<UnityEngine.UI.Text>().color = Color.white;
    }

    public void OnClick()
    {
        this.GetComponent<UnityEngine.UI.Text>().color = Color.blue;
    }

    


}
