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
}
