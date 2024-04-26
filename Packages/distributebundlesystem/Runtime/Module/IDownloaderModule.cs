using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using System;
using System.Threading;

namespace Runtime.AdvancedBundleSystem.Module
{
    public interface IDownloaderModule : IModule
    {
        void DownloadAssetBundleAsync(BundleInfo bundleInfo, CancellationToken cancellationToken,
            Action onStarted, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted, BundleDownloadPriority downloadPriority);
        void DownloadTextAsync(string url, Action<string> onCompleted);
        void DownloadBytesAsync(string url, Action<byte[]> onCompleted);
    }

}

