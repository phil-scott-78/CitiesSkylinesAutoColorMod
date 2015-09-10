using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AutoLineColor
{
    public class Console
    {
#if DEBUG
        private bool debug = true;
#else
        private bool debug = false;
#endif
        private static Console _instance;

        private StreamWriter log;
        private bool log_opened;

        private Console() {
            try {
                log = new StreamWriter(new FileStream(Constants.LogFileName, FileMode.Append | FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
            } catch (Exception e) {
                WriteMessage("Could not open log file", PluginManager.MessageType.Warning);
            }
            log_opened = true;
        }

        public static Console Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Console();
                }
                return _instance;
            }
        }

        private static string FormatMessage(string msg, PluginManager.MessageType Type)
        {
            string formatted;
            try {
                formatted = string.Format("{0}({1}) {2}", "[AutoLineColor]", Type.ToString(), msg);
            } catch (Exception e) {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, e.ToString());
                formatted = msg;
            }
            return formatted;
        }

        public void Message(string p, PluginManager.MessageType messageType)
        {
            this.WriteMessage(p, messageType);
        }

        public void Message(string p)
        {
            this.WriteMessage(p, PluginManager.MessageType.Message);
        }

        public void Warning(string p) {
            this.WriteMessage(p, PluginManager.MessageType.Warning);
        }

        public void Error(string p)
        {
            this.WriteMessage(p, PluginManager.MessageType.Error);
        }

        private void WriteMessage(string p, PluginManager.MessageType Type) {
            if(!this.debug)
            {
                return;
            }
            string msg = FormatMessage(p, Type);
            DebugOutputPanel.AddMessage(Type, msg);
            if (log_opened) {
                log.WriteLine(msg);
                log.Flush();
            }
            
            //Unity engine logger
            switch(Type)
            {
                case PluginManager.MessageType.Error:
                    Debug.LogError(msg);
                    break;
                case PluginManager.MessageType.Message:
                    Debug.Log(msg);
                    break;
                case PluginManager.MessageType.Warning:
                    Debug.LogWarning(msg);
                    break;
                default:
                    Debug.Log(msg);
                    break;
            }
            
        }
    }
}
