using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CommonsUtility
{
    internal sealed class GameCtrl : MonoBehaviour
    {

        internal void GameClose()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();

        }
    
        // 
        // 処理落ちさせないためガベージコレクターを止めることができる。
        // GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;



    }


}