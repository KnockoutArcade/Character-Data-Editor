using System;
using System.IO;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Resources;
using CharacterDataEditor.Screens;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;

namespace CharacterDataEditor.Services;

public interface IRenderUI
{
    public int StartUI();
}

public class RaylibImGui : IRenderUI
{
    private readonly IScreenManager _screenManager;
    private readonly ILogger<IRenderUI> _logger;
    private readonly IDisplayInfo _displayInfo;

    public RaylibImGui(IScreenManager screenManager, IDisplayInfo displayInfo, ILogger<IRenderUI> logger)
    {
        _screenManager = screenManager;
        _displayInfo = displayInfo;
        _logger = logger;
    }

    public int StartUI()
    {
        //set logging to error only when building for release
#if DEBUG
        Raylib.SetTraceLogLevel(TraceLogLevel.All);
#else
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_ERROR);
#endif

        var logoPath = Path.Combine(AppContext.BaseDirectory, ResourceConstants.LogoPath);
        var logo = Raylib.LoadImage(logoPath);

        _logger.LogInformation("Icon Loaded");

        //get hardware info about screen resolution...
        var monitorSize = _displayInfo.MonitorResolution;
        var clientWindow = _displayInfo.ClientSize;

        _logger.LogInformation($"Default client area determined based on resolution of {monitorSize.X} by {monitorSize.Y}");

        //initialize the graphics lib
        Raylib.InitWindow((int)clientWindow.X, (int)clientWindow.Y, TitleConstants.Title);
        Raylib.SetExitKey(KeyboardKey.Null); //disable escape to close
        Raylib.SetWindowIcon(logo);
        Raylib.SetTargetFPS(60);

        _logger.LogInformation("Window created with Raylib and sent to video card");

        //re-verify the screen resolution with RayLib...
        var raylibMonitor = Raylib.GetCurrentMonitor();
        var raylibWidth = Raylib.GetMonitorWidth(raylibMonitor);
        var raylibHeight = Raylib.GetMonitorHeight(raylibMonitor);

        if (_displayInfo.TryUpdateMonitorResolution(raylibWidth, raylibHeight))
        {
            var correctedSize = _displayInfo.ClientSize;

            if (correctedSize != clientWindow)
            {
                _logger.LogInformation($"Monitor mis-detected, correcting window to {correctedSize.X} by {correctedSize.Y}");
                Raylib.SetWindowSize((int)correctedSize.X, (int)correctedSize.Y);
            }
        }

        _screenManager.BackgroundColor = Color.DarkGray;

        _logger.LogInformation($"GUI Scaling calculated to be: {_screenManager.ScreenScale}");

        //create a context to access ImGui
        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        //create a reference to the gui controller
        var controller = new GuiController();

        _logger.LogInformation("DearIMGui context and controller created, beginning main render loop");

        _screenManager.NavigateTo(typeof(MainScreen));
        _logger.LogInformation("Setting initial screen to MainScreen");

        var fragShader = System.Text.Encoding.Default.GetString(Shaders.PaletteSwapFragment);

        ShaderHelper.InitShader(null, fragShader);
        _logger.LogInformation("Initializing shaders... ignore any warnings about missing shader variables...");

        //enter the program loop
        while (!_screenManager.ExitWindow)
        {
            _screenManager.CurrentScreen.CheckForExit(_screenManager);

            controller.NewFrame();
            controller.ProcessEvent();
            ImGui.NewFrame();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(_screenManager.BackgroundColor);

            //render goes here
            _screenManager.CurrentScreen.RenderImGui(_screenManager);

            ImGui.Render();
            controller.Render(ImGui.GetDrawData());

            _screenManager.CurrentScreen.RenderAfterImGui(_screenManager);

            Raylib.EndDrawing();
        }

        ShaderHelper.DeInitShader();
        _logger.LogInformation("Cleaning up shaders...");

        _logger.LogInformation("Render loop exited... shutting down...");

        controller.Shutdown();

        return 0;
    }
}
