#if DebugABS
using Runtime.AdvancedBundleSystem.Asset;
using Runtime.AdvancedBundleSystem.Debugger.GUI;
using Runtime.AdvancedBundleSystem.Debugger.GUI.Bundle;
using Runtime.AdvancedBundleSystem.Debugger.GUI.Concole;
using Runtime.AdvancedBundleSystem.Debugger.Util;
using System.Collections.Generic;
using UnityEngine;

public class ABSDebugger : MonoBehaviour
{
    private enum EDebuggerPosition
    {
        TopLeft,
        MiddleLeft,
        BottomLeft,
        TopRight,
        MiddleRight,
        BottomRight,
    }

    [SerializeField]
    private EDebuggerPosition m_DebuggerPosition = EDebuggerPosition.TopLeft;
    private Rect windowRect;
    public GUISkin debuggerSkin;
    
    ABSTreeViewItem root;
    Rect debuggerPanelRect;
    bool isDebuggerOpened;
    BundleTreeView m_BundleTreeView;
    ABSLogView m_LogView;
    public Texture img_start_btn;
    public Texture img_title;
    public Texture img_bundle_info;
    public Texture img_system_info;
    public Texture img_profiler;
    public Texture img_concole_logs;
    public Texture img_close_btn;
    private Texture procedure_boarder;
    internal Texture ProcudureMask;
    private Texture procedure_BottomLeft;
    private Texture procedure_BottomLeftBorder;

    public Texture ErrorIcon;
    public Texture WarningIcon;
    public Texture InfoIcon;
    public Texture ErrorGrayIcon;
    public Texture WarningGrayIcon;
    public Texture InfoGrayIcon;
    public Texture ToggleOn;
    public Texture ToggleOff;

    private GUIStyle m_HintMessageStyle;
    private GUIStyle m_FPSCounterStyle;

    private DebuggerPanel m_DebuggerPanel;
    float _windowTitleHeight;
    float gapBetweenAndPanel = 10f;
    int borderWidth = 7;

    public static ABSDebugger Instance;

    private void OnEnable()
    {
        Instance = this;
    }

    #region FPS_COUNTER
    [SerializeField] float fpsMeasurePeriod = 0.5f;
    [SerializeField] private bool showFps = true;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    private void Update()
    {
        if (!showFps)
        {
            return;
        }
        // measure average frames per second
        m_FpsAccumulator++;
        if (Time.time > m_FpsNextPeriod)
        {
            m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
        }
    }

    private void FPSCounter()
    {
        if (Event.current.type == EventType.Repaint)
        {
            if (m_FPSCounterStyle == null)
            {
                m_FPSCounterStyle = debuggerSkin.customStyles[4];
            }
            m_FPSCounterStyle.normal.textColor = new Color(1, 0, 0, 0.5f);
            m_FPSCounterStyle.fontSize = 18;
            m_FPSCounterStyle.Draw(new Rect(Screen.width - 100, 0, 150, 80), $"FPS:{m_CurrentFps.ToString()}", false, false, false, false);
        }
    }
    #endregion
    private void Start()
    {
        ProcudureMask = DebuggerUtils.CreateTexture(new Color(0f, 0f, 0f, 0.35f));
        procedure_boarder = DebuggerUtils.CreateTexture(new Color(0f, 0f, 1f, 0.9f));
        procedure_BottomLeft = DebuggerUtils.CreateTexture(new Color(0.761f, 0.761f, 0.961f, 0.8f));
        procedure_BottomLeftBorder = DebuggerUtils.CreateTexture(new Color(0.5f, 0.5f, 0.5f, 1f));
        //int width = (int)(Screen.width * 0.8);
        //int height = (int)(Screen.height * 0.8);
        //int height = (int)(width * 0.5625f);
        int width = 1536;
        int height = 864;
        int x = (Screen.width - width) / 2;
        int y = (Screen.height - height) / 2;
        windowRect = new Rect(x, y, width, height);
        _windowTitleHeight = 110 * 0.8f * Screen.width / 1920;
        ABSTreeViewColumn[] columns = new ABSTreeViewColumn[]
        {
            new ABSTreeViewColumn
            {
                headerContent = "Bundle Name \\ Asset Name",
                width = 600f,
                widthMax = 800f,
                widthMin = 300f
            },
            new ABSTreeViewColumn
            {
                headerContent = "Ref Instances \\ Asset Path",
                width = 600f,
                widthMax = 1500f,
                widthMin = 300f
            }
        };

        root = new ABSTreeViewItem(-1, -1);
        List<ABSTreeViewItem> items = TreeToList(root);
        m_BundleTreeView = new BundleTreeView(columns, items);
        m_LogView = new ABSLogView(this);

        debuggerPanelRect = new Rect(288, _windowTitleHeight + gapBetweenAndPanel, width - 288 - borderWidth, height - _windowTitleHeight - gapBetweenAndPanel - borderWidth);

        isDebuggerOpened = false;
        switch (m_DebuggerPosition)
        {
            case EDebuggerPosition.TopLeft:
                debuggerButtonRect = new Rect(0, 0, 302, 164);
                break;
            case EDebuggerPosition.MiddleLeft:
                debuggerButtonRect = new Rect(0, Screen.height * 0.5f - 164 * 0.5f, 302, 164);
                break;
            case EDebuggerPosition.BottomLeft:
                debuggerButtonRect = new Rect(0, Screen.height - 164, 302, 164);
                break;
            case EDebuggerPosition.TopRight:
                debuggerButtonRect = new Rect(Screen.width - 302, 0, 302, 164);
                break;
            case EDebuggerPosition.MiddleRight:
                debuggerButtonRect = new Rect(Screen.width - 302, Screen.height / 2 - 164 / 2, 302, 164);
                break;
            case EDebuggerPosition.BottomRight:
                debuggerButtonRect = new Rect(Screen.width - 302, Screen.height - 164, 302, 164);
                break;
        }
        m_DebuggerPanel = DebuggerPanel.BundleInfo;
    }

