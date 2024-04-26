using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using Runtime.AdvancedBundleSystem.Module;
using Runtime.AdvancedBundleSystem.ModuleImpl;
using System;
using System.Runtime.CompilerServices;
using Runtime.AdvancedBundleSystem.Common.Gen;

[assembly: InternalsVisibleTo("ABS_TestRuntime")]
namespace Runtime.AdvancedBundleSystem
{
    public static class ResourceManager
    {
        private static IAssetModule m_AssetModule;
        internal static IAssetModule AssetModule
        {
            get
            {
                return m_AssetModule;
            }
        }
        private static IAssetBundleModule m_AssetBundleModule;
        internal static IAssetBundleModule AssetBundleModule
        {
            get
            {
                return m_AssetBundleModule;
            }
        }

        private static IDownloaderModule m_DownloaderModule;
        internal static IDownloaderModule DownloaderModule
        {
            get
            {
                return m_DownloaderModule;
            }
        }

        private static bool m_IsEditorMode;

        public static bool IsEditorMode
        {
            get
            {
                return m_IsEditorMode;
            }
        }

        /// <summary>
        /// init modules.
        /// </summary>
        public static void Init()
        {
            m_IsEditorMode = RuntimeSettings.c_IsEditorMode;
#if UNITY_EDITOR
            if (m_IsEditorMode)
            {
                m_AssetModule = new EditorAssetModule();
                m_AssetBundleModule = new DummyAssetBundleModule();
                m_AssetModule.Init();
                m_AssetBundleModule.Init();
            }
            else
            {
                m_AssetModule = new BundleAssetModule();
                m_AssetBundleModule = new RealAssetBundleModule();
                m_DownloaderModule = new DownloaderModule();
                m_AssetModule.Init();
                m_AssetBundleModule.Init();
                m_DownloaderModule.Init();
            }
#else
            m_AssetModule = new BundleAssetModule();
            m_AssetBundleModule = new RealAssetBundleModule();
            m_DownloaderModule = new DownloaderModule();
            m_AssetModule.Init();
            m_AssetBundleModule.Init();
            m_DownloaderModule.Init();
#endif
        }

        public static void CheckIfRootCatalogChangedAsync(Action<bool, string> onCompleted)
        {
            AssetBundleModule.CheckIfRootCatalogChangedAsync(onCompleted);
        }

        /// <summary>
        /// destroy modules.
        /// </summary>
        public static void Destroy()
        {
            m_AssetModule?.Destroy();
            m_AssetBundleModule?.Destroy();
#if !UNITY_EDITOR
            m_DownloaderModule?.Destroy();
#endif
        }

        /// <summary>
        /// load group catalog before using asset group which group id is <paramref name="groupId"/>
        /// </summary>
        /// <param name="groupId">the id of asset group</param>
        public static void LoadGroupCatalog(int groupId, Action onCompleted)
        {
            m_AssetBundleModule.LoadGroupCatalogAsync(groupId, onCompleted);
        }

        /// <summary>
        /// unload group catalog after finished using asset group which group id is <paramref name="groupId"/>
        /// </summary>
        /// <param name="groupId">the id of asset group</param>
        public static void UnloadGroupCatalog(int groupId)
        {
            m_AssetBundleModule.UnloadGroupCatalog(groupId);
        }

        /// <summary>
        /// Don't increase counter when load an asset by <paramref name="weak"/> mode. see: <see cref="IsLoadingAnyAsset"/>
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="onCompleted"></param>
        /// <param name="weak">true: just load asset and don't increase counter. false: load asset and increase counter</param>
        public static void LoadAssetAsync(string assetPath, Action<AssetHandle> onCompleted, bool weak = false)
        {
            m_AssetModule.LoadAssetAsync(assetPath, onCompleted, weak);
        }

        /// <summary>
        /// Don't increase counter when load an asset by weak mode. see: <see cref="LoadAssetAsync"/>
        /// </summary>
        /// <returns>return true if counter is larger than zero, otherwise return false.</returns>
        public static bool IsLoadingAnyAsset()
        {
            return m_AssetModule.IsLoadingAnyAsset();
        }

        /// <summary>
        /// Download asset group which group id is <paramref name="groupId"/>
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="onGetDownloadSize"></param>
        /// <param name="onProgressValueChanged"></param>
        /// <param name="onCompleted"></param>
        public static void UpdateAssetGroupAsync(int groupId, Action<long> onGetDownloadSize, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted)
        {
            m_AssetBundleModule.UpdateAssetGroupAsync(groupId, onGetDownloadSize, onProgressValueChanged, onCompleted);
        }

        /// <summary>
        /// Cancel current downloading asset group which group id is <paramref name="groupId"/>
        /// </summary>
        /// <param name="groupId"></param>
        public static void CancelDownloadingAssetGroup(int groupId)
        {
            m_AssetBundleModule.CancelDownloadingAssetGroup(groupId);
        }

        /// <summary>
        /// Get asset group download status which group id is <paramref name="groupId"/>
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static GroupDownloadStatus GetAssetGroupStatus(int groupId)
        {
            return m_AssetBundleModule.GetAssetGroupStatus(groupId);
        }

        /// <summary>
        /// Delete downloaded asset group(in persistent path) which group id is <paramref name="groupId"/>
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static bool ClearAssetGroup(int groupId)
        {
            return m_AssetBundleModule.ClearAssetGroup(groupId);
        }

        /// <summary>
        /// Get asset group's sub group info map
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static GroupInfoDictionary GetSubCatalogInfoMap(int groupId)
        {
            return m_AssetBundleModule.GetSubGroupInfoMap(groupId);
        }
    }
}
