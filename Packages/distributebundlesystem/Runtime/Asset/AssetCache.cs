using Runtime.AdvancedBundleSystem.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Asset
{
    public class AssetCache
    {
        private static Dictionary<string, AssetCache> m_AssetCacheMap = new Dictionary<string, AssetCache>();

        public Object Result;
        public AssetBundleRefs AssetBundleRefs;

        public AssetCache(Object result, AssetBundleRefs assetBundleRefs)
        {
            Result = result;
            AssetBundleRefs = assetBundleRefs;
        }

        public static void AddAssetCache(string assetPath, AssetCache assetCache)
        {
            if (m_AssetCacheMap.ContainsKey(assetPath))
            {
                LoggerInternal.LogWarningFormat("inconsistent: already cached result of [{0}]", assetPath);
                return;
            }
            m_AssetCacheMap[assetPath] = assetCache;
            LoggerInternal.LogFormat("added asset cache[{0}]", assetPath);
        }

        public static void RemoveAssetCache(string assetPath)
        {
            if (!m_AssetCacheMap.ContainsKey(assetPath))
            {
                LoggerInternal.LogErrorFormat("inconsistent: cache unavailable [{0}]", assetPath);
                return;
            }
            m_AssetCacheMap.Remove(assetPath);
            LoggerInternal.LogFormat("removed asset cache[{0}]", assetPath);
        }

        public static bool TryGetAssetCache(string assetPath, out AssetCache assetCache)
        {
            if(m_AssetCacheMap.TryGetValue(assetPath, out assetCache))
            {
                return true;
            }
            return false;
        }
    }
}
