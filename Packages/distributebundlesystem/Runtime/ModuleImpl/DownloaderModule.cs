using Cysharp.Threading.Tasks;
using Runtime.AdvancedBundleSystem.Common;
using Runtime.AdvancedBundleSystem.Common.Enum;
using Runtime.AdvancedBundleSystem.Common.Serialized;
using Runtime.AdvancedBundleSystem.Common.Util;
using Runtime.AdvancedBundleSystem.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Runtime.AdvancedBundleSystem.Common.Gen;
using UnityEngine;
using UnityEngine.Networking;

namespace Runtime.AdvancedBundleSystem.ModuleImpl
{
    enum DownloadTaskState
    {
        None,
        CheckCRC,
        Canceled,
        Destroyed,
        Exceptional
    }

    class DownloadTask
    {
        private const float DOWNLOAD_PROGRESS_WEIGHT = 0.6f;
        private const float RECOMPRESS_PROGRESS_WEIGHT = 0.4f;
        private string m_Url;
        private readonly long m_TotalLength;
        private readonly uint m_CRC;
        private readonly string m_CachePath;
        private readonly bool m_CompressLocalCache;
        public CancellationToken CancellationToken;
        private readonly Action<float> m_OnProgressValueChanged;
        private readonly Action m_OnStarted;
        public readonly Action<GroupUpdateCompletedStatus> OnCompleted;

        public byte[] BufferRead;
        private const int BufferSize = 32 * 1024;
        
        private long m_FileLength;
        private float m_Progress;
        public volatile bool ProgressChanged;
        private DownloadTaskState m_TaskState;
        public DownloadTask(string url, long totalLength, uint crc, string cachePath, bool compressLocalCache, CancellationToken cancellationToken,
            Action<float> onProgressValueChanged, Action onStarted, Action<GroupUpdateCompletedStatus> onCompleted)
        {
            m_Url = url;
            m_TotalLength = totalLength;
            m_CRC = crc;
            m_CachePath = cachePath;
            m_CompressLocalCache = compressLocalCache;
            CancellationToken = cancellationToken;
            m_OnProgressValueChanged = onProgressValueChanged;
            m_OnStarted = onStarted;
            OnCompleted = onCompleted;
            m_Progress = 0;
            ProgressChanged = false;
            BufferRead = ABSBufferPool.Get(BufferSize, true);
        }

        

        public void Destroy()
        {
            m_TaskState = DownloadTaskState.Destroyed;
        }

        public void InvokeProgressChangedCallback()
        {
            m_OnProgressValueChanged?.Invoke(m_Progress);
        }

        private void OnTaskCheckCRC(Action onTaskFinished)
        {
            if (!RuntimeUtils.CheckCrc(m_CachePath, m_CRC))
            {
                LoggerInternal.LogWarningFormat("crc mismatch for path: {0}, expected is {1}", m_CachePath, m_CRC);
                OnCompleted?.Invoke(GroupUpdateCompletedStatus.CrcCheckOrRecompressFailed);
                onTaskFinished?.Invoke();
                return;
            }
            LoggerInternal.LogFormat("downloaded bundle: {0} finished and crc check passed, now waiting for recompresion.", m_Url);

            if (m_CompressLocalCache)
            {
                AssetBundleRecompressOperation recompressOp = AssetBundle.RecompressAssetBundleAsync(m_Url, m_Url, BuildCompression.LZ4Runtime);
                recompressOp.ToUniTask(Progress.Create<float>((progress) => 
                { 
                    m_Progress = RECOMPRESS_PROGRESS_WEIGHT * progress;
                    ProgressChanged = true;
                }));
                recompressOp.completed += (op) =>
                {
                    recompressOp = op as AssetBundleRecompressOperation;
                    if (op.isDone)
                    {
                        OnFinished(onTaskFinished);
                    }
                };
            }
            else
            {
                OnFinished(onTaskFinished);
            }
        }

        private void OnFinished(Action onTaskFinished)
        {
            m_Progress = 1;
            ProgressChanged = true;
            OnCompleted?.Invoke(GroupUpdateCompletedStatus.Success);
            onTaskFinished?.Invoke();
        }
        private void OnTaskCanceled(Action onTaskFinished)
        {
            LoggerInternal.LogFormat("canceled download bundle: [{0}]", m_Url);
            OnCompleted?.Invoke(GroupUpdateCompletedStatus.Canceled);
            onTaskFinished?.Invoke();
        }

