// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// LoadStreamingAsset
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Debug = UnityEngine.Debug;

public static class LoadStreamingAsset
{
	internal const string _STAGING_SUB_FOLDER = "staging";
	internal const string _PUBLIC_DOC_SUB_FOLDER = "public_doc";
	private const string _CSV_COMMENT_PREFIX = "#";
	
	// File name constants
	public const string YAML_FILE_EXTENSION = ".yaml";
	public const string STAGE_LIST_FILE_NAME = "stagelist.csv";
	public const string ABOUT_GAME_FILE_NAME = "aboutthisgame.txt";
	public const string NOTICE_FILE_NAME = "notice.txt";


	internal static YamlStream LoadYamlFile(string fileName)
	{
		string path = StageFilePath(fileName);
		if (!File.Exists(path))
		{
			return null;
		}
		StreamReader streamReader = new StreamReader(path);
		if (streamReader == null)
		{
			return null;
		}
		TextReader textReader = new StringReader(streamReader.ReadToEnd());
		if (textReader == null)
		{
			return null;
		}
		YamlStream yamlStream = new YamlStream();
		yamlStream.Load(textReader);
		if (yamlStream.Documents.Count == 0)
		{
			return null;
		}
		return yamlStream;
	}

	internal static string AllTextStream(string fileName, string pathType = _STAGING_SUB_FOLDER)
	{
		string path = "";
		if (pathType == _PUBLIC_DOC_SUB_FOLDER)
		{
			path = GetPublicDocFilePath(fileName);
		}
		else
		{
			path = StageFilePath(fileName);
		}
		if (!File.Exists(path))
		{
			Debug.LogWarning("LoadStreamingAsset AllTextStream: File not found: " + path);

			return null;
		}
		return new StreamReader(path)?.ReadToEnd();
	}

	internal static string GetPublicDocFilePath(string fileName)
	{
		return Path.Combine(Application.streamingAssetsPath, _PUBLIC_DOC_SUB_FOLDER, fileName);
	}

	internal static string StageFilePath(string fileName)
	{
		return Path.Combine(Application.streamingAssetsPath, _STAGING_SUB_FOLDER, fileName);
	}

	private static string RemoveQuotesWithRegex(string input)
	{
		input = input.Trim();
		return Regex.Replace(input, "^\"|\"$", "");
	}

	internal static string[] CsvLines(string fileName, string pathType = _STAGING_SUB_FOLDER)
	{
		string fileContent = AllTextStream(fileName, pathType);
		if (fileContent == null)
		{
			return null;
		}
		// コメント行と空行を除去
		fileContent = Regex.Replace(fileContent, $"^{_CSV_COMMENT_PREFIX}.*$(\\n|\\r\\n)?", "", RegexOptions.Multiline);
		return fileContent.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
	}

	internal static string[] CsvCols(string line)
	{
		string[] array = line.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = RemoveQuotesWithRegex(array[i]);
		}
		return array;
	}

	/// <summary>
	/// シーン名からYAMLファイル名を生成します
	/// </summary>
	/// <param name="sceneName">シーン名</param>
	/// <returns>YAMLファイル名（拡張子付き）</returns>
	public static string GetYamlFileName(string sceneName)
	{
		return Path.GetFileName(sceneName + YAML_FILE_EXTENSION);
	}

	/// <summary>
	/// 指定されたシーン名に対応するYAMLファイルが存在するかチェックします
	/// </summary>
	/// <param name="sceneName">シーン名</param>
	/// <returns>ファイルが存在する場合true</returns>
	public static bool YamlFileExists(string sceneName)
	{
		string yamlFileName = GetYamlFileName(sceneName);
		string yamlFilePath = StageFilePath(yamlFileName);
		return File.Exists(yamlFilePath);
	}
}
