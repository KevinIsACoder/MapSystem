using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Assembly-CSharp")]
[assembly: InternalsVisibleTo("Editor")]
[assembly: InternalsVisibleTo("ABS_Editor")]
namespace Runtime.AdvancedBundleSystem.Common
{
    internal class LoggerInternal
    {
        public const string MessageHead = "[ABS]";
        public const string c_LogCondition = "DebugABS";

        private static void Dump(params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                object arg = args[i];
                if (arg is Array)
                {
                    StringBuilder sb = new StringBuilder();
                    Array array = arg as Array;
                    for (int j = 0; j < array.Length; j++)
                    {
                        object element = array.GetValue(i);
                        sb.Append(element.ToString());
                        if (j < array.Length - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    args[i] = sb.ToString();
                }
            }
        }

        [Conditional(c_LogCondition)]
        internal static void Log(object message, string color = null)
        {
            if(color == null)
            {
                UnityEngine.Debug.Log(MessageHead + message);
            }
            else
            {
                UnityEngine.Debug.Log(MessageHead + $"<color={color}>{message}</color>");
            }
        }

        [Conditional(c_LogCondition)]
        internal static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(MessageHead + message);
        }

        [Conditional(c_LogCondition)]
        internal static void LogError(object message)
        {
            UnityEngine.Debug.LogError(MessageHead + message);
        }

        [Conditional(c_LogCondition)]
        internal static void LogFormat(string format, params object[] args)
        {
            Dump(args);
            UnityEngine.Debug.LogFormat(MessageHead + format, args);
        }

        [Conditional(c_LogCondition)]
        internal static void LogWarningFormat(string format, params object[] args)
        {
            Dump(args);
            UnityEngine.Debug.LogWarningFormat(MessageHead + format, args);
        }

        [Conditional(c_LogCondition)]
        internal static void LogErrorFormat(string format, params object[] args)
        {
            Dump(args);
            UnityEngine.Debug.LogErrorFormat(MessageHead + format, args);
        }
    }

}
