using System;
using System.Threading;
using AutoLineColor.Coloring;
using AutoLineColor.Naming;
using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AutoLineColor
{
    public class ColorMonitor : ThreadingExtensionBase
    {
        private static DateTimeOffset _lastOutputTime = DateTimeOffset.Now;
        private bool _initialized;
        private IColorStrategy _colorStrategy;
        private INamingStrategy _namingStrategy;
        private List<Color32> _usedColors;
        private Configuration _config;
        private static Console logger = Console.Instance;

        public override void OnCreated(IThreading threading)
        {
            logger.Message("loading auto color monitor");
            logger.Message("initializing colors");
            RandomColor.Initialize();
            CategorisedColor.Initialize();
            GenericNames.Initialize();

            logger.Message("loading current config");
            _config = Configuration.Instance;
            _colorStrategy = SetColorStrategy(_config.ColorStrategy);
            _namingStrategy = SetNamingStrategy(_config.NamingStrategy);
            _usedColors = new List<Color32>();

            logger.Message("Found color strategy of " + _config.ColorStrategy);
            logger.Message("Found naming strategy of " + _config.NamingStrategy);

            _initialized = true;
            base.OnCreated(threading);
        }

        private static INamingStrategy SetNamingStrategy(NamingStrategy namingStrategy)
        {
            logger.Message("Naming Strategy: " + namingStrategy.ToString());
            switch (namingStrategy)
            {
                case NamingStrategy.None:
                    return new NoNamingStrategy();
                case NamingStrategy.Districts:
                    return new DistrictNamingStrategy();
                case NamingStrategy.London:
                    return new LondonNamingStrategy();
                default:
                    logger.Error("unknown naming strategy");
                    return new NoNamingStrategy();
            }
        }

        private IColorStrategy SetColorStrategy(ColorStrategy colorStrategy)
        {
            logger.Message("Color Strategy: " + colorStrategy.ToString());
            switch (colorStrategy)
            {
                case ColorStrategy.RandomHue:
                    return new RandomHueStrategy();
                case ColorStrategy.RandomColor:
                    return new RandomColorStrategy();
                case ColorStrategy.CategorisedColor:
                    return new CategorisedColorStrategy();
                default:
                    logger.Error("unknown color strategy");
                    return new RandomHueStrategy();
            }
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            var theTransportManager = Singleton<TransportManager>.instance;
            var lines = theTransportManager.m_lines.m_buffer;

            //Digest changes
            if (_config.UndigestedChanges == true) {
                logger.Message("Applying undigested changes");
                _colorStrategy = SetColorStrategy(_config.ColorStrategy);
                _namingStrategy = SetNamingStrategy(_config.NamingStrategy);
                _config.UndigestedChanges = false;
            }

            try
            {
                if (_initialized == false)
                    return;

                // try and limit how often we are scanning for lines. this ain't that important
                if (_lastOutputTime.AddMilliseconds(1000) >= DateTimeOffset.Now)
                    return;

                _lastOutputTime = DateTimeOffset.Now;

                while (!Monitor.TryEnter(lines, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }

                _usedColors = lines.Where(l => l.IsActive()).Select(l => l.m_color).ToList();

                for (ushort counter = 0; counter < lines.Length - 1; counter++)
                {
                    var transportLine = lines[counter];

                    if (transportLine.m_flags == TransportLine.Flags.None)
                        continue;

                    // only worry about fully created lines 
                    if (!transportLine.IsActive() || transportLine.HasCustomName() || !transportLine.m_color.IsDefaultColor())
                        continue;

                    var lineName = _namingStrategy.GetName(transportLine);
                    var color = _colorStrategy.GetColor(transportLine, _usedColors);

                    if (!transportLine.HasCustomColor() || transportLine.m_color.IsDefaultColor())
                    {
                        // set the color
                        transportLine.m_color = color;
                        transportLine.m_flags |= TransportLine.Flags.CustomColor;
                        logger.Message(string.Format("Changed line color. '{0}' {1} -> {2}", lineName, transportLine.m_color, color));
                    }

                    if (string.IsNullOrEmpty(lineName) == false && transportLine.HasCustomName() == false)
                    {
                        // set the name
                        var line = Singleton<InstanceManager>.instance;
                        var instanceID = new InstanceID { TransportLine = counter };
                        logger.Message(string.Format("Renamed Line '{0}' -> '{1}'", line.GetName(instanceID), lineName));

                        line.SetName(instanceID, lineName);
                        transportLine.m_flags |= TransportLine.Flags.CustomName;
                    }

                    lines[counter] = transportLine;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                Monitor.Exit(Monitor.TryEnter(lines));
            }

        }
    }

    internal static class LineExtensions
    {
        private static Color32 _defaultBusColor = new Color32(44,142,191,255);
        private static Color32 _defaultMetroColor = new Color32(0,184,0,255);
        private static Color32 _defaultTrainColor = new Color32(219,86,0,255);

        public static bool IsDefaultColor(this Color32 color)
        {
            return (
                color.IsColorEqual(_defaultBusColor) ||
                color.IsColorEqual(_defaultMetroColor) ||
                color.IsColorEqual(_defaultTrainColor));
        }

        public static bool IsColorEqual(this Color32 color1, Color32 color2)
        {
            return (color1.r == color2.r && color1.g == color2.g && color1.b == color2.b && color1.a == color2.a);
        }

        public static bool IsActive(this TransportLine transportLine)
        {
            if ((transportLine.m_flags & (TransportLine.Flags.Complete | TransportLine.Flags.Created | TransportLine.Flags.Hidden)) == 0)
                return false;

            // stations are marked with this flag
            if ((transportLine.m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.Temporary)
                return false;

            return true;
        }

        public static bool HasCustomColor(this TransportLine transportLine)
        {
            return (transportLine.m_flags & TransportLine.Flags.CustomColor) == TransportLine.Flags.CustomColor;
        }

        public static bool HasCustomName(this TransportLine transportLine)
        {
            return (transportLine.m_flags & TransportLine.Flags.CustomName) == TransportLine.Flags.CustomName;
        }
    }
}