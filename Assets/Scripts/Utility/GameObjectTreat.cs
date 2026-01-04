using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonsUtility
{
    internal class GameObjectTreat: MonoBehaviour
    {
        // タグ名は手動で追加しておくこと
        public static GameObject[] FindObjectsByTag(string tag)
        {

            GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
            if (gos.Length > 0)
            {
                return gos;
            }
            return null;
        }

        internal static int IndexObjectByTag(string tag)
        {
            GameObject[] gos = FindObjectsByTag(tag);
            if (gos != null)
            {   
                if (gos.Length > 0)
                {
                    return gos.Length;
                }
            }
            return 0;
        }

        // internal static float rdNum(float min, float max)
        // {
        //     float num = Utility.fRandomRange(min, max);
        //     return num;
        // }

        internal static void DestroyAll(GameObject target)
        {
            DestroyChild(target);
            DestroyEx(target);
        }

        private static void DestroyEx(GameObject target)
        {
            if (target == null)
            {
                return;
            }
            Destroy(target);
            target = null;
        }

        private static void DestroyChild(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Transform children =  target.GetComponentInChildren<Transform>();
            if (children.childCount == 0)
            {
                return;
            }
            foreach(Transform tsObj in children)
            {
                DestroyAll(tsObj.gameObject);
            }
            // children.DetachChildren();
        }

        // マテリアルは明示的に破棄しないとメモリリークする
        internal static void DestroyMaterialsAll(Material[] materials)
        {
            if (materials == null)
            {
                return;
            }
            foreach (Material material in materials)
            {
                Destroy(material);
            }
        }

        internal static void ColorChange(GameObject targetObject ,Color setColor)
        {
            if (targetObject == null)
            {
                return;
            }
            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }
            renderer.material.color = setColor;
        }

        private static Color GetColor(GameObject targetObject)
        {
            if (targetObject == null)
            {
                return Color.black;
            }
            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return Color.black;
            }
            return renderer.material.color;
        }

        internal static void DebugColorChange(GameObject targetObject ,Color setColor)
        {
            if (GameConfig._APP_GAME_MODE != GlobalConst.GAME_MODE_DEBUG)
            {
                return;
            }
            if (GetColor(targetObject) == Color.black)
            {
                return; // 黒は変更しない
            }
            ColorChange(targetObject, setColor);
        }


    }
}