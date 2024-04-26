using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
	internal class AssetGroupTreeView : TreeView
	{
		private AssetGroupTreeModel m_AssetGroupTreeModel;
		public event Action m_TreeChanged;

		private AdvancedBundleSystemSettings m_Settings;

		enum ColumnName
		{
			GroupName = 0,
			AssetSets,
		}
		public AssetGroupTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetGroupTreeModel assetGroupTreeModel) : base(state, multiColumnHeader)
		{
			Init(assetGroupTreeModel);
		}

		private void Init(AssetGroupTreeModel assetGroupCatalogTreeModel)
		{
			m_AssetGroupTreeModel = assetGroupCatalogTreeModel;
			m_AssetGroupTreeModel.OnModelChanged += ModelChanged;
			m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
		}

		private void ModelChanged()
		{
			m_TreeChanged?.Invoke();
			Reload();
		}

		private void BuildAssetGroupsRecursively(AssetGroup assetGroup, AssetGroupTreeViewItem groupItem, int depth)
        {
			if (assetGroup.Children != null && assetGroup.Children.Count > 0)
			{
				foreach (AssetGroup group in assetGroup.Children)
				{
					string groupGUID = group.GUID;
					if(!groupItem.SubItemMap.TryGetValue(groupGUID, out AssetGroupTreeViewItem subGroupItem))
                    {
                        subGroupItem = new AssetGroupTreeViewItem(group, depth);
						group.Parent = assetGroup;
                        groupItem.AddChild(subGroupItem);
						groupItem.SubItemMap.Add(groupGUID, subGroupItem);
					}
                    if (!IsExpanded(groupItem.id))
                    {
						return;
                    }
					BuildAssetGroupsRecursively(group, subGroupItem, depth + 1);
				}
			}
		}
		protected override TreeViewItem BuildRoot()
		{
			TreeViewItem root = new TreeViewItem(-1, -1);
			root.children = new List<TreeViewItem>();
			AssetGroup rootGroup = m_AssetGroupTreeModel.RootAssetGroup;
			rootGroup.Parent = null;
			if (rootGroup != null)
            {
				AssetGroupTreeViewItem rootGroupItem = new AssetGroupTreeViewItem(rootGroup, 0);
				root.AddChild(rootGroupItem);
			}
			return root;
		}

		private void AddAssetGroup(object context)
        {
			if (!(context is List<AssetGroupTreeViewItem> items) || items.Count == 0)
			{
				return;
			}
			AssetGroupBrowser.OnCreateNoneRootAssetGroup(items[0].assetGroup);
		}

		private void RemoveAssetGroups(object context)
		{
			if (EditorUtility.DisplayDialog("Delete selected asset groups?", "Are you sure you want to delete the selected asset groups?\n\nThis will also deleted all related asset sets.\n\nYou cannot undo this action.", "Yes", "No"))
			{
				if (!(context is List<AssetGroupTreeViewItem> items) || items.Count == 0)
				{
					return;
				}
				bool containsChildren = false;
				foreach (AssetGroupTreeViewItem item in items)
				{
					if(item.assetGroup != null && item.assetGroup.Children.Count > 0)
                    {
						containsChildren = true;
						break;
                    }
				}
                if (containsChildren)
                {
					string hint = items.Count == 1 ? "Selected asset group has sub asset groups" : "One or more asset groups in selected items have sub asset groups";
					if(!EditorUtility.DisplayDialog("Warning!", hint + "\n\nAre you sure want to remove them?", "Yes", "No"))
                    {
						return;
                    }
				}
				foreach (AssetGroupTreeViewItem item in items)
                {
					m_Settings.RemoveAssetGroup(item.assetGroup);
                }
			}
		}

		protected override void ContextClickedItem(int id)
        {
			List<AssetGroupTreeViewItem> selectedItems = new List<AssetGroupTreeViewItem>();
			foreach (int itemId in GetSelection())
			{
				AssetGroupTreeViewItem item = FindItem(itemId, rootItem) as AssetGroupTreeViewItem;
				if (item != null)
				{
					selectedItems.Add(item);
				}
			}
			int selectedCount = selectedItems.Count;
			if (selectedCount == 0)
			{
				return;
			}
			GenericMenu menu = new GenericMenu();
			if(selectedCount == 1)
            {
				menu.AddItem(new GUIContent("Add Asset Group" ), false, AddAssetGroup, selectedItems);
				menu.AddItem(new GUIContent("Remove Asset Group"), false, RemoveAssetGroups, selectedItems);
			}
            else
            {
				menu.AddItem(new GUIContent("Remove Asset Groups"), false, RemoveAssetGroups, selectedItems);
			}
			menu.ShowAsContext();
		}


        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
            if (root.hasChildren)
            {
                AssetGroupTreeViewItem rootCatalogItem = root.children[0] as AssetGroupTreeViewItem;
                BuildAssetGroupsRecursively(rootCatalogItem.assetGroup, rootCatalogItem, 1);
            }
            var items = base.BuildRows(root);
			return items;

		}

		GUIStyle m_RowGUIStyle;
		protected override void RowGUI(RowGUIArgs args)
		{
			if (m_RowGUIStyle == null)
			{
				m_RowGUIStyle = UnityEngine.GUI.skin.GetStyle("Label");
			}
			AssetGroupTreeViewItem item = args.item as AssetGroupTreeViewItem;
			using (new EditorGUI.DisabledScope(false))
			{
				for (int i = 0; i < args.GetNumVisibleColumns(); i++)
				{
					CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
				}
			}
		}

		private void CellGUI(Rect cellRect, AssetGroupTreeViewItem item, int column, ref RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref cellRect);
			switch ((ColumnName)column)
			{
				case ColumnName.GroupName:
					float indent = GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
					cellRect.xMin += indent;
					if (Event.current.type == EventType.Repaint)
					{
						m_RowGUIStyle.Draw(cellRect, item.displayName, false, false, args.selected, args.focused);
					}
					break;
				case ColumnName.AssetSets:
					if (Event.current.type == EventType.Repaint)
					{
						m_RowGUIStyle.Draw(cellRect, new GUIContent(item.AssetGroupsStringShort, item.AssetGroupsStringLong), false, false, args.selected, args.focused);
					}
					break;
			}
		}

		protected override void SingleClickedItem(int id)
		{
			AssetGroupTreeViewItem item = FindItem(id, rootItem) as AssetGroupTreeViewItem;
			if (item != null && item.assetGroup != null)
			{
				EditorGUIUtility.PingObject(item.assetGroup);
				Selection.activeObject = item.assetGroup;
			}
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Group Id"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 200,
					minWidth = 150,
					autoResize = true,
					allowToggleVisibility = true,
					canSort = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Included Asset Sets"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 200,
					minWidth = 150,
					autoResize = true,
					allowToggleVisibility = true,
					canSort = true
				},
			};

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

		class AssetGroupTreeViewItem : TreeViewItem
		{
			public AssetGroup assetGroup;

			public AssetGroupTreeViewItem(AssetGroup assetGroup, int depth) : base(assetGroup.GUID.GetHashCode(), depth, assetGroup.GroupId.ToString())
			{
				this.assetGroup = assetGroup;
			}

			private Dictionary<string, AssetGroupTreeViewItem> m_SubItemMap = new Dictionary<string, AssetGroupTreeViewItem>();
			public Dictionary<string, AssetGroupTreeViewItem> SubItemMap
			{
				get
				{
					return m_SubItemMap;
				}
			}

			private bool m_ShowTip = false;
			public string AssetGroupsStringShort
            {
                get
                {
					if(assetGroup.AssetSets == null || assetGroup.AssetSets.Count == 0)
                    {
						return "";
                    }
					StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < assetGroup.AssetSets.Count; i++)
                    {
						AssetSet assetSets = assetGroup.AssetSets[i];
						sb.Append(assetSets.AssetSetName);
						if(i == assetGroup.AssetSets.Count - 1)
                        {
							break;
                        }
						sb.Append(", ");
                    }
					if(sb.Length > 80)
                    {
						m_ShowTip = true;
						return sb.ToString(0, 80) + "..."; 
                    }
					return sb.ToString();
                }
            }

			public string AssetGroupsStringLong
			{
				get
				{
					if (assetGroup.AssetSets == null || assetGroup.AssetSets.Count == 0 || !m_ShowTip)
					{
						return "";
					}
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < assetGroup.AssetSets.Count; i++)
					{
						AssetSet assetSet = assetGroup.AssetSets[i];
						sb.Append(assetSet.AssetSetName);
						if (i == assetGroup.AssetSets.Count - 1)
						{
							break;
						}
						sb.Append(", ");
					}
					return sb.ToString();
				}
			}
		}
	}
}
