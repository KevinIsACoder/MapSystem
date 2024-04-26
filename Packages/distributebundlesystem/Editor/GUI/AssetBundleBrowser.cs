using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.AdvancedBundleSystem.Build;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEditor.AdvancedBundleSystem.Utils;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public class AssetBundleBrowser : EditorWindow
    {
        [SerializeField]
        internal AssetSetEditor m_AssetSetEditor;

        private static AssetBundleBrowser s_AssetBundleBrowser;

        [MenuItem("AdvancedBundleSystem/AssetBundle Browser", false, 1)]
        internal static void Init()
        {
            s_AssetBundleBrowser = GetWindow<AssetBundleBrowser>();
            s_AssetBundleBrowser.titleContent = new GUIContent("AssetBundle Browser");
            s_AssetBundleBrowser.Show();
        }

        private void OnCreateNewAssetSet()
        {
            Rect pos = s_AssetBundleBrowser.position;
            pos.x += 50;
            pos.y += 50;
            pos.width = 600;
            pos.height = 400;
            AssetSetCreatorEditor.Init(pos);
        }

        private void OnCreateRootAssetGroup()
        {
            Rect pos = s_AssetBundleBrowser.position;
            pos.x += 50;
            pos.y += 50;
            pos.width = 600;
            pos.height = 400;
            AssetGroupCreatorEditor.Init(pos, true);
        }

        private void OnBuildAssetBundle()
        {
            AssetBundleBuilder.BuildAssetBundle();
            EditorUtils.EncryptAllBundlesToStreamingAssets();
        }

        private void OnClearBuildCache()
        {
            BuildCache.PurgeCache(true);
        }

        private void OnTopToolBar()
        {
            GUILayout.BeginArea(new Rect(0, 0, 200, 25));
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    {
                        GUIContent guiContent = new GUIContent("Create");
                        Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.toolbarDropDown);
                        if (EditorGUI.DropdownButton(rect, guiContent, FocusType.Passive, EditorStyles.toolbarDropDown))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("New Asset Set"), false, OnCreateNewAssetSet);
                            menu.DropDown(rect);
                        }
                    }
                    GUILayout.FlexibleSpace();
                    {
                        GUIContent guiContent = new GUIContent("Build");
                        Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.toolbarDropDown);
                        if (EditorGUI.DropdownButton(rect, guiContent, FocusType.Passive, EditorStyles.toolbarDropDown))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("New Build"), false, OnBuildAssetBundle);
                            menu.AddItem(new GUIContent("Clear Build Cache"), false, OnClearBuildCache);
                            menu.DropDown(rect);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
        public void OnGUI()
        {
            if(AdvancedBundleSystemSettingsDefaultObject.Settings == null)
            {
                GUILayout.Space(50);
                if (GUILayout.Button("Create Advanced Bundle System Settings"))
                {
                    AdvancedBundleSystemSettingsDefaultObject.Settings = AdvancedBundleSystemSettings.Create(AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigFolder, AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigAssetName);
                }
            }
            else if (AdvancedBundleSystemSettingsDefaultObject.Settings.RootAssetGroup == null)
            {
                GUILayout.Space(50);
                if (GUILayout.Button("Create Root Asset Group"))
                {
                    OnCreateRootAssetGroup();
                }
            }
            else
            {
                OnTopToolBar();
                Rect groupRect = new Rect(0, 25, position.width, position.height - 25);
                if(m_AssetSetEditor == null)
                {
                    m_AssetSetEditor = new AssetSetEditor();
                }

                if (m_AssetSetEditor.OnGUI(groupRect))
                {
                    Repaint();
                }
            }
        }
    }

}
