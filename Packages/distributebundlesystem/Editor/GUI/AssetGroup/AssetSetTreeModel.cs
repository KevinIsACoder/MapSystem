using System;
using System.Collections.Generic;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public class AssetSetTreeModel
    {
        private IList<AssetSet> m_AssetSets;
        internal IList<AssetSet> AssetSets
        {
            get 
            {
                return m_AssetSets;
            }
            set
            {
                m_AssetSets = value;
            }
        }
        public event Action OnModelChanged;

        public AssetSetTreeModel(IList<AssetSet> assetSets)
        {
            SetData(assetSets);
        }

        public void SetData(IList<AssetSet> assetSets)
        {
            if (assetSets == null)
                throw new ArgumentNullException("data", "Input data is null. Ensure input is a non-null list.");

            AssetSets = assetSets;
        }
        public void OnChanged()
        {
            OnModelChanged?.Invoke();
        }
    }

}
