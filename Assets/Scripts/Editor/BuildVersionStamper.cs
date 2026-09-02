using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CommonsUtility
{
    /// <summary>
    /// ビルド前にバージョンを採番し、BuildDate.txt と PlayerSettings.bundleVersion の
    /// 両方へ反映する（Editor 拡張）
    ///
    /// [なぜビルド前なのか]
    /// BuildDate.txt は Assets/Resources 配下にあり、ビルド時にプレイヤーへ焼き込まれる。
    /// 採番をビルド後に行うと、焼き込みが終わったあとにファイルを書き換えることになり、
    /// 配布物の中身は 1 つ前のビルド番号のままになる。
    ///
    /// [なぜ bundleVersion も更新するのか]
    /// Unity の Application.version（PlayerSettings.bundleVersion）は手で書き換える運用だったため
    /// 実際のビルド番号と食い違っていた。ここで一緒に更新して食い違いを無くす。
    ///
    /// [注意] ビルドが途中で失敗しても採番は進む。番号が飛ぶだけで実害は無いが、
    /// 戻したい場合は BuildDate.txt を手で書き換えてから再ビルドする。
    /// </summary>
    internal class BuildVersionStamper : IPreprocessBuildWithReport
    {
        private const string _FILE_PATH = "Assets/Resources/BuildDate.txt";
        private const string _DATETIME_FORMAT = "yyyy.MM.dd.HH.mm";
        private const string _DEFAULT_MAJOR = "0";
        private const string _DEFAULT_MINOR = "0";
        private const int _FIRST_BUILD_NUMBER = 1;

        /// <summary>バージョン情報の 5 行（MAJOR / MINOR / BUILD / 日時 / ターゲット）</summary>
        private struct VersionInfo
        {
            public string Major;
            public string Minor;
            public int Build;
        }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            VersionInfo info = ReadVersion();
            info.Build = info.Build + 1;

            WriteVersion(info, report.summary.platform);

            // Resources へ焼き込まれる前に、書き換えた内容を取り込ませる
            AssetDatabase.ImportAsset(_FILE_PATH, ImportAssetOptions.ForceSynchronousImport);

            PlayerSettings.bundleVersion = $"{info.Major}.{info.Minor}.{info.Build}";
            Debug.Log($"[BuildVersionStamper] バージョンを {PlayerSettings.bundleVersion} に採番しました");
        }

        /// <summary>
        /// 現在のバージョンを読む。ファイルが無い・壊れている場合は既定値を返す
        /// </summary>
        private VersionInfo ReadVersion()
        {
            VersionInfo info = new VersionInfo();
            info.Major = _DEFAULT_MAJOR;
            info.Minor = _DEFAULT_MINOR;
            info.Build = _FIRST_BUILD_NUMBER - 1;

            if (!File.Exists(_FILE_PATH))
            {
                Debug.LogWarning($"[BuildVersionStamper] {_FILE_PATH} が無いため新規に採番します");
                return info;
            }

            using (StreamReader reader = new StreamReader(_FILE_PATH, Encoding.UTF8))
            {
                string major = reader.ReadLine();
                string minor = reader.ReadLine();
                string build = reader.ReadLine();

                if (!string.IsNullOrEmpty(major))
                {
                    info.Major = major;
                }
                if (!string.IsNullOrEmpty(minor))
                {
                    info.Minor = minor;
                }
                if (Int32.TryParse(build, out int parsed))
                {
                    info.Build = parsed;
                }
            }
            return info;
        }

        private void WriteVersion(VersionInfo info, BuildTarget target)
        {
            using (StreamWriter writer = new StreamWriter(_FILE_PATH, append: false, Encoding.UTF8))
            {
                writer.WriteLine(info.Major);
                writer.WriteLine(info.Minor);
                writer.WriteLine(info.Build.ToString());
                writer.WriteLine(DateTime.Now.ToString(_DATETIME_FORMAT));
                writer.WriteLine(target.ToString());
            }
        }
    }
}
