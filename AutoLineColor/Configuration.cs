using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AutoLineColor
{
    [Serializable]
    public class Configuration
    {
        public ColorStrategy ColorStrategy { get; set; }
        public NamingStrategy NamingStrategy { get; set; }
        public int? MinimumColorDifferencePercentage { get; set; }
        public int? MaximunDifferentCollorPickAtempt { get; set; }

        private const int DefaultMaxDiffColorPickAttempt = 10;
        private const int DefaultMinColorDiffPercent = 5;

        private static Configuration _instance;

        public static Configuration LoadConfig()
        {
            bool isDirty = false;
            Configuration config;
            try
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                var fullConfigPath = Constants.ConfigFileName;

                if (File.Exists(fullConfigPath) == false)
                {
                    Console.Message("No config file. Building default and writing it to " + fullConfigPath);
                    config = GetDefaultConfig();
                    isDirty = true;
                }
                else
                {
                    using (var reader = XmlReader.Create(fullConfigPath)) {
                        config = (Configuration)serializer.Deserialize(reader);
                    }

                    //check new configuration properties
                    if (!config.MaximunDifferentCollorPickAtempt.HasValue ||
                        !config.MinimumColorDifferencePercentage.HasValue) {
                        
                        config.MaximunDifferentCollorPickAtempt = config.MaximunDifferentCollorPickAtempt.HasValue ?
                        config.MaximunDifferentCollorPickAtempt : DefaultMaxDiffColorPickAttempt;
                        config.MinimumColorDifferencePercentage = config.MinimumColorDifferencePercentage.HasValue ?
                            config.MinimumColorDifferencePercentage : DefaultMinColorDiffPercent;

                        isDirty = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Don't save changes if it failed for some reason
                Console.Error("Error reading configuration settings - " + ex);
                config = GetDefaultConfig();
            }

            if (isDirty) {
                config.Save();
            }

            return config;
        }

        public void ColorStrategyChange(int Strategy) {
            this.ColorStrategy = (ColorStrategy)Strategy;
        }

        public void NamingStrategyChange(int Strategy) {
            this.NamingStrategy = (NamingStrategy)Strategy;
        }

        public void MinColorDiffChange(float MinDiff) {
            this.MinimumColorDifferencePercentage = (int)MinDiff;
        }

        public void MaxDiffColorPickChange(float MaxColorPicks) {
            this.MaximunDifferentCollorPickAtempt = (int)MaxColorPicks;
        }

        public void Save() {
            var serializer = new XmlSerializer(typeof(Configuration));
            using (var writer = XmlWriter.Create(Constants.ConfigFileName)) {
                serializer.Serialize(writer, this);
            }
        }

        public static string GetModFileName(string fileName)
        {
            return fileName;
        }

        private static Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                ColorStrategy = ColorStrategy.RandomColor,
                NamingStrategy = NamingStrategy.Districts,
                MaximunDifferentCollorPickAtempt = DefaultMaxDiffColorPickAttempt,
                MinimumColorDifferencePercentage = DefaultMinColorDiffPercent
            };
        }

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadConfig();
                }
                return _instance;
            }
        }
    }

    public enum ColorStrategy
    {
        RandomHue,
        RandomColor,
        CategorisedColor
    }

    public enum NamingStrategy
    {
        None,
        Districts,
        London
    }
}
