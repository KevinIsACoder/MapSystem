using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.Settings
{
    public class AdvancedBundleSystemSettingsDefaultObject : ScriptableObject
    {
        private const string c_DefaultConfigObjectName = "com.macdeng.distributebundlesystem";
        public const string c_DefaultConfigFolder = "Assets/ABS_Data";
        public const string c_DefaultConfigAssetName = "ABS_Settings";
        private static AdvancedBundleSystemSettings s_DefaultSettingsObject;

        [SerializeField]
        internal string m_ABS_SettingsGuid;

        public static AdvancedBundleSystemSettings Settings
        {
            get 
            {
                if(s_DefaultSettingsObject == null)
                {
                    AdvancedBundleSystemSettingsDefaultObject so;
                    if(EditorBuildSettings.TryGetConfigObject(c_DefaultConfigObjectName, out so))
                    {
                        s_DefaultSettingsObject = so.LoadSettingsObject();
                    }
                }
                return s_DefaultSettingsObject;
            }
            set
            {
                s_DefaultSettingsObject = value;
                AdvancedBundleSystemSettingsDefaultObject so;
                if(!EditorBuildSettings.TryGetConfigObject(c_DefaultConfigObjectName, out so))
                {
                    so = CreateInstance<AdvancedBundleSystemSettingsDefaultObject>();
                    AssetDatabase.CreateAsset(so, c_DefaultConfigFolder + "/DefaultObject.asset");
                    AssetDatabase.SaveAssets();
                    EditorBuildSettings.AddConfigObject(c_DefaultConfigObjectName, so, true);
                }
                so.SetSettingsObject(s_DefaultSettingsObject);
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();

            }
        }

        internal AdvancedBundleSystemSettings LoadSettingsObject()
        {
            string path = AssetDatabase.GUIDToAssetPath(m_ABS_SettingsGuid);
            AdvancedBundleSystemSettings settings = AssetDatabase.LoadAssetAtPath<AdvancedBundleSystemSettings>(path);
            
            return settings;
        }

        internal void SetSettingsObject(AdvancedBundleSystemSettings settings)
        {
            if (settings == null)
            {
                m_ABS_SettingsGuid = null;
                return;
            }
            string path = AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogErrorFormat("invalid settings object path: {0}.", path);
                return;
            }
            m_ABS_SettingsGuid = AssetDatabase.AssetPathToGUID(path);
        }

    }

}
