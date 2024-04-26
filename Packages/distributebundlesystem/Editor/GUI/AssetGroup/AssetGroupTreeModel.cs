using System;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public class AssetGroupTreeModel
    {
        private AssetGroup m_RootAssetGroup;
        internal AssetGroup RootAssetGroup
        {
            get
            {
                return m_RootAssetGroup;
            }
            set
            {
                m_RootAssetGroup = value;
            }
        }
        public event Action OnModelChanged;

        public AssetGroupTreeModel(AssetGroup rootAssetGroup)
        {
            SetData(rootAssetGroup);
        }

        public void SetData(AssetGroup rootAssetGroup)
        {
            if (rootAssetGroup == null)
                throw new ArgumentNullException("data", "Input data is null. Ensure input is a non-null value.");

            RootAssetGroup = rootAssetGroup;
        }
        public void OnChanged()
        {
            OnModelChanged?.Invoke();
        }
    }

}
