using UnityEngine;

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
    }
}