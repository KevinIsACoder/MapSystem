using System;
using MapSystem;
using UnityEditor;
using Object = UnityEngine.Object;

namespace MapSystem.Runtime
{
    public class AssetsManager : Singleton<AssetsManager>
    {
        public async void LoadAsset<T>(string assetPath, Action<T> callback = null) where T : Object
        {
    #if UNITY_EDITOR
          var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
          callback?.Invoke(asset);
#else
#endif
        }
        
        public void UnLoadAssets(string assetPath)
        {
            
        }
    }
}