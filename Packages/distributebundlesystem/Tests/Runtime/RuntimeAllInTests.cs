using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Gen;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using Runtime.AdvancedBundleSystem.Common.Util;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runtime.AdvancedBundleSystem.Tests
{
    public class RuntimeAllInTests
    {
        private bool m_IsRootCatalogLoaded = false;
        private bool m_IsLoadAssetOfMainGameFinished = false;
       
        [OneTimeSetUp]
        public void SetUp()
        {
            m_IsRootCatalogLoaded = false;
        }

        [Test, Order(1)]
        public void Init()
        {
            ResourceManager.Init();
        }

        [Test, Order(2)]
        public void HotUpdateAssetGroupsOfRootCatalog()
        {
            ResourceManager.CheckIfRootCatalogChangedAsync((rootCatalogHashChanged, changedCatalogHash) => 
            {
                if (rootCatalogHashChanged)
                {
                    Debug.Log("root catalog changed");
                    ResourceManager.UpdateAssetGroupAsync(RuntimeSettings.c_RootAssetGroupId, 
                        (size) => 
                        {
                            Debug.LogFormat("total bundle size to download is: {0}", size);
                        }, 
                        (progress) => 
                        {
                            Debug.LogFormat("download progress is: {0}", progress);
                        },
                        (status) => 
                        {
                            Debug.LogFormat("download finished, status is: {0}", status);
                            RuntimeUtils.SafeWriteAllText(string.Format("{0}/{1}.hash", ResourceManager.AssetBundleModule.BundleBasePathP, RuntimeSettings.c_RootAssetGroupId.ToString()), changedCatalogHash);
                            ResourceManager.LoadGroupCatalog(RuntimeSettings.c_RootAssetGroupId, () => 
                            {
                                m_IsRootCatalogLoaded = true;
                            });
                            
                        }
                    );
                }
                else
                {
                    Debug.Log("root catalog not changed");
                    ResourceManager.LoadGroupCatalog(RuntimeSettings.c_RootAssetGroupId, () => 
                    {
                        m_IsRootCatalogLoaded = true;
                        Debug.Log("root catalog loaded!");
                    });
                    
                }
            });
        }

        [UnityTest, Order(3)]
        public IEnumerator LoadAssetOfMainGame()
        {
            yield return new WaitUntil(
                () => m_IsRootCatalogLoaded);
            string assetPath = "Assets/ABS_UnitTestData/MainGame/Sound/sound1.txt";
            ResourceManager.LoadAssetAsync(assetPath, 
                (handle1) => 
                {
                    TextAsset textAsset = handle1.Result as TextAsset;
                    Debug.LogFormat("text size is {0} ", (textAsset.dataSize / 1024).ToString()); 
                    handle1.Release();
                    m_IsLoadAssetOfMainGameFinished = true;
                });
        }
        
        [UnityTest, Order(4)]
        public IEnumerator GetSubGameStatus()
        {
            yield return new WaitUntil(() => m_IsRootCatalogLoaded && m_IsLoadAssetOfMainGameFinished);
        
            Debug.Log("list sub catalogs of root group:");
            GroupInfoDictionary subCatalogInfoMap = ResourceManager.GetSubCatalogInfoMap(RuntimeSettings.c_RootAssetGroupId);
            if (subCatalogInfoMap != null)
            {
                foreach(KeyValuePair<int, GroupInfo> entry in subCatalogInfoMap)
                {
                    int groupId = entry.Key;
                    GroupInfo groupInfo = entry.Value;
                    Debug.LogFormat("group[{0}]'s state is {1}", groupId, groupInfo.State);
                    if (groupInfo.State != GroupBundleState.Uptodate)
                    {
                        bool finished = false;
                        ResourceManager.UpdateAssetGroupAsync(groupId,
                            (size) =>
                            {
                                Debug.LogFormat($"group[{groupId}]'s total bundle size to download is: {0}", size);
                            },
                            (progress) =>
                            {
                                Debug.LogFormat($"group[{groupId}]'s download progress is: {0}", progress);
                            },
                            (status) =>
                            {
                                finished = true;
                                Debug.LogFormat($"group[{groupId}]'s download finished, status is: {0}", status);
                            }
                        );
                        yield return new WaitUntil(() => finished);
                    }
                    ResourceManager.LoadGroupCatalog(groupId,() =>
                    {
                        string assetPath = $"Assets/ABS_UnitTestData/MiniGame{groupId-10000}/Sound/sound1.txt";
                        ResourceManager.LoadAssetAsync(assetPath, 
                            (handle1) => 
                            {
                                TextAsset textAsset = handle1.Result as TextAsset;
                                Debug.LogFormat("asset in group[{0}]: text size is {1} ", groupId, (textAsset.dataSize / 1024).ToString()); 
                                handle1.Release();
                            });
                        Debug.Log($"list sub catalogs of group[{groupId}]:");
                        GroupInfoDictionary subCatalogInfoMap1 = ResourceManager.GetSubCatalogInfoMap(groupId);
                        if (subCatalogInfoMap1 != null)
                        {
                            foreach (KeyValuePair<int, GroupInfo> entry1 in subCatalogInfoMap1)
                            {
                                int groupId1 = entry1.Key;
                                GroupInfo groupInfo1 = entry1.Value;
                                Debug.LogFormat("group[{0}]'s state is {1}", groupId1, groupInfo1.State);
                            }
                        }
                    });
                }
            }

            if (RuntimeSettings.c_IsEditorMode)
            {
                int[] groupIds = new int[]
                {
                    10001,
                    10002,
                    10003,
                    10004
                };
                foreach (int groupId in groupIds)
                {
                    string assetPath = $"Assets/ABS_UnitTestData/MiniGame{groupId-10000}/Sound/sound1.txt";
                    ResourceManager.LoadAssetAsync(assetPath, 
                        (handle1) => 
                        {
                            TextAsset textAsset = handle1.Result as TextAsset;
                            Debug.LogFormat("asset in group[{0}]: text size is {1} ", groupId, (textAsset.dataSize / 1024).ToString()); 
                            handle1.Release();
                        });
                }
            }
        }


        [OneTimeTearDown]
        public void ClearUp()
        {
            

        }

    }
}
