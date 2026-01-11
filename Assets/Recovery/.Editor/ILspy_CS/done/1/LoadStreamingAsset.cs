// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// LoadStreamingAsset
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public static class LoadStreamingAsset
{
	private const string _STAGING_SUB_FOLDER = "staging";

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

	internal static string AllTextStream(string fileName)
	{
		string path = StageFilePath(fileName);
		if (!File.Exists(path))
		{
			return null;
		}
		return new StreamReader(path)?.ReadToEnd();
	}

	internal static string StageFilePath(string fileName)
	{
		return Path.Combine(Application.streamingAssetsPath, "staging", fileName);
	}

	private static string RemoveQuotesWithRegex(string input)
	{
		input = input.Trim();
		return Regex.Replace(input, "^\"|\"$", "");
	}

	internal static string[] CsvLines(string fileName)
	{
		return AllTextStream(fileName).Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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
