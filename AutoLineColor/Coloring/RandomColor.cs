using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AutoLineColor.Coloring
{
    class RandomColor
    {
        // generated from http://phrogz.net/css/distinct-colors.html
        // thanks to transport line color for the site - https://github.com/tuopppi/TransportLineColorMod/blob/master/TransportLineColorMod/ColorAssigner.cs
        private const string DefaultAllColors = "#f50302, #d60404, #990606, #c71818, #f52020, #991818, #d62f2f, #f53838, #991d02, #b82606, #d6340f, #993018, #f54f2a, #c74528, #c74900, #e55e10, #a8460d, #c75b1c, #994b1d, #e5712e, #b85e2a, #e58005, #b86909, #995c12, #e5922c, #c7812c, #997102, #d6a00b, #f5ba18, #b88c14, #e5b737, #997a25, #e5d410, #a89c19, #c7b922, #98a802, #b3c704, #d0e610, #8c991c, #8fc702, #7da811, #b0e627, #789925, #7de605, #6dc706, #5ea808, #629923, #3e990c, #69f51d, #37b81d, #54e637, #07f50b, #06c709, #29d64b, #22a83d, #38f55e, #0ec755, #29e671, #20a854, #069959, #11f592, #1ad685, #1fb876, #00f5b8, #02a87f, #2ee6b8, #22997b, #30c7a1, #03f5e5, #04d6c8, #28a8a0, #1ddcf5, #2ab5c7, #2599a8, #008bc7, #0aaef5, #147ca8, #29ade6, #0268c7, #0359a8, #107ee6, #155999, #2480d6, #216fb8, #3699f5, #0b45b8, #0f51d6, #0f3d99, #2268f5, #2058c7, #336ee6, #234a99, #2c5bb8, #0620b8, #1128a8, #2741d6, #2138b8, #1f3199, #3855f5, #0700d6, #0600b8, #0b02f5, #140f99, #3b35e6, #4619e6, #3e18c7, #4f2ec7, #3e2599, #4402a8, #7622f5, #712dd6, #5a25a8, #590099, #820dd6, #700db8, #a12cf5, #8830c7, #692599, #a502d6, #760399, #c014f5, #900fb8, #bc35e6, #8a28a8, #be0ec7, #ea14f5, #a223a8, #b80ba1, #e617ca, #991587, #b8027e, #f505a9, #990c6c, #d62da1, #a80054, #c70666, #e50e7a, #c72e7a, #f53b98, #99255f, #c7003f, #f5034f, #a80236, #99123d, #d61c57, #b81d4e, #f52f6d, #b8041c, #e5203a, #c72238, #a81e31, #e5374e";
        private const string DefaultBlueColors = "#16f5f1, #1ed6d3, #18a8a6, #00a6c7, #30c7e6, #2893a8, #0095e6, #056fa8, #1288c7, #2faff5, #226f99, #2c87b8, #034999, #0667d6, #075ab8, #167ef5, #1459a8, #1c73d6, #2c74c7, #3587e6, #255b99, #0a4cf5, #063099, #0b3cb8, #2761f5, #2255d6, #1f4ab8, #1a3e99, #3768e6, #031bf5, #0217d6, #0214b8, #021199, #1628c7, #1423a8, #2236e6, #2d3ed6";
        private const string DefaultGreenColors = "#6af500, #54b807, #7ad633, #8df53d, #467a1f, #66a832, #629939, #527a33, #87c756, #b6f587, #75995a, #a9d687, #97b87f, #738a62, #349912, #54d629, #3d8a24, #56b835, #83f55d, #87e667, #70b858, #629950, #8ed676, #7fb86c, #567a49, #b8f5a4, #087a00, #13a808, #2f7a2a, #45a83e, #427a3e, #a0f59a, #9cd698, #0be625, #3ed650, #4ef562, #6bd678, #7ff58d, #65b86f, #57995f, #79b880, #547a59, #06b83b, #048a2c, #2ab855, #28994a, #49e678, #2f8a4a, #367a4a, #74d691, #89f5a9, #5d996f, #93e6ac, #7fb890, #98d6ab";
        private const string DefaultOrangeColors = "#a80000, #c70202, #e51515, #991212, #c71c1c, #e52c2c, #b82727, #992525, #f52f02, #b82504, #991f03, #e5411c, #99311a, #b84025, #f55d3b, #d65133, #b84402, #f55e07, #d65911, #99400c, #c7632a, #a85525, #f57d38, #e58005, #c76f04, #a86718, #f5992a, #c7812c, #996423, #997000, #b88806, #f5bc20, #d6a51e, #a88628, #d6c400, #a89c16, #f5e322, #c7ba2c";

        private static Console logger = Console.Instance;
        private static Dictionary<ColorFamily, Color32[]> _colors;

        public static void Initialize()
        {
            _colors = new Dictionary<ColorFamily, Color32[]>
            {
                {ColorFamily.Any, BuildColorList(DefaultAllColors, "all.txt")},
                {ColorFamily.Blue, BuildColorList(DefaultBlueColors, "blues.txt")},
                {ColorFamily.Green, BuildColorList(DefaultGreenColors, "green.txt")},
                {ColorFamily.Orange, BuildColorList(DefaultOrangeColors, "orange.txt")}
            };
        }

        public static Color32 GetColor(ColorFamily colorFamily)
        {
            return _colors[colorFamily][Random.Range(0, _colors[colorFamily].Length - 1)];
        }

        private static Color32[] BuildColorList(string defaultColorList, string fileName)
        {
            // we need to load the color list
            var fullPath = Configuration.GetModFileName(fileName);
            var unparsedColors = defaultColorList;

            try
            {
                if (File.Exists(fullPath))
                {
                    unparsedColors = File.ReadAllText(fullPath);
                }
                else
                {
                    logger.Message("No colors found, writing default values to  " + fullPath);
                    File.WriteAllText(fullPath, unparsedColors);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error reading colors from disk " + ex);
            }

            // split on new lines, commas and semi-colons
            var colorHexValues = unparsedColors.Split(new[] { "\n", "\r", ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            var colorList = new List<Color32>();
            foreach (var colorHexValue in colorHexValues)
            {

                Color32 color;
                if (TryHexToColor(colorHexValue, out color))
                {
                    colorList.Add(color);
                }
            }
            return colorList.ToArray();
        }

        private static bool TryHexToColor(string hex, out Color32 color)
        {
            try
            {

                hex = hex.Replace("0x", ""); //in case the string is formatted 0xFFFFFF
                hex = hex.Replace("#", ""); //in case the string is formatted #FFFFFF
                hex = hex.Trim();

                byte alpha = 255; //assume fully visible unless specified in hex

                var red = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                var green = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                var blue = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

                //Only use alpha if the string has enough characters
                if (hex.Length == 8)
                {
                    alpha = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                }

                color = new Color32(red, green, blue, alpha);
                return true;

            }
            catch (Exception)
            {
                color = new Color32(0, 0, 0, 255);
                return false;
            }

        }

        public static Color32 GetColor(ColorFamily colorFamily, List<Color32> usedColors)
        {
            Color32 color;
            double difference;
            var atempts = 0;
            do
            {
                atempts++;
                color = GetColor(colorFamily);
                difference = CompareColorWithUsedColors(usedColors, color);

            } while (difference < Configuration.Instance.MinColorDiffPercentage && (atempts < Configuration.Instance.MaxDiffColorPickAttempt));

            if (difference <= 0)
            {
                foreach (var usedColor in usedColors)
                {
                    var differentColorFound = false;
                    foreach (var colorItem in _colors[colorFamily])
                    {
                        if (!usedColor.IsColorEqual(colorItem))
                        {
                            color = colorItem;
                            differentColorFound = true;
                            logger.Message(string.Format("Color not repeated: {0} Color2: {2} Diference: {1}", color, CompareColorWithUsedColors(usedColors, color), usedColor));
                            break;
                        }
                    }
                    if (differentColorFound)
                        break;
                }

            }

            logger.Message(string.Format("Diference: {0} Atempts: {1}", difference, atempts));

            return color;
        }

        private static double CompareColorWithUsedColors(IEnumerable<Color32> usedColors, Color32 color)
        {
            var difference = double.MaxValue;
            foreach (var usedColor in usedColors)
            {
                var auxDifference = CompareColors(color, usedColor);
                if (auxDifference < difference)
                {
                    difference = auxDifference;
                    if (difference <= 0)
                        return 0;
                }
            }
            return difference;
        }

        public static double CompareColors(Color32 color1, Color32 color2)
        {
            var r1 = color1.r;
            var r2 = color2.r;
            var g1 = color1.g;
            var g2 = color2.g;
            var b1 = color1.b;
            var b2 = color2.b;
            //var a1 = color1.a;
            //var a2 = color2.a;

            var d = Math.Sqrt(Math.Abs((r2 - r1) ^ 2 + (g2 - g1) ^ 2 + (b2 - b1) ^ 2));
            var p = d / Math.Sqrt((255) ^ 2 + (255) ^ 2 + (255) ^ 2 );

            if (Math.Abs(p) <= 0)
                logger.Message(string.Format("Color1: {1} Color2: {2} D: {0}", d, color1, color2));
            return p * 100;
        }
    }
}
