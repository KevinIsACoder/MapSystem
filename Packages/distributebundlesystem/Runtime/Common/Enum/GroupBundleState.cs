namespace Runtime.AdvancedBundleSystem.Common.Enum
{
    public enum GroupBundleState
    {
        /// <summary>
        /// init state
        /// </summary>
        None = 0,
        /// <summary>
        /// uptodate
        /// </summary>
        Uptodate,
        /// <summary>
        /// newly added on remote server. download only
        /// </summary>
        Add,
        /// <summary>
        /// changed on remote server. download and remove old one if it's in persistent path
        /// </summary>
        Modify,
        /// <summary>
        /// removed on remote server. 
        /// </summary>
        Remove,
    }
}