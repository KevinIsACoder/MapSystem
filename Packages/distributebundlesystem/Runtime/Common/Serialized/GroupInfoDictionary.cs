
using System;
using System.Runtime.Serialization;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
    /// <summary>
    /// Group info dictionary:
    /// (key: groupId,
    /// value: GroupInfo obj)
    /// </summary>
    [Serializable]
    public class GroupInfoDictionary : SerializableDictionary<int, GroupInfo>
    {
        public GroupInfoDictionary() : base()
        {

        }

        public GroupInfoDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        protected override bool KeysAreEqual(int a, int b)
        {
            return a == b;
        }

        protected override bool ValuesAreEqual(GroupInfo a, GroupInfo b)
        {
            return a.Hash == b.Hash;
        }
    }
}