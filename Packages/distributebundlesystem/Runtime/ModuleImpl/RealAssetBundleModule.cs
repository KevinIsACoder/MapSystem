using Cysharp.Threading.Tasks;
using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Common;
using Runtime.AdvancedBundleSystem.Common.Deserializer;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using Runtime.AdvancedBundleSystem.Common.Serializer;
using Runtime.AdvancedBundleSystem.Common.Util;
using Runtime.AdvancedBundleSystem.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using Runtime.AdvancedBundleSystem.Common.Gen;
using UnityEngine;
using Object = System.Object;

namespace Runtime.AdvancedBundleSystem.ModuleImpl
{
    public class RealAssetBundleModule : BaseModule, IAssetBundleModule
    {
        public override void Init()
        {
            if (m_Initialized)
            {
                return;
            }
            base.Init();
            BundleBasePathC = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, RuntimeSettings.c_AssetBundleOutputPath, RuntimeUtils.GetPlatformName());
            BundleBasePathP = string.Format("{0}/{1}/{2}", Application.persistentDataPath, RuntimeSettings.c_CachingRootPath, RuntimeSettings.c_AssetBundleOutputPath);
            BundleBasePathR = RuntimeSettings.c_ServerAddress;

            string rootAssetGroupIdString = RuntimeSettings.c_RootAssetGroupId.ToString();
            RootCatalogHashPathC = string.Format("{0}/{1}.hash", m_BundleBasePathC, rootAssetGroupIdString);
            RootCatalogHashPathP = string.Format("{0}/{1}.hash", m_BundleBasePathP, rootAssetGroupIdString);
            RootCatalogHashPathR = string.Format("{0}/{1}.hash", m_BundleBasePathR, rootAssetGroupIdString);

        }

        private string m_BundleBasePathC;
        public string BundleBasePathC
        {
            get
            {
                return m_BundleBasePathC;
            }
            set
            {
                m_BundleBasePathC = value;
            }
        }

        private string m_BundleBasePathP;
        public string BundleBasePathP
        {
            get
            {
                return m_BundleBasePathP;
            }
            set
            {
                m_BundleBasePathP = value;
            }
        }

        private string m_BundleBasePathR;
        public string BundleBasePathR
        {
            get
            {
                return m_BundleBasePathR;
            }
            set
            {
                m_BundleBasePathR = value;
            }
        }

        private string m_RootCatalogHashPathC;
        public string RootCatalogHashPathC
        {
            get
            {
                return m_RootCatalogHashPathC;
            }
            set
            {
                m_RootCatalogHashPathC = value;
            }
        }

        private string m_RootCatalogHashPathP;
        public string RootCatalogHashPathP
        {
            get
            {
                return m_RootCatalogHashPathP;
            }
            set
            {
                m_RootCatalogHashPathP = value;
            }
        }

        private string m_RootCatalogHashPathR;
        public string RootCatalogHashPathR
        {
            get
            {
                return m_RootCatalogHashPathR;
            }
            set
            {
                m_RootCatalogHashPathR = value;
            }
        }


        private Dictionary<int, GroupCatalog> m_LoadedCatalogs = new Dictionary<int, GroupCatalog>();

        private HashSet<int> m_DownloadingAssetGroupSet = new HashSet<int>();

        private HashSet<BundleInfo> m_BundleDecryptingSet = new();

        private void FetchRemoteRootCatalogHashAsync(Action<string> onCompleted)
        {
            ResourceManager.DownloaderModule.DownloadTextAsync(RootCatalogHashPathR, onCompleted);
        }
        public void CheckIfRootCatalogChangedAsync(Action<bool, string> onCompleted)
        {
            FetchRemoteRootCatalogHashAsync((remoteRootCatalogHash) =>
                {
                    LoggerInternal.LogFormat("remote root catalog hash: {0}", remoteRootCatalogHash);
                    string clientRootCatalogHashPath = File.Exists(RootCatalogHashPathP) ? RootCatalogHashPathP : RootCatalogHashPathC;
                    LoggerInternal.LogFormat("client root catalog hash path: {0}", clientRootCatalogHashPath);
                    RuntimeUtils.ReadTextAsync(clientRootCatalogHashPath, (clientRootCatalogHash) => 
                    {
                        LoggerInternal.LogFormat("client root catalog hash: {0}", clientRootCatalogHash);
                        bool rootCatalogChanged = clientRootCatalogHash != remoteRootCatalogHash;
                        onCompleted?.Invoke(rootCatalogChanged, rootCatalogChanged ? remoteRootCatalogHash : null);
                    });
                }
            );
        }

