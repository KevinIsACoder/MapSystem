using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    [Serializable]
    public class AssetGroup : ScriptableObject
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
        int m_GroupId;
        public int GroupId
        {
            get
            {
                return m_GroupId;
            }
            set
            {
                m_GroupId = value;
            }
        }

        [SerializeField]
        bool m_IsRootGroup;
        public bool IsRootGroup
        {
            get
            {
                return m_IsRootGroup;
            }
            set
            {
                m_IsRootGroup = value;
            }
        }

        [SerializeField]
        List<AssetGroup> m_Children = new List<AssetGroup>();
        public List<AssetGroup> Children
        {
            get
            {
                return m_Children;
            }
        }

        [SerializeField]
        List<AssetSet> m_AssetSets = new List<AssetSet>();
        public List<AssetSet> AssetSets
        {
            get
            {
                return m_AssetSets;
            }
        }
        
        [NonSerialized]
        AssetGroup m_Parent = null;
        public AssetGroup Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                m_Parent = value;
            }
        }
        
        public void AddChild(AssetGroup assetGroup)
        {
            if (!m_Children.Contains(assetGroup))
            {
                m_Children.Add(assetGroup);
            }
        }

        public bool IsGroupIdExistingRecursively(int groupId)
        {
            if (GroupId == groupId)
            {
                return true;
            }
            if (Children != null && Children.Count > 0)
            {
                foreach (AssetGroup catalog in Children)
                {
                    if (catalog.IsGroupIdExistingRecursively(groupId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
