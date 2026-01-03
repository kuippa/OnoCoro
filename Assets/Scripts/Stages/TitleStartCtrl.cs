using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonsUtility;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleStartCtrl : MonoBehaviour
{
    private int _STAGE_SELECTOR_HEIGHT = 100;
    private int _STAGE_SELECTOR_MARGIN = 2;
    GameObject _pnlStageSelector;

    // GameTutorial

    private void OnClickGameClose()
    {
        // Debug.Log("OnClickGameClose");
        GameCtrl gameCtrl = this.gameObject.AddComponent<GameCtrl>();
        gameCtrl.GameClose();
    }

    private void ClearContetArea(GameObject content)
    {
        // Debug.Log(content.transform.childCount);
        for (int i = 0; i < content.transform.childCount; i++)
        {
            GameObject child = content.transform.GetChild(i).gameObject;
            GameObjectTreat.DestroyAll(child);
        }
    }

    private void SetStageContents(GameObject content)
    {
        ClearContetArea(content);
        // GlobalConst._scene_path をループ
        foreach (KeyValuePair<string, string[]> kvp in GlobalConst._scene_path)
        {
            // Debug.Log("SetStageContents " + kvp.Key + " " + kvp.Value[0] + " " + kvp.Value[1] + " " + kvp.Value[2] + " ");
            string scenepath = kvp.Key;
            string[] stageinfo = kvp.Value;
            SetStageContent(content, scenepath);
        }

        RectTransform rectcontent = content.GetComponent<RectTransform>();
        int childCount = rectcontent.childCount;
        rectcontent.sizeDelta = new Vector2(0, _STAGE_SELECTOR_HEIGHT * childCount + _STAGE_SELECTOR_MARGIN * (childCount - 1));
    }

    private void SetStageContent(GameObject content, string scenepath)
    {
        string[] vals;
        GlobalConst._scene_path.TryGetValue(scenepath , out vals);
        // Debug.Log("SetStageContent " + scenepath + vals[0] + vals[1] + vals[2]);

        GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/pnlStage");
        GameObject instance = Instantiate(prefab, content.transform);
        TextMeshProUGUI txtStageDisplayName = instance.transform.Find("txtStageDisplayName").GetComponent<TextMeshProUGUI>();
        // txtStageDisplayName.text = GlobalConst._scene_path[scenepath][0];
        txtStageDisplayName.text = vals[0];

        // imgStageIcon
        Image imgStageIcon = instance.transform.Find("imgStageIcon").GetComponent<Image>();
        // imgStageIcon.sprite = Resources.Load<Sprite>("Sprites/StageIcon/StageIcon1");
        imgStageIcon.sprite = Resources.Load<Sprite>(vals[1]);

        // txtStageInfo
        TextMeshProUGUI txtStageInfo = instance.transform.Find("txtStageInfo").GetComponent<TextMeshProUGUI>();
        // txtStageInfo.text = "StageInfo1";
        txtStageInfo.text = vals[2];

        // txtStagePath
        Text txtStagePath = instance.transform.Find("txtStagePath").GetComponent<Text>();
        // txtStagePath.text = "path1";
        txtStagePath.text = scenepath;

        // Debug.Log("SetStageContent " + txtStageDisplayName.text + " " + imgStageIcon.sprite.name + " " + txtStageInfo.text + " " + txtStagePath.text);


        Button btn = instance.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            OnClickSelectStage(btn);
        });

    }



    private void OnClickSelectStage(Button btnStageSelect)
    {
        // Debug.Log("OnClickSelectStage " + btnStageSelect.name);
        // txtStagePath を取得
        Transform txtStagePath = btnStageSelect.transform.Find("txtStagePath");
        // if (txtStagePath == null)
        // {
        //     Debug.Log("txtStagePath is null");
        //     return;
        // }
        Text StagePath = txtStagePath.gameObject.GetComponent<Text>();
        if (StagePath == null || StagePath.text == "")
        {
            Debug.Log("StagePath is null");
            return;
        }
        // Debug.Log("StagePath.text " + StagePath.text);
        SceneManager.LoadScene(StagePath.text);
    }

    private void OnClickStageSelect()
    {
        // 押されたボタンの名前を取得
        // Debug.Log("OnClickStageSelect " + _pnlStageSelector.activeSelf);
        _pnlStageSelector.SetActive(!_pnlStageSelector.activeSelf);
        // _pnlStageSelector.SetActive(true);
    }


    void Awake()
    {
        Button btnClose = GameObject.Find("btnGameClose").GetComponent<Button>();
        btnClose.onClick.AddListener(() =>
        {
            OnClickGameClose();
        });

        Button btnStage = GameObject.Find("btnStage").GetComponent<Button>();
        btnStage.onClick.AddListener(() =>
        {
            OnClickStageSelect();
        });


        // pnlStageSelectorの子要素からContentを探す
        GameObject content = GameObject.Find("pnlStageSelector/Scroll View/Viewport/Content");
        SetStageContents(content);

        _pnlStageSelector = GameObject.Find("pnlStageSelector");
        _pnlStageSelector.SetActive(false);
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
