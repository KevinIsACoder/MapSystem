#if UNITY_EDITOR
using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Module;
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Runtime.AdvancedBundleSystem.ModuleImpl
{
    public class EditorAssetModule : BaseModule, IAssetModule
    {
        private Assembly m_EditorAssembly;
        private IEnumerable m_AssetSets;
        public bool IsLoadingAnyAsset()
        {
            return false;
        }

        public override void Init()
        {
            base.Init();
            m_EditorAssembly = Assembly.Load("ABS_Editor");
            Type defaultObjectType = m_EditorAssembly.GetType("UnityEditor.AdvancedBundleSystem.Settings.AdvancedBundleSystemSettingsDefaultObject");
            var settings = GetStaticProperty(defaultObjectType, "Settings");
            m_AssetSets = GetProperty(settings, "AssetSets", BindingFlags.Instance | BindingFlags.NonPublic) as IEnumerable;
            //build sub entries
            var itr = m_AssetSets.GetEnumerator();
            while (itr.MoveNext())
            {
                var assetSet = itr.Current;
                var assetEntries =
                    GetProperty(assetSet, "Entries", BindingFlags.Instance | BindingFlags.Public) as IEnumerable;
                var itr1 = assetEntries.GetEnumerator();
                while (itr1.MoveNext())
                {
                    var assetEntry = itr1.Current;
                    BuildSubEntriesRecursively(assetEntry);
                }
            }
        }
        
        private object GetProperty(object obj, string name, BindingFlags flags)
        {
            return obj.GetType().InvokeMember(name, BindingFlags.GetProperty | flags, null, obj, null);
        }

        private object InvokeMethod(object obj, string name, BindingFlags flags)
        {
            return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod | flags, null, obj, null);
        }

        private object GetStaticProperty(Type type, string name)
        {
            return type.InvokeMember(name, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static, null, null, null);
        }

        private void GenerateSubEntries(object assetEntry)
        {
            InvokeMethod(assetEntry, "GenerateSubEntries", BindingFlags.Public | BindingFlags.Instance);
        }

        private void BuildSubEntriesRecursively(object parentEntry)
        {
            GenerateSubEntries(parentEntry);
            var subEntries =
                GetProperty(parentEntry, "SubEntries", BindingFlags.Instance | BindingFlags.Public) as IEnumerable;
            var itr = subEntries.GetEnumerator();
            while (itr.MoveNext())
            {
                var assetEntry = itr.Current;
                BuildSubEntriesRecursively(assetEntry);
            }
        }

        private bool ValidateAssetPath(object assetEntry, string assetPath)
        {
            var value = GetProperty(assetEntry, "IsFolder", BindingFlags.Instance | BindingFlags.Public);
            bool isFolder = value is bool ? (bool)value : false;
            if (isFolder)
            {
                return false;
            }
            string entryAssetPath = GetProperty(assetEntry, "AssetPath", BindingFlags.Instance | BindingFlags.Public) as string;
            return assetPath.Equals(entryAssetPath);
        }

        private bool ValidateAssetPathRecursively(object parentEntry, string assetPath)
        {
            if (ValidateAssetPath(parentEntry, assetPath))
            {
                return true;
            }
            var subEntries =
                GetProperty(parentEntry, "SubEntries", BindingFlags.Instance | BindingFlags.Public) as IEnumerable;
            var itr = subEntries.GetEnumerator();
            while (itr.MoveNext())
            {
                var assetEntry = itr.Current;
                if (ValidateAssetPathRecursively(assetEntry, assetPath))
                {
                    return true;
                }
            }
            return false;
        }
        
        

        private bool IsAssetPathValid(string assetPath)
        {
            var itr = m_AssetSets.GetEnumerator();
            while (itr.MoveNext())
            {
                var assetSet = itr.Current;
                var assetEntries =
                    GetProperty(assetSet, "Entries", BindingFlags.Instance | BindingFlags.Public) as IEnumerable;
                var itr1 = assetEntries.GetEnumerator();
                while (itr1.MoveNext())
                {
                    var assetEntry = itr1.Current;
                    if (ValidateAssetPathRecursively(assetEntry, assetPath))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void LoadAssetAsync(string assetPath, Action<AssetHandle> onCompleted, bool weak = false)
        {
            if (!IsAssetPathValid(assetPath))
            {
                throw new Exception($"asset path [{assetPath}] is invalid. Please config your asset path in asset bundle browser");
            }
            AssetLoader assetLoader = AssetLoader.Acquire();
            AssetHandle assetHandle = new AssetHandle(assetLoader);
            assetHandle.Completed += onCompleted;
            assetLoader.Init(assetHandle, assetPath);
            assetLoader.Result = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            assetLoader.OnFakeCompleted();
        }
    }
}
#endif