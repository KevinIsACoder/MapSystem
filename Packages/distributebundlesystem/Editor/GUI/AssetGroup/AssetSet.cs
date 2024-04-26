using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public enum BundleCompression
    {
        LZMA = 0,
        LZ4,
        Uncompressed
    }

    public enum PackingMode
    {
        /// <summary>
        /// Pack by whole asset set(only one bundle per asset set)
        /// </summary>
        Together = 0,
        /// <summary>
        /// Pack by each asset entry(multiple bundle per asset set)
        /// </summary>
        Separate
    }

    [Serializable]
    public class AssetSet : ScriptableObject
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
        string m_AssetSetName;
        public string AssetSetName
        {
            get {
                return m_AssetSetName;
            }
            set
            {
                m_AssetSetName = value;
            }
        }

        [SerializeField]
        AssetGroup m_AssetGroup;
        public AssetGroup AssetGroup
        {
            get
            {
                return m_AssetGroup;
            }
            set
            {
                m_AssetGroup = value;
            }
        }

        [SerializeField]
        BundleCompression m_BundleCompressionMethod;
        public BundleCompression BundleCompressionMethod
        {
            get
            {
                return m_BundleCompressionMethod;
            }
            set
            {
                m_BundleCompressionMethod = value;
            }
        }

        [SerializeField]
        PackingMode m_PackingMode;
        public PackingMode PackingMode
        {
            get
            {
                return m_PackingMode;
            }
            set
            {
                m_PackingMode = value;
            }
        }

        [SerializeField]
        bool m_CompressLocalCache;
        public bool CompressLocalCache
        {
            get
            {
                return m_CompressLocalCache;
            }
            set
            {
                m_CompressLocalCache = value;
            }
        }

        [SerializeField]
        bool m_InPackage;
        public bool InPackage
        {
            get
            {
                return m_InPackage;
            }
            set
            {
                m_InPackage = value;
            }
        }

        [SerializeField]
        List<AssetEntry> m_Entries = new List<AssetEntry>();

        public List<AssetEntry> Entries
        {
            get
            {
                return m_Entries;
            }
        }

        public bool IsAssetEntryExisting(string entryGUID)
        {
            return m_Entries.Find(x => x.GUID == entryGUID) != null;
        }

        public void AddEntry(AssetEntry entry)
        {
            m_Entries.Add(entry);
        }

        public void RemoveEntry(AssetEntry entry)
        {
            m_Entries.Remove(entry);
        }

        public BuildCompression GetBuildCompressionForBundle()
        {
            switch (BundleCompressionMethod)
            {
                case BundleCompression.Uncompressed: return BuildCompression.Uncompressed;
                case BundleCompression.LZ4: return BuildCompression.LZ4;
                case BundleCompression.LZMA: return BuildCompression.LZMA;
            }
            return default;
        }
    }

}
