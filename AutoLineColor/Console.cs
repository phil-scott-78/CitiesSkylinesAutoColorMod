using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoLineColor
{
    public static class Console
    {
#if DEBUG
        private static bool debug = true;
#else
        private static bool debug = false;
#endif

        public static void Message(string p, PluginManager.MessageType messageType)
        {
            if (!debug)
                return;

            DebugOutputPanel.AddMessage(messageType, p);
        }

        public static void Message(string p)
        {
            if (!debug)
                return;

            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, p);
        }

        internal static void Error(string p)
        {
            if (!debug)
                return;

            DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, p);
        }
    }
}
