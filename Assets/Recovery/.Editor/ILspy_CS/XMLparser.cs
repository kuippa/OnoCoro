// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// XMLparser
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class XMLparser : MonoBehaviour
{
	internal Dictionary<string, string> _buildingUsageDict = new Dictionary<string, string>();

	private void ReadBuildingXML()
	{
		_buildingUsageDict.Clear();
		TextAsset textAsset = Resources.Load<TextAsset>("xml/Building_usage");
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		xmlNamespaceManager.AddNamespace("gml", "http://www.opengis.net/gml");
		foreach (XmlNode item in xmlDocument.SelectNodes("//gml:dictionaryEntry", xmlNamespaceManager))
		{
			XmlNode xmlNode = item.SelectSingleNode("gml:Definition", xmlNamespaceManager);
			if (xmlNode != null)
			{
				XmlNode xmlNode2 = xmlNode.SelectSingleNode("gml:description", xmlNamespaceManager);
				XmlNode xmlNode3 = xmlNode.SelectSingleNode("gml:name", xmlNamespaceManager);
				if (xmlNode2 != null && xmlNode3 != null)
				{
					_buildingUsageDict.Add(xmlNode3.InnerText, xmlNode2.InnerText);
				}
			}
		}
	}

	public string GetBuildingUsage(string key)
	{
		string value = "";
		_buildingUsageDict.TryGetValue(key, out value);
		if (string.IsNullOrEmpty(value))
		{
			value = "Unknown:" + key;
		}
		return value;
	}

	private void Awake()
	{
		ReadBuildingXML();
	}
}
