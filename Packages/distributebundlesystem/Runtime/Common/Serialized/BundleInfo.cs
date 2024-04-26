using Runtime.AdvancedBundleSystem.Common.Enum;
using System;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
    [Serializable]
    public struct DependencyBundle
    {
        public DependencyBundle(string bundleName, int groupId)
        {
            BundleName = bundleName;
            GroupId = groupId;
        }
        public string BundleName;
        public int GroupId;
    }

    [Serializable]
    public class BundleInfo
    {
        public BundleInfo(string name, string setName, int groupId, string hash, long size, uint crc, bool compressLocalCache, BundleState state, BundleLocation location, DependencyBundle[] dependencies)
        {
            Name = name;
            AssetSetName = setName;
            GroupId = groupId;
            Hash = hash;
            Size = size;
            Crc = crc;
            CompressLocalCache = compressLocalCache;
            State = state;
            Location = location;
            Dependencies = dependencies;
        }

        public BundleInfo(BundleInfo bundleInfo)
        {
            Name = bundleInfo.Name;
            AssetSetName = bundleInfo.AssetSetName;
            GroupId = bundleInfo.GroupId;
            Hash = bundleInfo.Hash;
            Size = bundleInfo.Size;
            Crc = bundleInfo.Crc;
            CompressLocalCache = bundleInfo.CompressLocalCache;
            State = bundleInfo.State;
            Location = bundleInfo.Location;
            Dependencies = bundleInfo.Dependencies;

        }
        
        /// <summary>
        /// unique bundle name
        /// </summary>
        public string Name;
        /// <summary>
        /// name of asset set which the bundle belongs to
        /// </summary>
        public string AssetSetName;
        /// <summary>
        /// id of asset group hich the parent asset set belongs to
        /// </summary>
        public int GroupId;
        /// <summary>
        /// 128 bit hash code
        /// </summary>
        public string Hash;
        /// <summary>
        /// bundle size, in bytes.
        /// </summary>
        public long Size;
        /// <summary>
        /// CRC code
        /// </summary>
        public uint Crc;
        /// <summary>
        /// recompress downloaded bundle to LZ4 or not
        /// </summary>
        public bool CompressLocalCache;
        /// <summary>
        /// indicate download state of one assetbundle
        /// </summary>
        public BundleState State;
        /// <summary>
        /// indicate storage location of one assetbundle
        /// </summary>
        public BundleLocation Location;
        /// <summary>
        /// name array of dependency bundles
        /// </summary>
        public DependencyBundle[] Dependencies;
    }
}

