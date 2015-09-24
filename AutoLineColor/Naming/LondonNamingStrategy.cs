using System;
using System.Linq;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Plugins;
using Random = UnityEngine.Random;

namespace AutoLineColor.Naming
{
    internal class LondonNamingStrategy : INamingStrategy
    {
        private static string[] _trains =
        {
            "{0}",
            "{0} Service",
            "{0} Rail",
            "{0} Railway",
            "{0} Flyer",
            "{0} Zephyr",
            "{0} Rocket",
            "{0} Arrow",
            "{0} Special",
            "Spirit of {0}",
            "Pride of {0}",
        };

        public string GetName(TransportLine transportLine)
        {
            switch (transportLine.Info.m_transportType)
            {
            case TransportInfo.TransportType.Bus:
                return GetBusLineName (transportLine);
            case TransportInfo.TransportType.Metro:
                return GetMetroLineName (transportLine);
            default:
                return GetTrainLineName (transportLine);
            }
        }

        private void AnalyzeLine(TransportLine transportLine, List<string> districtNames, out int districtCount, out int stopCount, out bool nonDistrict)
        {
            var theNetManager = Singleton<NetManager>.instance;
            var theDistrictManager = Singleton<DistrictManager>.instance;
            var stop = transportLine.m_stops;
            var firstStop = stop;
            stopCount = 0;
            districtCount = 0;
            nonDistrict = false;
            do
            {
                var stopInfo = theNetManager.m_nodes.m_buffer[stop];
                var district = theDistrictManager.GetDistrict(stopInfo.m_position);

                if (district == 0)
                {
                    nonDistrict = true;
                }
                else
                {
                    var districtName = theDistrictManager.GetDistrictName(district).Trim();
                    if (!districtNames.Contains(districtName))
                    {
                        districtNames.Add(districtName);
                        districtCount++;
                    }
                }

                stop = TransportLine.GetNextStop (stop);
                stopCount++;
            } while (stopCount < 25 && stop != firstStop);
        }

