#if DebugABS
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI.Bundle
{
    using GUI = UnityEngine.GUI;
    public class BundleTreeView : ABSTreeView
    {
        enum ColumnId
        {
            BundleNameOrAssetName = 0,
            BundlePathOrAssetDetails
        }

        public BundleTreeView(ABSTreeViewColumn[] columns, List<ABSTreeViewItem> items) : base(columns, items)
        {
        }

        public override void Reload(List<ABSTreeViewItem> items)
        {
            base.Reload(items);
        }

        float ident = 15f;
        float identCollapse = 20f;
        protected override void CellGUI(Rect cellRect, ABSTreeViewItem rawItem, int columnIndex)
        {
            BundleTreeViewItem item = rawItem as BundleTreeViewItem;
            switch ((ColumnId)columnIndex)
            {
                case ColumnId.BundleNameOrAssetName:
                    if (Event.current.type == EventType.Repaint)
                    {
                        cellRect.x += item.depth * ident;
                        if (item.hasChildren && !item.isExpand)
                        {
                            RowGUIStyle.Draw(new Rect(cellRect.x, cellRect.y, 20, cellRect.height), ">", false, false, false, false);
                        }
                        cellRect.x += identCollapse;
                        RowGUIStyle.Draw(cellRect, item.displayName, false, false, false, false);

                    }
                    if (item.isBundle)
                    {
                        int buttonWidth = 200;
                        Rect buttonRect = new Rect(cellRect.width - buttonWidth, cellRect.y, buttonWidth, cellRect.height);
                        if (GUI.Button(buttonRect, "Force Unload"))
                        {
                            item.ForceUnload();
                        }
                    }
                    goto default;
                case ColumnId.BundlePathOrAssetDetails:
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (item.isBundle)
                        {
                            RowGUIStyle.Draw(cellRect, item.RefAssets, false, false, false, false);
                        }
                        else
                        {
                            RowGUIStyle.Draw(cellRect, item.path, false, false, false, false);
                        }
                    }
                    goto default;
                default:
                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        bool? result = item.ToggleExpand();
                        if (result != null)
                        {
                            //TODO:macdeng only draw OnGUI when repaint triggerred.
                        }
                    }
                    break;

            }
        }
    }

}
#endif