using System;

namespace AutoLineColor {
    public static class Constants {
        public const string ConfigFileName = "AutoLineColorSettings.xml";
        public const string LogFileName = "AutoLineColorSettings.log";
        public const string ModName = "Auto Line Color";
        public const string Description = 
                    "Monitors all transport line looking for lines set to the default color." +
                    " When found it sets a new color and a line name";
    }
}
