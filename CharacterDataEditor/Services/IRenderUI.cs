using CharacterDataEditor.Screens;
using Hardware.Info;
using ImGuiNET;
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
        public RaylibImGui(IScreenManager screenManager)
        {
            _screenManager = screenManager;
        }

        public int StartUI()
        {
            var logo = Raylib.LoadImage("Resources/logo.png");

            //get hardware info about screen resolution...
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshVideoControllerList();

            var height = hardwareInfo.VideoControllerList[0].CurrentVerticalResolution;
            var width = hardwareInfo.VideoControllerList[0].CurrentHorizontalResolution;

            var windowHeight = (int)(height - (height * 0.1));
            var windowWidth = (int)(width - (width * 0.1));

            //initialize the graphics lib
            Raylib.InitWindow(windowWidth, windowHeight, "Knockout Arcade - Character Data Editor");
            Raylib.SetWindowIcon(logo);
            Raylib.SetTargetFPS(60);

            _screenManager.ScreenScale = height / 720.0f;


            //create a context to access ImGui
            var context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            //create a reference to the gui controller
            var controller = new GuiController();

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

            controller.Shutdown();

            return 0;
        }
    }
}
