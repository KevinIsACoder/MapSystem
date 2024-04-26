using System;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
    [Serializable]
    public class GroupCatalog
    {
        public GroupCatalog(int groupId, string hash, BundleInfoDictionary bundleInfoMap, AssetToBundleDictionary assetToBundleMap, bool isRootCatalog, GroupInfoDictionary groupInfoMap, int parentGroupId)
        {
            GroupId = groupId;
            Hash = hash;
            BundleInfoMap = bundleInfoMap;
            AssetToBundleMap = assetToBundleMap;
            IsRootAssetGroup = isRootCatalog;
            SubGroupInfoMap = groupInfoMap;
            ParentGroupId = parentGroupId;
        }
        /// <summary>
        /// group id
        /// </summary>
        public int GroupId;
        /// <summary>
        /// 128 bit hash code
        /// </summary>
        public string Hash;
        /// <summary>
        /// bundle name to bundle info map of all included asset sets
        /// </summary>
        public BundleInfoDictionary BundleInfoMap;
        /// <summary>
        /// asset path to bundle name map of all included asset sets
        /// </summary>
        public AssetToBundleDictionary AssetToBundleMap;
        /// <summary>
        /// indicate if it is root asset group. only one root asset group in whole system
        /// </summary>
        public bool IsRootAssetGroup;
        /// <summary>
        /// group id to group info
        /// </summary>
        public GroupInfoDictionary SubGroupInfoMap;

        /// <summary>
        /// parent group id (the value is -1 if no parent asset group)
        /// </summary>
        public int ParentGroupId;
    }
}

