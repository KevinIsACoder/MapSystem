using Cysharp.Threading.Tasks;
using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using System;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Module
{
    public interface IAssetBundleModule : IModule
    {
        string BundleBasePathC { get; set; }
        string BundleBasePathP { get; set; }
        string BundleBasePathR { get; set; }
        string RootCatalogHashPathC { get; set; }
        string RootCatalogHashPathP { get; set; }
        string RootCatalogHashPathR { get; set; }
        void CheckIfRootCatalogChangedAsync(Action<bool, string> onCompleted);
        void LoadGroupCatalogAsync(int groupId, Action onCompleted);
        void UnloadGroupCatalog(int groupId);
        void UpdateAssetGroupAsync(int groupId, Action<long> onGetDownloadSize, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted);
        void CancelDownloadingAssetGroup(int groupId);
        GroupDownloadStatus GetAssetGroupStatus(int groupId);
        bool ClearAssetGroup(int groupId);
        UniTask<AssetBundleRefs> ProvideAssetBundle(string assetPath);
        GroupInfoDictionary GetSubGroupInfoMap(int groupId);
    }

}

