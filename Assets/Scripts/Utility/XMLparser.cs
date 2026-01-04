using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class XMLparser : MonoBehaviour
{
    internal Dictionary<string, string> _buildingUsageDict = new Dictionary<string, string>();

    private void ReadBuildingXML()
    {
        _buildingUsageDict.Clear();
        TextAsset xml = Resources.Load<TextAsset>("xml/Building_usage");
        // ex.
        // <gml:Dictionary xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:gml="http://www.opengis.net/gml" xsi:schemaLocation="http://www.opengis.net/gml http://schemas.opengis.net/gml/3.1.1/profiles/SimpleDictionary/1.0.0/gmlSimpleDictionaryProfile.xsd" gml:id="cl_6c44d07d-ac62-4a3f-abd2-81da7943a50b">
        // <gml:name>Building_usage</gml:name>
        // <gml:dictionaryEntry>
            // <gml:Definition gml:id="Building_usage_1">
                // <gml:description>業務施設</gml:description>
                // <gml:name>401</gml:name>
            // </gml:Definition>
        // </gml:dictionaryEntry>

        // https://www.geospatial.jp/iur/codelists/3.0/Building_usage.xml
        // コード	説明
        // 401	業務施設
        // 402	商業施設
        // 403	宿泊施設
        // 404	商業系複合施設
        // 411	住宅
        // 412	共同住宅
        // 413	店舗等併用住宅
        // 414	店舗等併用共同住宅
        // 415	作業所併用住宅
        // 421	官公庁施設
        // 422	文教厚生施設
        // 431	運輸倉庫施設
        // 441	工場
        // 451	農林漁業用施設
        // 452	供給処理施設
        // 453	防衛施設
        // 454	その他
        // 461	不明


        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml.text);
        // XmlElement root = xmlDoc.DocumentElement;
        // Debug.Log(root.Name);
        // Debug.Log(root.ChildNodes.Count);

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("gml", "http://www.opengis.net/gml");
        XmlNodeList entryList = xmlDoc.SelectNodes("//gml:dictionaryEntry", nsmgr);

        foreach (XmlNode entry in entryList)
        {
            XmlNode definitionNode = entry.SelectSingleNode("gml:Definition", nsmgr);
            if (definitionNode != null)
            {
                XmlNode descriptionNode = definitionNode.SelectSingleNode("gml:description", nsmgr);
                XmlNode nameNode = definitionNode.SelectSingleNode("gml:name", nsmgr);
                if (descriptionNode != null && nameNode != null)
                {
                    _buildingUsageDict.Add(nameNode.InnerText, descriptionNode.InnerText);
                    // Debug.Log(descriptionNode.InnerText + " : " + nameNode.InnerText);
                }
            }
        }

        // Debug.Log(_buildingUsageDict);
        // _buildingUsageDict.TryGetValue("401", out var val);
        // Debug.Log(val);
    }

    public string GetBuildingUsage(string key)
    {
        string ret = "";
        _buildingUsageDict.TryGetValue(key, out ret);
        return ret;
    }


    void Awake()
    {
        ReadBuildingXML();
    }

    



}
