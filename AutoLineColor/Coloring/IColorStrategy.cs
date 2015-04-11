using UnityEngine;

namespace AutoLineColor.Coloring
{
    internal interface IColorStrategy
    {
        Color32 GetColor(TransportLine transportLine);

        Color32 GetColor(TransportLine transportLine, System.Collections.Generic.List<Color32> usedColors);
    }
}