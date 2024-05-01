using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace MilitarySimulator.Core.Utils
{
    public class HttpUtils
    {
        private static readonly object lockObj = new object();
        private static HttpClient m_httpClient;
        private static HttpUtils m_httpUtils;
        public static HttpUtils Instance
        {
            get
            {
                if (m_httpUtils == null)
                {
                    lock (lockObj)
                    {
                        m_httpUtils ??= new HttpUtils();
                        m_httpClient ??= new HttpClient();
                    }

                }
                return m_httpUtils;
            }
        }

        public async Task<T> PostAsync<T>(string url, string content, Action<T> callback = null) where T: class
        {
            try
            {
                var httpContent = new StringContent(content);
                httpContent.Headers.Add("Content-Type", "application/json;chartset=UTF-8");
                var httpResponse = await m_httpClient.PostAsync(url, httpContent);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                { 
                    var result = await httpResponse.Content.ReadAsStringAsync();
                    var data = JsonUtility.FromJson<T>(result);
                    if (callback != null)
                    {
                        callback(data);
                    }
                    return data;
                }
            }
            catch (HttpRequestException ex)
            {
               // UIManager.Instance.TryShowMsg( $"HttpUtil PostAsync Error! url: { url } ErrorData: {ex.Message}");
            }
            return default;
        }

        public async Task<T> GetAsync<T>(string url, Action<T> callback = null) where T: class
        {
            try
            {
                var httpResponse = await m_httpClient.GetAsync(url);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var result = await httpResponse.Content.ReadAsStringAsync();
                    var data = JsonUtility.FromJson<T>(result);
                    
                    if (callback != null)
                    {
                        callback(data);
                    }

                    return data;
                }
                else
                {
                    //UIManager.Instance.TryShowMsg($"HttpUtil Error! url: {url} ErrorData: {httpResponse.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                //UIManager.Instance.TryShowMsg( $"HttpUtil GetAsync Error! url: { url } ErrorData: {ex.Message}");
            }
            return default;
        }

        public async Task<T> SendAysnc<T>(HttpRequestMessage requestMessage, Action<T> callback = null) where T : class
        {
            try
            {
                var httpResponse = await m_httpClient.SendAsync(requestMessage);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var result = await httpResponse.Content.ReadAsStringAsync();
                    var data = JsonUtility.FromJson<T>(result);
                    if (callback != null)
                    {
                        callback(data);
                    }
                    return data;
                }
                else
                {
                    //UIManager.Instance.TryShowMsg($"HttpUtil Error! url: {requestMessage.RequestUri} ErrorData: {httpResponse.StatusCode}");
                }
            }
            catch (HttpRequestException e)
            {
                //UIManager.Instance.TryShowMsg( $"HttpUtil GetAsync Error! url: { requestMessage.RequestUri } ErrorData: {e.Message}");
            }

            return default;
        }

    }
}