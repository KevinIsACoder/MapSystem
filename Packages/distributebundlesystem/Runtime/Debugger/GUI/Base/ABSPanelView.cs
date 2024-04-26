#if DebugABS
using Runtime.AdvancedBundleSystem.Debugger.Util;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI
{
    using GUI = UnityEngine.GUI;
    public abstract class ABSPanelView
    {
        private Texture m_ProcedurePanelBG;
        protected Texture m_ProcedureItemBG;
        protected Texture m_ProcedureItemBgSelected;
        protected Texture m_ProcudureBoarder;
        public ABSPanelView()
        {

            m_ProcedurePanelBG = DebuggerUtils.CreateTexture(new Color(0.761f, 0.761f, 0.961f, 0.8f));
            m_ProcedureItemBG = DebuggerUtils.CreateTexture(new Color(0.792f, 0.792f, 0.992f, 0.8f));
            m_ProcedureItemBgSelected = DebuggerUtils.CreateTexture(new Color(0f, 0f, 1f, 1.0f));
            m_ProcudureBoarder = DebuggerUtils.CreateTexture(new Color(0.5f, 0.5f, 0.5f, 1.0f));
        }

        private GUIStyle m_RowGUIStyle;
        protected GUIStyle RowGUIStyle
        {
            get
            {
                return m_RowGUIStyle;
            }
            set
            {
                m_RowGUIStyle = value;
            }
        }
        public virtual void OnGUI(Rect rect)
        {
            if (RowGUIStyle == null)
            {
                RowGUIStyle = GUI.skin.customStyles[2];
            }
            GUI.DrawTexture(rect, m_ProcedurePanelBG);
        }
    }

}
#endif