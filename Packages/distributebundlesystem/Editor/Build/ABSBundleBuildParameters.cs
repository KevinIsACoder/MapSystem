using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Content;
using BuildCompression = UnityEngine.BuildCompression;
using UnityEditor.AdvancedBundleSystem.Settings;
using System.Collections.Generic;
using UnityEditor.AdvancedBundleSystem.GUI;

namespace UnityEditor.AdvancedBundleSystem.Build
{
    public class ABSBundleBuildParameters : BundleBuildParameters
    {
        private Dictionary<string, string> m_BundleIdentifier2GroupName;
        private AdvancedBundleSystemSettings m_Settings;
        public ABSBundleBuildParameters(AdvancedBundleSystemSettings settings, Dictionary<string, string>  bundleIdentifier2GroupName, BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
        {
            m_Settings = settings;
            m_BundleIdentifier2GroupName = bundleIdentifier2GroupName;
            BundleCompression = BuildCompression.LZMA;
            ContentBuildFlags |= ContentBuildFlags.StripUnityVersion;
        }

        public override BuildCompression GetCompressionForIdentifier(string identifier)
        {
            if(m_BundleIdentifier2GroupName.TryGetValue(identifier, out string groupName))
            {
                AssetSet group = m_Settings.FindAssetSet(groupName);
                if (group != null)
                {
                    return group.GetBuildCompressionForBundle();
                }
            }
            return base.GetCompressionForIdentifier(identifier);
        }

    }
}
