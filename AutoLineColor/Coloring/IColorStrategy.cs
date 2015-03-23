using UnityEngine;

namespace AutoLineColor.Coloring
{
    internal interface IColorStrategy
    {
        Color32 GetColor(TransportLine transportLine);
    }
}