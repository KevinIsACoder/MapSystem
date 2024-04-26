using System;
using Object = UnityEngine.Object;

namespace Runtime.AdvancedBundleSystem.Asset
{
    public struct AssetHandle
    {
        private AssetLoader m_AssetLoader;
        private AssetLoader AssetLoader
        {
            get
            {
                if(m_AssetLoader == null)
                {
                    throw new Exception("Invalid asset handle: AssetLoader is null");
                }
                if (m_AssetLoader.IsDestroyed)
                {
                    throw new Exception("Invalid asset handle: AssetLoader is destroyed");
                }
                return m_AssetLoader;
            }
        }

        internal AssetHandle(AssetLoader assetLoader)
        {
            m_AssetLoader = assetLoader;
        }

        public Object Result => AssetLoader.Result;

        public void Release()
        {
            AssetLoader.OnDestroy();
            m_AssetLoader = null;
        }

        public event Action<AssetHandle> Completed
        {
            add => AssetLoader.Completed += value;
            remove => AssetLoader.Completed -= value;
        }
    }
}
