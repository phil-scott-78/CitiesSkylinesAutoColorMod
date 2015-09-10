using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Plugins;
using Random = UnityEngine.Random;

namespace AutoLineColor.Naming
{
    internal class DistrictNamingStrategy : INamingStrategy
    {
        private static Console logger = Console.Instance;
        public string GetName(TransportLine transportLine)
        {
            int stopCount; 
            var districts = GetDistrictsForLine(transportLine, Singleton<NetManager>.instance, Singleton<DistrictManager>.instance, out stopCount);
            return BuildRandomName(transportLine.m_lineNumber, transportLine.Info.m_transportType, districts, stopCount);
        }

        private string BuildRandomName(ushort lineNumber, TransportInfo.TransportType transportType, List<string> districts, int stopCount)
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

                    return string.Format("#{0} Unlimited", lineNumber);

                }

                if (transportType == TransportInfo.TransportType.Bus ||
                    transportType == TransportInfo.TransportType.Metro)
                {
                    if (districts.Count == 1)
                    {
                        if (string.IsNullOrEmpty(districts[0]))
                        {
                            return string.Format("#{0} Line", lineNumber);
                        }

                        return string.Format("#{0} {1} Local", lineNumber, districts[0]);
                    }

                    if (districts.Count == 2 && string.IsNullOrEmpty(districts[0]) && string.IsNullOrEmpty(districts[1]))
                        return string.Format("#{0} Line", lineNumber);


                    if (districts.Count == 2 && stopCount <= 4)
                        return string.Format("#{0} {1} / {2} Express", lineNumber, districts[0], districts[1]);

                    if (districts.Count == 2)
                        return string.Format("#{0} {1} / {2} Line", lineNumber, districts[0], districts[1]);

                    return string.Format("#{0} Line", lineNumber);
                }
            }
            catch (Exception ex)
            {
                // if we get an exception we'll just drop back to Line number and color name
                logger.Error(ex.ToString());
            }

            return string.Format("#{0} Line", lineNumber);
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