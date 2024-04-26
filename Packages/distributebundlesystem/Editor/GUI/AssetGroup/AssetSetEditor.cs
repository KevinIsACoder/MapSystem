using System;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    [Serializable]
    public class AssetSetEditor
    {
        [SerializeField] 
        TreeViewState m_TreeViewState;

        [SerializeField] 
        MultiColumnHeaderState m_MultiColumnHeaderState;
        internal AssetSetTreeView m_EntryTree;

		private AdvancedBundleSystemSettings m_Settings;
		public AssetSetEditor()
        {
			m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
        }

        internal void InitlizeEntryTree()
        {
            if(m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }
			bool firstInit = m_MultiColumnHeaderState == null;
            MultiColumnHeaderState headerState = AssetSetTreeView.CreateDefaultMultiColumnHeaderState();
            if(MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
            {
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            }
            m_MultiColumnHeaderState = headerState;
			MultiColumnHeader header = new MultiColumnHeader(headerState);
            if (firstInit)
            {
				header.ResizeToFit();
            }
			AssetSetTreeModel model = new AssetSetTreeModel(m_Settings.AssetSets);
			m_Settings.AssetSetTreeModel = model;
			m_EntryTree = new AssetSetTreeView(m_TreeViewState, header, model);
			m_EntryTree.Reload();
		}

        public bool OnGUI(Rect pos)
        {
            if(m_EntryTree == null)
            {
				InitlizeEntryTree();
			}
			m_EntryTree.OnGUI(pos);
            return true;//TODO:macdeng, to be optimized
        }
    }
}
