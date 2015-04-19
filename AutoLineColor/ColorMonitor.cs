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
        private static DateTimeOffset _lastOutputTime = DateTimeOffset.Now.AddSeconds(-100);
        private bool _initialized;
        private IColorStrategy _colorStrategy;
        private INamingStrategy _namingStrategy;
        private List<Color32> _usedColors;

        public override void OnCreated(IThreading threading)
        {

            Console.Message("loading auto color monitor");
            Console.Message("initializing colors");
            RandomColor.Initialize();

            Console.Message("loading current config");
            var config = Configuration.LoadConfig();
            _colorStrategy = SetColorStrategy(config.ColorStrategy);
            _namingStrategy = SetNamingStrategy(config.NamingStrategy);
            _usedColors = new List<Color32>();

            Console.Message("Found color strategy of " + config.ColorStrategy);
            Console.Message("Found naming strategy of " + config.NamingStrategy);

            _initialized = true;
            base.OnCreated(threading);
        }

        private static INamingStrategy SetNamingStrategy(NamingStrategy namingStrategy)
        {
            switch (namingStrategy)
            {
                case NamingStrategy.None:
                    return new NoNamingStrategy();
                case NamingStrategy.Districts:
                    return new DistrictNamingStrategy();
                default:
                    Console.Error("unknown naming strategy");
                    return new NoNamingStrategy();
            }
        }

        private static IColorStrategy SetColorStrategy(ColorStrategy colorStrategy)
        {
            switch (colorStrategy)
            {
                case ColorStrategy.RandomHue:
                    return new RandomHueStrategy();
                case ColorStrategy.RandomColor:
                    return new RandomColorStrategy();
                default:
                    Console.Error("unknown color strategy");
                    return new RandomHueStrategy();
            }
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            var theTransportManager = Singleton<TransportManager>.instance;
            var lines = theTransportManager.m_lines.m_buffer;

            try
            {
                if (_initialized == false)
                    return;

                // try and limit how often we are scanning for lines. this ain't that important
                if (_lastOutputTime.AddMilliseconds(1000) >= DateTimeOffset.Now)
                    return;

                _lastOutputTime = DateTimeOffset.Now;

                while (!Monitor.TryEnter(lines, SimulationManager.SYNCHRONIZE_TIMEOUT))
                { }

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

                    Console.Message(string.Format("New line found. {0} {1}", lineName, color));

                    if (!transportLine.HasCustomColor() || transportLine.m_color.IsDefaultColor())
                    {
                        // set the color
                        transportLine.m_color = color;
                        transportLine.m_flags |= TransportLine.Flags.CustomColor;
                    }
                    else
                    {
                        Console.Message(transportLine.m_color.ToString());
                    }

                    if (string.IsNullOrEmpty(lineName) == false && transportLine.HasCustomName() == false)
                    {
                        // set the name
                        Singleton<InstanceManager>.instance.SetName(new InstanceID { TransportLine = counter },
                            lineName);
                        transportLine.m_flags |= TransportLine.Flags.CustomName;
                    }

                    lines[counter] = transportLine;
                }
            }
            catch (Exception ex)
            {
                Console.Message(ex.ToString(), PluginManager.MessageType.Message);
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
            if ((transportLine.m_flags & TransportLine.Flags.Created) != TransportLine.Flags.Created)
                return false;

            if ((transportLine.m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.Hidden)
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