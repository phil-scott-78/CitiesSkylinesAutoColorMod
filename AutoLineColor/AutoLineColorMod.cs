using ICities;

namespace AutoLineColor
{
    public class AutoLineColorMod : IUserMod
    {
        public string Name
        {
            get { return "Auto Line Color"; }
        }

        public string Description
        {
            get
            {
                return
                    "Monitors all transport line looking for lines set to the default color. When found it sets a new color and a line name";
            }
        }
    }
}