    private Dictionary<AssetBundleCache, ABSTreeViewItem> m_bundleCache2bundleViewItemMap = new Dictionary<AssetBundleCache, ABSTreeViewItem>();
    public void OnAddBundle(AssetBundleCache bundleCache)
    {
        string bundleName = bundleCache.BundleName;
        ABSTreeViewItem bundleItem = new BundleTreeViewItem(bundleName.GetHashCode(), 0, bundleName, true, false)
        {
            bundleCahce = bundleCache
        };
        m_bundleCache2bundleViewItemMap.Add(bundleCache, bundleItem);
        string[] assets = bundleCache.AssetBundle.GetAllAssetNames();
        for(int i = 0; i < assets.Length; i++)
        {
            string assetName = assets[i];
            string[] splitted = assetName.Split('/');
            string assetShortName = splitted[splitted.Length-1];
            ABSTreeViewItem assetItem = new BundleTreeViewItem(assetName.GetHashCode(), 1, assetShortName, false, false)
            {
                path = assetName
            };
            assetItem.parent = bundleItem;
            bundleItem.AddChild(assetItem);
        }
        bundleItem.parent = root;
        root.AddChild(bundleItem);
        m_BundleTreeView.Reload(TreeToList(root));
    }

    public void OnRemoveBundle(AssetBundleCache bundleCache)
    {
        if(m_bundleCache2bundleViewItemMap.TryGetValue(bundleCache, out ABSTreeViewItem item))
        {
            root.RemoveChild(item);
            m_BundleTreeView.Reload(TreeToList(root));
            m_bundleCache2bundleViewItemMap.Remove(bundleCache);
        }
    }

    private void TreeToListRecursively(ABSTreeViewItem item, List<ABSTreeViewItem> itemList)
    {
        itemList.Add(item);
        if (item.hasChildren)
        {
            foreach(BundleTreeViewItem child in item.children)
            {
                TreeToListRecursively(child, itemList);
            }
        }
    }
    private List<ABSTreeViewItem> TreeToList(ABSTreeViewItem root)
    {
        List<ABSTreeViewItem> itemList = new List<ABSTreeViewItem>();//TODO:macdeng to be optimized
        if (root.hasChildren)
        {
            foreach(BundleTreeViewItem item in root.children)
            {
                TreeToListRecursively(item, itemList);
            }
        }
        return itemList;
    }