        public void CancelDownloadingAssetGroup(int groupId)
        {
            CancellationTokenSource cts = GetCtsByGroupId(groupId);
            if(cts == null)
            {
                LoggerInternal.LogErrorFormat("invalid operation when cancel downloading asset group[{0}], cts token is null", groupId);
                return;
            }
            cts.Cancel();
            DeleteCtsByGroupId(groupId);
        }

        public bool ClearAssetGroup(int groupId)
        {
            m_LoadedCatalogs.TryGetValue(groupId, out GroupCatalog groupCatalog);
            if(groupCatalog != null && groupCatalog.IsRootAssetGroup)
            {
                LoggerInternal.LogError("can not remove root asset group");
                return false;
            }
            string path = string.Format("{0}/{1}.catalog", BundleBasePathC, groupId);
            if (File.Exists(path))//TODO:macdeng need test on real android device
            {
                LoggerInternal.LogErrorFormat("group[{0}] is an in package asset group, you can't remove it!", groupId);
            }
            path = string.Format("{0}/{1}.catalog", BundleBasePathP, groupId);
            if (File.Exists(path))//TODO:macdeng need test on real android device
            {
                //TODO:macdeng
                return true;
            }
            else
            {
                LoggerInternal.LogErrorFormat("group[{0}] is not available!", groupId);
                return false;
            }
        }

        public GroupDownloadStatus GetAssetGroupStatus(int groupId)
        {
            if (m_LoadedCatalogs.ContainsKey(groupId))
            {
                return GroupDownloadStatus.Uptodate;
            }
            if (m_DownloadingAssetGroupSet.Contains(groupId))
            {
                return GroupDownloadStatus.Downloading;
            }
            foreach(GroupCatalog groupCatalog in m_LoadedCatalogs.Values)
            {
                foreach (KeyValuePair<int, GroupInfo> entry in groupCatalog.SubGroupInfoMap)
                {
                    int _groupId = entry.Key;
                    GroupInfo catalogInfo = entry.Value;
                    if (_groupId == groupId)
                    {
                        return catalogInfo.State == GroupBundleState.Uptodate ? GroupDownloadStatus.Uptodate : GroupDownloadStatus.NeedDownload;
                    }
                }
            }
            return GroupDownloadStatus.None;
            
        }

        public void LoadGroupCatalogAsync(int groupId, Action onCompleted)
        {
            //TODO:macdeng using GroupCatalogLocationDictionary to generate path
            string path = string.Format("{0}/{1}.catalog", BundleBasePathP, groupId);
            if (File.Exists(path))
            {
                ObjectDeserializer.DeserializeAsync<GroupCatalog>(path, (groupCatalog) =>
                {
                    m_LoadedCatalogs[groupId] = groupCatalog;
                    onCompleted?.Invoke();
                });
            }
            else
            {
                path = string.Format("{0}/{1}.catalog", BundleBasePathC, groupId);
                ObjectDeserializer.DeserializeAsync<GroupCatalog>(path, (groupCatalog) =>
                {
                    m_LoadedCatalogs[groupId] = groupCatalog;
                    onCompleted?.Invoke();
                });
            }
        }

        public void UnloadGroupCatalog(int groupId)
        {
            if (m_LoadedCatalogs.ContainsKey(groupId))
            {
                m_LoadedCatalogs.Remove(groupId);
            }
        }

        private void LoadClientGroupCatalogAsyncInternal(int groupId, Action<GroupCatalog> onCompleted)
        {
            if(m_LoadedCatalogs.TryGetValue(groupId, out GroupCatalog groupCatalog))
            {
                onCompleted?.Invoke(groupCatalog);
                return;
            }
            string path = string.Format("{0}/{1}.catalog", m_BundleBasePathP, groupId);
            ObjectDeserializer.DeserializeAsync<GroupCatalog>(path, (groupCatalogP) =>
            {
                if(groupCatalogP != null)
                {
                    onCompleted?.Invoke(groupCatalogP);
                }
                else
                {
                    path = string.Format("{0}/{1}.catalog", m_BundleBasePathC, groupId);
                    ObjectDeserializer.DeserializeAsync<GroupCatalog>(path, (groupCatalogC) =>
                    {
                        onCompleted?.Invoke(groupCatalogC);
                    });
                }
            });
        }

