using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// Provides functionality for reading/writing XML data.
/// </summary>
public class XMLReadWrite
{
    /// <summary>
    /// Gets the given attribute of the given node, or null if the attribute doesn't exist.
    /// </summary>
    protected static string GetAttribute(XmlNode node, string attributeName)
    {
        for (int i = 0; i < node.Attributes.Count; ++i)
        {
            if (node.Attributes[i].Name == attributeName)
            {
                return node.Attributes[i].Value;
            }
        }

        return null;
    }
    /// <summary>
    /// Gets the given child of the given node, or null if the child doesn't exist.
    /// </summary>
    protected static XmlNode GetChild(XmlNode node, string childName)
    {
        for (int i = 0; i < node.ChildNodes.Count; ++i)
        {
            if (node.ChildNodes[i].Name == childName)
            {
                return node.ChildNodes[i];
            }
        }

        return null;
    }

    public string FileName { get; private set; }
    public string FullPath { get { return Application.dataPath + "/Resources/" + FileName + ".xml"; } }

    protected XmlNode RootNode;
    protected XmlDocument Document { get { return RootNode.OwnerDocument; } }

    /// <summary>
    /// The error message from reading the XML data, or "" if none exists.
    /// </summary>
    public string ErrorMessage { get; protected set; }

    public XMLReadWrite(TextAsset file, string fileName, string rootNodeName)
    {
        FileName = fileName;
        ErrorMessage = "";

        try
        {
            System.IO.StringReader stringReader = new System.IO.StringReader(file.text);
            stringReader.Read();

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(stringReader));
			XmlNodeList list = doc.GetElementsByTagName(rootNodeName);
			
			List<XmlNode> listNs = new List<XmlNode>();
			for (int i = 0; i < list.Count; ++i)
				listNs.Add (list[i]);
			
            RootNode = listNs[0];
        }
        catch (Exception e)
        {
            Debug.Log("Error reading XML file: " + e.Message);
            ErrorMessage = e.Message;
        }
    }
    public XMLReadWrite(XmlDocument loadedDocument, string rootNodeName)
    {
		ErrorMessage = "";
		
        try
        {
            XmlNodeList list = loadedDocument.GetElementsByTagName(rootNodeName);

            List<XmlNode> listNs = new List<XmlNode>();
            for (int i = 0; i < list.Count; ++i)
                listNs.Add(list[i]);

            RootNode = listNs[0];
        }

        catch (Exception e)
        {
            Debug.Log("Error reading XML file: " + e.Message);
            ErrorMessage = e.Message;
        }
    }
}