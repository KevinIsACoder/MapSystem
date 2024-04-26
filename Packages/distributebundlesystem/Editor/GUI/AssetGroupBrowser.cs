using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public class AssetGroupBrowser : EditorWindow
    {
        [SerializeField]
        internal AssetGroupEditor m_AssetGroupEditor;

        private static AssetGroupBrowser s_AssetGroupBrowser;

        [MenuItem("AdvancedBundleSystem/Asset Group Hierarchy", false, 2)]
        internal static void Init()
        {
            s_AssetGroupBrowser = GetWindow<AssetGroupBrowser>();
            s_AssetGroupBrowser.titleContent = new GUIContent("Asset Group Hierarchy");
            s_AssetGroupBrowser.Show();
        }

        internal static void OnCreateNoneRootAssetGroup(AssetGroup parentCatalog)
        {
            Rect pos = s_AssetGroupBrowser.position;
            pos.x += 50;
            pos.y += 50;
            pos.width = 600;
            pos.height = 400;
            AssetGroupCreatorEditor.Init(pos, false, parentCatalog);
        }

        private void OnCreateRootAssetGroup()
        {
            Rect pos = s_AssetGroupBrowser.position;
            pos.x += 50;
            pos.y += 50;
            pos.width = 600;
            pos.height = 400;
            AssetGroupCreatorEditor.Init(pos, true);
        }
        private void OnTopToolBar()
        {
            //TODO:macdeng
            GUILayout.BeginArea(new Rect(0, 0, 200, 25));
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    //{
                    //    GUIContent guiContent = new GUIContent("Create");
                    //    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.toolbarDropDown);
                    //    if (EditorGUI.DropdownButton(rect, guiContent, FocusType.Passive, EditorStyles.toolbarDropDown))
                    //    {
                    //        GenericMenu menu = new GenericMenu();
                    //        menu.AddItem(new GUIContent("New Asset Group"), false, OnCreateNewAssetGroup);
                    //        menu.DropDown(rect);
                    //    }
                    //}
                    //GUILayout.FlexibleSpace();
                    //{
                    //    GUIContent guiContent = new GUIContent("Build");
                    //    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.toolbarDropDown);
                    //    if (EditorGUI.DropdownButton(rect, guiContent, FocusType.Passive, EditorStyles.toolbarDropDown))
                    //    {
                    //        GenericMenu menu = new GenericMenu();
                    //        menu.AddItem(new GUIContent("New Build"), false, OnBuildAssetBundle);
                    //        menu.AddItem(new GUIContent("Clear Build Cache"), false, OnClearBuildCache);
                    //        menu.DropDown(rect);
                    //    }
                    //}
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
        public void OnGUI()
        {
            if (AdvancedBundleSystemSettingsDefaultObject.Settings == null)
            {
                GUILayout.Space(50);
                if (GUILayout.Button("Create Distrbute Bundle System Settings"))
                {
                    AdvancedBundleSystemSettingsDefaultObject.Settings = AdvancedBundleSystemSettings.Create(AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigFolder, AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigAssetName);
                }
            }
            else if(AdvancedBundleSystemSettingsDefaultObject.Settings.RootAssetGroup == null)
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
                if (m_AssetGroupEditor == null)
                {
                    m_AssetGroupEditor = new AssetGroupEditor();
                }

                if (m_AssetGroupEditor.OnGUI(groupRect))
                {
                    Repaint();
                }
            }
        }
    }

}
