using Runtime.AdvancedBundleSystem.Asset;
using System;

namespace Runtime.AdvancedBundleSystem.Module
{
    public interface IAssetModule : IModule
    {
        void LoadAssetAsync(string assetPath, Action<AssetHandle> onCompleted, bool weak = false);

        bool IsLoadingAnyAsset();
    }

}
