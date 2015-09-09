using ICities;
using System;
using System.Collections.Generic;

namespace AutoLineColor
{
    public class AutoLineColorMod : IUserMod
    {
        private Configuration Config;
        public string Name
        {
            get { return Constants.ModName; }
        }

        public string Description
        {
            get { return Constants.Description; }
        }

        public void OnSettingsUI(UIHelperBase helper) {
            Config = Configuration.Instance;
            //Generate arrays of colors and naming strategies
            String[] ColorStrategies = Enum.GetNames(typeof(ColorStrategy));
            String[] NamingStrategies = Enum.GetNames(typeof(NamingStrategy));

            UIHelperBase group = helper.AddGroup(Constants.ModName);
            group.AddDropdown("Color Strategy", ColorStrategies, (int)Config.ColorStrategy, Config.ColorStrategyChange);
            group.AddDropdown("Naming Strategy", NamingStrategies, (int)Config.NamingStrategy, Config.NamingStrategyChange);
            group.AddSpace(5);

            group.AddGroup("Advanced Settings");
            group.AddSlider("Max Different Color Picks", 1f, 20f, 1f,  (float)Config.MaximunDifferentCollorPickAtempt, Config.MaxDiffColorPickChange);
            group.AddSlider("MinColorDifference", 1f, 100f, 5f, (float)Config.MinimumColorDifferencePercentage, Config.MinColorDiffChange);
            group.AddButton("Save", Config.Save);
        }
    }
}