using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text;
using System;

// コンパイルエラーになるのでビルド時にはEditor下フォルダに配置する
namespace PostProcessBuild
{
    public class ReleaseDateWhenBuild : MonoBehaviour
    // public class ReleaseDateWhenBuild : AssetPostprocessor
    {
        private static readonly string FILE_PATH = "Assets/Resources/BuildDate.txt";

        [PostProcessBuild(1)]   // (N)は実行順序
        // [PostProcessBuild(0)]   // (N)は実行順序
        // [PostProcessBuildAttribute(1)]
        // [RunBeforeAssembly("Unity.Addressables.Editor")]

        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            // // ユーザーデータを削除する
            // // PlayerPrefs.DeleteAll();

            string version_major;
            string version_minor;
            string version_build;

            using (var fs = File.Open(FILE_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            // using (var sr = new Stream(fs, Encoding.UTF8))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                // sr.Read();         // 1文字 
                version_major = sr.ReadLine();
                version_minor = sr.ReadLine();
                version_build = sr.ReadLine();
                var ret = sr.ReadToEnd();    // 全文
                Debug.Log(ret);
            }

            using (var fs = File.Open(FILE_PATH, FileMode.OpenOrCreate, FileAccess.Write))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                if (Int32.TryParse(version_build, out int build))
                {
                    version_build = (build+1).ToString();
                }
                String writeStr = DateTime.Now.ToString("yyyy.MM.dd.HH.mm");
                sw.WriteLine(version_major);
                sw.WriteLine(version_minor);
                sw.WriteLine(version_build);
                sw.WriteLine(writeStr); // 
                sw.WriteLine(target);   // コンパイルターゲット
            }

            // Debug.Log("OnPostProcessBuild" + path + " " + target);

            // using (var writer = new BinaryWriter(File.Open(FILE_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite) ,Encoding.UTF8 ))
            // // using (var writer = new BinaryWriter(File.Open(FILE_PATH, FileMode.OpenOrCreate, FileAccess.Write)))
            // {
            //     String writeStr = DateTime.Now.ToString("yyyy.MM.dd.HH.mm");
            //     Debug.Log("OnPostProcessBuild" + writeStr);
            //     writer.Write(writeStr);
            //     writer.Write(@"c:\Temp");
            //     writer.Write("あいう");
            // }


        }
    }
}