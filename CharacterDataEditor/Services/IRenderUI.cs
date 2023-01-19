using CharacterDataEditor.Screens;
using Hardware.Info;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System.Threading;

namespace CharacterDataEditor.Services
{
    public interface IRenderUI
    {
        public int StartUI();
    }

    public class RaylibImGui : IRenderUI
    {
        private readonly IScreenManager _screenManager;
        private readonly ILogger<IRenderUI> _logger;
        public RaylibImGui(IScreenManager screenManager, ILogger<IRenderUI> logger)
        {
            _screenManager = screenManager;
            _logger = logger;
        }

        public int StartUI()
        {
            var logo = Raylib.LoadImage("Resources/logo.png");
            _logger.LogInformation("Icon Loaded");

            //get hardware info about screen resolution...
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshVideoControllerList();

            var height = hardwareInfo.VideoControllerList[0].CurrentVerticalResolution;
            var width = hardwareInfo.VideoControllerList[0].CurrentHorizontalResolution;

            var windowHeight = (int)(height - (height * 0.1));
            var windowWidth = (int)(width - (width * 0.1));

            _logger.LogInformation($"Default client area determined based on resolution of {width} by {height}");

            //initialize the graphics lib
            Raylib.InitWindow(windowWidth, windowHeight, "Knockout Arcade - Character Data Editor");
            Raylib.SetWindowIcon(logo);
            Raylib.SetTargetFPS(60);
            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_ERROR);

            _logger.LogInformation("Window created with Raylib and sent to video card");

            _screenManager.ScreenScale = height / 720.0f;

            _logger.LogInformation($"GUI Scaling calculated to be: {_screenManager.ScreenScale}");


            //create a context to access ImGui
            var context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            //create a reference to the gui controller
            var controller = new GuiController();

            _logger.LogInformation("DearIMGui context and controller created, beginning main render loop");

            _screenManager.NavigateTo(typeof(MainScreen), new { height = (float)windowHeight, width = (float)windowWidth });
            _logger.LogInformation("Setting initial screen to MainScreen");

            //enter the program loop
            while (!Raylib.WindowShouldClose())
            {
                controller.NewFrame();
                controller.ProcessEvent();
                ImGui.NewFrame();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DARKGRAY);

                //render goes here
                _screenManager.CurrentScreen.Render(_screenManager);

                ImGui.Render();
                controller.Render(ImGui.GetDrawData());
                Raylib.EndDrawing();
            }
            
            _logger.LogInformation("Render loop exited... shutting down...");

            controller.Shutdown();

            return 0;
        }
    }
}
