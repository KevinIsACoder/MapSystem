using Cysharp.Threading.Tasks;
using Runtime.AdvancedBundleSystem.Common.Algorithm;
using Runtime.AdvancedBundleSystem.Common.Type;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

[assembly: InternalsVisibleTo("ABS_TestRuntime")]
namespace Runtime.AdvancedBundleSystem.Common.Util
{
    public static class RuntimeUtils
    {
        internal static void CheckFileAndCreateDirWhenNeeded(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            FileInfo file_info = new FileInfo(filePath);
            DirectoryInfo dir_info = file_info.Directory;
            if (!dir_info.Exists)
            {
                Directory.CreateDirectory(dir_info.FullName);
            }
        }



        /// <summary>
        /// Editor usage: what ever you like.
        /// Runtime usage: only for very samll file like like hash or will cause memory leak.
        /// </summary>
        /// <param name="inFile"></param>
        /// <returns></returns>
        internal static byte[] SafeReadAllBytes(string inFile)
        {
            if (string.IsNullOrEmpty(inFile) || !File.Exists(inFile))
            {
                return null;
            }
            byte[] buffer = null;
            using (FileStream fs = File.OpenRead(inFile))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            return buffer;
        }

        internal static bool SafeWriteAllText(string outFile, string text)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllText(outFile, text);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggerInternal.LogError(string.Format("SafeWriteAllText failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        internal static void GenerateFileHash(string source, string outputFile)
        {
            string hashString;
            Hash128 hash = new Hash128();
            byte[] bytes = SafeReadAllBytes(source);
            HashUtilities.ComputeHash128(bytes, ref hash);
            hashString = hash.ToString();
            SafeWriteAllText(outputFile, hashString);
        }

        internal static void ReadTextAsync(string inputFile, Action<string> onCompleted)
        {
            if (string.IsNullOrEmpty(inputFile))
            {
                onCompleted?.Invoke(null);
                return;
            }
            if (Application.platform == RuntimePlatform.Android && inputFile.StartsWith("jar:"))
            {
                DownloadTextAsync(inputFile, (text) => { onCompleted?.Invoke(text); });
            }
            else
            {
                if (!File.Exists(inputFile))
                {
                    onCompleted?.Invoke(null);
                    return;
                }
                File.SetAttributes(inputFile, FileAttributes.Normal);
                onCompleted?.Invoke(File.ReadAllText(inputFile));
            }
        }
        const int BufferSize = 32 * 1024;
        internal static bool CheckCrc(string filePath, uint expectedCrc)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }
            //test code start
            /*System.Collections.Generic.List<string> testBundleList = new System.Collections.Generic.List<string>
            {
                "IconEffect_10001.bundle",
                "XXXXXXX.bundle",
            };
            foreach(string bundleName in testBundleList)
            {
                if (filePath.Contains(bundleName))
                {
                    if (UnityEngine.Random.Range(0, 9) <= 4)
                    {
                        LoggerInternal.LogErrorFormat("CRC check failed[debug mode] for bundle:[{0}]", filePath);
                        return false;
                    }
                }

            }*/
            //test code end
            byte[] buffer = new byte[BufferSize];
            uint currentCrc = 0;
            using (FileStream fs = File.OpenRead(filePath))
            {
                int len = fs.Read(buffer, 0, buffer.Length);
                while (len > 0)
                {
                    currentCrc = Crc32Algorithm.Append(currentCrc, buffer, 0, len);
                    len = fs.Read(buffer, 0, buffer.Length);
                }
            }
            if (currentCrc == expectedCrc)
            {
                LoggerInternal.LogFormat("crc check passed, current is: {0}, expected is: {1}", currentCrc, expectedCrc);
                return true;
            }
            LoggerInternal.LogWarningFormat("crc check failed, current is: {0}, expected is: {1}", currentCrc, expectedCrc);
            return false;
        }

        public static void ReadBytesAsync(string url, Action<byte[]> callback)
        {
            if (Application.platform == RuntimePlatform.Android && url.StartsWith("jar:"))
            {
                DownloadBytesAsync(url, (bytes) => { callback?.Invoke(bytes); });
            }
            else
            {
                if (!File.Exists(url))
                {
                    callback?.Invoke(null);
                    return;
                }
                File.SetAttributes(url, FileAttributes.Normal);
                callback?.Invoke(File.ReadAllBytes(url));
            }
        }

        internal static void DownloadBytesAsync(string url, Action<byte[]> callback)
        {
            try
            {
                UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    UnityWebRequestAsyncOperation op = asyncOp as UnityWebRequestAsyncOperation;
                    if (!RequestHasErrors(op.webRequest, out _))
                    {
                        callback?.Invoke(op.webRequest.downloadHandler.data);
                    }
                    else
                    {
                        callback?.Invoke(null);
                    }
                };
            }
            catch (UnityWebRequestException)
            {
                LoggerInternal.LogFormat("file bytes: {0} not found", url);
                callback?.Invoke(null);
            }
        }

        internal static void DownloadTextAsync(string url, Action<string> callback)
        {
            try
            {
                UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    UnityWebRequestAsyncOperation op = asyncOp as UnityWebRequestAsyncOperation;
                    if (!RequestHasErrors(op.webRequest, out _))
                    {
                        callback?.Invoke(op.webRequest.downloadHandler.text);
                    }
                    else
                    {
                        callback?.Invoke(null);
                    }
                    request?.Dispose();
                };
            }
            catch (UnityWebRequestException)
            {
                LoggerInternal.LogFormat("text: {0} not found", url);
                callback?.Invoke(null);
            }
        }


        internal static string GetPlatformName()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_STANDALONE_WIN
            return "Windows";
#elif UNITY_STANDALONE_OSX
            return "MacOS";
#elif UNITY_WEBGL
            return "WebGL";
#else
            return "UnsupportedPlatform";
#endif
        }

        public static bool RequestHasErrors(UnityWebRequest webReq, out UnityWebRequestResult result)
        {
            result = null;
            if (webReq == null || !webReq.isDone)
                return false;

#if UNITY_2020_1_OR_NEWER
            switch (webReq.result)
            {
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.Success:
                    return false;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    result = new UnityWebRequestResult(webReq);
                    return true;
                default:
                    throw new NotImplementedException($"Cannot determine whether UnityWebRequest succeeded or not from result : {webReq.result}");
            }
#else
            var isError = webReq.isHttpError || webReq.isNetworkError;
            if (isError)
                result = new UnityWebRequestResult(webReq);

            return isError;
#endif
        }

        internal static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }

                if (!File.Exists(filePath))
                {
                    return true;
                }
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggerInternal.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
                return false;
            }
        }
    }
}
