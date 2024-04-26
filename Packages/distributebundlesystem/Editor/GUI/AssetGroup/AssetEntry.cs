using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    [Serializable]
    public class AssetEntry
    {
        [SerializeField]
        string m_GUID;
        public string GUID
        {
            get
            {
                return m_GUID;
            }
            set
            {
                m_GUID = value;
            }
        }

        [SerializeField]
        string m_AssetPath;
        public string AssetPath
        {
            get
            {
                return m_AssetPath;
            }
            set
            {
                m_AssetPath = value;
            }
        }

        [SerializeField]
        bool m_IsFolder;
        public bool IsFolder
        {
            get
            {
                return m_IsFolder;
            }
            set
            {
                m_IsFolder = value;
            }
        }

        [NonSerialized]
        List<AssetEntry> m_SubEntries = new List<AssetEntry>();
        public List<AssetEntry> SubEntries
        {
            get
            {
                return m_SubEntries;
            }
        }

        public string AssetName
        {
            get
            {
                if (IsFolder)
                {
                    string[] splitted = AssetPath.Split('/');
                    return splitted[splitted.Length - 1];
                }
                else
                {
                    string[] splitted = AssetPath.Split('/');
                    return splitted[splitted.Length - 1].Split('.')[0];
                }
            }
        }

        public void GenerateSubEntries()
        {
            SubEntries.Clear();
            if (AssetDatabase.IsValidFolder(AssetPath))
            {
                IsFolder = true;
                IEnumerable<string> subPaths = Directory.GetFileSystemEntries(AssetPath).Where(x => !x.EndsWith(".meta"));
                foreach(string subPath in subPaths)
                {
                    string assetPath = subPath.Replace('\\', '/');
                    AssetEntry entry = new AssetEntry();
                    entry.AssetPath = assetPath;
                    entry.GUID = AssetDatabase.AssetPathToGUID(assetPath);
                    entry.IsFolder = AssetDatabase.IsValidFolder(assetPath);
                    SubEntries.Add(entry);
                }
            }
            else
            {
                IsFolder = false;
            }
        }

        private void GatherAssetPathsRecursively(List<string> allAssetPaths, string folderPath)
        {
            Assert.IsTrue(AssetDatabase.IsValidFolder(folderPath));
            IEnumerable<string> subPaths = Directory.GetFileSystemEntries(folderPath).Where(x => !x.EndsWith(".meta"));
            foreach(string subPath in subPaths)
            {
                string assetPath = subPath.Replace('\\', '/');
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    GatherAssetPathsRecursively(allAssetPaths, assetPath);
                }
                else
                {
                    allAssetPaths.Add(assetPath);
                }
            }
        }

        public List<string> GatherAllAssetPathsInFolderEntry()
        {
            List<string> allAssetPaths = new List<string>();
            GatherAssetPathsRecursively(allAssetPaths, AssetPath);
            return allAssetPaths;
        }
    }

}
