#if DebugABS
using Runtime.AdvancedBundleSystem.Asset;
using System.Text;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI.Bundle
{
    public class BundleTreeViewItem : ABSTreeViewItem
    {
        public BundleTreeViewItem(int id, int depth, string name, bool isBundle, bool isExpand) : base(id, depth, name, isExpand)
        {
            this.isBundle = isBundle;
        }

        public BundleTreeViewItem(int id, int depth) : this(id, depth, "", false, true) { }

        public bool isBundle { get; set; }

        public AssetBundleCache bundleCahce { get; set; }
        public string path { get; set; }

        public void ForceUnload()
        {
            bundleCahce.ForceUnload();
        }

        public string RefAssets
        {
            get
            {
                if(bundleCahce.RefAssets.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();//TODO:macdeng added global cache.
                    foreach(Object obj in bundleCahce.RefAssets)
                    {
                        string name = "";
                        int instanceId = 0;
                        if(obj is GameObject GO)
                        {
                            name = GO.name;
                            instanceId = GO.GetInstanceID();
                        }
                        else if(obj is TextAsset text)
                        {
                            name = text.name;
                            instanceId = text.GetInstanceID();
                        }
                        else if(obj is Texture texture)
                        {
                            name = texture.name;
                            instanceId = texture.GetInstanceID();
                        }
                        else if(obj is Material material)
                        {
                            name = material.name;
                            instanceId = material.GetInstanceID();
                        }
                        else
                        {
                            Debug.LogError("unknown asset!");
                        }
                        sb.Append(name)
                            .Append('-')
                            .Append(instanceId)
                            .Append(',');
                  
                    }
                    return sb.ToString(0, sb.Length - 1);
                }
                else
                {
                    return "No Ref Assets";
                }
            }
        }

        public override string displayName
        {
            get
            {
                if (isBundle)
                {
                    return name + "|ref[" + bundleCahce.ReferenceCount + "]";
                }
                return name;
            }
        }
    }
}
#endif