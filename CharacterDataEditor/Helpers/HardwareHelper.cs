using Hardware.Info;
using System.Numerics;

namespace CharacterDataEditor.Helpers
{
    public static class HardwareHelper
    {
        public static Vector2 GetClientWindowSize()
        {
            var monitorResolution = GetMonitorResolution();

            var windowHeight = (int)(monitorResolution.Y - (monitorResolution.Y * 0.1));
            var windowWidth = (int)(monitorResolution.X - (monitorResolution.X * 0.1));

            return new Vector2(windowWidth, windowHeight);
        }

        public static Vector2 GetMonitorResolution()
        {
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshVideoControllerList();

            var height = hardwareInfo.VideoControllerList[0].CurrentVerticalResolution;
            var width = hardwareInfo.VideoControllerList[0].CurrentHorizontalResolution;

            return new Vector2(width, height);
        }
    }
}
