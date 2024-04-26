using Runtime.AdvancedBundleSystem.Common;
using UnityEngine;
using System.Collections.Generic;

namespace Runtime.AdvancedBundleSystem.Asset
{
    public class AssetBundleCache
    {
        private string m_BundleName;
#if DebugABS
        public string BundleName => m_BundleName;

        public List<Object> RefAssets => m_RefAssets;
        private List<Object> m_RefAssets = new List<Object>();
#endif
        private AssetBundle m_AssetBundle;
        public AssetBundle AssetBundle => m_AssetBundle;

        public int ReferenceCount => m_ReferenceCount;
        private int m_ReferenceCount;

        private Dictionary<string, int> m_AssetCacheKey2InstanceCountMap = new Dictionary<string, int>();
        public AssetBundleCache(string bundleName, AssetBundle assetBundle, string assetCacheKey)
        {
            m_BundleName = bundleName;
            m_AssetBundle = assetBundle;
            m_ReferenceCount = 1;
            AddAssetInstance(assetCacheKey);
#if DebugABS
            if (ABSDebugger.Instance)
            {
                ABSDebugger.Instance.OnAddBundle(this);
            }
#endif
        }

        public void AddAssetInstance(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (m_AssetCacheKey2InstanceCountMap.ContainsKey(assetPath))
                {
                    m_AssetCacheKey2InstanceCountMap[assetPath] = m_AssetCacheKey2InstanceCountMap[assetPath] + 1;
                }
                else
                {
                    m_AssetCacheKey2InstanceCountMap.Add(assetPath, 1);
                }
#if DebugABS
                LoggerInternal.Log("++++++++++++asset paths after add+++++++++++", "green");
                foreach (var entry in m_AssetCacheKey2InstanceCountMap)
                {
                    LoggerInternal.Log(entry.Key + ": " + entry.Value);
                }
                LoggerInternal.Log("++++++++++++++++++++++++++++++++++++++++++++", "green");
#endif
            }
        }

        private void RemoveAssetInstance(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (m_AssetCacheKey2InstanceCountMap.ContainsKey(assetPath))
                {
                    m_AssetCacheKey2InstanceCountMap[assetPath] = m_AssetCacheKey2InstanceCountMap[assetPath] - 1;
                    if (m_AssetCacheKey2InstanceCountMap[assetPath] <= 0)
                    {
                        AssetCache.RemoveAssetCache(assetPath);
                        m_AssetCacheKey2InstanceCountMap.Remove(assetPath);
                    }
                }
#if DebugABS
                LoggerInternal.Log("-----------asset paths after remove---------", "red");
                foreach (var entry in m_AssetCacheKey2InstanceCountMap)
                {
                    LoggerInternal.Log(entry.Key + ": " + entry.Value);
                }
                LoggerInternal.Log("--------------------------------------------", "red");
#endif
            }
        }

        
        public void IncreaseReference()
        {
            ++m_ReferenceCount;
        }

        public void DecreaseReference(string assetPath = null)
        {
            RemoveAssetInstance(assetPath);
            if (--m_ReferenceCount == 0)
            {
                if(AssetBundle != null)
                {
                    AssetBundle.Unload(true);
#if DebugABS
                    if (ABSDebugger.Instance)
                    {
                        ABSDebugger.Instance.OnRemoveBundle(this);
                    }
#endif
                    LoggerInternal.LogFormat("unload asset bundle[{0}]", m_BundleName);
                }
                
            }
            
        }

#if DebugABS
        public void ForceUnload()
        {
            m_ReferenceCount = 0;
            if (AssetBundle != null)
            {
                AssetBundle.Unload(true);
                if (ABSDebugger.Instance)
                {
                    ABSDebugger.Instance.OnRemoveBundle(this);
                }
                LoggerInternal.LogFormat("unload asset bundle[{0}]", m_BundleName);
            }
        }
#endif

        public override string ToString()
        {
            return m_BundleName + "[" + m_ReferenceCount + "]";
        }
    }

    public class AssetBundleRefs
    {
        public AssetBundleRefs(AssetBundleCache bundleCache, AssetBundleCache[] dependencies)
        {
            BundleCache = bundleCache;
            BundleCacheDeps = dependencies;
        }
        public AssetBundleCache BundleCache;
        public AssetBundleCache[] BundleCacheDeps;
    }
}
