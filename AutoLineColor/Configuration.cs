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

        private const string ConfigFileName = "Settings.xml";
        private const string ModName = "AutoLineColor";

        public static Configuration LoadConfig()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Configuration));

                var fullConfigPath = GetModFileName(ConfigFileName);
                if (File.Exists(fullConfigPath) == false)
                {
                    Console.Message("No config file. Building default and writing it to " + fullConfigPath);
                    var config = GetDefaultConfig();
                    using (var writer = XmlWriter.Create(fullConfigPath))
                    {
                        serializer.Serialize(writer, config);
                    }
                    return config;
                }

                using (var reader = XmlReader.Create(fullConfigPath))
                {
                    return (Configuration) serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.Error("Error reading configuration settings - " + ex);
                return GetDefaultConfig();
            }
        }

        public static string GetModFileName(string fileName)
        {
            return Path.Combine(Path.Combine(DataLocation.modsPath, ModName), fileName);           
        }

        private static Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                ColorStrategy = ColorStrategy.RandomHue,
                NamingStrategy = NamingStrategy.Districts
            };
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
