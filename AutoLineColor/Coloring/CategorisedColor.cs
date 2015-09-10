using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Categorised coloring scheme
 * ---------------------------
 * 
 * This scheme uses a deterministic categorised coloring scheme.
 * 
 * The basis is a set of 18 hues:
 *   00EE20,Green
 *   00FF8A Mint green
 *   00FFC9 Turquoise
 *   00E4FF Aqua
 *   00ACFF Cornflower blue
 *   006EFF Cerulean blue
 *   0014FF Blue
 *   4900FF Violet
 *   9900FF Purple
 *   EA00FF Magenta
 *   FF00B4 Pink
 *   FF005F Salmon
 *   FF0300 Red
 *   FF4500 Orangered
 *   FFC000 Gold
 *   FFF700 Yellow
 *   A5FF00 Lawn green
 *   60FF00 Lime green
 * 
 * These are transformed as follows:
 *   Bus lines (pale):
 *     Start from green, step 7 places each time
 *     At first lightened by ~55%, then ~40%, then ~25%
 *   Metro lines (bright):
 *     Start from cerulean blue
 *     At first as-is, and then darkened by 50%
 *   Train lines (dark):
 *     Start from orangered
 *     Darkened by 75%
 * 
 * Once all colours of a particular category are exhausted, the sequence repeats.
 */

namespace AutoLineColor.Coloring
{
    class CategorisedColor
    {
        private const string DefaultBrightColors = "#00ee20, #4900ff, #ffc000, #00e4ff, #ff00b4, #60ff00, #0014ff, #ff4500, #00ffc9, #ea00ff, #a5ff00, #006eff, #ff0300, #00ff8a, #9900ff, #fff700, #00acff, #ff005f, #00a816, #3300b4, #b48700, #00a1b4, #b4007f, #43b400, #000eb4, #b43000, #00b48e, #a500b4, #74b400, #004db4, #b40200, #00b461, #6c00b4, #b4ae00, #0079b4, #b40043"; 
        private const string DefaultPaleColors = "#bccaff, #ffbcbc, #bcffd2, #d6bcff, #fffbbc, #bcddff, #ffbcc6, #bcf7bd, #c2bcff, #ffe4bc, #bcf3ff, #ffbce0, #c7ffbc, #bcbcff, #ffc1bc, #bcffe8, #f5bcff, #daffbc, #a6baff, #ffa6a6, #a6ffc4, #cba6ff, #fffaa6, #a6d3ff, #ffa6b5, #a6f5a8, #afa6ff, #ffdda6, #a6efff, #ffa6d7, #b6ffa6, #a6a7ff, #ffaea6, #a6ffe1, #f3a6ff, #d0ffa6, #7f9eff, #ff7f7f, #7fffae, #b77fff, #fff97f, #7fc3ff, #ff7f97, #7ff282, #8d7fff, #ffd17f, #7feaff, #ff7fc9, #97ff7f, #7f80ff, #ff8c7f, #7fffd7, #ef7fff, #bfff7f";
        private const string DefaultDarkColors = "#7f2200, #007f64, #75007f, #527f00, #00377f, #7f0100, #007f45, #4c007f, #7f7b00, #00567f, #7f002f, #007710, #24007f, #7f6000, #00727f, #7f005a, #307f00, #000a7f";

        private static Console logger = Console.Instance;
        private static List<Color32> _bright_colors;
        private static List<Color32> _pale_colors;
        private static List<Color32> _dark_colors;
        private static Color32 _black = new Color32(0, 0, 0, 255);

        public static void Initialize()
        {
            _bright_colors = BuildColorList(DefaultBrightColors, "bright.txt");
            _pale_colors = BuildColorList(DefaultPaleColors, "pale.txt");
            _dark_colors = BuildColorList(DefaultDarkColors, "dark.txt");
        }

        private static List<Color32> BuildColorList(string defaultColorList, string fileName)
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
            return colorList;
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
                color = _black;
                return false;
            }

        }

        private static Color32 GetColor(List<Color32> colors, List<Color32> usedColors)
        {
            var color = _black;
            int usedCount = -1;
            foreach (var candidateColor in colors)
            {
                int candidateUsedCount = 0;
                if (usedColors != null)
                {
                    foreach (var usedColor in usedColors)
                    {
                        if (candidateColor.IsColorEqual(usedColor))
                        {
                            candidateUsedCount++;
                        }
                    }
                }
                if (usedCount == -1 || candidateUsedCount < usedCount)
                {
                    color = candidateColor;
                    usedCount = candidateUsedCount;
                }
            }
            return color;
        }

        public static Color32 GetBrightColor(List<Color32> usedColors)
        {
            return GetColor(_bright_colors, usedColors);
        }

        public static Color32 GetPaleColor(List<Color32> usedColors)
        {
            return GetColor(_pale_colors, usedColors);
        }

        public static Color32 GetDarkColor(List<Color32> usedColors)
        {
            return GetColor(_dark_colors, usedColors);
        }
    }
}
