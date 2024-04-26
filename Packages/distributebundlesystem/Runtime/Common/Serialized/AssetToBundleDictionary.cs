using System;
using System.Runtime.Serialization;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
    /// <summary>
    /// key: asset path, value: bundle name which contains the asset
    /// </summary>
    [Serializable]
    public class AssetToBundleDictionary : SerializableDictionary<string, string>
    {
        public AssetToBundleDictionary() : base()
        {

        }

        public AssetToBundleDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        protected override bool KeysAreEqual(string a, string b)
        {
            return a == b;
        }

        protected override bool ValuesAreEqual(string a, string b)
        {
            return a == b;
        }
    }
}