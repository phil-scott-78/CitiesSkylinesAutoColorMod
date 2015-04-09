using UnityEngine;

namespace AutoLineColor.Coloring
{
    internal class RandomColorStrategy : IColorStrategy
    {
        public Color32 GetColor(TransportLine transportLine)
        {
            return RandomColor.GetColor(ColorFamily.Any);
        }
    }
}