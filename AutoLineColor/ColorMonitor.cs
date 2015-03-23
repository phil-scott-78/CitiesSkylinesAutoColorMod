using System;
using System.Threading;
using AutoLineColor.Coloring;
using AutoLineColor.Naming;
using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;

namespace AutoLineColor
{
    public class ColorMonitor : ThreadingExtensionBase
    {
        private static DateTimeOffset _lastOutputTime = DateTimeOffset.Now.AddSeconds(-100);
        private bool _initialized;
        private IColorStrategy _colorStrategy;
        private INamingStrategy _namingStrategy;



        public override void OnCreated(IThreading threading)
        {
            
            Console.Message("loading auto color monitor");
            Console.Message("initializing colors");
            RandomColor.Initialize();

            Console.Message("loading current config");
            var config = Configuration.LoadConfig();
            _colorStrategy = SetColorStrategy(config.ColorStrategy);
            _namingStrategy = SetNamingStrategy(config.NamingStrategy);

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
            try
            {
                if (_initialized == false)
                    return;

                // try and limit how often we are scanning for lines. this ain't that important
                if (_lastOutputTime.AddMilliseconds(250) >= DateTimeOffset.Now)
                    return;

                _lastOutputTime = DateTimeOffset.Now;

                var theTransportManager = Singleton<TransportManager>.instance;
                var lines = theTransportManager.m_lines.m_buffer;


                for (ushort counter = 0; counter < lines.Length - 1; counter++)
                {
                    var transportLine = lines[counter];
                    // only worry about fully created lines 
                    if (transportLine.IsActive() == false || transportLine.HasCustomColor() || transportLine.HasCustomName())
                        continue;

                    var lineName = _namingStrategy.GetName(transportLine);
                    var color = _colorStrategy.GetColor(transportLine);

                    Console.Message(string.Format("New line found. {0} {1}", lineName, color));

                    while (!Monitor.TryEnter(lines, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        
                        // set the color
                        transportLine.m_color = color;
                        transportLine.m_flags |= TransportLine.Flags.CustomColor;

                        if (string.IsNullOrEmpty(lineName) == false)
                        {
                            // set the name
                            Singleton<InstanceManager>.instance.SetName(new InstanceID {TransportLine = counter},
                                lineName);
                            transportLine.m_flags |= TransportLine.Flags.CustomName;
                        }

                        lines[counter] = transportLine;
                    }
                    finally
                    {
                        Monitor.Exit(Monitor.TryEnter(lines));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Message(ex.ToString(), PluginManager.MessageType.Message);
            }
           
        }
    }

    internal static class LineExtensions
    {
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