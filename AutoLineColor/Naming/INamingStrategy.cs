namespace AutoLineColor.Naming
{
    internal interface INamingStrategy
    {
        string GetName(TransportLine transportLine);
    }
}