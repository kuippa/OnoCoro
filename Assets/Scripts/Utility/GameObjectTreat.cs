using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
// using System.Math;

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
                    // TODO: 重複チェック
                    // 途中で削除された場合、インデックスが重複するので、重複チェック
                    // あるいはラストインデックスの取得が必要

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
            if (renderer.material == null)
            {
                return Color.black;
            }
            if (renderer.material.color == null)
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

        internal static Transform GetParentTransform(string parentName)
        {
            GameObject parent = GameObject.Find(parentName);
            Transform parentTransform = null;
            if (parent == null)
            {
                parent = new GameObject(parentName);
            }
            parentTransform = parent.transform;
            return parentTransform;
        }


        // Scriptのファイル名リストを一覧で出力
        internal static void DebugScriptList()
        {
            #if UNITY_EDITOR
            StringBuilder output = new StringBuilder();
            string[] guids = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Scripts" });
            var sortedScripts = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .OrderBy(path => Path.GetDirectoryName(path))
                .ThenBy(path => Path.GetFileName(path))
                .ToList();

            string previousFolder = "";
            foreach (string path  in sortedScripts)
            {
                string fileName = Path.GetFileName(path);
                string folderPath = Path.GetDirectoryName(path);
                string folderName = Path.GetFileName(folderPath);
                // "Assets/Scripts"からの相対的な深さを計算
                // int depth = Math.Max(0, folderPath.Split('/').Length - 3); // "Assets/Scripts"で3
                int depth = Mathf.Max(0, folderPath.Split('/').Length - 3); // "Assets/Scripts"で3
                string indent = new string(' ', depth * 2);

                // 新しいフォルダに入ったときは空行を追加
                if (folderName != previousFolder)
                {
                    if (previousFolder != "")
                        output.AppendLine();
                    previousFolder = folderName;
                }

                // output.AppendLine($"{indent}{folderName}: {fileName} {path}");                
                output.AppendLine($"{indent}{folderName}: {fileName}");                
                // out_buf += ($"{indent}{folderName}: {fileName}") + "\n";
            }
            // Debug.Log(out_buf);
            Debug.Log(output.ToString());
            #endif
        }

    }
}