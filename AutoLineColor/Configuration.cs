using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework.IO;

namespace AutoLineColor
{
    [Serializable]
    public class Configuration
    {
        public ColorStrategy ColorStrategy { get; set; }
        public NamingStrategy NamingStrategy { get; set; }
        public int? MinimumColorDifferencePercentage { get; set; }
        public int? MaximunDifferentCollorPickAtempt { get; set; }

        private const string ConfigFileName = "AutoLineColorSettings.xml";
        private const string ModName = "AutoLineColor";

        public static Configuration LoadConfig()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                Configuration config;

                var fullConfigPath = GetModFileName(ConfigFileName);
                if (File.Exists(fullConfigPath) == false)
                {
                    Console.Message("No config file. Building default and writing it to " + fullConfigPath);
                    config = GetDefaultConfig();
                    SaveConfig(config);
                    return config;
                }


                using (var reader = XmlReader.Create(fullConfigPath))
                {
                    config = (Configuration)serializer.Deserialize(reader);
                }
                //check new configuration properties
                if (!config.MaximunDifferentCollorPickAtempt.HasValue || !config.MinimumColorDifferencePercentage.HasValue)
                {
                    var defaultConfig = GetDefaultConfig();
                    config.MinimumColorDifferencePercentage = defaultConfig.MinimumColorDifferencePercentage;
                    config.MaximunDifferentCollorPickAtempt = defaultConfig.MaximunDifferentCollorPickAtempt;
                    SaveConfig(config);
                }
                return config;

            }
            catch (Exception ex)
            {
                Console.Error("Error reading configuration settings - " + ex);
                return GetDefaultConfig();
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
                MaximunDifferentCollorPickAtempt = 10,
                MinimumColorDifferencePercentage = 5
            };
        }
        private static void SaveConfig(Configuration config)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            using (var writer = XmlWriter.Create(GetModFileName(ConfigFileName)))
            {
                serializer.Serialize(writer, config);
            }
        }

        private static Configuration _instance;
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
        RandomColor
    }

    public enum NamingStrategy
    {
        None,
        Districts
    }
}
