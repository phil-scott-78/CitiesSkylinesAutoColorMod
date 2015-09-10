using System;
using UnityEngine;

namespace AutoLineColor.Coloring
{
    public class RandomColorStrategy : IColorStrategy
    {
        public Color32 GetColor(TransportLine transportLine)
        {
            return RandomColor.GetColor(ColorFamily.Any);
        }

        public Color32 GetColor(TransportLine transportLine, System.Collections.Generic.List<Color32> usedColors)
        {
            return RandomColor.GetColor(ColorFamily.Any, usedColors);
        }
    }
}