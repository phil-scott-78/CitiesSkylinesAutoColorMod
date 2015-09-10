using UnityEngine;
using System.Linq;
using System;

namespace AutoLineColor.Coloring
{
    internal class CategorisedColorStrategy : IColorStrategy
    {
        public Color32 GetColor(TransportLine transportLine)
        {
            switch (transportLine.Info.m_transportType)
            {
                case TransportInfo.TransportType.Bus:
                    return CategorisedColor.GetPaleColor(null);
                case TransportInfo.TransportType.Metro:
                    return CategorisedColor.GetBrightColor(null);
                default:
                    return CategorisedColor.GetDarkColor(null);
            }
        }

        public Color32 GetColor(TransportLine transportLine, System.Collections.Generic.List<Color32> usedColors)
        {
            switch (transportLine.Info.m_transportType)
            {
                case TransportInfo.TransportType.Bus:
                    return CategorisedColor.GetPaleColor(usedColors);
                case TransportInfo.TransportType.Metro:
                    return CategorisedColor.GetBrightColor(usedColors);
                default:
                    return CategorisedColor.GetDarkColor(usedColors);
            }
        }
    }
}
