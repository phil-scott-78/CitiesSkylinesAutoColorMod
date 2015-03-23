namespace AutoLineColor.Naming
{
    internal class NoNamingStrategy : INamingStrategy
    {
        public string GetName(TransportLine transportLine)
        {
            return null;
        }
    }
}