using ColossalFramework.Plugins;

namespace AutoLineColor
{
    class Helper
    {
        public static void PrintValue(string message)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                message);
        }
    }
}