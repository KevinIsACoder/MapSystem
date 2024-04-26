using System;
using System.Runtime.Serialization;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
    /// <summary>
    /// bundle info dictionary:
    /// (key: unique bundle name,
    /// value: BundleInfo obj)
    /// </summary>
    [Serializable]
    public class BundleInfoDictionary : SerializableDictionary<string, BundleInfo>
    {
        public BundleInfoDictionary() : base()
        {

        }

        public BundleInfoDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        protected override bool KeysAreEqual(string a, string b)
        {
            return a == b;
        }

        protected override bool ValuesAreEqual(BundleInfo a, BundleInfo b)
        {
            return a.Hash == b.Hash && a.Size == b.Size;
        }
    }
}