using Runtime.AdvancedBundleSystem.Common.Enum;
using System;

namespace Runtime.AdvancedBundleSystem.Common.Serialized
{
    [Serializable]
    public class GroupInfo
    {
        public GroupInfo(string hash, GroupBundleState state)
        {
            Hash = hash;
            State = state;
        }
        /// <summary>
        /// 128 bit hash code
        /// </summary>
        public string Hash;

        /// <summary>
        /// state
        /// </summary>
        public GroupBundleState State;
    }
}