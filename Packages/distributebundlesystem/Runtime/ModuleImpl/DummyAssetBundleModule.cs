#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using Runtime.AdvancedBundleSystem.Module;
using System;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.ModuleImpl
{
    public class DummyAssetBundleModule : BaseModule, IAssetBundleModule
    {
        public string BundleBasePathR { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string BundleBasePathC { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string BundleBasePathP { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RootCatalogHashPathC { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RootCatalogHashPathP { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RootCatalogHashPathR { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void CancelDownloadingAssetGroup(int groupId)
        {
        }

        public void CheckIfRootCatalogChangedAsync(Action<bool, string> onCompleted)
        {
            onCompleted?.Invoke(false, null);
        }

        public bool ClearAssetGroup(int groupId)
        {
            return true;
        }

        public GroupDownloadStatus GetAssetGroupStatus(int groupId)
        {
            return GroupDownloadStatus.Uptodate;
        }

        public UniTask<AssetBundleRefs> ProvideAssetBundle(string assetPath)
        {
            return default;
        }

        public void LoadGroupCatalogAsync(int groupId, Action onCompleted)
        {
            onCompleted?.Invoke();
        }

        public void UnloadGroupCatalog(int groupId)
        {
        }

        public void UpdateAssetGroupAsync(int groupId, Action<long> onGetDownloadSize, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted)
        {
            throw new NotImplementedException();
        }

        public GroupInfoDictionary GetSubGroupInfoMap(int groupId)
        {
            return null;
        }
    }
}
#endif