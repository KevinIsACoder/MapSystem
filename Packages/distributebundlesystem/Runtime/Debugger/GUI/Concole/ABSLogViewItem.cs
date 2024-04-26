#if DebugABS
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.AdvancedBundleSystem.Debugger.GUI.Concole
{
    public class ABSLogViewItem
    {
        public ABSLogViewItem(int id, string condition, string stackTrace, LogType logType)
        {
            this.id = id;
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.logType = logType;
            
        }

        public int id { get; set; }
        public string condition { get; set; }
        public string stackTrace { get; set; }
        public LogType logType { get; set; }
    }

}
#endif