        private static string GetInitials(string words)
        {
            string initials = words[0].ToString();
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words [i] == ' ')
                {
                    initials += words[i + 1];
                }
            }
            return initials;
        }

        private static List<string> GetExistingNames()
        {
            var names = new List<string>();
            var theTransportManager = Singleton<TransportManager>.instance;
            var theInstanceManager = Singleton<InstanceManager>.instance;
            var lines = theTransportManager.m_lines.m_buffer;
            for (ushort lineIndex = 0; lineIndex < lines.Length - 1; lineIndex++)
            {
                if (lines[lineIndex].HasCustomName())
                {
                    string name = theInstanceManager.GetName(new InstanceID { TransportLine = lineIndex });
                    if (!String.IsNullOrEmpty(name))
                    {
                        names.Add(name);
                    }
                }
            }
            return names;
        }

        private static List<string> GetNumbers(List<string> names)
        {
            var numbers = new List<string>();
            foreach (var name in names)
            {
                numbers.Add(FirstWord(name));
            }
            return numbers;
        }

        private static string FirstWord(string words)
        {
            return words.Contains(" ") ? (words.Substring(0, words.IndexOf(" "))) : words;
        }

        private static string TryBakerlooify(string word1, string word2)
        {
            int offset1 = Math.Min(word1.Length - 1, Math.Max(word1.Length / 2, 4));
            int offset2 = word2.Length / 4;
            int length2 = Math.Max(word2.Length / 2, 3);

            string substring2 = word2.Substring(offset2, length2);

            for (int offset = offset1; offset < word1.Length; offset++)
            {
                if (substring2.IndexOf(word1[offset]) >= 0)
                {
                    return word1.Substring(0, offset) + word2.Substring(offset2 + substring2.IndexOf(word1[offset]));
                }
            }
            return null;
        }

        /*
         * Bus line numbers are based on district:
         *
         * Given districts "Hamilton Park", "Ivy Square", "King District" in a city called "Springwood", bus line names look like:
         *
         * HP43 Local
         * 22 Hamilton Park
         * 345 Ivy to King Express
         * 9 Hamilton, Ivy and King
         * 6 Springwood Express
         */

        private string GetBusLineName(TransportLine transportLine)
        {
            var districtNames = new List<string>();
            bool nonDistrict;
            int districtCount;
            int stopCount;
            AnalyzeLine(transportLine, districtNames, out districtCount, out stopCount, out nonDistrict);
            string prefix = null;
            int number;
            string name = null;
            string suffix = null;
            var existingNames = GetExistingNames();
            var existingNumbers = GetNumbers(existingNames);

            // Work out the bus number (and prefix)
            if (!nonDistrict && districtCount == 1)
            {
                /* District Initials */
                prefix = GetInitials(districtNames[0]);
                number = 0;
                string prefixed_number;
                do
                {
                    number++;
                    prefixed_number = String.Format("{0}{1}", prefix, number);
                } while (existingNumbers.Contains(prefixed_number));
            }
            else
            {
                int step;
                if (stopCount < 15)
                {
                    number = Random.Range(100, 900);
                    step = Random.Range(7, 20);
                }
                else if (stopCount < 30)
                {
                    number = Random.Range(20, 100);
                    step = Random.Range(2, 10);
                }
                else
                {
                    number = Random.Range(1, 20);
                    step = Random.Range(1, 4);
                }
                while (existingNumbers.Contains(number.ToString()))
                {
                    number += step;
                }
            }

            // Work out the bus name
            if (districtCount == 1)
            {
                name = nonDistrict ? districtNames[0] : "Local";
            }

            if (districtCount == 2)
            {
                name = String.Format("{0} to {1}", FirstWord(districtNames[0]), FirstWord(districtNames[1]));
            }

            if (districtCount == 3)
            {
                name = String.Format("{0}, {1} and {2}",
                    FirstWord(districtNames[0]), FirstWord(districtNames[1]), FirstWord(districtNames[2]));
            }

            if (districtCount == 0 || districtCount > 3)
            {
                var theSimulationManager = Singleton<SimulationManager>.instance;
                name = theSimulationManager.m_metaData.m_CityName;
            }

            if (stopCount <= 4)
            {
                suffix = "Express";
            }

            string lineName = String.Format("{0}{1}", prefix ?? "", number);
            if (!String.IsNullOrEmpty(name))
            {
                lineName += " " + name;
            }
            if (!String.IsNullOrEmpty(suffix))
            {
                lineName += " " + suffix;
            }
            return lineName;
        }

        /*
         * Metro line names are based on district, with generic names from a list thrown in.
         *
         * Given districts "Manor Park", "Ivy Square", "Hickory District", metro line names look like:
         *
         * Manor Line
         * Ivy Loop Line
         * Hickory & Ivy Line
         * Hickory, Manor & Ivy Line
         * Foxtrot Line
         *
         * There's also some attempt to "Bakerlooify" line names.  No idea how well that will work.
         */

        private string GetMetroLineName(TransportLine transportLine)
        {
            var districtNames = new List<string>();
            bool nonDistrict;
            int districtCount;
            int stopCount;
            AnalyzeLine(transportLine, districtNames, out districtCount, out stopCount, out nonDistrict);
            string name = null;
            var districtFirstNames = districtNames.Select(FirstWord).ToList();
            var existingNames = GetExistingNames();
            int count = 0;

            if (districtCount == 1)
            {
                name = districtNames[0];
            }
            else if (districtCount == 2)
            {
                if (districtFirstNames[0].Equals(districtFirstNames[1]))
                {
                    name = districtFirstNames[0];
                }
                else
                {
                    name = TryBakerlooify(districtFirstNames[0], districtFirstNames[1]) ??
                        TryBakerlooify(districtFirstNames[1], districtFirstNames[0]) ??
                        String.Format("{0} & {1}", districtFirstNames[0], districtFirstNames[1]);
                }
            }
            else if (districtCount >= 3)
            {
                int totalLength = districtFirstNames.Sum(d => d.Length);
                if (totalLength < 20)
                {
                    var districtFirstNamesArray = districtFirstNames.ToArray();
                    name = String.Format("{0} & {1}",
                        String.Join(", ", districtFirstNamesArray, 0, districtFirstNamesArray.Length - 1),
                        districtFirstNamesArray[districtFirstNamesArray.Length - 1]);
                }
            }

            var lineName = name == null ? "Metro Line" : String.Format("{0} Line", name);
            while (name == null || existingNames.Contains(lineName))
            {
                name = GenericNames.GetGenericName(count / 2);
                lineName = String.Format("{0} Line", name);
                count++;
            }
            return lineName;
        }

        /*
         * Train line names are based on the British designations, with some liberties taken.
         *
         * The format is AXNN Name:
         *
         * A is the number of districts the train stops at.
         * X is the first letter of the last district, or X if the train stops outside of a district.
         * NN are random digits.
         *
         * The name is based on the district names.
         */

        private string GetTrainLineName(TransportLine transportLine)
        {
            var districtNames = new List<string>();
            bool nonDistrict;
            int districtCount;
            int stopCount;
            AnalyzeLine(transportLine, districtNames, out districtCount, out stopCount, out nonDistrict);
            string ident = null;
            int number = Random.Range(1, 90);
            string name = null;
            var districtFirstNames = districtNames.Select(FirstWord).ToList();
            var existingNames = GetExistingNames();
            var existingNumbers = GetNumbers(existingNames);

            var lastDistrictName = districtNames.LastOrDefault();
            if (String.IsNullOrEmpty(lastDistrictName))
            {
                lastDistrictName = "Z";
            }

            ident = String.Format("{0}{1}", districtCount, nonDistrict ? "X" : lastDistrictName.Substring(0, 1));

            if (districtCount == 0)
            {
                var theSimulationManager = Singleton<SimulationManager>.instance;
                name = String.Format(_trains[Random.Range(0, _trains.Length)], theSimulationManager.m_metaData.m_CityName);
            }
            else if (districtCount == 1)
            {
                name = String.Format(_trains[Random.Range(0, _trains.Length)], districtNames[0]);
            }
            else if (districtCount == 2)
            {
                if (districtFirstNames[0].Equals(districtFirstNames[1]))
                {
                    name = districtFirstNames[0];
                }
                else if (stopCount == 2)
                {
                    name = String.Format("{0} {1} Shuttle", districtFirstNames[0], districtFirstNames[1]);
                }
                else if (!nonDistrict)
                {
                    name = String.Format("{0} {1} Express", districtFirstNames[0], districtFirstNames[1]);
                }
                else
                {
                    name = String.Format("{0} via {1}", districtFirstNames[0], districtFirstNames[1]);
                }
            }
            else
            {
                int totalLength = districtFirstNames.Sum(d => d.Length);
                if (totalLength < 15)
                {
                    var districtFirstNamesArray = districtFirstNames.ToArray();
                    name = String.Format("{0} and {1} via {2}",
                        String.Join(", ", districtFirstNamesArray, 0, districtFirstNamesArray.Length - 2),
                        districtFirstNamesArray[districtFirstNamesArray.Length - 1],
                        districtFirstNamesArray[districtFirstNamesArray.Length - 2]);
                }
                else
                {
                    name = String.Format(_trains[Random.Range(0, _trains.Length)], districtNames.First());
                }
            }

            var lineNumber = String.Format("{0}{1:00}", ident, number);
            while (existingNumbers.Contains(lineNumber))
            {
                number++;
                lineNumber = String.Format("{0}{1:00}", ident, number);
            }
            return String.Format("{0} {1}", lineNumber, name);
        }
    }
}
