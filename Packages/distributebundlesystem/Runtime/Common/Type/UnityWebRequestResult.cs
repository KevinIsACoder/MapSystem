using System.Text;
using UnityEngine.Networking;

namespace Runtime.AdvancedBundleSystem.Common.Type
{
    /// <summary>
    /// Container class for the result of a unity web request.
    /// </summary>
    public class UnityWebRequestResult
    {
        /// <summary>
        /// Creates a new instance of <see cref="UnityWebRequestResult"/>.
        /// </summary>
        /// <param name="request">The unity web request.</param>
        public UnityWebRequestResult(UnityWebRequest request)
        {
            string error = request.error;
#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.DataProcessingError && request.downloadHandler != null)
            {
                // https://docs.unity3d.com/ScriptReference/Networking.DownloadHandler-error.html
                // When a UnityWebRequest ends with the result, UnityWebRequest.Result.DataProcessingError, the message describing the error is in the download handler
                error = $"{error} : {request.downloadHandler.error}";
            }

            Result = request.result;
#endif
            Error = error;
            ResponseCode = request.responseCode;
            Method = request.method;
            Url = request.url;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

#if UNITY_2020_1_OR_NEWER
            sb.AppendLine($"{Result} : {Error}");
#else
            if (!string.IsNullOrEmpty(Error))
                sb.AppendLine(Error);
#endif
            if (ResponseCode > 0)
                sb.AppendLine($"ResponseCode : {ResponseCode}, Method : {Method}");
            sb.AppendLine($"url : {Url}");

            return sb.ToString();
        }

        /// <summary>
        /// A string explaining the error that occured.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// The numeric HTTP response code returned by the server, if any.
        /// See <a href="https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest-responseCode.html">documentation</a> for more details.
        /// </summary>
        public long ResponseCode { get; }

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// The outcome of the request.
        /// </summary>
        public UnityWebRequest.Result Result { get; }
#endif
        /// <summary>
        /// The HTTP verb used by this UnityWebRequest, such as GET or POST.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// The target url of the request.
        /// </summary>
        public string Url { get; }
    }
}