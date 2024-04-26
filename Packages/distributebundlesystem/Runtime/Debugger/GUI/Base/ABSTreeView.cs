#if DebugABS
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI
{
    using GUI = UnityEngine.GUI;
    public class ABSTreeView : ABSPanelView
    {
        public ABSTreeView(ABSTreeViewColumn[] columns, List<ABSTreeViewItem> items) : base()
        {
            this.columns = columns;
            this.items = items;
        }
        public virtual void Reload(List<ABSTreeViewItem> items)
        {
            this.items = items;
        }

        private GUIStyle m_HeaderGUIStyle;
        protected GUIStyle HeaderGUIStyle
        {
            get
            {
                return m_HeaderGUIStyle;
            }
            set
            {
                m_HeaderGUIStyle = value;
            }
        }

        float itemsTotalWidth = 0;
        float itemsTotalHeight = 0;
        bool dragging = false;
        Vector2 scrollPosition = Vector2.zero;
        ABSTreeViewColumn[] columns;
        List<ABSTreeViewItem> items;
        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if(HeaderGUIStyle == null)
            {
                HeaderGUIStyle = GUI.skin.GetStyle("Box");
            }
            int itemsCount = items.Count;
            float headerHeight = 70f;
            
            GUI.BeginGroup(rect);
            
            int delimiterWidth = 20;
            float contentRowHeight = 50f;
            int columnsLength = columns.Length;
            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, rect.width, rect.height), scrollPosition, new Rect(0, 0, itemsTotalWidth, itemsTotalHeight));
            itemsTotalWidth = 0;
            for (int columnIndex = 0; columnIndex < columnsLength; columnIndex++)
            {
                itemsTotalHeight = 0;
                ABSTreeViewColumn column = columns[columnIndex];
                Rect headerCellRect = new Rect(itemsTotalWidth, 0, column.width, headerHeight);
                GUI.Label(headerCellRect, column.headerContent, HeaderGUIStyle);
                itemsTotalHeight += headerHeight;
                if (Event.current.type == EventType.MouseDown && headerCellRect.Contains(Event.current.mousePosition))
                {
                    dragging = true;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (dragging)
                    {
                        dragging = false;
                    }
                }
                if (dragging && headerCellRect.Contains(Event.current.mousePosition))
                {
                    column.width += Event.current.delta.x;
                    column.width = Mathf.Clamp(column.width, column.widthMin, column.widthMax);
                }
                foreach(ABSTreeViewItem item in items)
                {
                    if (item.parent.isExpand)
                    {
                        CellGUI(new Rect(itemsTotalWidth, itemsTotalHeight, column.width, contentRowHeight), item, columnIndex);
                        itemsTotalHeight += contentRowHeight;
                    }
                }
                itemsTotalWidth += column.width;
                if (columnIndex != columns.Length - 1)
                {
                    GUI.Label(new Rect(itemsTotalWidth, 0, delimiterWidth, headerHeight), "  ");
                    itemsTotalWidth += delimiterWidth;
                }
            }
            GUI.EndScrollView();
            GUI.EndGroup();
        }

        protected virtual void CellGUI(Rect cellRect, ABSTreeViewItem item, int columnIndex) { }
        
    }

}
#endif