using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Module;
using System;

namespace Runtime.AdvancedBundleSystem.ModuleImpl
{
    public class BundleAssetModule : BaseModule, IAssetModule
    {
        private int m_LoadingAssetsCount;
        public override void Init()
        {
            if (m_Initialized)
            {
                return;
            }
            m_LoadingAssetsCount = 0;
            base.Init();
        }
        public bool IsLoadingAnyAsset()
        {
            return m_LoadingAssetsCount > 0;
        }

        public void LoadAssetAsync(string assetPath, Action<AssetHandle> onCompleted, bool weak = false)
        {
            AssetLoader assetLoader = AssetLoader.Acquire();
            AssetHandle assetHandle = new AssetHandle(assetLoader);
            assetHandle.Completed += (handle) => {
                if (!weak)
                {
                    --m_LoadingAssetsCount;
                }
                onCompleted?.Invoke(handle);
            };
            assetLoader.Init(assetHandle, assetPath);
            if (!weak)
            {
                ++m_LoadingAssetsCount;
            }
            assetLoader.BeginLoad().Forget();
        }
    }
}
