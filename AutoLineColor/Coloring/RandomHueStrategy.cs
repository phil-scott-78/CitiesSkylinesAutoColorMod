using UnityEngine;
using System.Linq;
using System;

namespace AutoLineColor.Coloring
{
    internal class RandomHueStrategy : IColorStrategy
    {
        public Color32 GetColor(TransportLine transportLine)
        {
            switch (transportLine.Info.m_transportType)
            {
                case TransportInfo.TransportType.Bus:
                    return RandomColor.GetColor(ColorFamily.Blue);
                case TransportInfo.TransportType.Metro:
                    return RandomColor.GetColor(ColorFamily.Green);
                case TransportInfo.TransportType.Train:
                    return RandomColor.GetColor(ColorFamily.Orange);
                default:
                    return RandomColor.GetColor(ColorFamily.Any);
            }
        }


        public Color32 GetColor(TransportLine transportLine, System.Collections.Generic.List<Color32> usedColors)
        {
            switch (transportLine.Info.m_transportType)
            {
                case TransportInfo.TransportType.Bus:
                    return RandomColor.GetColor(ColorFamily.Blue, usedColors);
                case TransportInfo.TransportType.Metro:
                    return RandomColor.GetColor(ColorFamily.Green, usedColors);
                case TransportInfo.TransportType.Train:
                    return RandomColor.GetColor(ColorFamily.Orange, usedColors);
                default:
                    return RandomColor.GetColor(ColorFamily.Any, usedColors);
            }
        }
    }
}