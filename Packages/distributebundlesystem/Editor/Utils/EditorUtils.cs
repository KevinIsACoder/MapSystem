using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Runtime.AdvancedBundleSystem.Common;
using Runtime.AdvancedBundleSystem.Common.Gen;
using Runtime.AdvancedBundleSystem.Common.Util;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.Utils
{
    public static class EditorUtils
    {
        [MenuItem("AdvancedBundleSystem/Generate Runtime Settings", false, 3)]
        private static void GenerateRuntimeSettings()
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return;
            }
            string filePath = Path.Combine(new string[]
            {
                Directory.GetCurrentDirectory(),
                "Packages/DistributeBundleSystem/Runtime/Common/Gen/RuntimeSettings.cs",
                
            });
            using (FileStream fs = File.Open(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(
                    "/*\n"
                    + " *  auto generated code, do not modify it!\n"
                    + " */\n"
                    + "namespace Runtime.AdvancedBundleSystem.Common.Gen\n"
                    + "{\n"
                    + "    public static class RuntimeSettings\n"
                    + "    {\n"
                    + $"        public const string c_AssetBundleOutputPath = \"{settings.AssetBundleOutputPath}\";\n"
                    + $"        public const string c_CachingRootPath = \"{settings.CachingRootPath}\";\n"
                    + $"        public const string c_ServerAddress = \"{settings.ServerAddress}\";\n"
                    + $"        public const int c_RootAssetGroupId = {settings.RootAssetGroupId.ToString()};\n"
                    + $"        public const int c_MaxConcurrentDownloadingTaskSizeNormalPriority = {settings.MaxConcurrentDownloadingTaskSizeNormalPriority.ToString()};\n"
                    + $"        public const int c_MaxConcurrentDownloadingTaskSizeHighPriority = {settings.MaxConcurrentDownloadingTaskSizeHighPriority.ToString()};\n"
                    + $"        public const int c_ConnectionLimit = {settings.ConnectionLimit.ToString()};\n"
                    + $"        public const bool c_IsEditorMode = {settings.IsEditorMode.ToString().ToLower()};\n"
                    + $"        public const bool c_ForceDllEditorMode = {settings.IsEditorMode.ToString().ToLower()};\n"
                    + "    }\n"
                    + "}"
                );
                fs.Write(info, 0, info.Length);
            }
            EditorUtility.DisplayDialog("Info", "Successfully generated code.", "OK");
        }
        
        [MenuItem("AdvancedBundleSystem/Open PersistentPath", false, 4)]
        private static void OpenPersistentPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath + "/" + RuntimeSettings.c_CachingRootPath);
        }

        [MenuItem("AdvancedBundleSystem/Open AssetBundle Output Path", false, 5)]
        private static void OpenAssetBundleOutputPath()
        {
            string path = new DirectoryInfo(Application.dataPath).Parent.FullName + "/" + RuntimeSettings.c_AssetBundleOutputPath;
            EditorUtility.RevealInFinder(path);
        }
        
        [MenuItem("AdvancedBundleSystem/Open ClientData Path", false, 6)]
        private static void OpenClientDataPath()
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return;
            }
            string path = new DirectoryInfo(Application.dataPath).Parent.FullName + "/" + settings.ClientDataPath;
            EditorUtility.RevealInFinder(path);
        }
        
        [MenuItem("AdvancedBundleSystem/Copy ClientData to StreamAssets", false, 7)]
        public static void CopyClientDataToStreamingAssetsPath()
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return;
            }
            string clientDataPath = new DirectoryInfo(Application.dataPath).Parent.FullName + "/" + settings.ClientDataPath;
            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, settings.AssetBundleOutputPath);
            CopyFolder(clientDataPath, streamingAssetsPath);
        }

        
        [MenuItem("AdvancedBundleSystem/Unload All Loaded AssetBundles", false, 7)]
        private static void UnloadAllLoadedAssetBundles()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            EditorUtility.DisplayDialog("Info", "Successfully unload all asset bundles!", "OK");
        }

        private static BuildTargetGroup GetBuildTargetGroup()
        {
#if UNITY_ANDROID
            return BuildTargetGroup.Android;
#elif UNITY_IOS
            return BuildTargetGroup.iOS;
#elif UNITY_STANDALONE_WIN
            return BuildTargetGroup.Standalone;
#elif UNITY_STANDALONE_OSX
            return BuildTargetGroup.Standalone;
#elif UNITY_WEBGL
            return BuildTargetGroup.WebGL;
#else
            return BuildTargetGroup.Unknown;
#endif
        }

        public static bool IsDebuggerEnabled()
        {
            BuildTargetGroup buildTargetGroup = GetBuildTargetGroup();
            
            PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup, out string[] defineSymbols);
            if (defineSymbols == null)
            {
                return false;
            }
            return defineSymbols.Contains(LoggerInternal.c_LogCondition);
        }
        
        [MenuItem("AdvancedBundleSystem/Debugger/Enable", false, 8)]
        public static void EnableDebugger()
        {
            BuildTargetGroup buildTargetGroup = GetBuildTargetGroup();
            PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup, out string[] defineSymbols);
            if (defineSymbols == null)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, LoggerInternal.c_LogCondition);
                return;
            }
            List<string> defineSymbolList = defineSymbols.ToList();
            defineSymbolList.Add(LoggerInternal.c_LogCondition);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbolList.ToArray());
        }
        
        [MenuItem("AdvancedBundleSystem/Debugger/Enable", true, 8)]
        private static bool EnableDebuggerValidate()
        {
            bool isDebuggerEnabled = IsDebuggerEnabled();
            Menu.SetChecked("AdvancedBundleSystem/Debugger/Enable", false);
            return !isDebuggerEnabled;
        }

        [MenuItem("AdvancedBundleSystem/Debugger/Disable", false, 8)]
        public static void DisableDebugger()
        {
            BuildTargetGroup buildTargetGroup = GetBuildTargetGroup();
            PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup, out string[] defineSymbols);
            if (defineSymbols == null)
            {
                return;
            }
            List<string> defineSymbolList = defineSymbols.ToList();
            defineSymbolList.Remove(LoggerInternal.c_LogCondition);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbolList.ToArray());
        }
        
        [MenuItem("AdvancedBundleSystem/Debugger/Disable", true, 8)]
        private static bool DisableDebuggerValidate()
        {
            bool isDebuggerEnabled = IsDebuggerEnabled();
            Menu.SetChecked("AdvancedBundleSystem/Debugger/Disable", false);
            return isDebuggerEnabled;
        }

        [MenuItem("AdvancedBundleSystem/Crypto/EncryptAllBundles")]
        public static bool EncryptAllBundlesToStreamingAssets()
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return false;
            }
            CopyClientDataToStreamingAssetsPath();
            string clientDataPath = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, settings.ClientDataPath,RuntimeUtils.GetPlatformName());
            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, settings.AssetBundleOutputPath,RuntimeUtils.GetPlatformName());
            
            string[] files = Directory.GetFiles(clientDataPath, "*" + settings.AssetBundleSuffix);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                Debug.Log($"Encrypt {fileName}");
                EncryptFile(file, streamingAssetsPath + $"/{fileName}");
            }
            Debug.Log("Encryption Done!");
            return true;
        }
        
        [MenuItem("AdvancedBundleSystem/Crypto/DecryptAllBundles")]
        private static bool DecryptAllBundles()
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return false;
            }
            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, settings.AssetBundleOutputPath,RuntimeUtils.GetPlatformName());
            string[] files = Directory.GetFiles(streamingAssetsPath, "*" + settings.AssetBundleSuffix);
            string decryptPath = streamingAssetsPath + "/DeCryptBundles/";
            if (!Directory.Exists(decryptPath)) Directory.CreateDirectory(decryptPath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                Debug.Log($"Decrypt {fileName}");
                DecryptFile(file, decryptPath + $"{fileName}");
            }
            Debug.Log("Decryption Done!");
            return true;
        }
        const int BufferSize = 1024 * 1024;
        private static void EncryptFile(string inputFile, string outputFile)
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return;
            }
            using (DES des = DES.Create())
            {
                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        des.Key = settings.key;
                        des.IV = settings.iv;

                        ICryptoTransform encryptor = des.CreateEncryptor();

                        using (CryptoStream cs = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[BufferSize];
                            int read;
                            while ((read = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, read);
                            }
                            cs.FlushFinalBlock();
                        }
                    }
                }
            }
        }
        
        public static void DecryptFile(string inputFile, string outputFile)
        {
            if (!TryLoadSettings(out AdvancedBundleSystemSettings settings))
            {
                return;
            }
            using (DES des = DES.Create())
            {
                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        des.Key = settings.key;
                        des.IV = settings.iv;

                        ICryptoTransform decryptor = des.CreateDecryptor();

                        using (CryptoStream cs = new CryptoStream(fsOutput, decryptor, CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[BufferSize];
                            int read;
                            while ((read = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, read);
                            }
                            cs.FlushFinalBlock();
                        }
                    }
                }
            }
        }
        
        
        private static bool TryLoadSettings(out AdvancedBundleSystemSettings settings)
        {
            settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Info", "Please generate AdvancedBundleSystemSettings!.", "OK");
                return false;
            }
            return true;
        }

        public static bool IsPathValidEntry(string path)
        {
            if (!path.StartsWith("Assets"))
            {
                return false;
            }
            return true;
        }

        public static void DeleteDirectory(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] dirs = Directory.GetDirectories(dirPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(dirPath, false);
        }

        public static void CopyFileToFolder(string filePath, string destFolder, string suffix = "", bool deleteDestFolder = false)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            else
            {
                if (deleteDestFolder)
                {
                    DeleteDirectory(destFolder);
                    Directory.CreateDirectory(destFolder);
                }
            }

            string fileName = Path.GetFileName(filePath);
            string destPath = Path.Combine(destFolder, fileName + suffix);
            File.Copy(filePath, destPath);
        }
        
        public static void CopyFolder( string sourceFolder, string destFolder )
        {
            if (Directory.Exists(destFolder))
            {
                DeleteDirectory(destFolder);
            }
            Directory.CreateDirectory( destFolder );
            string[] files = Directory.GetFiles( sourceFolder );
            foreach (string file in files)
            {
                string name = Path.GetFileName( file );
                string dest = Path.Combine( destFolder, name );
                File.Copy( file, dest );
            }
            string[] folders = Directory.GetDirectories( sourceFolder );
            foreach (string folder in folders)
            {
                string name = Path.GetFileName( folder );
                string dest = Path.Combine( destFolder, name );
                CopyFolder( folder, dest );
            }
        }

        public static bool IsNameValid(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Name should not be null!");
            }
            bool isValid = true;
            char char0 = name[0];
            if (!((char0 >= 'A' && char0 <= 'Z') ||
                (char0 >= 'a' && char0 <= 'z') || char0 == '_'))
            {
                isValid = false;
            }
            else
            {
                for (int i = 1; i < name.Length; i++)
                {
                    if (!((name[i] >= 'A' && name[i] <= 'Z') ||
                        (name[i] >= 'a' && name[i] <= 'z') ||
                        (name[i] >= '0' && name[i] <= '9') ||
                        name[i] == '_' || name[i] == '-'))
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }
    }

}
