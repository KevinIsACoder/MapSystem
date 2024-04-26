using System;
using System.Collections.Generic;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.AdvancedBundleSystem.Utils;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    internal class AssetSetTreeView : TreeView
    {
        private AssetSetTreeModel m_AssetSetTreeModel;
        public event Action m_TreeChanged;

		private AdvancedBundleSystemSettings m_Settings;

		enum ColumnName
        {
			EntryValue = 0,
			AssetTypeOrPackingMode,
			BundleCompression,
			CompressLocalCache,
			InPackage
        }
        public AssetSetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetSetTreeModel assetGroupEntryModel) : base(state, multiColumnHeader)
        {
            Init(assetGroupEntryModel);
        }

        private void Init(AssetSetTreeModel assetSetTreeModel)
        {
            m_AssetSetTreeModel = assetSetTreeModel;
            m_AssetSetTreeModel.OnModelChanged += ModelChanged;
			m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
        }

        private void ModelChanged()
        {
            m_TreeChanged?.Invoke();
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
			TreeViewItem root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            foreach (AssetSet set in m_AssetSetTreeModel.AssetSets)
            {
				AssetSetTreeViewItem item = new AssetSetTreeViewItem(set, 0);//TODO:macdeng deal with missing AssetGroups
				root.AddChild(item);
            }
            return root;
		}

		private void BuildAssetSetEntries(AssetEntry entry, AssetSetTreeViewItem setItem, int depth)
        {
            string entryGUID = entry.GUID;
			if (!setItem.SubItemMap.TryGetValue(entryGUID, out AssetSetTreeViewItem entryItem))
			{
				entryItem = new AssetSetTreeViewItem(entry, depth);
				setItem.AddChild(entryItem);
				setItem.SubItemMap.Add(entryGUID, entryItem);
			}
            if (!IsExpanded(setItem.id))
            {
                return;
            }
            BuildSubEntriesRecursively(entryItem, entry, depth + 1);
        }

		private void BuildSubEntriesRecursively(AssetSetTreeViewItem parentItem, AssetEntry parentEntry, int depth)
        {
			parentEntry.GenerateSubEntries();
			foreach(AssetEntry entry in parentEntry.SubEntries)
            {
                string entryGUID = entry.GUID;
                if (!parentItem.SubItemMap.TryGetValue(entryGUID, out AssetSetTreeViewItem item))
                {
					item = new AssetSetTreeViewItem(entry, depth);
					parentItem.AddChild(item);
					parentItem.SubItemMap.Add(entryGUID, item);
                }
                if (!IsExpanded(parentItem.id))
                {
					return;
                }
                BuildSubEntriesRecursively(item, entry, depth + 1);
			}
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
			List<TreeViewItem> setItems = root.children;
			foreach (TreeViewItem item in setItems)
			{
				AssetSetTreeViewItem setItem = item as AssetSetTreeViewItem;
				AssetSet assetSet = setItem.assetSet;
				if (assetSet != null && assetSet.Entries.Count > 0)
				{
					foreach (AssetEntry entry in assetSet.Entries)
					{
                        BuildAssetSetEntries(entry, setItem, 1);
					}
				}
			}
            var items = base.BuildRows(root);
            return items;
			
        }

        GUIStyle m_RowGUIStyle;
		protected override void RowGUI(RowGUIArgs args)
        {
			if (m_RowGUIStyle == null) //TODO:macdeng
			{
                m_RowGUIStyle = UnityEngine.GUI.skin.GetStyle("Label");
			}
			AssetSetTreeViewItem item = args.item as AssetSetTreeViewItem;
			using(new EditorGUI.DisabledScope(item.IsReadOnly))
            {
				for(int i = 0; i < args.GetNumVisibleColumns(); i++)
                {
					CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
				}
            }
        }

		private void CellGUI(Rect cellRect, AssetSetTreeViewItem item, int column, ref RowGUIArgs args)
		{
            CenterRectUsingSingleLineHeight(ref cellRect);
            switch ((ColumnName)column)
			{
				case ColumnName.EntryValue:
					float indent = GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
					cellRect.xMin += indent;
					if (Event.current.type == EventType.Repaint)
					{
						m_RowGUIStyle.Draw(cellRect, item.displayName, false, false, args.selected, args.focused);
					}
					break;
				case ColumnName.BundleCompression:
					if (Event.current.type == EventType.Repaint)
					{
						cellRect.xMin += 47;
						m_RowGUIStyle.Draw(cellRect, item.BundleCompressionString, false, false, args.selected, args.focused);
					}
					break;
				case ColumnName.CompressLocalCache:
					if (Event.current.type == EventType.Repaint)
					{
						cellRect.xMin += 57;
						m_RowGUIStyle.Draw(cellRect, item.CompressLocalCacheString, false, false, args.selected, args.focused);
					}
					break;
				case ColumnName.InPackage:
					if (Event.current.type == EventType.Repaint)
					{
						m_RowGUIStyle.Draw(cellRect, item.InPackageString, false, false, args.selected, args.focused);
					}
					break;
				case ColumnName.AssetTypeOrPackingMode:
					if (Event.current.type == EventType.Repaint)
					{
						if (item.IsAssetSet)
						{
							cellRect.xMin += 36;
							m_RowGUIStyle.Draw(cellRect, item.PackingString, false, false, args.selected, args.focused);
						}
						else if (item.assetIcon != null)
						{
							UnityEngine.GUI.DrawTexture(cellRect, item.assetIcon, ScaleMode.ScaleToFit, true);
						}
					}
					break;

			}
		}

        protected override void ContextClickedItem(int id)
        {
			List<AssetSetTreeViewItem> selectedItems = new List<AssetSetTreeViewItem>();
			foreach(int itemId in GetSelection())
            {
				AssetSetTreeViewItem item = FindItem(itemId, rootItem) as AssetSetTreeViewItem;
				if(item != null)
                {
					selectedItems.Add(item);
                }
            }

			int selectedCount = selectedItems.Count;
			if(selectedCount == 0)
            {
				return;
            }
			bool hasAssetSet = false;
			bool hasAssetEntry = false;
			bool hasReadOnly = false;
			bool hasFolder = false;
			foreach(AssetSetTreeViewItem item in selectedItems)
            {
                if (item.IsAssetSet)
                {
					hasAssetSet = true;
                }
                else
                {
					hasAssetEntry = true;
                }
                if (item.IsReadOnly)
                {
					hasReadOnly = true;
                }
                if (item.IsFolder)
                {
					hasFolder = true;
                }
            }
			if (hasAssetSet && hasAssetEntry)
            {
				return;
            }
			GenericMenu menu = new GenericMenu();
            if (!hasReadOnly && hasAssetSet)
            {
				menu.AddItem(new GUIContent("Remove Asset Set" + (selectedCount > 1 ? "s" : "")), false, RemoveAssetSets, selectedItems);
            }
			else if (!hasReadOnly && hasAssetEntry)
            {
				menu.AddItem(new GUIContent("Remove Asset " + (selectedCount > 1 ? "Entries" : "Entry")), false, RemoveAssetEntries, selectedItems);
			}
			if(selectedCount == 1 && !hasFolder && !hasAssetSet)
            {
				menu.AddItem(new GUIContent("Copy Asset Path to Clipboard"), false, CopyAssetPath, selectedItems[0].AssetPath);
            }

			menu.ShowAsContext();
		}

		private void CopyAssetPath(object context)
        {
			EditorGUIUtility.systemCopyBuffer = context as string;
		}


		private void RemoveAssetSets(object context)
        {
            if(EditorUtility.DisplayDialog("Delete selected asset sets?", "Are you sure you want to delete the selected asset sets?\n\nYou cannot undo this action.", "Yes", "No"))
            {
                if (!(context is List<AssetSetTreeViewItem> items) || items.Count == 0)
                {
                    return;
                }
                foreach (AssetSetTreeViewItem item in items)
                {
					m_Settings.RemoveAssetSet(item.assetSet);
				}
            }
        }

		private void RemoveAssetEntries(object context)
        {
			if (EditorUtility.DisplayDialog("Delete selected asset entries?", "Are you sure you want to delete the selected asset entries?\n\nYou cannot undo this action.", "Yes", "No"))
			{
				if (!(context is List<AssetSetTreeViewItem> items) || items.Count == 0)
				{
					return;
				}
				foreach (AssetSetTreeViewItem item in items)
				{
					AssetSetTreeViewItem parent = item.parent as AssetSetTreeViewItem; //TODO:macdeng find the group which item belong to recursively

					m_Settings.RemoveAssetEntry(item.assetEntry, parent.assetSet);
				}
			}
		}

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
			DragAndDropVisualMode mode = DragAndDropVisualMode.None;
			AssetSetTreeViewItem target = args.parentItem as AssetSetTreeViewItem;
			//if(target != null && target.IsAssetSet) //TODO:macdeng other cases need to be considerate
   //         {
			//	mode = DragAndDropVisualMode.Copy;
   //         }
   //         else
   //         {
			//	mode = DragAndDropVisualMode.Rejected;
			//	//TODO:macdeng
			//}
			if(DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            {
				mode = HandleDragAndDropOutsideTreeView(target, args);
            }
            else
            {
				mode = HandleDragAndDropInsideTreeView(target, args);
			}
			return mode;
		}

		private bool IsPathAsseSet(string path)
		{
			return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AssetSet);
		}

		private DragAndDropVisualMode HandleDragAndDropOutsideTreeView(AssetSetTreeViewItem target, DragAndDropArgs args)
        {
			DragAndDropVisualMode mode = DragAndDropVisualMode.None;
			bool isPathsContainsAssetSet = false;
			foreach(string path in DragAndDrop.paths)
            {
                if (IsPathAsseSet(path))
                {
					isPathsContainsAssetSet = true;
					break;
                }
                if (!EditorUtils.IsPathValidEntry(path))
                {
					return DragAndDropVisualMode.Rejected;
                }
            }
            if (isPathsContainsAssetSet)
            {
				return DragAndDropVisualMode.Rejected;
			}
			mode = DragAndDropVisualMode.Copy;
            if (args.performDrop)
            {
                if (target.IsAssetSet)
                {
					AssetSet assetSet = target.assetSet;
					if(assetSet != null)
                    {
						m_Settings.CreateAssetEntries(DragAndDrop.paths, assetSet);
                    }
                }
            }
			return mode;
        }

		private DragAndDropVisualMode HandleDragAndDropInsideTreeView(AssetSetTreeViewItem target, DragAndDropArgs args)
        {
			//TODO:macdeng
			return DragAndDropVisualMode.None;
        }


		protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
			DragAndDrop.PrepareStartDrag();

			DragAndDrop.paths = null;
			DragAndDrop.objectReferences = new UnityEngine.Object[] { };
			//DragAndDrop.SetGenericData("AssetEntryTreeViewItem", selectedNodes);
			//DragAndDrop.visualMode = selectedNodes.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
			DragAndDrop.StartDrag("AssetSetTree");
		}

        protected override void SingleClickedItem(int id)
        {
			AssetSetTreeViewItem item = FindItem(id, rootItem) as AssetSetTreeViewItem;
			if(item != null)
            {
				if(item.assetSet != null)
                {
					EditorGUIUtility.PingObject(item.assetSet);
					Selection.activeObject = item.assetSet;
				}
				else if(item.assetEntry != null)
                {
					UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.assetEntry.AssetPath);
					if (go != null)
					{
                        EditorGUIUtility.PingObject(go);
                        Selection.activeObject = go;
					}
				}
            }
        }


        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Asset Set Name \\ Asset Path"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 230,
					minWidth = 150,
					autoResize = true,
					allowToggleVisibility = true,
					canSort = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Packing \\ Asset Type"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 135,
					minWidth = 135,
					maxWidth = 135,
					autoResize = false,
					allowToggleVisibility = true,
					canSort = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Bundle Compression"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 135,
					minWidth = 135,
					maxWidth = 135,
					autoResize = false,
					allowToggleVisibility = true,
					canSort = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Compress Local Cache"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 147,
					minWidth = 147,
					maxWidth = 147,
					autoResize = false,
					allowToggleVisibility = true,
					canSort = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("In Package"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 100,
					maxWidth = 100,
					autoResize = false,
					allowToggleVisibility = true,
					canSort = true
				},
			};

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

		class AssetSetTreeViewItem: TreeViewItem
        {
			public AssetEntry assetEntry;
			public AssetSet assetSet;
			public Texture2D assetIcon;

			public bool IsReadOnly
            {
                get
                {
                    if (IsAssetSet)
                    {
						return false;
                    }
					if(assetEntry != null && parent != null && ((parent as AssetSetTreeViewItem).IsAssetSet))
                    {
						return false;
                    }
					return true;
                }
            }

			public bool IsFolder
            {
                get
                {
                    if (IsAssetSet)
                    {
						return false;
                    }
					if(assetEntry != null && assetEntry.IsFolder)
                    {
						return true;
                    }
					return false;
                }
            }

			public string AssetPath
            {
                get
                {
                    if (IsFolder)
                    {
						return null;
                    }
                    if (IsAssetSet)
                    {
						return null;
                    }
					return assetEntry.AssetPath;
                }
            }

			public AssetSetTreeViewItem(AssetSet assetSet, int depth) : base(assetSet.GUID.GetHashCode(), depth, assetSet.AssetSetName)
            {
				this.assetSet = assetSet;
				assetEntry = null;
				assetIcon = null;
            }

			public AssetSetTreeViewItem(AssetEntry entry, int depth) : base(entry.GUID.GetHashCode(), depth, entry.AssetPath)
            {
				assetEntry = entry;
				assetSet = null;
				assetIcon = AssetDatabase.GetCachedIcon(entry.AssetPath) as Texture2D;

			}
			public bool IsAssetSet
            {
				get 
				{ 
					return assetSet != null && assetEntry == null; 
				}
            }

			public string BundleCompressionString
            {
                get
                {
					if(assetSet == null)
                    {
						return "";
                    }
					string ret;
                    switch (assetSet.BundleCompressionMethod) 
					{
						case BundleCompression.LZ4:
							ret = "LZ4";
							break;
						case BundleCompression.LZMA:
							ret = "LZMA";
							break;
						case BundleCompression.Uncompressed:
							ret = "None";
							break;
						default:
							ret = "";
							break;
					}
					return ret;
                }
            }

			public string PackingString
            {
                get
                {
					if (assetSet == null)
					{
						return "";
					}
					string ret;
					switch (assetSet.PackingMode)
					{
						case PackingMode.Together:
							ret = "Together";
							break;
						case PackingMode.Separate:
							ret = "Separate";
							break;
						default:
							ret = "";
							break;
					}
					return ret;
				}
            }

			public string CompressLocalCacheString
            {
                get
                {
					return assetSet == null ? "" : assetSet.CompressLocalCache ? "Yes" : "No";
                }
            }

			public string InPackageString
			{
				get
				{
					return assetSet == null ? "" : assetSet.InPackage ? "Yes" : "No";
				}
			}

			private Dictionary<string, AssetSetTreeViewItem> m_SubItemMap = new Dictionary<string, AssetSetTreeViewItem>();
			public Dictionary<string, AssetSetTreeViewItem> SubItemMap
			{
                get
                {
					return m_SubItemMap;
                }
            }

            public override string displayName 
			{
				get
                {
					return IsAssetSet ? base.displayName + "[" + assetSet.AssetGroup.GroupId + "]" : base.displayName;
                }
                set
                {
					base.displayName = value;
                }
			}

        }
	}
}
