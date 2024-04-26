namespace Runtime.AdvancedBundleSystem.Common.Enum
{
    public enum BundleLocation
    {
        /// <summary>
        /// in streamingassets folder. readonly
        /// </summary>
        InPackage = 0,
        /// <summary>
        /// in persistent directory. read and write
        /// </summary>
        InPersistent,
        /// <summary>
        /// on remote server. need download
        /// </summary>
        Remote
    }
}