    private bool buttonDragging = false;
    private Rect debuggerButtonRect;

    
    void OnGUI()
    {
        if (!isDebuggerOpened)
        {
            if (debuggerButtonRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    buttonDragging = true;
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                buttonDragging = false;
            }
            if (buttonDragging && Event.current.type == EventType.MouseDrag)
            {
                debuggerButtonRect.x += Event.current.delta.x;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                debuggerButtonRect.y += Event.current.delta.y;
#else
                debuggerButtonRect.y -= Event.current.delta.y;
#endif
                debuggerButtonRect.x = Mathf.Clamp(debuggerButtonRect.x, 0f, Screen.width - debuggerButtonRect.width);
                debuggerButtonRect.y = Mathf.Clamp(debuggerButtonRect.y, 0f, Screen.height - debuggerButtonRect.height);
            }
            if (GUI.Button(debuggerButtonRect, img_start_btn, GUI.skin.customStyles[0]))
            {
                isDebuggerOpened = true;
            }
            if (m_ErrorHintOn)
            {
                PlayErrorHintAnimation();
            }
        }
        else
        {
            windowRect = GUI.Window(0, windowRect, WindowFunction, "", GUI.skin.customStyles[1]);
        }

        if (showFps)
        {
            FPSCounter();
        }
    }

    

    private void PlayErrorHintAnimation()
    {
        float iconSize = debuggerButtonRect.height * 0.25f;
        if (m_Blink)
        {
            if(++m_ErrorHintBlinkElapsedTime > c_ErrorHintBlinkDuration)
            {
                m_Blink = false;
            }
            
            if (m_Appearred)
            {
                GUI.DrawTexture(new Rect(debuggerButtonRect.x + debuggerButtonRect.width - iconSize, debuggerButtonRect.y, iconSize, iconSize), ErrorIcon);
                if (++m_ErrorHintBlinkAppearElapsedTime > c_ErrorHintBlinkAppearInternal)
                {
                    m_Appearred = false;
                    m_ErrorHintBlinkAppearElapsedTime = 0;
                }
            }
            else
            {
                if (++m_ErrorHintBlinkDisappearElapsedTime > c_ErrorHintBlinkDisappearInternal)
                {
                    m_Appearred = true;
                    m_ErrorHintBlinkDisappearElapsedTime = 0;
                }
            }
        }
        else
        {
            GUI.DrawTexture(new Rect(debuggerButtonRect.x + debuggerButtonRect.width - iconSize, debuggerButtonRect.y, iconSize, iconSize), ErrorIcon);
        }
    }