        private void OnTaskExceptional(Action onTaskFinished)
        {
            LoggerInternal.LogFormat("task finished with an excetpion caught, bundle: [{0}]", m_Url);
            OnCompleted?.Invoke(GroupUpdateCompletedStatus.Exceptional);
            onTaskFinished?.Invoke();
        }
        private void AbortRequest(HttpWebRequest request)
        {
            if (request != null)
            {
                request.Abort();
            }
        }

        public async UniTaskVoid Execute(Action onTaskFinished)
        {
            LoggerInternal.LogFormat("downloading bundle: {0}", m_Url);
            m_OnStarted?.Invoke();
            m_Progress = 0f;
            var awaiter = UniTask.RunOnThreadPool(async () =>
            {
                RuntimeUtils.CheckFileAndCreateDirWhenNeeded(m_CachePath);
                using (FileStream fs = File.OpenWrite(m_CachePath))
                {
                    m_FileLength = fs.Length;
                    m_Progress = (float)m_FileLength / m_TotalLength * DOWNLOAD_PROGRESS_WEIGHT;
                    ProgressChanged = true;
                    LoggerInternal.LogFormat("file: {0}, downloaded:{1} M, remain:{2} M", m_CachePath, m_FileLength / 1048576f, (m_TotalLength - m_FileLength) / 1048576f);
                    if (m_FileLength < m_TotalLength)
                    {
                        fs.Seek(m_FileLength, SeekOrigin.Begin);
                        try
                        {
                            LoggerInternal.LogFormat("before_create_httpwebrequet: {0}", m_Url);
                            HttpWebRequest request = WebRequest.Create(m_Url) as HttpWebRequest;
                            LoggerInternal.LogFormat("after_create_httpwebrequet: {0}", m_Url);
                            request.AddRange((int)m_FileLength);
                            request.ServicePoint.ConnectionLimit = DownloaderModule.MaxConnectionLimit;
                            request.ServicePoint.Expect100Continue = false;
                            request.KeepAlive = true;
                            request.Proxy = null;
                            using (WebResponse response = await request.GetResponseAsync().AsUniTask(false))
                            {
                                LoggerInternal.LogFormat("after_create_GetResponseAsync: {0}", m_Url);
                                using (Stream stream = response.GetResponseStream())
                                {
                                    int readLength = await stream.ReadAsync(BufferRead, 0, BufferSize).AsUniTask(false);
                                    while (readLength > 0)
                                    {
                                        if (CancellationToken.IsCancellationRequested)
                                        {
                                            LoggerInternal.LogWarningFormat("cancel download[by user] bundle[{0}]", m_Url);
                                            AbortRequest(request);
                                            m_TaskState = DownloadTaskState.Canceled;
                                            return;
                                        }
                                        if (m_TaskState == DownloadTaskState.Destroyed)
                                        {
                                            LoggerInternal.LogWarningFormat("cancel download[on destroy] bundle[{0}]", m_Url);
                                            AbortRequest(request);
                                            m_TaskState = DownloadTaskState.Canceled;
                                            return;
                                        }
                                        if (m_TaskState == DownloadTaskState.Exceptional)
                                        {
                                            LoggerInternal.LogWarningFormat("caught an excetpion on bundle[{0}] ", m_Url);
                                            AbortRequest(request);
                                            return;
                                        }
                                        fs.Write(BufferRead, 0, readLength);
                                        m_FileLength += readLength;
                                        m_Progress = (float)m_FileLength / m_TotalLength * DOWNLOAD_PROGRESS_WEIGHT;
                                        ProgressChanged = true;
                                        readLength = await stream.ReadAsync(BufferRead, 0, BufferSize).AsUniTask(false);
                                    }
                                }
                            }
                            LoggerInternal.LogFormat("download finished for url: {0}, progress: {1}", m_Url, m_Progress);
                            m_Progress = DOWNLOAD_PROGRESS_WEIGHT;
                            m_TaskState = DownloadTaskState.CheckCRC;
                        }
                        catch (WebException)
                        {
                            m_TaskState = DownloadTaskState.Exceptional;
                        }
                        catch (Exception)
                        {
                            m_TaskState = DownloadTaskState.Exceptional;
                        }
                    }
                    else
                    {
                        m_Progress = DOWNLOAD_PROGRESS_WEIGHT;
                        m_TaskState = DownloadTaskState.CheckCRC;
                    }
                }

            }, true, CancellationToken).GetAwaiter();

            await UniTask.WaitUntil(() => m_TaskState == DownloadTaskState.CheckCRC || m_TaskState == DownloadTaskState.Canceled || m_TaskState == DownloadTaskState.Exceptional, PlayerLoopTiming.PreUpdate);
            awaiter.OnCompleted(() => {
                switch (m_TaskState)
                {
                    case DownloadTaskState.CheckCRC:
                        OnTaskCheckCRC(onTaskFinished);
                        break;
                    case DownloadTaskState.Canceled:
                        OnTaskCanceled(onTaskFinished);
                        break;
                    case DownloadTaskState.Exceptional:
                        OnTaskExceptional(onTaskFinished);
                        break;
                    default:
                        LoggerInternal.LogErrorFormat("invalid task state: {0}", m_TaskState);
                        break;
                }
            });
        }
    }
    public class DownloaderModule : BaseModule, IDownloaderModule
    {
        private List<DownloadTask> m_DownloadingTaskList = new List<DownloadTask>();