        private void DownloadRemoteGroupCatalogAsyncInternal(int groupId, Action<GroupCatalog> onCompleted)
        {
            ResourceManager.DownloaderModule.DownloadBytesAsync(string.Format("{0}/{1}.catalog", m_BundleBasePathR, groupId), (bytes) => 
            {
                if(bytes != null)
                {
                    GroupCatalog catalog = ObjectDeserializer.DeserializeBytes<GroupCatalog>(bytes);
                    onCompleted?.Invoke(catalog);
                }
                else
                {
                    LoggerInternal.LogErrorFormat("Failed to get catalog of group[{0}] data from remote server!", groupId);
                    onCompleted?.Invoke(null);
                }
            });
        }

        private void CompareGroupCatalogBetweenClientAndRemote(GroupCatalog clientCatalog, GroupCatalog remoteCatalog, out long downloadSize, out HashSet<string> toRemoveBundleInfoSet)
        {
            BundleInfoDictionary bundleInfoMapClient = clientCatalog == null ? null : clientCatalog.BundleInfoMap;
            BundleInfoDictionary bundleInfoMapRemote = remoteCatalog.BundleInfoMap;
            HashSet<string> hitted = new HashSet<string>();//TODO:macdeng zero GC
            downloadSize = 0;
            foreach(KeyValuePair<string, BundleInfo> entry in bundleInfoMapRemote)
            {
                string bundleName = entry.Key;
                BundleInfo bundleInfoRemote = entry.Value;
                BundleInfo bundleInfoClient = null;
                if (bundleInfoMapClient != null)
                {
                    bundleInfoMapClient.TryGetValue(bundleName, out bundleInfoClient);
                }
                if(bundleInfoClient == null || bundleInfoClient.State == BundleState.None || bundleInfoClient.State == BundleState.Add)
                {
                    LoggerInternal.LogFormat("newly added bundle: {0}", bundleName);
                    bundleInfoRemote.State = BundleState.Add;
                    downloadSize += bundleInfoRemote.Size;
                }
                else if(bundleInfoClient.Hash != null && bundleInfoClient.Hash != bundleInfoRemote.Hash)
                {
                    LoggerInternal.LogFormat("changed bundle: {0}", bundleName);
                    bundleInfoRemote.State = BundleState.Modify;
                    downloadSize += bundleInfoRemote.Size;
                }
                else if(bundleInfoClient.Hash != null && bundleInfoClient.Hash == bundleInfoRemote.Hash)
                {
                    LoggerInternal.LogFormat("uptodate bundle: {0}", bundleName);
                    bundleInfoRemote.State = BundleState.Uptodate;
                    bundleInfoRemote.Location = bundleInfoClient.Location;
                }
                hitted.Add(bundleName);
            }
            toRemoveBundleInfoSet = new HashSet<string>();//TODO:macdeng zero GC
            if(bundleInfoMapClient != null)
            {
                foreach(string bundleName in bundleInfoMapClient.Keys)
                {
                    if (hitted.Contains(bundleName))
                    {
                        continue;
                    }
                    LoggerInternal.LogFormat("to delete bundle: {0}", bundleName);
                    toRemoveBundleInfoSet.Add(bundleName);
                }
            }

            foreach(KeyValuePair<int, GroupInfo> entry in remoteCatalog.SubGroupInfoMap)
            {
                int groupId = entry.Key;
                GroupInfo serverGroupInfo = entry.Value;
                GroupInfo clientGroupInfo = null;
                if(clientCatalog != null)
                {
                    clientCatalog.SubGroupInfoMap.TryGetValue(groupId, out clientGroupInfo);
                }
                if(clientGroupInfo == null)
                {
                    LoggerInternal.LogFormat("added asset group: {0}", groupId);
                    remoteCatalog.SubGroupInfoMap[groupId].State = GroupBundleState.Add;
                }
                else if(clientGroupInfo.State == GroupBundleState.Add || clientGroupInfo.State == GroupBundleState.Modify || clientGroupInfo.State == GroupBundleState.Remove || clientGroupInfo.State == GroupBundleState.None)
                {
                    remoteCatalog.SubGroupInfoMap[groupId].State = clientGroupInfo.State;
                }
                else if(clientGroupInfo.Hash != serverGroupInfo.Hash)
                {
                    LoggerInternal.LogFormat("modified asset group: {0}", groupId);
                    remoteCatalog.SubGroupInfoMap[groupId].State = GroupBundleState.Modify;
                }
                else if(clientGroupInfo.Hash == serverGroupInfo.Hash)
                {
                    LoggerInternal.LogFormat("asset group[{0}] is uptodate, original group change state: {1}", groupId, serverGroupInfo.State);
                    remoteCatalog.SubGroupInfoMap[groupId].State = GroupBundleState.Uptodate;
                }
            }
        }

