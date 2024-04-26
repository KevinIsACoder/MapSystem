#if DebugABS
using System.Collections.Generic;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI
{
    public class ABSTreeViewItem
    {
        public ABSTreeViewItem(int id, int depth, string name, bool isExpand)
        {
            this.id = id;
            this.depth = depth;
            this.name = name;
            this.isExpand = isExpand;
        }

        public ABSTreeViewItem(int id, int depth) : this(id, depth, "", true) { }
        public int id { get; set; }
        public string name { get; set; }
        public int depth { get; set; }
        public bool isExpand { get; set; }
        public bool hasChildren
        {
            get
            {
                return children != null && children.Count > 0;
            }
        }

        public bool? ToggleExpand()
        {
            if (!isExpand && hasChildren)
            {
                isExpand = true;
                return true;
            }
            else if (isExpand)
            {
                isExpand = false;
                return false;
            }
            return null;
        }

        public virtual string displayName
        {
            get
            {
                if (hasChildren)
                {
                    return name + "(" + children.Count + ")";
                }
                return name;
            }
        }
        public List<ABSTreeViewItem> children { get; set; }
        public ABSTreeViewItem parent { get; set; }
        public void AddChild(ABSTreeViewItem child)
        {
            if (children == null)
            {
                children = new List<ABSTreeViewItem>();
            }
            children.Add(child);
        }

        public void RemoveChild(ABSTreeViewItem child)
        {
            if(children == null)
            {
                throw new System.Exception("no children to remove!");
            }
            children.Remove(child);
        }
    }

}
#endif