        private readonly Queue<DownloadTask> m_PendingQueueNormal = new Queue<DownloadTask>();
        private int m_PendingQueueNormalSize = 0;
        private readonly Stack<DownloadTask> m_PendingStackHigh = new Stack<DownloadTask>();
        private int m_PendingQueueHighSize = 0;

        private int m_MaxConcurrentDownloadingTaskSizeNormal;
        private int m_MaxConcurrentDownloadingTaskSizeHigh;
        public static int MaxConnectionLimit;

        private int m_DownloadingTaskCountNormal;
        private int m_DownloadingTaskCountHigh;

        private bool m_ProcessingPendingQueue;
        public override void Init()
        {
            if (m_Initialized)
            {
                return;
            }
            base.Init();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            MaxConnectionLimit = RuntimeSettings.c_ConnectionLimit;
            ServicePointManager.DefaultConnectionLimit = MaxConnectionLimit;
            ServicePointManager.Expect100Continue = false;
            m_DownloadingTaskCountNormal = 0;
            m_DownloadingTaskCountHigh = 0;
            m_ProcessingPendingQueue = false;
            m_MaxConcurrentDownloadingTaskSizeNormal = Mathf.Max(SystemInfo.processorCount, RuntimeSettings.c_MaxConcurrentDownloadingTaskSizeNormalPriority);
            m_MaxConcurrentDownloadingTaskSizeHigh = RuntimeSettings.c_MaxConcurrentDownloadingTaskSizeHighPriority;
            MaxConnectionLimit = RuntimeSettings.c_ConnectionLimit;
            ABSBufferPool.IsEnabled = true;
        }

        public override void Destroy()
        {
            base.Destroy();
            LoggerInternal.LogFormat("downloading task list size: {0}", m_DownloadingTaskList.Count);
            foreach (DownloadTask downloadingTask in m_DownloadingTaskList)
            {
                downloadingTask.Destroy();
            }
            LoggerInternal.LogFormat("pending queue normal size: {0}", m_PendingQueueNormal.Count);
            LoggerInternal.LogFormat("pending queue high size: {0}", m_PendingStackHigh.Count);
        }