        private Dictionary<int, CancellationTokenSource> m_GroupIdToCtsMap = new Dictionary<int, CancellationTokenSource>();
        private CancellationTokenSource CreateCtsByGroupId(int groupId)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            m_GroupIdToCtsMap[groupId] = cts;
            return cts;
        }

        private void DeleteCtsByGroupId(int groupId)
        {
            if (m_GroupIdToCtsMap.ContainsKey(groupId))
            {
                m_GroupIdToCtsMap.Remove(groupId);
            }
        }

        private CancellationTokenSource GetCtsByGroupId(int groupId)
        {
            if (m_GroupIdToCtsMap.ContainsKey(groupId))
            {
                return m_GroupIdToCtsMap[groupId];
            }
            return null;
        }

        private Dictionary<int, Dictionary<string, BundleTransientStatus>> bundleTransientStatusMap = new Dictionary<int, Dictionary<string, BundleTransientStatus>>();
        private BundleTransientStatus GetBundleTransientStatus(string bundleName, int groupId)
        {
            if(!bundleTransientStatusMap.ContainsKey(groupId) || !bundleTransientStatusMap[groupId].ContainsKey(bundleName))
            {
                return BundleTransientStatus.None;
            }
            return bundleTransientStatusMap[groupId][bundleName];
        }

        private void SetBundleTransientStatus(string bundleName, int groupId, BundleTransientStatus transientStatus)
        {
            if (!bundleTransientStatusMap.ContainsKey(groupId))
            {
                bundleTransientStatusMap.Add(groupId, new Dictionary<string, BundleTransientStatus>());
            }
            Dictionary<string, BundleTransientStatus> bundleName2StatusMap = bundleTransientStatusMap[groupId];
            if (!bundleName2StatusMap.ContainsKey(bundleName))
            {
                bundleName2StatusMap.Add(bundleName, transientStatus);
                return;
            }
            bundleName2StatusMap[bundleName] = transientStatus;
        }

        private void ClearBundleDownloadStatusMap(int groupId)
        {
            if (bundleTransientStatusMap.ContainsKey(groupId))
            {
                bundleTransientStatusMap[groupId].Clear();
                bundleTransientStatusMap.Remove(groupId);
            }
        }

        private void DownloadBundleAsync(BundleInfo bundleInfo, GroupCatalog groupCatalog, CancellationToken cancellationToken, Action onStarted, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted, BundleDownloadPriority downloadPriority)
        {
            string bundleName = bundleInfo.Name;
            ResourceManager.DownloaderModule.DownloadAssetBundleAsync(bundleInfo, cancellationToken,
                () =>
                {
                    //if (forceOverrite) //when CRC check failed or recompress failed
                    //{
                    //    RuntimeUtils.SafeDeleteFile(string.Format("{0}/{1}", BundleBasePathP, bundleName));
                    //    LoggerInternal.LogFormat("force deleted bundle: {0}", bundleName);
                    //}
                    onStarted?.Invoke();
                },
                onProgressValueChanged,
                (completedStatus) =>
                {
                    groupCatalog.BundleInfoMap[bundleName].State = BundleState.Uptodate;
                    groupCatalog.BundleInfoMap[bundleName].Location = BundleLocation.InPersistent;
                    //TODO:macdeng need added retry.
                    onCompleted?.Invoke(GroupUpdateCompletedStatus.Success);
                },
                downloadPriority);
        }
        private void DownloadChangedBundlesInCatalogAsync(GroupCatalog groupCatalog, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted)
        {
            CancellationToken cancellationToken = CreateCtsByGroupId(groupCatalog.GroupId).Token;
            BundleInfoDictionary changedBundleInfoMap = groupCatalog.BundleInfoMap;
            int groupId = groupCatalog.GroupId;
            float totalProgress = 0f;
            int toDownloadCount = 0;
            long toDownloadSize = 0;
            int downloadedCount = 0;
            int unDownloadedCount = 0;
            bool cancelInvoked = false;
            Dictionary<string, float> bundleWeightedProgressMap = new Dictionary<string, float>();
            foreach(KeyValuePair<string, BundleInfo> entry in changedBundleInfoMap)
            {
                string bundleName = entry.Key;
                if(GetBundleTransientStatus(bundleName, groupId) == BundleTransientStatus.Downloaded)
                {
                    LoggerInternal.LogWarningFormat("bundle[{0}] already downloaded, won't download again.", bundleName);
                    continue;
                }
                BundleInfo bundleInfo = entry.Value;
                BundleState bundleState = bundleInfo.State;
                switch (bundleState)
                {
                    case BundleState.Add:
                        toDownloadCount += 1;
                        toDownloadSize += bundleInfo.Size;
                        bundleWeightedProgressMap[bundleName] = 0f;
                        LoggerInternal.LogFormat("Bundle {0} is newly added.", bundleName);
                        goto default;
                    case BundleState.Modify:
                        toDownloadCount += 1;
                        toDownloadSize += bundleInfo.Size;
                        bundleWeightedProgressMap[bundleName] = 0f;
                        LoggerInternal.LogFormat("Bundle {0} is modified.", bundleName);
                        goto default;
                    case BundleState.None:
                        break;
                    case BundleState.Remove:
                        break;
                    case BundleState.Uptodate:
                        LoggerInternal.LogFormat("Bundle {0} is uptodate.", bundleName);
                        break;
                    default:
                        DownloadBundleAsync(bundleInfo, groupCatalog, cancellationToken,
                            () =>
                            {
                                if (GetBundleTransientStatus(bundleName, groupId) != BundleTransientStatus.Downloading)
                                {
                                    RuntimeUtils.SafeDeleteFile(string.Format("{0}/{1}", BundleBasePathP, bundleName));
                                    SetBundleTransientStatus(bundleName, groupId, BundleTransientStatus.Downloading);
                                }

                            },
                            (progressValue) =>
                            {
                                float previous = bundleWeightedProgressMap[bundleName];
                                float current = progressValue * bundleInfo.Size / toDownloadSize;
                                totalProgress += (current - previous);
                                bundleWeightedProgressMap[bundleName] = current;
                                onProgressValueChanged?.Invoke(totalProgress);
                            },
                            (completedStatus) =>
                            {
                                switch (completedStatus)
                                {
                                    case GroupUpdateCompletedStatus.Success:
                                        LoggerInternal.LogFormat("successfully downloaded {0} assetbundle[{1}]", bundleState == BundleState.Add? "newly added" : "modified", bundleName);
                                        downloadedCount += 1;
                                        SetBundleTransientStatus(bundleName, groupId, BundleTransientStatus.Downloaded);
                                        break;
                                    case GroupUpdateCompletedStatus.Canceled:
                                        if (!cancelInvoked)
                                        {
                                            cancelInvoked = true;
                                            onCompleted?.Invoke(GroupUpdateCompletedStatus.Canceled);
                                        }
                                        break;
                                    case GroupUpdateCompletedStatus.CrcCheckOrRecompressFailed:
                                        unDownloadedCount += 1;
                                        LoggerInternal.LogFormat("failed to {0} assetbundle[{1}]", bundleState == BundleState.Add ? "newly added" : "modified", bundleName);
                                        break;
                                    case GroupUpdateCompletedStatus.CatalogNotAvailable:
                                        LoggerInternal.LogErrorFormat("invalid state for bundle[{0}]", bundleName);
                                        break;
                                }
                                if(downloadedCount == toDownloadCount)
                                {
                                    LoggerInternal.LogFormat("successfully downloaded {0} bundles", downloadedCount);
                                    onProgressValueChanged?.Invoke(1);
                                    ClearBundleDownloadStatusMap(groupId);
                                    onCompleted?.Invoke(GroupUpdateCompletedStatus.Success);
                                }
                                else if(unDownloadedCount > 0 && downloadedCount + unDownloadedCount == toDownloadCount)
                                {
                                    LoggerInternal.LogWarningFormat("failed to download {0} bundles, downloaded count: {1}, unDownloaded count: {2}", toDownloadCount, downloadedCount, unDownloadedCount);
                                    ClearBundleDownloadStatusMap(groupId);
                                    onCompleted?.Invoke(GroupUpdateCompletedStatus.CrcCheckOrRecompressFailed);
                                }
                            },
                            BundleDownloadPriority.Normal);
                        break;
                }
            }
        }


        private void RemoveBundles(HashSet<string> toRemoveBundleInfoSet)
        {
            foreach(string bundleName in toRemoveBundleInfoSet)
            {
                string bundleCachingPath = string.Format("{0}/{1}", BundleBasePathP, bundleName);
                if (File.Exists(bundleCachingPath))
                {
                    if (RuntimeUtils.SafeDeleteFile(bundleCachingPath))
                    {
                        LoggerInternal.LogFormat("deleted bundle[{0}] in path: {1}", bundleName, bundleCachingPath);
                    }
                }
                else
                {
                    LoggerInternal.LogFormat("bundle[{0}] is not in cache", bundleName);
                }
            }
        }
        public void UpdateAssetGroupAsync(int groupId, Action<long> onGetDownloadSize, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted)
        {
            LoadClientGroupCatalogAsyncInternal(groupId, (groupCatalogClient) => 
            {
                DownloadRemoteGroupCatalogAsyncInternal(groupId, (groupCatalogRemote) => 
                {
                    if(groupCatalogRemote == null)
                    {
                        LoggerInternal.LogErrorFormat("catalog of remote group[{0}] is not available.", groupId);
                        onCompleted?.Invoke(GroupUpdateCompletedStatus.CatalogNotAvailable);
                        return;
                    }
                    CompareGroupCatalogBetweenClientAndRemote(groupCatalogClient, groupCatalogRemote, out long downloadSize, out HashSet<string> toRemoveBundleInfoSet);
                    onGetDownloadSize?.Invoke(downloadSize);
                    if(downloadSize > 0)
                    {
                        DownloadChangedBundlesInCatalogAsync(groupCatalogRemote, onProgressValueChanged,
                            (completedStatus) =>
                            {
                                if(completedStatus == GroupUpdateCompletedStatus.Success)
                                {
                                    DeleteCtsByGroupId(groupId);
                                    if(toRemoveBundleInfoSet.Count > 0)
                                    {
                                        RemoveBundles(toRemoveBundleInfoSet);
                                    }
                                    if(groupCatalogRemote.ParentGroupId >= 0 && m_LoadedCatalogs.TryGetValue(groupCatalogRemote.ParentGroupId, out GroupCatalog parentGroupCatalog))
                                    {
                                        parentGroupCatalog.SubGroupInfoMap[groupId].State = GroupBundleState.Uptodate;
                                        parentGroupCatalog.SubGroupInfoMap[groupId].Hash = groupCatalogRemote.Hash;
                                        ObjectSerializer.Serialize(parentGroupCatalog, string.Format("{0}/{1}.catalog", BundleBasePathP, groupCatalogRemote.ParentGroupId));
                                    }
                                    ObjectSerializer.Serialize(groupCatalogRemote, string.Format("{0}/{1}.catalog", BundleBasePathP, groupId));
                                }
                                onCompleted?.Invoke(completedStatus);
                            });
                    }
                    else if(toRemoveBundleInfoSet.Count > 0)
                    {
                        RemoveBundles(toRemoveBundleInfoSet);
                        onCompleted?.Invoke(GroupUpdateCompletedStatus.Success);
                    }
                });
            });
        }

        private Dictionary<string, AssetBundleCache> m_AssetBundleCacheMap = new Dictionary<string, AssetBundleCache>();
        private bool TryGetBundleFromCache(string bundleName, out AssetBundleCache bundleCache)
        {
            if (m_AssetBundleCacheMap.TryGetValue(bundleName, out bundleCache))
            {
                if(bundleCache.ReferenceCount == 0)
                {
                    RemoveBundleFromCache(bundleName);
                    return false;
                }
                bundleCache.IncreaseReference();
                return true;
            }
            return false;
        }

        private AssetBundleCache AddBundleToCache(string bundleName, AssetBundle bundle, string assetPath)
        {
            AssetBundleCache bundleCache = new AssetBundleCache(bundleName, bundle, assetPath);
            m_AssetBundleCacheMap[bundleName] = bundleCache;
            return bundleCache;
        }

        private void RemoveBundleFromCache(string bundleName)
        {
            if (!m_AssetBundleCacheMap.ContainsKey(bundleName))
            {
                LoggerInternal.LogErrorFormat("bundle[{0}] have been removed incorrectly", bundleName);
            }
            m_AssetBundleCacheMap.Remove(bundleName);
        }
        private HashSet<string> m_LoadingBundleSet = new HashSet<string>();

        private Queue<List<UniTask<AssetBundleCache>>> m_DepUniTaskListQueue = new Queue<List<UniTask<AssetBundleCache>>>(); //TODO:macdeng limit queue size.
        private bool TryGetDepUniTaskListFromCache(out List<UniTask<AssetBundleCache>> depUniTaskList)
        {
            if (m_DepUniTaskListQueue.Count > 0)
            {
                depUniTaskList = m_DepUniTaskListQueue.Dequeue();
                return true;
            }
            depUniTaskList = null;
            return false;
        }

        private void RecycleDepUniTaskListToCache(List<UniTask<AssetBundleCache>> depUniTaskList)
        {
            depUniTaskList.Clear();
            m_DepUniTaskListQueue.Enqueue(depUniTaskList);
        }

        private async UniTask<AssetBundleCache[]> LoadDependencyBundles(DependencyBundle[] deps, GroupCatalog catalog)
        {
            if(deps == null || deps.Length == 0)
            {
                return null;
            }
            int depsLen = deps.Length;
            if(!TryGetDepUniTaskListFromCache(out List<UniTask<AssetBundleCache>> tasks))
            {
                tasks = new List<UniTask<AssetBundleCache>>();
            }
            for (int i = 0; i < depsLen; i++)
            {
                DependencyBundle depBundle = deps[i];
                string bundleName = depBundle.BundleName;
                int groupId = depBundle.GroupId;
                BundleInfo bundleInfo = m_LoadedCatalogs[groupId].BundleInfoMap[bundleName];
                UniTask<AssetBundleCache> task = LoadBundleInternal(bundleInfo, catalog);
                tasks.Add(task);
            }
            AssetBundleCache[] ret = await UniTask.WhenAll(tasks);
            RecycleDepUniTaskListToCache(tasks);
            return ret; 
        }

        private async UniTask<AssetBundleCache> LoadBundleInternal(BundleInfo bundleInfo, GroupCatalog catalog, string assetPath = null)
        {
            string bundleName = bundleInfo.Name;
            await UniTask.WaitUntil(() => !m_LoadingBundleSet.Contains(bundleName));
            if (!TryGetBundleFromCache(bundleName, out AssetBundleCache bundleCache))
            {
                m_LoadingBundleSet.Add(bundleName);
                bool isUptodate = bundleInfo.State == BundleState.Uptodate;
                if (!isUptodate)
                {
                    DownloadBundleAsync(bundleInfo, catalog, default, null, null, 
                        (completedStatus) => 
                        {
                            if(completedStatus == GroupUpdateCompletedStatus.Success)
                            {
                                LoggerInternal.LogFormat("successfully downloaded bundle:[{0}]", bundleName);
                                isUptodate = true;
                            }
                            else
                            {
                                //TODO:macdeng retry needed
                            }
                        }, 
                        BundleDownloadPriority.High);
                    await UniTask.WaitUntil(() => isUptodate);
                }
                AssetBundle bundle = await LoadBundleFromFile(bundleInfo);
                bundleCache = AddBundleToCache(bundleName, bundle, assetPath);
                m_LoadingBundleSet.Remove(bundleName);
                LoggerInternal.LogFormat("load bundle from file: {0}", bundleCache);
            }
            else
            {
                bundleCache.AddAssetInstance(assetPath);
                LoggerInternal.LogFormat("load bundle from cache: {0}", bundleCache);
            }
            return bundleCache;
        }

        private async UniTask<AssetBundle> LoadBundleFromFile(BundleInfo bundleInfo)
        {
            string bundleName = bundleInfo.Name;
            string bundleBasePath;
            BundleLocation bundleLocation = bundleInfo.Location;
            switch (bundleLocation)
            {
                case BundleLocation.InPackage:
                    bundleBasePath = BundleBasePathC;
                    break;
                case BundleLocation.InPersistent:
                    bundleBasePath = BundleBasePathP;
                    break;
                case BundleLocation.Remote:
                default:
                    throw new Exception(string.Format("can't load bundle[{0}]", bundleName));
            }
            string bundlePath = string.Format("{0}/{1}", bundleBasePath, bundleName);
            // FileStream fs = new FileStream(bundlePath, FileMode.Open, FileAccess.Read);
            // AssetBundle bundle = await AssetBundle.LoadFromFileAsync(bundlePath);

            
            var bytes = await DecryptABtoBytes(bundlePath, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            AssetBundle bundle = await AssetBundle.LoadFromMemoryAsync(bytes);

            return bundle;
        }
        

        const int BufferSize = 3 * 1024;
        public static async UniTask<byte[]> DecryptABtoBytes(string filePath, byte[] key, byte[] iv)
        {
            AssetBundle deBundle = null;
            using (DES des = DES.Create())
            {
                using (MemoryStream memOutput = new MemoryStream())
                {
                    des.Key = key;
                    des.IV = iv;

                    ICryptoTransform decryptor = des.CreateDecryptor();

                    using (CryptoStream cs = new CryptoStream(memOutput, decryptor, CryptoStreamMode.Write))
                    {
                        // byte[] bs;
                        // bs = await File.ReadAllBytesAsync(filePath);
                        // await cs.WriteAsync(bs);
                        //
                        // cs.FlushFinalBlock();
                        //     
                        // memOutput.Flush();
                        // memOutput.Position = 0;
                        // return memOutput.ToArray();
                        
                        using (FileStream fs = File.OpenRead(filePath))
                        {
                            int read;
                            int bufferSize = (int)(fs.Length / 10);
                            byte[] buffer = new byte[bufferSize];
                            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                await cs.WriteAsync(buffer, 0, read);
                            }
                            
                            cs.FlushFinalBlock();
                            
                            await memOutput.FlushAsync();
                            memOutput.Position = 0;
                            return memOutput.ToArray();
                            // deBundle = await AssetBundle.LoadFromStreamAsync(memOutput);
                        }
                    }
                }
            }
        }
        
        private async UniTask<AssetBundleRefs> ProvideAssetBundleInternal(BundleInfo bundleInfo, GroupCatalog catalog, string assetPath)
        {
            LoggerInternal.LogFormat("try load dependency bundles.");
            AssetBundleCache[] bundleCaches = await LoadDependencyBundles(bundleInfo.Dependencies, catalog);
            if (bundleCaches != null && bundleCaches.Length > 0)
            {
                LoggerInternal.LogFormat("successfully load dependency bundles.");
            }
            AssetBundleCache bundleCache = await LoadBundleInternal(bundleInfo, catalog, assetPath);
            return new AssetBundleRefs(bundleCache, bundleCaches);
        }

        public async UniTask<AssetBundleRefs> ProvideAssetBundle(string assetPath)
        {
            foreach(GroupCatalog catalog in m_LoadedCatalogs.Values)
            {
                if(catalog.AssetToBundleMap.TryGetValue(assetPath, out string bundleName))
                {
                    BundleInfo bundleInfo = catalog.BundleInfoMap[bundleName];
                    
                    return await ProvideAssetBundleInternal(bundleInfo, catalog, assetPath);
                }
            }
            return default;
        }

        public GroupInfoDictionary GetSubGroupInfoMap(int groupId)
        {
            if(!m_LoadedCatalogs.TryGetValue(groupId, out GroupCatalog catalog))
            {
                LoggerInternal.LogErrorFormat("Please load catalog [{0}] first", groupId);
                return null;
            }
            return catalog.SubGroupInfoMap;
        }
    }
}