    enum DebuggerPanel
    {
        SystemInfo = 0,
        BundleInfo,
        Profiler,
        Concole
    }
    void WindowFunction(int windowID)
    {
        GUI.skin = debuggerSkin;
        GUI.DrawTexture(new Rect(borderWidth, borderWidth, windowRect.width - borderWidth * 2, _windowTitleHeight), img_title);
        GUI.DrawTexture(new Rect(0, 0, windowRect.width, borderWidth), procedure_boarder);//TOP
        GUI.DrawTexture(new Rect(0, borderWidth, borderWidth, windowRect.height - borderWidth), procedure_boarder);//Left
        GUI.DrawTexture(new Rect(borderWidth, windowRect.height - borderWidth, windowRect.width - borderWidth, borderWidth), procedure_boarder);//Bottom
        GUI.DrawTexture(new Rect(windowRect.width - borderWidth, borderWidth, borderWidth, windowRect.height - 2 * borderWidth), procedure_boarder);//Right
        if (GUI.Button(new Rect(windowRect.width - 80, 8, 72, 72), img_close_btn, GUI.skin.customStyles[0]))
        {
            isDebuggerOpened = false;
        }

        float panelButtonHeight = 120.8f;
        float panelButtonWidth = 276f;
        float panelButtonGap = 120.8f;
        
        if(GUI.Button(new Rect(10, gapBetweenAndPanel + _windowTitleHeight, panelButtonWidth, panelButtonHeight), img_system_info, GUI.skin.customStyles[0]))
        {
            m_DebuggerPanel = DebuggerPanel.SystemInfo;
        }
        else if(GUI.Button(new Rect(10, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap, panelButtonWidth, panelButtonHeight), img_bundle_info, GUI.skin.customStyles[0]))
        {
            m_DebuggerPanel = DebuggerPanel.BundleInfo;
            
        }
        else if (GUI.Button(new Rect(10, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap * 2, panelButtonWidth, panelButtonHeight), img_profiler, GUI.skin.customStyles[0]))
        {
            m_DebuggerPanel = DebuggerPanel.Profiler;
        }
        else if (GUI.Button(new Rect(10, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap * 3, panelButtonWidth, panelButtonHeight), img_concole_logs, GUI.skin.customStyles[0]))
        {
            m_DebuggerPanel = DebuggerPanel.Concole;
        }
        switch (m_DebuggerPanel)
        {
            case DebuggerPanel.SystemInfo:
                GUI.DrawTexture(new Rect(10, gapBetweenAndPanel + _windowTitleHeight, panelButtonWidth, panelButtonHeight), ProcudureMask);
                break;
            case DebuggerPanel.BundleInfo:
                GUI.DrawTexture(new Rect(10, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap, panelButtonWidth, panelButtonHeight), ProcudureMask);
                m_BundleTreeView.OnGUI(debuggerPanelRect);
                break;
            case DebuggerPanel.Profiler:
                GUI.DrawTexture(new Rect(10, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap * 2, panelButtonWidth, panelButtonHeight), ProcudureMask);
                break;
            case DebuggerPanel.Concole:
                GUI.DrawTexture(new Rect(10, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap * 3, panelButtonWidth, panelButtonHeight), ProcudureMask);
                m_LogView.OnGUI(debuggerPanelRect);
                break;
            default:
                break;
        }
        Rect bottomLeftRect = new Rect(borderWidth, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap * 4, panelButtonWidth + borderWidth, windowRect.height - panelButtonGap * 4 - gapBetweenAndPanel - _windowTitleHeight - borderWidth);
        GUI.DrawTexture(bottomLeftRect, procedure_BottomLeft);

        if(Event.current.type == EventType.Repaint && !string.IsNullOrEmpty(m_HintMessage))
        {
            if(--m_HintMessageDuration < 0)
            {
                m_HintMessage = null;
                m_HintMessageOn = false;
            }
            bottomLeftRect.x += 10;
            bottomLeftRect.y += 10;
            if(m_HintMessageStyle == null)
            {
                m_HintMessageStyle = GUI.skin.customStyles[3];
            }
            m_HintMessageStyle.normal.textColor = m_HintColor;
            m_HintMessageStyle.Draw(bottomLeftRect, m_HintMessage, false, false, false, false);
        }
        GUI.DrawTexture(new Rect(panelButtonWidth + borderWidth + 2, gapBetweenAndPanel + _windowTitleHeight + panelButtonGap * 4, 3, windowRect.height - panelButtonGap * 4 - gapBetweenAndPanel - _windowTitleHeight - borderWidth), procedure_BottomLeftBorder);
        GUI.DragWindow(new Rect(0, 0, windowRect.width, _windowTitleHeight));//TOP
        GUI.DragWindow(new Rect(0, windowRect.height - _windowTitleHeight, windowRect.width, _windowTitleHeight));//Buttom
        GUI.DragWindow(new Rect(0, _windowTitleHeight, _windowTitleHeight, windowRect.height - _windowTitleHeight * 2));//Left
        GUI.DragWindow(new Rect(windowRect.width - _windowTitleHeight, _windowTitleHeight, _windowTitleHeight, windowRect.height - _windowTitleHeight * 2));//Right
    }

    private string m_HintMessage = null;
    private bool m_HintMessageOn = false;
    private int m_HintMessageDuration = -1;//in frame
    private Color m_HintColor = Color.green;

    internal void ShowHintMessage(string message, Color hintColor, int duration = 300)
    {
        m_HintMessageDuration = duration;
        if(m_HintMessage != message)
        {
            m_HintMessage = message;
        }
        m_HintColor = hintColor;
        if (!m_HintMessageOn)
        {
            m_HintMessageOn = true;
        }
    }

    private bool m_ErrorHintOn = false;
    private const int c_ErrorHintBlinkAppearInternal = 30; //in frame
    private const int c_ErrorHintBlinkDisappearInternal = 30; //in frame
    private const int c_ErrorHintBlinkDuration = 600; //in frame
    private int m_ErrorHintBlinkAppearElapsedTime = 0; //in frame
    private int m_ErrorHintBlinkDisappearElapsedTime = 0; //in frame
    private int m_ErrorHintBlinkElapsedTime = 0;
    private bool m_Appearred = false;
    private bool m_Blink = false;
    internal void ShowErrorHint()
    {
        if (!m_ErrorHintOn)
        {
            m_ErrorHintOn = true;
            m_ErrorHintBlinkAppearElapsedTime = 0;
            m_ErrorHintBlinkDisappearElapsedTime = 0;
            m_ErrorHintBlinkElapsedTime = 0;
            m_Appearred = true;
            m_Blink = true;
        }
        if (isDebuggerOpened)
        {
            ShowHintMessage("Error message received!", Color.red);
        }
    }

    internal void ClearErrorHint()
    {
        if (m_ErrorHintOn)
        {
            m_ErrorHintOn = false;
            m_Appearred = false;
            m_Blink = false;
        }
    }

    

    
}
#endif