        private async UniTaskVoid ProcessPendingQueue()
        {
            m_ProcessingPendingQueue = true;

            while (true)
            {
                if (m_Destroyed)
                {
                    return;
                }
                if (m_PendingQueueHighSize > 0 && m_DownloadingTaskCountHigh < m_MaxConcurrentDownloadingTaskSizeHigh)
                {
                    DownloadTask downloadingTask = m_PendingStackHigh.Pop();
                    --m_PendingQueueHighSize;
                    if (!downloadingTask.CancellationToken.IsCancellationRequested)
                    {
                        m_DownloadingTaskCountHigh++;
                        m_DownloadingTaskList.Add(downloadingTask);
                        downloadingTask.Execute(() => {
                            m_DownloadingTaskCountHigh--;
                            m_DownloadingTaskList.Remove(downloadingTask);
                            ABSBufferPool.Release(downloadingTask.BufferRead);
                        }).Forget();
                    }
                    else
                    {
                        LoggerInternal.LogWarning("downloading task is canceled, won't add to downloading task");
                        downloadingTask.OnCompleted?.Invoke(GroupUpdateCompletedStatus.Canceled);
                    }

                }
                if (m_PendingQueueNormalSize > 0 && m_DownloadingTaskCountNormal < m_MaxConcurrentDownloadingTaskSizeNormal)
                {
                    DownloadTask downloadingTask = m_PendingQueueNormal.Dequeue();
                    --m_PendingQueueNormalSize;
                    if (!downloadingTask.CancellationToken.IsCancellationRequested)
                    {
                        m_DownloadingTaskCountNormal++;
                        m_DownloadingTaskList.Add(downloadingTask);
                        downloadingTask.Execute(() => {
                            m_DownloadingTaskCountNormal--;
                            m_DownloadingTaskList.Remove(downloadingTask);
                            ABSBufferPool.Release(downloadingTask.BufferRead);
                        }).Forget();
                    }
                    else
                    {
                        LoggerInternal.LogWarning("downloading task is canceled, won't add to downloading task");
                        downloadingTask.OnCompleted?.Invoke(GroupUpdateCompletedStatus.Canceled);
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.PreUpdate);
                foreach (DownloadTask task in m_DownloadingTaskList)
                {
                    if (task.ProgressChanged)
                    {
                        task.ProgressChanged = false;
                        task.InvokeProgressChangedCallback();
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.LastUpdate);
                if (m_PendingQueueNormalSize == 0 && m_PendingQueueHighSize == 0 && m_DownloadingTaskCountNormal == 0 && m_DownloadingTaskCountHigh == 0)
                {
                    m_ProcessingPendingQueue = false;
                    if (ABSBufferPool.PoolSize > 1048576)
                    {
                        ABSBufferPool.Clear();
                        GC.Collect();
                    }
                    break;
                }
            }
        }

        public void DownloadAssetBundleAsync(BundleInfo bundleInfo, CancellationToken cancellationToken, Action onStarted, Action<float> onProgressValueChanged, Action<GroupUpdateCompletedStatus> onCompleted, BundleDownloadPriority downloadPriority)
        {
            string bundleName = bundleInfo.Name;
            string bundleRemoteBasePath = ResourceManager.AssetBundleModule.BundleBasePathR;
            string bundlePersistentBasePath = ResourceManager.AssetBundleModule.BundleBasePathP;
            DownloadTask downloadTask = new DownloadTask(string.Format("{0}/{1}", bundleRemoteBasePath, bundleName), bundleInfo.Size, bundleInfo.Crc, string.Format("{0}/{1}", bundlePersistentBasePath, bundleName), bundleInfo.CompressLocalCache, cancellationToken, onProgressValueChanged, onStarted, onCompleted);
            if (downloadPriority == BundleDownloadPriority.Normal)
            {
                m_PendingQueueNormal.Enqueue(downloadTask);
                ++m_PendingQueueNormalSize;
            }
            else if(downloadPriority == BundleDownloadPriority.High)
            {
                m_PendingStackHigh.Push(downloadTask);
                ++m_PendingQueueHighSize;
            }
            else if(downloadPriority == BundleDownloadPriority.Low)
            {
                //TODO:macdeng
            }
            if (!m_ProcessingPendingQueue)
            {
                ProcessPendingQueue().Forget();
            }
        }

        public void DownloadBytesAsync(string url, Action<byte[]> onCompleted)
        {
            try
            {
                UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    UnityWebRequestAsyncOperation op = asyncOp as UnityWebRequestAsyncOperation;
                    if (!RuntimeUtils.RequestHasErrors(op.webRequest, out _))
                    {
                        onCompleted?.Invoke(op.webRequest.downloadHandler.data);
                    }
                    else
                    {
                        onCompleted?.Invoke(null);
                    }
                };
            }
            catch (UnityWebRequestException)
            {
                LoggerInternal.LogFormat("file bytes: {0} not found", url);
                onCompleted?.Invoke(null);
            }
        }

        public void DownloadTextAsync(string url, Action<string> onCompleted)
        {
            RuntimeUtils.DownloadTextAsync(url, onCompleted);
        }
    }
}
