using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework;
using ICities;
using Random = UnityEngine.Random;

namespace AutoLineColor
{
    public class ColorMonitor : ThreadingExtensionBase
    {
        private static DateTimeOffset _lastOutputTime = DateTimeOffset.Now.AddSeconds(-100);
        private bool _initialized;
        private Dictionary<TransportInfo.TransportType, ColorName[]> _colorMap;

        public override void OnCreated(IThreading threading)
        {
            _colorMap = ColorName.BuildColorMap();
            _initialized = true;
            base.OnCreated(threading);
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            try
            {
                if (_initialized == false)
                    return;

                // try and limit how often we are scanning for lines. this ain't that important
                if (_lastOutputTime.AddSeconds(3) < DateTimeOffset.Now)
                {
                    _lastOutputTime = DateTimeOffset.Now;
                }
                else
                {
                    return;
                }

                var theTransportManager = Singleton<TransportManager>.instance;
                var theNetworkManager = Singleton<NetManager>.instance;
                var theDistrictManager = Singleton<DistrictManager>.instance;

                var lines = theTransportManager.m_lines;
                if (theTransportManager.m_lines == null)
                    return;

                for (ushort counter = 0; counter < lines.m_buffer.Length - 1; counter++)
                {
                    var transportLine = lines.m_buffer[counter];
                    // only worry about fully created lines 
                    if ((transportLine.m_flags & TransportLine.Flags.Created) != TransportLine.Flags.Created) continue;
                    if ((transportLine.m_flags & TransportLine.Flags.Hidden) == TransportLine.Flags.Hidden) continue;
                    
                    // stations are marked with this flag
                    if ((transportLine.m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.Temporary) continue;

                    // if we already have a color and a name then we are set 
                    if ((transportLine.m_flags & TransportLine.Flags.CustomColor) == TransportLine.Flags.CustomColor &&
                        (transportLine.m_flags & TransportLine.Flags.CustomName) == TransportLine.Flags.CustomName)
                        continue;

                    // get random color based on the transport type
                    var colorNames = _colorMap[transportLine.Info.m_transportType];
                    if (colorNames == null)
                        continue;
                    
                    var colorName = colorNames[Random.Range(0, colorNames.Length - 1)];
                    var mycolor = colorName.Color;
                    
                    int stopCount;
                    var districts = GetDistrictsForLine(transportLine, theNetworkManager, theDistrictManager, out stopCount);

                    var myName = BuildRandomName(transportLine.m_lineNumber, transportLine.Info.m_transportType, colorName, districts, stopCount);

                    while (!Monitor.TryEnter(lines.m_buffer, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        // set the color
                        transportLine.m_color = mycolor;
                        transportLine.m_flags |= TransportLine.Flags.CustomColor;
                        
                        // set the name
                        Singleton<InstanceManager>.instance.SetName(new InstanceID {TransportLine = counter}, myName);
                        transportLine.m_flags |= TransportLine.Flags.CustomName;

                        lines.m_buffer[counter] = transportLine;
                    }
                    finally
                    {
                        Monitor.Exit(Monitor.TryEnter(lines.m_buffer));
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.PrintValue(ex.ToString());
            }
           
        }

        private string BuildRandomName(ushort lineNumber, TransportInfo.TransportType transportType, ColorName mycolor, List<string> districts, int stopCount)
        {
            // todo could this be localized?
            try
            {
                if (transportType == TransportInfo.TransportType.Train)
                {
                    if (districts.Count == 1)
                    {
                        if (districts[0] == string.Empty)
                        {
                            return string.Format("#{0} {1} Shuttle", lineNumber, districts[0]);
                        }

                        var rnd = Random.value;
                        if (rnd <= .33f)
                        {
                            return string.Format("#{0} {1} Limited", lineNumber, districts[0]);
                        }

                        if (rnd <= .66f)
                        {
                            return string.Format("#{0} {1} Service", lineNumber, districts[0]);
                        }

                        return string.Format("#{0} {1} Shuttle", lineNumber, districts[0]);
                    }
                    if (districts.Count == 2)
                    {
                        if (string.IsNullOrEmpty(districts[0]) || string.IsNullOrEmpty(districts[1]))
                        {
                            return string.Format("#{0} {1} Shuttle", lineNumber, districts[0]);
                        }

                        var rnd = Random.value;
                        if (rnd <= .33f)
                        {
                            return string.Format("#{0} {1}&{2}", lineNumber, districts[0].Substring(0, 1),
                                districts[1].Substring(0, 1));
                        }
                        if (rnd <= .5)
                        {
                            return string.Format("#{0} {1} Zephr", lineNumber, districts[0].Substring(0, 1));
                        }
                        if (rnd <= .7)
                        {
                            return string.Format("#{0} {1} Flyer", lineNumber, districts[0].Substring(0, 1));
                        }
                        return string.Format("#{0} {1} & {2}", lineNumber, districts[0], districts[1]);
                    }

                    return string.Format("#{0} {1} Unlimited", lineNumber, mycolor.Name);

                }

                if (transportType == TransportInfo.TransportType.Bus ||
                    transportType == TransportInfo.TransportType.Metro)
                {
                    if (districts.Count == 1)
                    {
                        if (string.IsNullOrEmpty(districts[0]))
                        {
                            return string.Format("#{0} {1} Line", lineNumber, mycolor.Name);
                        }

                        return string.Format("#{0} {1} Local", lineNumber, districts[0]);
                    }

                    if (districts.Count == 2 && string.IsNullOrEmpty(districts[0]) && string.IsNullOrEmpty(districts[1]))
                        return string.Format("#{0} {1} Line", lineNumber, mycolor.Name);


                    if (districts.Count == 2 && stopCount <= 4)
                        return string.Format("#{0} {1} / {2} Express", lineNumber, districts[0], districts[1]);

                    if (districts.Count == 2)
                        return string.Format("#{0} {1} / {2} Line", lineNumber, districts[0], districts[1]);

                    return string.Format("#{0} {1} Line", lineNumber, mycolor.Name);
                }
            }
            catch (Exception ex)
            {
                // if we get an exception we'll just drop back to Line number and color name
                Helper.PrintValue(ex.ToString());
            }

            return string.Format("#{0} {1} Line", lineNumber, mycolor.Name);
        }

        private static List<string> GetDistrictsForLine(TransportLine transportLine, NetManager theNetworkManager, DistrictManager theDistrictManager, out int stopCount)
        {
            var stop = TransportLine.GetPrevStop(transportLine.m_stops);
            var firstStop = stop;
            stopCount = 0;
            var districts = new List<string>();
            while (stop != 0)
            {
                stopCount++;
                var position = theNetworkManager.m_nodes.m_buffer[stop].m_position;
                var district = theDistrictManager.GetDistrict(position);
                if (district != 0)
                {
                    var districtName = theDistrictManager.GetDistrictName(district);
                    districtName = districtName.Trim();
                    if (districts.Contains(districtName) == false)
                        districts.Add(districtName);
                }
                else
                {
                    if (districts.Contains(string.Empty) == false)
                        districts.Add(string.Empty);
                }


                stop = TransportLine.GetNextStop(stop);
                if (stop == firstStop)
                    break;
            }
            return districts;
        }

        
    }
}