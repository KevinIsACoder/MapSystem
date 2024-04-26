#if DebugABS
using Runtime.AdvancedBundleSystem.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI.Concole
{
    using GUI = UnityEngine.GUI;
    public class ABSLogView : ABSPanelView
    {
        private int m_LogTypeFilter = 0;
        private int m_InfoFilterMask = 1;
        private int m_WarningFilterMask = 1 << 1;
        private int m_ErrorFilterMask = 1 << 2;

        private string m_ContentFilter = "";
        private bool m_OnlyABSLog = false;

        private ABSDebugger m_ABSDebugger;


        public ABSLogView(ABSDebugger absDebugger) : base()
        {
            m_ABSDebugger = absDebugger;
            if(m_Items == null)
            {
                m_Items = new List<ABSLogViewItem>(3000);
            }
            Application.logMessageReceived += OnLogReceived;
            FilterAllLogType();
        }


        private bool IsLogTypeFiltered(LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    return (m_LogTypeFilter & m_InfoFilterMask) != 0;
                case LogType.Warning:
                    return (m_LogTypeFilter & m_WarningFilterMask) != 0;
                case LogType.Error:
                    return (m_LogTypeFilter & m_ErrorFilterMask) != 0;
                case LogType.Exception:
                    return (m_LogTypeFilter & m_ErrorFilterMask) != 0;
                case LogType.Assert:
                    return (m_LogTypeFilter & m_ErrorFilterMask) != 0;
                default:
                    return true;
            }
        }

        private void FilterAllLogType()
        {
            m_LogTypeFilter |= 1;//info
            m_LogTypeFilter |= (1 << 1);//warning
            m_LogTypeFilter |= (1 << 2);//error
        }
        public void FilterByLogType(LogType logType, bool filter)
        {
            switch (logType)
            {
                case LogType.Log:
                    if (filter)
                    {
                        m_LogTypeFilter |= 1;
                    }
                    else
                    {
                        m_LogTypeFilter &= (~1);
                    }
                    break;
                case LogType.Warning:
                    if (filter)
                    {
                        m_LogTypeFilter |= (1 << 1);
                    }
                    else
                    {
                        m_LogTypeFilter &= (~(1 << 1));
                    }
                    break;
                case LogType.Error:
                    if (filter)
                    {
                        m_LogTypeFilter |= (1 << 2);
                    }
                    else
                    {
                        m_LogTypeFilter &= (~(1 << 2));
                    }
                    break;
                case LogType.Exception:
                    if (filter)
                    {
                        m_LogTypeFilter |= (1 << 2);
                    }
                    else
                    {
                        m_LogTypeFilter &= (~(1 << 2));
                    }
                    break;
                case LogType.Assert:
                    if (filter)
                    {
                        m_LogTypeFilter |= (1 << 2);
                    }
                    else
                    {
                        m_LogTypeFilter &= (~(1 << 2));
                    }
                    break;
                default:
                    break;
            }
        }



        private List<ABSLogViewItem> m_Items;

        private void AddLogItem(ABSLogViewItem item)
        {
            if(m_Items.Count > 5000)
            {
                m_Items.RemoveAt(0);
            }
            m_Items.Add(item);
        }

        private void OnLogReceived(string condition, string stackTrace, LogType logType)
        {
            AddLogItem(new ABSLogViewItem(System.Guid.NewGuid().GetHashCode(), condition, stackTrace, logType));
            if(logType == LogType.Error || logType == LogType.Assert || logType == LogType.Exception)
            {
                m_ABSDebugger.ShowErrorHint();
            }
        }

        private bool IsSelected(int id)
        {
            return selectedId == id;
        }


        private Vector2 GetTextAreaWidthAndHeight(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Vector2.zero;
            }
            int stringLen = text.Length;
            int maxWidth = 0;
            int maxHeight = 0;
            int currentWidth = 0;
            for(int i = 0; i < stringLen; i++)
            {
                char c = text[i];
                if(c != '\n')
                {
                    ++currentWidth;
                }
                else
                {
                    if(currentWidth > maxWidth)
                    {
                        maxWidth = currentWidth;
                    }
                    currentWidth = 0;
                    ++maxHeight;
                }
            }
            return new Vector2(maxWidth, maxHeight);
        }
        Vector2 logItemsScrollPosition = Vector2.zero;
        Vector2 logDetailsScrollPosition = Vector2.zero;
        float totalHeight;
        int selectedId = -1;
        string selectedLogDetials = "";
        Vector2 selectedLogDetialsSize = Vector2.zero;
        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            GUI.BeginGroup(rect);
            float borderWidth = 1f;
            float topToolBarHeight = rect.height * 0.06f;
            float logItemsHeight = rect.height * 0.6f;
            float logDetailsHeight = rect.height - logItemsHeight - topToolBarHeight - borderWidth * 2;
            Rect topToolBarRect = new Rect(0, borderWidth, rect.width, topToolBarHeight);
            Rect logItemsRect = new Rect(0, borderWidth * 2 + topToolBarHeight, rect.width, logItemsHeight);
            Rect logDetailsRect = new Rect(0, borderWidth * 2 + topToolBarHeight + logItemsHeight, rect.width, logDetailsHeight);
            GUI.DrawTexture(new Rect(0, 0, rect.width, borderWidth), m_ProcudureBoarder);
            

            GUI.DrawTexture(new Rect(0, borderWidth + topToolBarHeight, rect.width, borderWidth), m_ProcudureBoarder);
            GUI.BeginGroup(logItemsRect);
            logItemsScrollPosition = GUI.BeginScrollView(new Rect(0, 0, rect.width, logItemsHeight), logItemsScrollPosition, new Rect(0, 0, rect.width - 28, totalHeight));
            float logRowHeight = 45f;
            int itemCount = m_Items.Count;
            totalHeight = 0f;
            int infoCount = 0;
            int warningCount = 0;
            int errorCount = 0;
            for(int i = 0; i < itemCount; i++)
            {
                ABSLogViewItem item = m_Items[i];
                LogType logType = item.logType;
                if (logType == LogType.Log)
                {
                    ++infoCount;
                }
                else if (logType == LogType.Warning)
                {
                    ++warningCount;
                }
                else
                {
                    ++errorCount;
                }

                if (!IsLogTypeFiltered(logType))
                {
                    continue;
                }
                if(!string.IsNullOrEmpty(m_ContentFilter) && !item.condition.Contains(m_ContentFilter) && !item.stackTrace.Contains(m_ContentFilter))
                {
                    continue;
                }
                if(m_OnlyABSLog && !item.condition.StartsWith(LoggerInternal.MessageHead) && !item.stackTrace.StartsWith(LoggerInternal.MessageHead))
                {
                    continue;
                }
                GUI.DrawTexture(new Rect(4, totalHeight + 4, logRowHeight - 8, logRowHeight - 8), GetLogIcon(logType, 1));
                Rect itemRect = new Rect(logRowHeight, totalHeight, rect.width, logRowHeight);
                if (Event.current.type == EventType.Repaint)
                {
                    if (IsSelected(item.id))
                    {
                        GUI.DrawTexture(itemRect, m_ProcedureItemBgSelected);
                    }
                    else if ((i & 1) == 0)
                    {
                        GUI.DrawTexture(itemRect, m_ProcedureItemBG);
                    }
                    RowGUIStyle.Draw(itemRect, item.condition, false, false, false, false);
                }
                if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
                {
                    selectedId = item.id;
                    selectedLogDetials = string.Format("{0}\n{1}", string.IsNullOrEmpty(item.condition) ? "empty log message" : item.condition,
                string.IsNullOrEmpty(item.condition) ? "no stack trace" : item.stackTrace);
                    selectedLogDetialsSize = GetTextAreaWidthAndHeight(selectedLogDetials);

                }
                totalHeight += logRowHeight;
            }
            GUI.EndScrollView();
            GUI.EndGroup();
            GUI.DrawTexture(new Rect(0, borderWidth * 2 + topToolBarHeight + logItemsHeight, rect.width, borderWidth), m_ProcudureBoarder);

            GUI.BeginGroup(logDetailsRect);
            if(!string.IsNullOrEmpty(selectedLogDetials) && GUI.Button(new Rect(logDetailsRect.width - 140f, 4f, 100f, 50f), "Copy")){
                CopySelectedLogToClipboard();
            }
            logDetailsScrollPosition = GUI.BeginScrollView(new Rect(0, 0, logDetailsRect.width, logDetailsRect.height), logDetailsScrollPosition, new Rect(0, 0, selectedLogDetialsSize.x * 20f, selectedLogDetialsSize.y * 45f));
            ShowSelectLogDetails(new Rect(0, 0, rect.width, logDetailsHeight));
            GUI.EndScrollView();
            GUI.EndGroup();

            GUI.BeginGroup(topToolBarRect);

            float iconSize = topToolBarHeight - 8f;
            float totalX = topToolBarRect.width;
            float numberWidthInfo = GetWidthsByNumber(infoCount);
            float numberWidthWarning = GetWidthsByNumber(warningCount);
            float numberWidthError = GetWidthsByNumber(errorCount);
            float widthInfo = numberWidthInfo + borderWidth + 8 + topToolBarHeight;
            float widthWarning = numberWidthWarning + borderWidth + 8 + topToolBarHeight;
            float widthError = numberWidthError + borderWidth + 8 + topToolBarHeight;
            totalX -= (widthInfo + widthWarning + widthError);
            OnRenderLogCount(ref totalX, infoCount, LogType.Log, iconSize, topToolBarHeight, borderWidth, numberWidthInfo, widthInfo);
            totalX += widthInfo;
            OnRenderLogCount(ref totalX, warningCount, LogType.Warning, iconSize, topToolBarHeight, borderWidth, numberWidthInfo, widthWarning);
            totalX += widthWarning;
            OnRenderLogCount(ref totalX, errorCount, LogType.Error, iconSize, topToolBarHeight, borderWidth, numberWidthInfo, widthError);

            if (GUI.Button(new Rect(4f, 4f, topToolBarHeight * 3, iconSize), "Clear"))
            {
                ClearLogs();
            }
            GUI.DrawTexture(new Rect(4f + topToolBarHeight * 3 + 10f, 0f, borderWidth, topToolBarHeight), m_ProcudureBoarder);
            GUI.DrawTexture(new Rect(4f + topToolBarHeight * 3 + 40f + iconSize * 8.5f, 0f, borderWidth, topToolBarHeight), m_ProcudureBoarder);
            if (Event.current.type == EventType.Repaint)
            {
                RowGUIStyle.Draw(new Rect(4f + topToolBarHeight * 3 + 20f, 4f, iconSize * 2.5f, iconSize), "Filter:", false, false, false, false);
                RowGUIStyle.Draw(new Rect(4f + topToolBarHeight * 3 + 20f + iconSize * 6 + 122f, 4f, iconSize * 4.0f, iconSize), "Only ABS Logs:", false, false, false, false);
            }

            Rect toggelRect = new Rect(4f + topToolBarHeight * 3 + 20f + iconSize * 10 + 220f, 4f, iconSize * 1.89f, iconSize);
            GUI.DrawTexture(toggelRect, m_OnlyABSLog ? m_ABSDebugger.ToggleOn : m_ABSDebugger.ToggleOff);
            if (Event.current.type == EventType.MouseDown && toggelRect.Contains(Event.current.mousePosition))
            {
                m_OnlyABSLog = !m_OnlyABSLog;
                Event.current.Use();
            }
            m_ContentFilter = GUI.TextField(new Rect(4f + topToolBarHeight * 3 + 30f + iconSize * 2.5f, 4f, iconSize * 6, iconSize), m_ContentFilter);

            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void CopySelectedLogToClipboard()
        {
            GUIUtility.systemCopyBuffer = selectedLogDetials;
            m_ABSDebugger.ShowHintMessage("Successfully copied selected logs to clipboard!", Color.green);
        }

        private Texture GetLogIcon(LogType logType, int count)
        {
            Texture logIcon = null;
            if (logType == LogType.Error)
            {
                if (count > 0)
                {
                    logIcon = m_ABSDebugger.ErrorIcon;
                }
                else
                {
                    logIcon = m_ABSDebugger.ErrorGrayIcon;
                }
            }
            else if (logType == LogType.Warning)
            {
                if (count > 0)
                {
                    logIcon = m_ABSDebugger.WarningIcon;
                }
                else
                {
                    logIcon = m_ABSDebugger.WarningGrayIcon;
                }
            }
            else if (logType == LogType.Log)
            {
                if (count > 0)
                {
                    logIcon = m_ABSDebugger.InfoIcon;
                }
                else
                {
                    logIcon = m_ABSDebugger.InfoGrayIcon;
                }
            }
            return logIcon;
        }
        private void OnRenderLogCount(ref float totalX, int count, LogType logType, float iconSize, float topToolBarHeight, float borderWidth, float numberWidth, float totalWidth)
        {
            GUI.DrawTexture(new Rect(totalX, 0f, borderWidth, topToolBarHeight), m_ProcudureBoarder);
            GUI.DrawTexture(new Rect(totalX + borderWidth, 4f, iconSize, iconSize), GetLogIcon(logType, count));
            if(Event.current.type == EventType.Repaint)
            {
                RowGUIStyle.Draw(new Rect(totalX + borderWidth + 4 + topToolBarHeight, 4, numberWidth, iconSize), GetLogCountString(count, logType), false, false, false, false);
            }
            Rect buttonRect = new Rect(totalX + borderWidth, 0, totalWidth - borderWidth, topToolBarHeight);
            if (!IsLogTypeFiltered(logType))
            {
                GUI.DrawTexture(buttonRect, m_ABSDebugger.ProcudureMask);
            }

            if (Event.current.type == EventType.MouseDown && buttonRect.Contains(Event.current.mousePosition))
            {
                FilterByLogType(logType, !IsLogTypeFiltered(logType));
                Event.current.Use();
            }

        }

        private float GetWidthsByNumber(int number)
        {
            float unit = 20f;
            if(number < 10)
            {
                return unit;
            }
            else if(number >= 10 && number < 100)
            {
                return unit * 2;
            }
            else if(number >= 100 && number < 1000)
            {
                return unit * 3;
            }
            else
            {
                return unit * 4;
            }
        }

        private bool isErrorCountOverflow = false;
        private bool isWarningCountOverflow = false;
        private bool isInfoCountOverflow = false;
        private string GetLogCountString(int count, LogType logType)
        {
            if (logType == LogType.Error && isErrorCountOverflow)
            {
                return "999+";
            }
            if (logType == LogType.Warning && isWarningCountOverflow)
            {
                return "999+";
            }
            if (logType == LogType.Log && isInfoCountOverflow)
            {
                return "999+";
            }

            if (count <= 999)
            {
                return count.ToString();
            }

            if(logType == LogType.Log)
            {
                isInfoCountOverflow = true;
            }
            else if(logType == LogType.Warning)
            {
                isWarningCountOverflow = true;
            }
            else if(logType == LogType.Error)
            {
                isErrorCountOverflow = true;
            }
            isErrorCountOverflow = true;
            return "999+";
        }

        private void ClearLogs()
        {
            m_Items.Clear();
            isInfoCountOverflow = false;
            isWarningCountOverflow = false;
            isErrorCountOverflow = false;
            m_ABSDebugger.ClearErrorHint();
            selectedId = -1;
            selectedLogDetials = null;
            m_ABSDebugger.ShowHintMessage("Cleared all logs.", Color.blue);
        }

        private void ShowSelectLogDetails(Rect rect)
        {
            if(selectedId == -1)
            {
                return;
            }
            if (Event.current.type == EventType.Repaint)
            {
                RowGUIStyle.Draw(rect, selectedLogDetials, false, false, false, false);
            }
        }
    }
}
#endif