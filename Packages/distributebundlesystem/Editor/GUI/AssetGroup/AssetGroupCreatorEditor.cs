using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.GUI
{
    public class AssetGroupCreatorEditor : EditorWindow
    {
        private int m_GroupId;

        private bool m_CreateRootGroup;
        public bool CreateRootGroup
        {
            get
            {
                return m_CreateRootGroup;
            }
            set
            {
                m_CreateRootGroup = value;
            }
        }

        private AssetGroup m_ParentGroup;
        public AssetGroup ParentGroup
        {
            get
            {
                return m_ParentGroup;
            }
            set
            {
                m_ParentGroup = value;
            }
        }

        internal static void Init(Rect pos, bool createRootGroup, AssetGroup parentGroup = null)
        {
            AssetGroupCreatorEditor window = GetWindow<AssetGroupCreatorEditor>();
            window.position = pos;
            window.CreateRootGroup = createRootGroup;
            window.ParentGroup = parentGroup;
            window.titleContent = new GUIContent("Asset Group Creator");
            window.Show();
        }


        public void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    GUIContent guiContent = new GUIContent(CreateRootGroup ? "Root Group Id:" : "Group Id:");
                    Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.numberField);
                    m_GroupId = EditorGUI.IntField(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, m_GroupId);
                }
                GUILayout.EndHorizontal();

                if (CreateRootGroup)
                {
                    GUILayout.Space(20);
                }
                else
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20);
                        GUIContent guiContent = new GUIContent("Parent Group:");
                        Rect rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.numberField);
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.IntField(new Rect(rect.x, rect.y, rect.width - 20, rect.height), guiContent, ParentGroup.GroupId);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(position.width / 2 - 70);
                    if (GUILayout.Button("Create", GUILayout.Width(60)))
                    {
                        AdvancedBundleSystemSettingsDefaultObject.Settings.CreateAssetGroup(m_GroupId, CreateRootGroup, ParentGroup);
                        AssetGroupCreatorEditor window = GetWindow<AssetGroupCreatorEditor>();
                        window.Close();
                    }
                    GUILayout.Space(20);
                    if (GUILayout.Button("Reset", GUILayout.Width(60)))
                    {
                        m_GroupId = -1;
                    }

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

}


