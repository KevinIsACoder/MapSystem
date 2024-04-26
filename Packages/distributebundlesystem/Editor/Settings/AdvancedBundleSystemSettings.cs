using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AdvancedBundleSystem.GUI;
using UnityEditor.AdvancedBundleSystem.Utils;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.Settings
{
    public class AdvancedBundleSystemSettings : ScriptableObject
    {
        [SerializeField]
        private List<AssetSet> m_AssetSets = new List<AssetSet>();
        internal List<AssetSet> AssetSets
        {
            get 
            {
                return m_AssetSets;
            }
        }

        [SerializeField] 
        private AssetGroup m_RootAssetGroup;
        public AssetGroup RootAssetGroup
        {
            get
            {
                return m_RootAssetGroup;
            }
            set
            {
                m_RootAssetGroup = value;
            }
        }

        [Header("AssetBundle Settings:")]
        [SerializeField]
        private bool m_IsEditorMode = false;

        public bool IsEditorMode
        {
            get
            {
                return m_IsEditorMode;
            }
            set
            {
                m_IsEditorMode = value;
            }
        }

        [SerializeField]
        private bool m_ForceDllEditorMode = false;
        public bool ForceDllEditorMode
        {
            get
            {
                return m_ForceDllEditorMode;
            }
            set
            {
                m_ForceDllEditorMode = value;
            }
        }
        
        [SerializeField]
        private string m_AssetBundleOutputPath = "AssetBundles";

        public string AssetBundleOutputPath
        {
            get
            {
                return m_AssetBundleOutputPath;
            }
            set
            {
                m_AssetBundleOutputPath = value;
            }
        }

        [SerializeField]
        private string m_ClientDataPath = "ClientData";

        public string ClientDataPath
        {
            get
            {
                return m_ClientDataPath;
            }
            set
            {
                m_ClientDataPath = value;
            }
        }
        
        [SerializeField]
        private string m_CachingRootPath = "ABS_Caching";

        public string CachingRootPath
        {
            get
            {
                return m_CachingRootPath;
            }
            set
            {
                m_CachingRootPath = value;
            }
        }
        
        [SerializeField]
        private string m_ServerAddress = "http://localhost:12345/Android";

        public string ServerAddress
        {
            get
            {
                return m_ServerAddress;
            }
            set
            {
                m_ServerAddress = value;
            }
        }
        
        [SerializeField]
        private string m_AssetBundleSuffix = ".bundle";

        public string AssetBundleSuffix
        {
            get
            {
                return m_AssetBundleSuffix;
            }
            set
            {
                m_AssetBundleSuffix = value;
            }
        }
        
        
        [SerializeField]
        private string m_GroupCatalogNamePattern = "{0}.catalog";

        public string GroupCatalogNamePattern
        {
            get
            {
                return m_GroupCatalogNamePattern;
            }
            set
            {
                m_GroupCatalogNamePattern = value;
            }
        }
        
        [SerializeField]
        private string m_GroupCatalogHashNamePattern = "{0}.hash";

        public string GroupCatalogHashNamePattern
        {
            get
            {
                return m_GroupCatalogHashNamePattern;
            }
            set
            {
                m_GroupCatalogHashNamePattern = value;
            }
        }
        
        [SerializeField]
        private int m_RootAssetGroupId = 0;

        public int RootAssetGroupId
        {
            get
            {
                return m_RootAssetGroupId;
            }
            set
            {
                m_RootAssetGroupId = value;
            }
        }
        [Header("Downloader Settings:")]
        [SerializeField]
        private int m_MaxConcurrentDownloadingTaskSizeNormalPriority = 1;

        public int MaxConcurrentDownloadingTaskSizeNormalPriority
        {
            get
            {
                return m_MaxConcurrentDownloadingTaskSizeNormalPriority;
            }
            set
            {
                m_MaxConcurrentDownloadingTaskSizeNormalPriority = value;
            }
        }
        
        [SerializeField]
        private int m_MaxConcurrentDownloadingTaskSizeHighPriority = 6;

        public int MaxConcurrentDownloadingTaskSizeHighPriority
        {
            get
            {
                return m_MaxConcurrentDownloadingTaskSizeHighPriority;
            }
            set
            {
                m_MaxConcurrentDownloadingTaskSizeHighPriority = value;
            }
        }
        
        [SerializeField]
        private int m_ConnectionLimit = 10;

        public int ConnectionLimit
        {
            get
            {
                return m_ConnectionLimit;
            }
            set
            {
                m_ConnectionLimit = value;
            }
        }


        [NonSerialized]
        private AssetSetTreeModel m_AssetSetTreeModel;
        internal AssetSetTreeModel AssetSetTreeModel
        {
            get
            {
                return m_AssetSetTreeModel;
            }
            set
            {
                m_AssetSetTreeModel = value;
            }
        }

        [NonSerialized]
        private AssetGroupTreeModel m_AssetGroupTreeModel;
        internal AssetGroupTreeModel AssetGroupTreeModel
        {
            get
            {
                return m_AssetGroupTreeModel;
            }
            set
            {
                m_AssetGroupTreeModel = value;
            }
        }
        
        [Header("CryptoKey & iv:")] 
        public byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        public byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static AdvancedBundleSystemSettings Create(string configFolder, string configName)
        {
            AdvancedBundleSystemSettings settings;
            string path = configFolder + "/" + configName + ".asset";
            settings = AssetDatabase.LoadAssetAtPath<AdvancedBundleSystemSettings>(path);
            if(settings == null)
            {
                settings = CreateInstance<AdvancedBundleSystemSettings>();
                //TODO:macdeng
                Directory.CreateDirectory(configFolder);
                AssetDatabase.CreateAsset(settings, path);
                settings = AssetDatabase.LoadAssetAtPath<AdvancedBundleSystemSettings>(path);
                //TODO:macdeng
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        private bool IsAssetSetNameExisting(string assetSetName)
        {
            bool existing = false;
            foreach(AssetSet set in AssetSets)
            {
                if(set != null && set.AssetSetName == assetSetName)
                {
                    existing = true;
                    break;
                }
            }
            return existing;
        }

        private bool IsAssetGroupIdExisting(int groupId)
        {
            if(RootAssetGroup == null)
            {
                return false;
            }
            return RootAssetGroup.IsGroupIdExistingRecursively(groupId);
        }

        private AssetGroup FindAssetGroupRecursively(AssetGroup group, int groupId)
        {
            if(group.GroupId == groupId)
            {
                return group;
            }
            foreach(AssetGroup subGroup in group.Children)
            {
                AssetGroup find = FindAssetGroupRecursively(subGroup, groupId);
                if(find != null)
                {
                    return find;
                }
            }
            return null;
        }

        public AssetGroup FindAssetGroup(int groupId)
        {
            if(RootAssetGroup == null)
            {
                return null;
            }
            return FindAssetGroupRecursively(RootAssetGroup, groupId);
        }

        public AssetGroup CreateAssetGroup(int groupId, bool isRootGroup, AssetGroup parentGroup)
        {
            if(groupId < 0)
            {
                throw new Exception("Group id should not smaller than zero");
            }
            if (IsAssetGroupIdExisting(groupId))
            {
                throw new Exception("Asset Group:[" + groupId + "] already exists!");
            }
            if(isRootGroup && RootAssetGroup != null)
            {
                throw new Exception("Root Asset Group[" + groupId + "] already exists! (Only one root group in whole system.)");
            }
            AssetGroup assetGroup = CreateInstance<AssetGroup>();
            assetGroup.GUID = GUID.Generate().ToString();
            assetGroup.GroupId = groupId;
            assetGroup.IsRootGroup = isRootGroup;

            string catalogFolder = AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigFolder + "/AssetGroups";
            if (!Directory.Exists(catalogFolder))
            {
                Directory.CreateDirectory(catalogFolder);
            }
            AssetDatabase.CreateAsset(assetGroup, catalogFolder + "/" + assetGroup.GroupId + ".asset");
            if (isRootGroup)
            {
                RootAssetGroup = assetGroup;
            }
            else
            {
                parentGroup.AddChild(assetGroup);
            }
            
            MarkDirty(null, isRootGroup ? null : parentGroup);
            return assetGroup;
        }

        public AssetSet CreateAssetSet(string assetSetName, AssetGroup assetGroup,  BundleCompression bundleCompressMethod, PackingMode packing, bool compressLocalCache, bool inPackage)
        {
            if (!EditorUtils.IsNameValid(assetSetName))
            {
                throw new Exception("Asset set name:[" + assetSetName + "] is not valid!");
            }
            if (IsAssetSetNameExisting(assetSetName))
            {
                throw new Exception("Asset set name:[" + assetSetName + "] already exists!");
            }
            AssetSet assetSet = CreateInstance<AssetSet>();
            assetSet.GUID = GUID.Generate().ToString();
            assetSet.AssetSetName = assetSetName;
            assetSet.AssetGroup = assetGroup;
            assetSet.BundleCompressionMethod = bundleCompressMethod;
            assetSet.PackingMode = packing;
            assetSet.CompressLocalCache = compressLocalCache;
            assetSet.InPackage = inPackage;
            

            string groupFolder = AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigFolder + "/AssetSets";
            if (!Directory.Exists(groupFolder))
            {
                Directory.CreateDirectory(groupFolder);
            }
            AssetDatabase.CreateAsset(assetSet, groupFolder + "/" + assetSet.AssetSetName + ".asset");
            if (!AssetSets.Contains(assetSet))
            {
                AssetSets.Add(assetSet);
            }
            if (!assetGroup.AssetSets.Contains(assetSet))
            {
                assetGroup.AssetSets.Add(assetSet);
            }
            MarkDirty();
            return assetSet;
        }

        public bool IsAssetEntryExisting(string guid, AssetSet assetSet)
        {
            return assetSet.IsAssetEntryExisting(guid);
        }

        public AssetEntry CreateAssetEntry(string assetPath, AssetSet assetSet)
        {
            if (!EditorUtils.IsPathValidEntry(assetPath))
            {
                throw new Exception("Entry path:[" + assetPath + "] is invalid!");
            }
            string entryGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(entryGUID))
            {
                throw new Exception("Entry GUID for :[" + assetPath + "] is invalid!");
            }
            if(IsAssetEntryExisting(entryGUID, assetSet))
            {
                return null;
            }
            AssetEntry assetEntry = new AssetEntry
            {
                GUID = entryGUID,
                AssetPath = assetPath,
                IsFolder = AssetDatabase.IsValidFolder(assetPath)
            };
            assetSet.AddEntry(assetEntry);
            MarkDirty(assetSet);
            return assetEntry;
        }

        public void CreateAssetEntries(string[] assetPaths, AssetSet assetSet)
        {
            foreach(string assetPath in assetPaths)
            {
                CreateAssetEntry(assetPath, assetSet);
            }
        }

        public void RemoveAssetEntry(AssetEntry assetEntry, AssetSet assetSet)
        {
            if(assetSet != null && assetSet.Entries.Contains(assetEntry))
            {
                assetSet.RemoveEntry(assetEntry);
            }
            MarkDirty(assetSet);
        }

        private void DeleteAssetSetAsset(AssetSet assetSet)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assetSet, out string GUID, out long _))
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private void DeleteAssetGroupAsset(AssetGroup assetGroup)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(assetGroup, out string GUID, out long _))
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        public void RemoveAllAssetSets()
        {
            foreach(AssetSet set in AssetSets)
            {
                DeleteAssetSetAsset(set);
                
            }
            AssetSets.Clear();
        }

        private void RemoveAssetSetInternal(AssetSet assetSet)
        {
            if (assetSet != null)
            {
                AssetSets.Remove(assetSet);
                DeleteAssetSetAsset(assetSet);
            }
        }

        public void RemoveAssetSet(AssetSet assetSet)
        {
            RemoveAssetSetInternal(assetSet);
            assetSet.AssetGroup.AssetSets.Remove(assetSet);
            MarkDirty();
        }

        private void RemoveAssetSetsInAssetGroupRecursively(AssetGroup assetGroup)
        {
            foreach(AssetSet assetSet in assetGroup.AssetSets)
            {
                RemoveAssetSetInternal(assetSet);
            }
            assetGroup.AssetSets.Clear();
            foreach(AssetGroup group in assetGroup.Children)
            {
                RemoveAssetSetsInAssetGroupRecursively(group);
            }
        }

        private void DeleteGroupAssetRecursively(AssetGroup assetGroup)
        {
            DeleteAssetGroupAsset(assetGroup);
            foreach (AssetGroup subGroup in assetGroup.Children)
            {
                DeleteGroupAssetRecursively(subGroup);
            }
        }

        private void GetAssetGroupListRecursively(AssetGroup assetGroup, List<int> groupIdList, List<AssetGroup> groupList)
        {
            groupList.Add(assetGroup);
            groupIdList.Add(assetGroup.GroupId);
            foreach (AssetGroup subGroup in assetGroup.Children)
            {
                GetAssetGroupListRecursively(subGroup, groupIdList, groupList);
            }
        }

        public void GetAssetGroupList(out List<int> assetGroupIdList, out List<AssetGroup> assetGroupList)
        {
            assetGroupIdList = new List<int>();
            assetGroupList = new List<AssetGroup>();
            GetAssetGroupListRecursively(RootAssetGroup, assetGroupIdList, assetGroupList);
        }

        public void RemoveAssetGroup(AssetGroup assetGroup)
        {
            if(assetGroup != null)
            {
                if (assetGroup.IsRootGroup && RootAssetGroup.GUID == assetGroup.GUID)
                {
                    RemoveAllAssetSets();
                    DeleteGroupAssetRecursively(RootAssetGroup);
                    RootAssetGroup = null;
                    MarkDirty();
                    return;
                }
                if(assetGroup.Parent != null)
                {
                    assetGroup.Parent.Children.Remove(assetGroup);
                    RemoveAssetSetsInAssetGroupRecursively(assetGroup);
                    DeleteGroupAssetRecursively(assetGroup);
                    MarkDirty();
                }
            }
        }

        public AssetSet FindAssetSet(string assetSetName)
        {
            return AssetSets.Find(x => x.AssetSetName == assetSetName);
        }

        private void MarkDirty(AssetSet assetSet = null, AssetGroup assetGroup = null)
        {
            AssetSetTreeModel?.OnChanged();
            AssetGroupTreeModel?.OnChanged();
            EditorUtility.SetDirty(this);
            if(assetSet != null)
            {
                EditorUtility.SetDirty(assetSet);
            }
            if (assetGroup != null)
            {
                EditorUtility.SetDirty(assetGroup);
            }
            if(RootAssetGroup != null)
            {
                EditorUtility.SetDirty(RootAssetGroup);
            }
        }

        private void DirtyGroupRecursively(AssetGroup assetGroup)
        {
            if (AssetSets != null && AssetSets.Count > 0)
            {
                foreach (var assetSet in AssetSets)
                {
                    EditorUtility.SetDirty(assetSet);
                }
            }

            if (assetGroup.Children != null && assetGroup.Children.Count > 0)
            {
                foreach (var subAssetGroup in assetGroup.Children)
                {
                    DirtyGroupRecursively(subAssetGroup);
                }
            }
            EditorUtility.SetDirty(assetGroup);
        }

        public void MarkDirtyForAll()
        {
            DirtyGroupRecursively(m_RootAssetGroup);
        }


    }

}
