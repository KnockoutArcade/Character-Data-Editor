using CharacterDataEditor.Screens;
using ImGuiNET;
using Raylib_cs;

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
            //initial the graphics lib
            Raylib.InitWindow(1920, 1080, "Knockout Arcade - Character Data Editor");
            Raylib.SetWindowIcon(logo);
            Raylib.SetTargetFPS(60);

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
                _screenManager.CurrentScreen.Render();

                ImGui.Render();
                controller.Render(ImGui.GetDrawData());
                Raylib.EndDrawing();
            }

            controller.Shutdown();

            return 0;
        }
    }
}
