using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MapSystem
{
    public class AssetsManager : Singleton<AssetsManager>
    {
        public delegate void Test();
        public event Test TestEvent;

        private Test testdelegate;
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
            TestEvent.Invoke();
            testdelegate += TestData;
        }

        void TestData()
        {
            
        }
    }
}