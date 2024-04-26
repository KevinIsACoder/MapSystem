using System;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    [Serializable]
    public class AssetGroupEditor
    {
        [SerializeField]
        TreeViewState m_TreeViewState;

        [SerializeField]
        MultiColumnHeaderState m_MultiColumnHeaderState;
        internal AssetGroupTreeView m_GroupTree;

        private AdvancedBundleSystemSettings m_Settings;
        public AssetGroupEditor()
        {
            m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
        }

        internal void InitializeGroupTree()
        {
            if (m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }
            bool firstInit = m_MultiColumnHeaderState == null;
            MultiColumnHeaderState headerState = AssetGroupTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
            {
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            }
            m_MultiColumnHeaderState = headerState;
            MultiColumnHeader header = new MultiColumnHeader(headerState);
            if (firstInit)
            {
                header.ResizeToFit();
            }
            AssetGroupTreeModel model = new AssetGroupTreeModel(m_Settings.RootAssetGroup);
            m_Settings.AssetGroupTreeModel = model;
            m_GroupTree = new AssetGroupTreeView(m_TreeViewState, header, model);
            m_GroupTree.Reload();
        }

        public bool OnGUI(Rect pos)
        {
            if (m_GroupTree == null)
            {
                InitializeGroupTree();
            }
            m_GroupTree.OnGUI(pos);
            return true;
        }
    }
}
