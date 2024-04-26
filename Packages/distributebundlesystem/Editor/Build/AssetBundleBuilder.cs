using Runtime.AdvancedBundleSystem.Common;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using Runtime.AdvancedBundleSystem.Common.Serializer;
using Runtime.AdvancedBundleSystem.Common.Util;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AdvancedBundleSystem.GUI;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.AdvancedBundleSystem.Utils;
using UnityEditor.Build.Pipeline;
using UnityEngine.Build.Pipeline;
using UnityEngine;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Algorithm;
using UnityEditor.Build.Pipeline.Interfaces;

namespace UnityEditor.AdvancedBundleSystem.Build
{ 
    public static class AssetBundleBuilder
    {
        private static AdvancedBundleSystemSettings m_Settings;
        private static string GetOutputPath()
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - 6) + m_Settings.AssetBundleOutputPath + "/" + RuntimeUtils.GetPlatformName();
        }

        private static string GetClientDataPath()
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - 6) + m_Settings.ClientDataPath + "/" + RuntimeUtils.GetPlatformName();
        }
        
        private static void GenerateBuildContentPerGroup(AssetSet assetGroup, ref List<AssetBundleBuild> bundleBuilds, ref Dictionary<string, string> bundleName2AssetSetNameDict,
            ref Dictionary<string, int> bundleName2GroupIdDict, ref Dictionary<int, AssetToBundleDictionary> groupId2AssetToBundleMap)
        {
            List<AssetEntry> entries = assetGroup.Entries;
            string assetSetName = assetGroup.AssetSetName;
            int groupId = assetGroup.AssetGroup.GroupId;
            if (!groupId2AssetToBundleMap.TryGetValue(groupId, out AssetToBundleDictionary assetToBundleMap))
            {
                assetToBundleMap = new AssetToBundleDictionary();
            }

            if (assetGroup.PackingMode == PackingMode.Together)
            {
                AssetBundleBuild groupBundleBuild = new AssetBundleBuild();
                string bundleName = assetSetName.ToLower() + m_Settings.AssetBundleSuffix;
                List<string> assetNames = new List<string>();
                foreach (AssetEntry entry in entries)
                {
                    if (entry.IsFolder)
                    {
                        assetNames.AddRange(entry.GatherAllAssetPathsInFolderEntry());
                    }
                    else
                    {
                        assetNames.Add(entry.AssetPath);
                    }
                }
                groupBundleBuild.assetBundleName = bundleName;
                groupBundleBuild.assetNames = assetNames.ToArray();
                bundleName2AssetSetNameDict.Add(bundleName, assetSetName);
                bundleName2GroupIdDict.Add(bundleName, groupId);
                bundleBuilds.Add(groupBundleBuild);
                foreach (string assetPath in assetNames)
                {
                    assetToBundleMap[assetPath] = bundleName;
                }
            }
            else if (assetGroup.PackingMode == PackingMode.Separate)
            {
                List<AssetBundleBuild> groupBundleBuilds = new List<AssetBundleBuild>();
                foreach (AssetEntry entry in entries)
                {
                    AssetBundleBuild bundleBuild = new AssetBundleBuild();
                    string bundleName = assetSetName + "_" + entry.AssetName.ToLower() + m_Settings.AssetBundleSuffix;
                    string[] assetNamesArray;
                    if (entry.IsFolder)
                    {
                        assetNamesArray = entry.GatherAllAssetPathsInFolderEntry().ToArray();
                    }
                    else
                    {
                        assetNamesArray = new string[] { entry.AssetPath };
                    }
                    bundleBuild.assetBundleName = bundleName;
                    bundleBuild.assetNames = assetNamesArray;
                    bundleName2AssetSetNameDict.Add(bundleName, assetSetName);
                    bundleName2GroupIdDict.Add(bundleName, groupId);
                    groupBundleBuilds.Add(bundleBuild);
                    foreach (string assetPath in assetNamesArray)
                    {
                        assetToBundleMap[assetPath] = bundleName;
                    }
                }
                bundleBuilds.AddRange(groupBundleBuilds);
            }

            groupId2AssetToBundleMap[groupId] = assetToBundleMap;
        }

        private static string GenerateCatalogHash(BundleInfoDictionary bundleInfoMap, List<AssetGroup> children, ref Dictionary<int, string> groupId2CatalogHashMap)
        {
            Hash128 hash = new Hash128();
            foreach (var entry in bundleInfoMap)
            {
                BundleInfo bundleInfo = entry.Value;
                Hash128 inHash = Hash128.Parse(bundleInfo.Hash);
                HashUtilities.AppendHash(ref inHash, ref hash);
            }
            if(children != null)
            {
                foreach(AssetGroup assetGroup in children)
                {
                    int groupId = assetGroup.GroupId;
                    LoggerInternal.LogFormat("subGroup[{0}]'s hash is {1}", groupId, groupId2CatalogHashMap[groupId]);
                    Hash128 inHash = Hash128.Parse(groupId2CatalogHashMap[groupId]);
                    HashUtilities.AppendHash(ref inHash, ref hash);
                }
            }
            return hash.ToString();
        }

        private static bool IsCatalogInPackage(AssetGroup catalog)
        {
            foreach (AssetSet assetGroup in catalog.AssetSets)
            {
                if (!assetGroup.InPackage)
                {
                    return false;
                }
            }
            return true;
        }

        private static void GenerateGroupCatalogRecursively(AssetGroup assetGroup,
            ref Dictionary<int, AssetToBundleDictionary> groupId2AssetToBundleMap,
            ref Dictionary<int, BundleInfoDictionary> groupId2BundleInfoMap,
            ref Dictionary<int, string> groupId2CatalogHashMap, AssetGroup parentGroupCatalog)
        {
            //recursive call
            List<AssetGroup> children = assetGroup.Children;
            if (children != null && children.Count > 0)
            {
                foreach (AssetGroup subGroup in children)
                {
                    GenerateGroupCatalogRecursively(subGroup, ref groupId2AssetToBundleMap, ref groupId2BundleInfoMap, ref groupId2CatalogHashMap, assetGroup);
                }
            }

            //generate group catalog
            int groupId = assetGroup.GroupId;
            bool isRootCatalog = assetGroup.IsRootGroup;
            BundleInfoDictionary bundleInfoMap = groupId2BundleInfoMap[groupId];
            GroupInfoDictionary subCatalogInfoMap = null;
            if (assetGroup.Children != null)
            {
                subCatalogInfoMap = new GroupInfoDictionary();
                for (int i = 0; i < assetGroup.Children.Count; i++)
                {
                    AssetGroup subGroup = assetGroup.Children[i];
                    subCatalogInfoMap[subGroup.GroupId] = new GroupInfo(groupId2CatalogHashMap[subGroup.GroupId], IsCatalogInPackage(subGroup) ? GroupBundleState.Uptodate : GroupBundleState.None);
                }
            }
            LoggerInternal.LogFormat("Start generate hash for : {0}", groupId);
            string catalogHash = GenerateCatalogHash(bundleInfoMap, assetGroup.Children, ref groupId2CatalogHashMap);
            LoggerInternal.LogFormat("Generate hash for : {0}, the hash is {1}", groupId, catalogHash);
            groupId2CatalogHashMap[groupId] = catalogHash;
            GroupCatalog runtimeGroupCatalog = new GroupCatalog(assetGroup.GroupId, catalogHash, bundleInfoMap, groupId2AssetToBundleMap[groupId], isRootCatalog, subCatalogInfoMap, parentGroupCatalog == null ? -1 : parentGroupCatalog.GroupId);
            string catalogPath = GetOutputPath() + "/" + string.Format(m_Settings.GroupCatalogNamePattern, groupId);
            ObjectSerializer.Serialize(runtimeGroupCatalog, catalogPath);
            LoggerInternal.LogFormat("Generated catalog at path: {0}", catalogPath);
            if (IsCatalogInPackage(assetGroup))
            {
                EditorUtils.CopyFileToFolder(catalogPath, GetClientDataPath());
            }
            //serialize root catalog hash
            if (isRootCatalog)
            {
                string hashPath = GetOutputPath() + "/" + string.Format(m_Settings.GroupCatalogHashNamePattern, groupId);
                RuntimeUtils.SafeWriteAllText(hashPath, catalogHash);
                EditorUtils.CopyFileToFolder(hashPath, GetClientDataPath());
            }
        }

        private static void GenerateBundleInfo(string bundleName, string groupName, int groupId, BundleInfoDictionary bundleInfoMap, IBundleBuildResults bundleBuildResults,
            bool compressLocalCache, bool inPackage, ref Dictionary<string, int> bundleName2GroupIdDict)
        {
            BundleDetails bundleDetails = bundleBuildResults.BundleInfos[bundleName];
            string bundlePath = bundleDetails.FileName;
            FileInfo fileInfo = new FileInfo(bundlePath);
            byte[] bundleData = File.ReadAllBytes(bundlePath);
            uint bundleCrc = Crc32Algorithm.Compute(bundleData);
            string[] depNames = bundleDetails.Dependencies;
            DependencyBundle[] dependencies = new DependencyBundle[depNames.Length];
            for (int i = 0; i < depNames.Length; i++)
            {
                DependencyBundle dependency = new DependencyBundle(depNames[i], bundleName2GroupIdDict[depNames[i]]);
                dependencies[i] = dependency;
            }

            if (inPackage)
            {
                EditorUtils.CopyFileToFolder(Path.Combine(GetOutputPath(), bundleName), GetClientDataPath());
            }
            BundleInfo bundleInfo = new BundleInfo(bundleName, groupName, groupId, bundleDetails.Hash.ToString(), fileInfo.Length, bundleCrc, compressLocalCache,
                inPackage ? BundleState.Uptodate : BundleState.None,
                inPackage ? BundleLocation.InPackage : BundleLocation.Remote,
                dependencies);
            bundleInfoMap[bundleName] = bundleInfo;
        }
        private static  void GenerateBundleInfoMapPerGroup(AssetSet assetGroup, IBundleBuildResults bundleBuildResults,
            ref Dictionary<int, BundleInfoDictionary> groupId2BundleInfoMap, ref Dictionary<string, int> bundleName2GroupIdDict)
        {
            List<AssetEntry> entries = assetGroup.Entries;
            string groupName = assetGroup.AssetSetName;
            AssetGroup groupCatalog = assetGroup.AssetGroup;
            int groupId = groupCatalog.GroupId;
            bool compressLocalCache = assetGroup.CompressLocalCache;
            bool inPackage = assetGroup.InPackage;

            if (!groupId2BundleInfoMap.TryGetValue(groupId, out BundleInfoDictionary bundleInfoMap))
            {
                bundleInfoMap = new BundleInfoDictionary();
            }
            if (assetGroup.PackingMode == PackingMode.Together)
            {
                string bundleName = groupName.ToLower() + m_Settings.AssetBundleSuffix;
                GenerateBundleInfo(bundleName, groupName, groupId, bundleInfoMap, bundleBuildResults, compressLocalCache, inPackage, ref bundleName2GroupIdDict);
            }
            else if (assetGroup.PackingMode == PackingMode.Separate)
            {
                foreach (AssetEntry entry in entries)
                {
                    string bundleName = groupName + "_" + entry.AssetName.ToLower() + m_Settings.AssetBundleSuffix;
                    GenerateBundleInfo(bundleName, groupName, groupId, bundleInfoMap, bundleBuildResults, compressLocalCache, inPackage, ref bundleName2GroupIdDict);
                }
            }
            groupId2BundleInfoMap[groupId] = bundleInfoMap;
        }
        public static void BuildAssetBundle()
        {
            LoggerInternal.Log("Build Asset Bundle Start");
            m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
            List<AssetBundleBuild> bundleBuilds = new List<AssetBundleBuild>();

            Dictionary<int, AssetToBundleDictionary> groupId2AssetToBundleMap = new Dictionary<int, AssetToBundleDictionary>();
            Dictionary<string, string> bundleName2AssetSetNameDict = new Dictionary<string, string>();
            Dictionary<string, int> bundleName2GroupIdDict = new Dictionary<string, int>();
            foreach (AssetSet assetGroup in m_Settings.AssetSets)
            {
                GenerateBuildContentPerGroup(assetGroup, ref bundleBuilds, ref bundleName2AssetSetNameDict, ref bundleName2GroupIdDict, ref groupId2AssetToBundleMap);
            }
            ABSBundleBuildParameters buildParams;
            BuildTarget target;
            BuildTargetGroup targetGroup;
            string outputPath = GetOutputPath();
            if (Directory.Exists(outputPath))
            {
                EditorUtils.DeleteDirectory(outputPath);
            }
            Directory.CreateDirectory(outputPath);

            string clientDataPath = GetClientDataPath();
            if (Directory.Exists(clientDataPath))
            {
                EditorUtils.DeleteDirectory(clientDataPath);
            }
            Directory.CreateDirectory(clientDataPath);
#if UNITY_ANDROID
            target = BuildTarget.Android;
            targetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
            target = BuildTarget.iOS;
            targetGroup = BuildTargetGroup.iOS;
#elif UNITY_STANDALONE_WIN
            target = BuildTarget.StandaloneWindows64;
            targetGroup = BuildTargetGroup.Standalone;
#elif UNITY_STANDALONE_OSX
            target = BuildTarget.StandaloneOSX;
            targetGroup = BuildTargetGroup.Standalone;
#elif UNITY_WEBGL
            target = BuildTarget.WebGL;
            targetGroup = BuildTargetGroup.WebGL;
#endif
            buildParams = new ABSBundleBuildParameters(m_Settings, bundleName2AssetSetNameDict, target, targetGroup, outputPath);
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, new BundleBuildContent(bundleBuilds), out IBundleBuildResults bundleBuildResults);
            if (exitCode < ReturnCode.Success)
            {
                LoggerInternal.LogErrorFormat("SBP Error: {0}", exitCode);
                return;
            }
            LoggerInternal.Log("Build Asset Bundle End, Generate Group Catalogs Start");
            Dictionary<int, BundleInfoDictionary> groupId2BundleInfoMap = new Dictionary<int, BundleInfoDictionary>();
            foreach (AssetSet assetGroup in m_Settings.AssetSets)
            {
                GenerateBundleInfoMapPerGroup(assetGroup, bundleBuildResults, ref groupId2BundleInfoMap, ref bundleName2GroupIdDict);
            }
            Dictionary<int, string> groupId2CatalogHashMap = new Dictionary<int, string>();
            GenerateGroupCatalogRecursively(m_Settings.RootAssetGroup, ref groupId2AssetToBundleMap, ref groupId2BundleInfoMap, ref groupId2CatalogHashMap, null);
            LoggerInternal.Log("Generate Group Catalogs End");
        }
    }
}

