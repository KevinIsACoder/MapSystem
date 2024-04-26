using Runtime.AdvancedBundleSystem.Common.Util;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Common.Deserializer
{
    public static class ObjectDeserializer
    {
        public static T DeserializeBytes<T>(byte[] bytes)
        {
            if (bytes == null)
            {
                return default;
            }
            T ret;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                ret = (T)formatter.Deserialize(ms);
            }
            catch (SerializationException e)
            {
                LoggerInternal.LogErrorFormat("Failed to deserialize bytes: {0}", e.Message);
                throw;
            }
            finally
            {
                ms.Close();
            }
            return ret;
        }

        public static void DeserializeAsync<T>(string inputFile, Action<T> callback)
        {
            if (string.IsNullOrEmpty(inputFile))
            {
                callback?.Invoke(default);
                return;
            }
            T ret;
            if (Application.platform == RuntimePlatform.Android && inputFile.StartsWith("jar:"))
            {
                RuntimeUtils.DownloadBytesAsync(inputFile, (bytes) =>
                {
                    ret = DeserializeBytes<T>(bytes);
                    callback?.Invoke(ret);
                });
            }
            else
            {
                if (!File.Exists(inputFile))
                {
                    callback?.Invoke(default);
                    return;
                }

                FileStream fs = File.OpenRead(inputFile);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    ret = (T)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    LoggerInternal.LogErrorFormat("Failed to deserialize: {0}", e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
                callback?.Invoke(ret);
            }
        }
    }
}
