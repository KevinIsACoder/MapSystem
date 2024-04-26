using Cysharp.Threading.Tasks;
using Runtime.AdvancedBundleSystem.Common;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Runtime.AdvancedBundleSystem.Asset
{
    
    internal class AssetLoader
    {
        private AssetHandle m_AssetHandle;
        private string m_AssetPath;
        private AssetBundleRefs m_AssetBundleRefs;
        private bool m_IsDestroyed = false;

        internal static AssetLoader Acquire()
        {
            //TODO:macdeng 
            return new AssetLoader();
        }

        internal static void Recycle(AssetLoader assetLoader)
        {
            //TODO:macdeng 
            assetLoader.Reset();
        }

        internal void Init(AssetHandle assetHandle, string assetPath)
        {
            m_AssetHandle = assetHandle;
            m_AssetPath = assetPath;
        }

        internal async UniTaskVoid BeginLoad()
        {
            if(AssetCache.TryGetAssetCache(m_AssetPath, out AssetCache assetCache))
            {
                m_AssetBundleRefs = assetCache.AssetBundleRefs;
                m_AssetBundleRefs.BundleCache.AddAssetInstance(m_AssetPath);
                if (m_AssetBundleRefs.BundleCacheDeps != null)
                {
                    foreach (AssetBundleCache bundleCache in m_AssetBundleRefs.BundleCacheDeps)
                    {
                        bundleCache.IncreaseReference();
                    }
                }
                m_AssetBundleRefs.BundleCache.IncreaseReference();
                Result = assetCache.Result;
#if DebugABS
                AddRefAsset();
#endif
                LoggerInternal.LogFormat("get asset[{0}] from asset cache", m_AssetPath);
                Completed(m_AssetHandle);
                return;
            }
            if(m_AssetBundleRefs == null)
            {
                m_AssetBundleRefs = await ResourceManager.AssetBundleModule.ProvideAssetBundle(m_AssetPath);
            }
            BeginAssetLoad();
        }

        private void BeginAssetLoad()
        {
            if(m_AssetBundleRefs == null)
            {
                throw new Exception(string.Format("can't load asset[{0}]", m_AssetPath));
            }
            AssetBundleRequest request = m_AssetBundleRefs.BundleCache.AssetBundle.LoadAssetAsync<Object>(m_AssetPath);
            request.completed += OnLoadAssetCompleted;
        }

#if DebugABS
        private void AddRefAsset()
        {
            if(m_AssetBundleRefs.BundleCacheDeps != null)
            {
                foreach (AssetBundleCache bundleCache in m_AssetBundleRefs.BundleCacheDeps)
                {
                    bundleCache.RefAssets.Add(Result);
                }
            }   
            m_AssetBundleRefs.BundleCache.RefAssets.Add(Result);
        }

        private void RemoveRefAsset()
        {
            if (m_AssetBundleRefs.BundleCacheDeps != null)
            {
                foreach (AssetBundleCache bundleCache in m_AssetBundleRefs.BundleCacheDeps)
                {
                    bundleCache.RefAssets.Remove(Result);
                }
            }
            m_AssetBundleRefs.BundleCache.RefAssets.Remove(Result);
        }
#endif
        private void OnLoadAssetCompleted(AsyncOperation obj)
        {
            AssetBundleRequest req = (AssetBundleRequest)obj;
            
            //TODO:macdeng
            if (req.asset != null)
            {
                Result = req.asset;
                AssetCache assetCache = new AssetCache(Result, m_AssetBundleRefs);
#if DebugABS
                AddRefAsset();
#endif
                AssetCache.AddAssetCache(m_AssetPath, assetCache);
                Completed(m_AssetHandle);
            }
            else
            {
                Result = null;
                Completed(m_AssetHandle);
            }
        }

#if UNITY_EDITOR
        internal void OnFakeCompleted()
        {
            Completed(m_AssetHandle);
        }
#endif

        internal bool IsDestroyed
        {
            get
            {
                return m_IsDestroyed;
            }
        }
        internal void OnDestroy()
        {
            LoggerInternal.Log("start unload asset loader", "blue");
            //TODO:macdeng need to optimized. perhaps using linked list way to organize all assetloader.
            //TODO:macdeng draw a graph to describe the relationship between assetbundlecache and assetcache
            if (!ResourceManager.IsEditorMode)
            {
                if(m_AssetBundleRefs.BundleCacheDeps != null)
                {
                    foreach (AssetBundleCache bundleCache in m_AssetBundleRefs.BundleCacheDeps) 
                    {
                        bundleCache.DecreaseReference();
                    }
                }
                m_AssetBundleRefs.BundleCache.DecreaseReference(m_AssetPath);
                LoggerInternal.Log("end unload asset loader", "blue");
            }
#if DebugABS
            if (!ResourceManager.IsEditorMode)
            {
                RemoveRefAsset();
            }
#endif
            Result = null;
            m_AssetBundleRefs = null;
            m_IsDestroyed = true;
        }

        

        internal Object Result
        {
            get;set;
        }

        private void Reset()
        {
            //TODO:macdeng
        }

        internal event Action<AssetHandle> Completed;


    }
}
