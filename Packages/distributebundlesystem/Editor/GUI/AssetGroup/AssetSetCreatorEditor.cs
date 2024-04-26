using System.Collections.Generic;
using UnityEditor.AdvancedBundleSystem.GUI;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public class AssetSetCreatorEditor : EditorWindow
    {
        private string m_AssetSetName;
        private bool m_CompressLocalCache;
        private bool m_InPackage;
        private BundleCompression m_BundleCompression = BundleCompression.LZMA;
        private PackingMode m_PackingMode = PackingMode.Together;

        private AdvancedBundleSystemSettings m_Settings;

        internal static void Init(Rect pos)
        {
            
            AssetSetCreatorEditor window = GetWindow<AssetSetCreatorEditor>();
            window.titleContent = new GUIContent("Asset Set Creator");
            window.position = pos;
            window.m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
            window.FetchAssetGroupList();
            window.Show();
        }


        string[] m_AssetGroupIdList;
        AssetGroup[] m_AssetGroupList;
        private int m_AssetGroupIndex;
        private void FetchAssetGroupList()
        {
            m_Settings.GetAssetGroupList(out List<int> groupIdList, out List<AssetGroup> groupList);
            int len = groupIdList.Count;
            m_AssetGroupIdList = new string[len];
            for(int i = 0; i < len; i++)
            {
                m_AssetGroupIdList[i] = groupIdList[i].ToString();
            }
            m_AssetGroupList = groupList.ToArray();
        }


        public void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent("Asset Set Name:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.textField);
                    m_AssetSetName = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, m_AssetSetName);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent("Asset Group Id:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.popup);
                    m_AssetGroupIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width - 20, rect.height), "Asset Group Id:", m_AssetGroupIndex, m_AssetGroupIdList);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent("Bundle Compression:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.popup);
                    m_BundleCompression = (BundleCompression)EditorGUI.EnumPopup(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, m_BundleCompression);

                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent("Packing Mode:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.popup);
                    m_PackingMode = (PackingMode)EditorGUI.EnumPopup(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, m_PackingMode);

                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent("Compress Local Cache:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.toggle);
                    m_CompressLocalCache = EditorGUI.Toggle(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, m_CompressLocalCache);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent("In Package:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.toggle);
                    m_InPackage = EditorGUI.Toggle(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, m_InPackage);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(position.width / 2 - 70);
                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                    {
                        AdvancedBundleSystemSettingsDefaultObject.Settings.CreateAssetSet(m_AssetSetName, m_AssetGroupList[m_AssetGroupIndex], m_BundleCompression, m_PackingMode, m_CompressLocalCache, m_InPackage);
                        AssetSetCreatorEditor window = GetWindow<AssetSetCreatorEditor>();
                        window.Close();
                    }
                    GUILayout.Space(20);
                    if (GUILayout.Button("Reset", GUILayout.Width(60)))
                    {
                        m_AssetSetName = "";
                        m_CompressLocalCache = true;
                        m_InPackage = true;
                        m_AssetGroupIndex = 0;
                        m_BundleCompression = BundleCompression.LZMA;
                        m_PackingMode = PackingMode.Together;
                    }

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

}
