using System.Numerics;
using Hardware.Info;
using Microsoft.Extensions.Logging;

namespace CharacterDataEditor.Services;

public interface IDisplayInfo
{
    public Vector2 MonitorResolution { get; }
    public Vector2 ClientSize { get; }
    public float ScreenScale { get; }
    public bool TryUpdateMonitorResolution(int width, int height);
}

public class DisplayInfo : IDisplayInfo
{
    private readonly ILogger<IDisplayInfo> _logger;

    private Vector2 _monitorResolution;
    private bool _init;

    public DisplayInfo(ILogger<DisplayInfo> logger)
    {
        _logger = logger;
    }

    public Vector2 MonitorResolution
    {
        get
        {
            EnsureInitialized();
            return _monitorResolution;
        }
    }

    public Vector2 ClientSize
    {
        get
        {
            EnsureInitialized();

            var clientHeight = (int)(MonitorResolution.Y - (MonitorResolution.Y * 0.1));
            var clientWidth = (int)(MonitorResolution.X - (MonitorResolution.X * 0.1));

            return new Vector2(clientWidth, clientHeight);
        }
    }

    public float ScreenScale => MonitorResolution.Y / 650.0f;

    public bool TryUpdateMonitorResolution(int width, int height)
    {
        EnsureInitialized();

        if (width <= 0 || height <= 0)
        {
            _logger.LogWarning("Ignoring invalid resolution size {Width} x {Height}", width, height);
            return false;
        }

        var resolution = new Vector2(width, height);

        if (resolution != _monitorResolution)
        {
            _logger.LogInformation(
                "Correcting monitor resolution from {OldWidth} x {OldHeight} to {Width} x {Height}",
                _monitorResolution.X,
                _monitorResolution.Y,
                width,
                height
            );

            _monitorResolution = resolution;
        }

        return true;
    }

    private void EnsureInitialized()
    {
        if (_init)
        {
            return;
        }

        var hardwareInfo = new HardwareInfo();
        hardwareInfo.RefreshVideoControllerList(false);

        var resolution = new Vector2(1280, 720);
        var detected = false;

        foreach (var controller in hardwareInfo.VideoControllerList)
        {
            var width = controller.CurrentHorizontalResolution;
            var height = controller.CurrentVerticalResolution;

            if (width > 0 && height > 0)
            {
                resolution = new Vector2(width, height);
                detected = true;
                break;
            }
        }

        if (!detected)
        {
            _logger.LogWarning("HardwareInfo returned no valid display resolutions... using 720P fallback.");
        }

        _monitorResolution = resolution;
        _init = true;
    }
}
