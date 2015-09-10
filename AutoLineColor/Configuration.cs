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
        public int? MinColorDiffPercentage { get; set; }
        public int? MaxDiffColorPickAttempt { get; set; }
        public volatile bool UndigestedChanges;

        //Staged changes. These are not applied until 'Save' is clicked
        private ColorStrategy? StagedColorStrategy { get; set; }
        private NamingStrategy? StagedNamingStrategy { get; set; }
        private int? StagedMinColorDiffPercentage { get; set; }
        private int? StagedMaxDiffColorPickAttempt { get; set; }


        private const int DefaultMaxDiffColorPickAttempt = 10;
        private const int DefaultMinColorDiffPercent = 5;

        private static Configuration _instance;
        private static Console logger = Console.Instance;

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
                    logger.Message("No config file. Building default and writing it to " + fullConfigPath);
                    config = GetDefaultConfig();
                    isDirty = true;
                }
                else
                {
                    logger.Message("Config file exists. Using it");
                    using (var reader = XmlReader.Create(fullConfigPath)) {
                        config = (Configuration)serializer.Deserialize(reader);
                    }

                    //check new configuration properties
                    if (!config.MaxDiffColorPickAttempt.HasValue ||
                        !config.MinColorDiffPercentage.HasValue) {

                        config.UndigestedChanges = false;
                        config.MaxDiffColorPickAttempt = config.MaxDiffColorPickAttempt.HasValue ?
                        config.MaxDiffColorPickAttempt : DefaultMaxDiffColorPickAttempt;
                        config.MinColorDiffPercentage = config.MinColorDiffPercentage.HasValue ?
                            config.MinColorDiffPercentage : DefaultMinColorDiffPercent;

                        isDirty = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Don't save changes if it failed for some reason
                logger.Error("Error reading configuration settings - " + ex);
                config = GetDefaultConfig();
            }

            if (isDirty)
            {
                config.Save();
            }

            return config;
        }

        public void ColorStrategyChange(int Strategy)
        {
            this.StagedColorStrategy = (ColorStrategy)Strategy;
        }

        public void NamingStrategyChange(int Strategy)
        {
            this.StagedNamingStrategy = (NamingStrategy)Strategy;
        }

        public void MinColorDiffChange(float MinDiff)
        {
            this.StagedMinColorDiffPercentage = (int)MinDiff;
        }

        public void MaxDiffColorPickChange(float MaxColorPicks)
        {
            this.StagedMaxDiffColorPickAttempt = (int)MaxColorPicks;
        }

        public void FlushStagedChanges()
        {
            StagedColorStrategy = null;
            StagedNamingStrategy = null;
            StagedMaxDiffColorPickAttempt = null;
            StagedMinColorDiffPercentage = null;
        }

        public void Save()
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            logger.Message("Saving changes to config file");

            //If any changes have occured, apply them, otherwise keep the current value
            this.ColorStrategy = this.StagedColorStrategy.HasValue
                ? this.StagedColorStrategy.Value
                : this.ColorStrategy;
            this.NamingStrategy = this.StagedNamingStrategy.HasValue
                ? this.StagedNamingStrategy.Value
                : this.NamingStrategy;
            this.MaxDiffColorPickAttempt =
                this.StagedMaxDiffColorPickAttempt.HasValue
                    ? this.StagedMaxDiffColorPickAttempt.Value
                    : this.MaxDiffColorPickAttempt;
            this.MinColorDiffPercentage =
                this.StagedMinColorDiffPercentage.HasValue
                    ? this.StagedMinColorDiffPercentage.Value
                    : this.MinColorDiffPercentage;

            //clear changes and log
            if (this.StagedColorStrategy.HasValue)
            {
                logger.Message("ColorStrategy changed to " + this.StagedColorStrategy.Value.ToString());
            }

            if (this.StagedNamingStrategy.HasValue)
            {
                logger.Message("NamingStrategy changed to " + this.StagedNamingStrategy.Value.ToString());
            }

            if (this.StagedMaxDiffColorPickAttempt.HasValue)
            {
                logger.Message("MaxDiffColorPickAttempt changed to " + this.StagedMaxDiffColorPickAttempt.Value.ToString());
            }

            if (this.StagedMinColorDiffPercentage.HasValue)
            {
                logger.Message("MinColorDiffPercentage changed to " + this.StagedMinColorDiffPercentage.Value.ToString());
            }

            FlushStagedChanges();

            //How we let the ColorMonitor thread know to update the strategies
            logger.Message("Marking undigested changes");
            this.UndigestedChanges = true;

            //Save to disk
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
                MaxDiffColorPickAttempt = DefaultMaxDiffColorPickAttempt,
                MinColorDiffPercentage = DefaultMinColorDiffPercent,
                UndigestedChanges = false
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
